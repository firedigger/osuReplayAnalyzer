using BMAPI.v1;
using Microsoft.Win32;
using osuDodgyMomentsFinder;
using ReplayAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using WinForms = System.Windows.Forms;
using osuDatabase = OsuDbAPI;
using System.Diagnostics;

namespace WPF_GUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private List<Replay> listReplays;
		private Dictionary<string, string> dMapsDatabase;
		private MainControlFrame settings;

		private string sCachedLabelText = string.Empty;
		private string osuFilePath = string.Empty;
		//private string osuReplaysPath = string.Empty;

		public MainWindow()
		{
			listReplays = new List<Replay>();
			dMapsDatabase = new Dictionary<string, string>();
            settings = new MainControlFrame();

            try
            {
                InitializeComponent();

                if (File.Exists(settings.pathSettings))
                {
                    settings.LoadSettings();
                }

                if (!string.IsNullOrEmpty(settings.pathSongs) && !string.IsNullOrEmpty(settings.pathOsuDB))
                {
                    ParseDatabaseFile(settings.pathOsuDB, settings.pathSongs);
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show("Error!\n" + exp.ToString());
            }
		}

		private void button_ChooseReplays_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog replayFileDialog = new OpenFileDialog();
			replayFileDialog.Multiselect = true;
			replayFileDialog.Filter = "osu! replay files|*.osr";

			if(replayFileDialog.ShowDialog() ?? true)
			{
				bool bErrors = false;
				listReplays = new List<Replay>();

				string log = string.Empty;

				foreach(var replayFile in replayFileDialog.FileNames)
				{
					try
					{
						listReplays.Add(new Replay(replayFile, true));
					}

					catch(Exception ex)
					{
						if(!bErrors)
						{
							log = "Some of the replays could not be read:" + Environment.NewLine;
						}

						log += string.Format("Failed to process {0} ({1}){2}", replayFile, ex.Message, Environment.NewLine);

						bErrors = true;
					}
				}

				if(!bErrors)
				{
					log = "All replays were successfully read!";
                    if (replayFileDialog.FileNames.Length > 0)
                    {
                        string directoryName = Path.GetDirectoryName(replayFileDialog.FileNames[0]);
                        if (string.IsNullOrEmpty(settings.pathReplays))
                            settings.pathReplays = directoryName;

                    }
                }

				MessageBox.Show(log);
			}
		}

		private void button_ChooseFolder_Click(object sender, RoutedEventArgs e)
		{
			WinForms.FolderBrowserDialog replayFolderDialog = new WinForms.FolderBrowserDialog();

			if(replayFolderDialog.ShowDialog() == WinForms.DialogResult.OK)
			{
				settings.pathReplays = replayFolderDialog.SelectedPath;
			}
		}

		private void button_ChooseMap_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog osuFileDialog = new OpenFileDialog();
			osuFileDialog.Filter = "osu! beatmap file|*.osu";

			if(osuFileDialog.ShowDialog() ?? true)
			{
				osuFilePath = osuFileDialog.FileName;
			}
		}

		private Beatmap FindBeatmapInDatabase(Replay replay)
		{
			Beatmap beatMap = null;

			if(!string.IsNullOrEmpty(osuFilePath))
			{
				beatMap = new Beatmap(osuFilePath);
			}

			if(beatMap != null && replay.MapHash.Equals(beatMap.BeatmapHash))
			{
				return beatMap;
			}

			else
			{
				if(dMapsDatabase.ContainsKey(replay.MapHash))
				{
					return new Beatmap(dMapsDatabase[replay.MapHash]);
				}

				else
				{
					return null;
				}
			}
		}

		private void button_AnalyzeReplays_Click(object sender, RoutedEventArgs e)
		{
			Beatmap beatMap = null;
			StringBuilder sb = new StringBuilder();

			labelTask.Content = "Analyzing replays...";

			if(listReplays.Count > 0)
			{
				int iQueue = 0;
                bool found = false;

				foreach(Replay replay in listReplays)
				{
					progressBar_Analyzing.Value = (100.0 / listReplays.Count) * iQueue++;

					beatMap = FindBeatmapInDatabase(replay);

					if(beatMap != null)
					{
                        found = true;
						sb.AppendLine(Program.ReplayAnalyzing(beatMap, replay).ToString());
					}

					else
					{
						sb.AppendLine(string.Format("{0} does not correspond to any known map", replay.Filename));
					}
				}

                if (!found)
                {
                    sb.AppendLine("You have probably not imported the map(s). Make sure to load your osu DB using the button.");
                }

				progressBar_Analyzing.Value = 100.0;

				SaveResult(sb);
			}
			else
			{
				MessageBox.Show("Error! No replays selected.");
			}

			labelTask.Content = "Finished analyzing replays.";
		}

		private void ParseDatabaseFile(string databasePath, string songsFolder)
		{
            try
            {
                osuDatabase.OsuDbFile osuDatabase = new osuDatabase.OsuDbFile(databasePath);


                labelTask.Content = "Processing beatmaps from database...";

                Dictionary<string, string> dTempDatabase = new Dictionary<string, string>();

                int iQueue = 0;

                foreach (osuDatabase.Beatmap beatMap in osuDatabase.Beatmaps)
                {
                    progressBar_Analyzing.Value = (100.0 / osuDatabase.Beatmaps.Count) * iQueue++;

                    osuDatabase.Beatmap dbBeatmap = beatMap;

                    if (dbBeatmap != null && !string.IsNullOrEmpty(dbBeatmap.Hash))
                    {
                        string beatmapPath = string.Format("{0}\\{1}\\{2}", songsFolder, dbBeatmap.FolderName, dbBeatmap.OsuFile);
                        if(!dTempDatabase.ContainsKey(dbBeatmap.Hash))
                                dTempDatabase.Add(dbBeatmap.Hash, beatmapPath);
                    }
                }

                progressBar_Analyzing.Value = 100.0;
                dMapsDatabase = dTempDatabase;

                labelTask.Content = "Finished processing beatmaps from osuDB.";
            } catch (Exception exp)
            {
                MessageBox.Show("Error reading osuDB \n" + exp.ToString());
            }
		}

		private void SaveResult(StringBuilder sb)
		{
			// remove whitespaces
			string sTemp = sb.ToString();
			sTemp = sTemp.Trim();
			sb = new StringBuilder(sTemp);

			if(checkBox_SaveToFile.IsChecked ?? true)
			{
				SaveFileDialog saveReportDialog = new SaveFileDialog();
				saveReportDialog.FileName = "Report " + DateTime.Now.ToString("MMMM dd, yyyy H-mm-ss") + ".txt";
				saveReportDialog.Filter = "All Files|*.*;";

				if(saveReportDialog.ShowDialog() ?? true)
				{
					File.WriteAllText(saveReportDialog.FileName, sb.ToString());
				}
			}

			if(checkBox_AlertOutput.IsChecked ?? true)
			{
				ReportDialog dialog = new ReportDialog();
				dialog.textBox_Results.Text = sb.ToString();
				dialog.textBox_Results.HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled;
				dialog.textBox_Results.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
				dialog.Show();
			}
		}

		private void button_OpenDatabase_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog osuFileDialog = new OpenFileDialog();
			osuFileDialog.Filter = "osu! database file|osu!.db";
			osuFileDialog.Multiselect = false;

			if(osuFileDialog.ShowDialog() ?? true)
			{
				settings.pathOsuDB = osuFileDialog.FileName;
				MessageBox.Show("Now select Songs folder.");

				WinForms.FolderBrowserDialog songsFolderDialogue = new WinForms.FolderBrowserDialog();
				songsFolderDialogue.SelectedPath = Path.GetDirectoryName(osuFileDialog.FileName);

				if(songsFolderDialogue.ShowDialog() == WinForms.DialogResult.OK)
				{
					settings.pathSongs = songsFolderDialogue.SelectedPath;
					ParseDatabaseFile(osuFileDialog.FileName, songsFolderDialogue.SelectedPath);
				}
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			settings.saveSettings();
		}

		private void button_AnalyzeFolder_Click(object sender, RoutedEventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			Dictionary<Beatmap, Replay> pairs = new Dictionary<Beatmap, Replay>();

			labelTask.Content = "Analyzing replays in folder...";

			try
			{
				DirectoryInfo directory = new DirectoryInfo(settings.pathReplays);
				FileInfo[] files = directory.GetFiles();

				List<string> replaysFiles = new List<string>();

				foreach(FileInfo file in files)
				{
					if(file.Extension == ".osr")
					{
						replaysFiles.Add(file.FullName);
					}
				}

				progressBar_Analyzing.Value = 0.0;

				List<Replay> replays = new List<Replay>();
				int iQueue = 0;

				foreach(string file in replaysFiles)
				{
					progressBar_Analyzing.Value = (100.0 / replaysFiles.Count) * iQueue++;

					string path = file;
					Replay replay = new Replay(path, true);
					Beatmap map = FindBeatmapInDatabase(replay);
					sb.AppendLine(Program.ReplayAnalyzing(map, replay).ToString());
				}
			}

			catch(Exception exp)
			{
				MessageBox.Show(exp.ToString());
			}

			finally
			{
				progressBar_Analyzing.Value = 100.0;
				labelTask.Content = "Finished analyzing replays in folder...";

				SaveResult(sb);
			}
		}

		private StringBuilder AvsBReplaysCompare(List<Replay> replaysA, List<Replay> replaysB)
		{
			StringBuilder sb = new StringBuilder();

			labelTask.Content = "Comparing replays...";
			progressBar_Analyzing.Value = 0.0;

			int iQueue = 0;
			int iCount = replaysA.Count * replaysB.Count;

			foreach(Replay replayA in replaysA)
			{
				foreach(Replay replayB in replaysB)
				{
					ReplayComparator comparator = new ReplayComparator(replayA, replayB);
					double diff = comparator.compareReplays();

					if(diff < 10.0)
					{
						sb.Append("IDENTICAL REPLAY PAIR: ");
					}

					sb.AppendLine(string.Format("{0} vs. {1} = {2}", replayA.Filename, replayB.Filename, diff));

					progressBar_Analyzing.Value = (100.0 / iCount) * iQueue++;
				}
			}

			progressBar_Analyzing.Value = 100.0;
			labelTask.Content = "Finished comparing replays.";

			return sb;
		}

		private StringBuilder AllVSReplaysCompare(List<Replay> replays)
		{
			StringBuilder sb = new StringBuilder();

			labelTask.Content = "Comparing replays...";
			progressBar_Analyzing.Value = 0.0;

			int iFirstQueue = 0;
			int iQueue = 0;
			int count = replays.Count * (replays.Count - 1) / 2;

			foreach(Replay replayA in replays)
			{
				for(int i = iFirstQueue + 1; i < replays.Count; i++)
				{
					if(replayA.ReplayHash == replays[i].ReplayHash)
					{
						continue;
					}

					ReplayComparator comparator = new ReplayComparator(replayA, replays[i]);
					double diff = comparator.compareReplays();

					if(diff < 10.0)
					{
						sb.Append("IDENTICAL REPLAY PAIR: ");
					}

					sb.AppendLine(string.Format("{0} vs. {1} = {2}", replayA.Filename, replays[i].Filename, diff));
					progressBar_Analyzing.Value = (100.0 / count) * iQueue++;
				}

				iFirstQueue++;
			}

			progressBar_Analyzing.Value = 100.0;
			labelTask.Content = "Finished comparing replays.";

			return sb;
		}

		private void button_CompareSelected_Click(object sender, RoutedEventArgs e)
		{
			SaveResult(AllVSReplaysCompare(listReplays));
		}

		private void button_CompareFolder_Click(object sender, RoutedEventArgs e)
		{
			StringBuilder sb = new StringBuilder();

			Dictionary<Beatmap, Replay> pairs = new Dictionary<Beatmap, Replay>();
			DirectoryInfo directory = new DirectoryInfo(settings.pathReplays);
			FileInfo[] files = directory.GetFiles();

			List<string> replaysFiles = new List<string>();

			foreach(FileInfo file in files)
			{
				if(file.Extension == ".osr")
				{
					replaysFiles.Add(file.FullName);
				}
			}

			labelTask.Content = "Comparing replays in folder...";
			progressBar_Analyzing.Value = 0.0;

			List<Replay> replays = new List<Replay>();

			int iQueue = 0;

			foreach(string path in replaysFiles)
			{
				try
				{
					progressBar_Analyzing.Value = (100.0 / replaysFiles.Count) * iQueue++;

					Replay replay = new Replay(path, true);
					replays.Add(replay);
				}

				catch(Exception ex)
				{
					sb.AppendLine(string.Format("Failed to process {0} ({1})", path, ex.Message));
				}
			}

			progressBar_Analyzing.Value = 100.0;
			labelTask.Content = "Finished comparing replays in folder.";

			SaveResult(new StringBuilder(sb.ToString() + AllVSReplaysCompare(replays).ToString()));
		}

		private void button_CompareSelectedAgainstFolder_Click(object sender, RoutedEventArgs e)
		{
			StringBuilder sb = new StringBuilder();

			Dictionary<Beatmap, Replay> pairs = new Dictionary<Beatmap, Replay>();
			DirectoryInfo directory = new DirectoryInfo(settings.pathReplays);
			FileInfo[] files = directory.GetFiles();

			List<string> replaysFiles = new List<string>();

			foreach(FileInfo file in files)
			{
				if(file.Extension == ".osr")
				{
					replaysFiles.Add(file.FullName);
				}
			}

			progressBar_Analyzing.Value = 0;
			List<Replay> replays = new List<Replay>();

			int iQueue = 0;

			foreach(string path in replaysFiles)
			{
				try
				{
					progressBar_Analyzing.Value = (100.0 / replaysFiles.Count) * iQueue++;

					Replay replay = new Replay(path, true);
					replays.Add(replay);
				}

				catch(Exception ex)
				{
					sb.AppendLine(string.Format("Failed to process {0} ({1})", path, ex.Message));
				}
			}

			SaveResult(new StringBuilder(sb.ToString() + AvsBReplaysCompare(listReplays, replays).ToString()));
		}

        private void rawData_button_Click(object sender, RoutedEventArgs e)
        {
            labelTask.Content = "Converting replays...";

            if (listReplays.Count > 0)
            {
                int iQueue = 0;

                foreach (Replay replay in listReplays)
                {
                    progressBar_Analyzing.Value = (100.0 / listReplays.Count) * iQueue++;
                    File.WriteAllText(replay.Filename + ".RAW.txt", replay.SaveText());
                }

                progressBar_Analyzing.Value = 100.0;

                //SaveResult(sb);
            }
            else
            {
                MessageBox.Show("Error! No replays selected.");
            }

            labelTask.Content = "Finished converting replays.";
        }

        private void openReplaysFolder_button_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();

            process.StartInfo.UseShellExecute = true;

            process.StartInfo.FileName = "explorer";

            if (!string.IsNullOrEmpty(settings.pathReplays))
                process.StartInfo.Arguments = settings.pathReplays;

            process.Start();
        }

        private void rawFeatures_button_Click(object sender, RoutedEventArgs e)
        {
            Beatmap beatMap = null;
            StringBuilder sb = new StringBuilder();

            labelTask.Content = "Collecting data from replays...";

            if (listReplays.Count > 0)
            {
                int iQueue = 0;
                bool found = false;

                foreach (Replay replay in listReplays)
                {
                    progressBar_Analyzing.Value = (100.0 / listReplays.Count) * iQueue++;

                    beatMap = FindBeatmapInDatabase(replay);

                    if (beatMap != null)
                    {
                        found = true;
                        sb.AppendLine(Program.ReplayDataCollecting(beatMap, replay).ToString());
                    }

                    else
                    {
                        sb.AppendLine(string.Format("{0} does not correspond to any known map", replay.Filename));
                    }
                }

                if (!found)
                {
                    sb.AppendLine("You have probably not imported the map(s). Make sure to load your osu DB using the button.");
                }

                progressBar_Analyzing.Value = 100.0;

                SaveFileDialog saveReportDialog = new SaveFileDialog();
                saveReportDialog.FileName = "Data " + DateTime.Now.ToString("MMMM dd, yyyy H-mm-ss") + ".txt";
                saveReportDialog.Filter = "All Files|*.*;";

                if (saveReportDialog.ShowDialog() ?? true)
                {
                    File.WriteAllText(saveReportDialog.FileName, sb.ToString());
                }
            }
            else
            {
                MessageBox.Show("Error! No replays selected.");
            }

            labelTask.Content = "Finished collecting data from replays.";
        }
    }
}
