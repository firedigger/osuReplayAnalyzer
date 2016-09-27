using BMAPI.v1;
using osuDodgyMomentsFinder;
using ReplayAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI
{
    public partial class Form1 : Form
    {
        private List<Replay> replays;
        private Dictionary<string, string> mapsDB;

        private MainControlFrame settings;
        private bool cacheLabel = false;
        private string cachedLabelText = "";

        private string osuFilePath = "";
        private string osuReplaysPath = "";

        public Form1()
        {
            settings = new MainControlFrame();
            if (File.Exists(settings.pathSettings))
                settings.LoadSettings();
                            
            InitializeComponent();

            if (!string.IsNullOrEmpty(settings.pathSongs) && !string.IsNullOrEmpty(settings.pathOsuDB))
                parseOsuDB(settings.pathOsuDB, settings.pathSongs);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog replayFileDialogue = new OpenFileDialog();
            replayFileDialogue.Multiselect = true;
            replayFileDialogue.Filter = "osu replays (*.osr)|*.osr;";

            if (!string.IsNullOrEmpty(settings.pathReplays))
                replayFileDialogue.InitialDirectory = settings.pathReplays;

            if (replayFileDialogue.ShowDialog() == DialogResult.OK)
            {
                replays = new List<Replay>();
                bool clean = true;
                string log = "";
                foreach(var replayFile in replayFileDialogue.FileNames)
                {
                    try
                    {
                        replays.Add(new Replay(replayFile, true));
                        //log += replayFile + "\n";
                    }
                    catch (Exception exception)
                    {
                        if (clean)
                            log = "Some of the replays were not read:\n";
                        log += "Failed to process " + replayFile + " (" + exception.Message + ")\n";
                        clean = false;
                    }
                }
                if (clean)
                    log = "All replays were read successfully!";
                MessageBox.Show(log);
            }

            if (replayFileDialogue.FileNames.Length > 0)
                this.osuReplaysPath = replayFileDialogue.FileNames[0];

        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void chooseReplaysFolderButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog replayFolderDialogue = new FolderBrowserDialog();

            if (replayFolderDialogue.ShowDialog() == DialogResult.OK)
            {
                settings.pathReplays = replayFolderDialogue.SelectedPath;
            }
        }

        private void chooseMapButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog osuFileDialogue = new OpenFileDialog();
            osuFileDialogue.Multiselect = false;
            osuFileDialogue.Filter = "osu beatmaps (*.osu)|*.osu;";

            if (osuFileDialogue.ShowDialog() == DialogResult.OK)
            {
                osuFilePath = osuFileDialogue.FileName;
            }
        }

        private Beatmap findBeatmapInOsuDB(Replay replay)
        {
            Beatmap map = null;
            if (!string.IsNullOrEmpty(osuFilePath))
                map = new Beatmap(osuFilePath);

            if (map != null && replay.MapHash.Equals(map.BeatmapHash))
                return map;
            else
            {
                if (mapsDB.ContainsKey(replay.MapHash))
                {
                    return new Beatmap(mapsDB[replay.MapHash]);
                }
                else
                    return null;
            }
        }


        private void analyzeReplayButton_Click(object sender, EventArgs e)
        {
            Beatmap map = null;
            string res = "";
            currentTaskLabel.Text = "Analyzing replays...";
            currentTaskLabel.Refresh();
            for (int i = 0; i < replays.Count; ++i)
            {
                progressBar1.Value = (int)((100.0 / replays.Count) * i);
                progressBar1.Refresh();
                Replay replay = replays[i];

                map = findBeatmapInOsuDB(replay);

                if (map != null)
                    res += osuDodgyMomentsFinder.Program.ReplayAnalyzing(map, replay);
                else
                    res += replay.Filename + " does not correspond to any known map\n";
            }
            progressBar1.Value = 100;
            progressBar1.Refresh();
            currentTaskLabel.Text = "Finished analyzing replays.";
            currentTaskLabel.Refresh();
            if (replays.Count == 0)
            {
                MessageBox.Show("Error! No replays selected.");
            }
            else
            {
                saveResult(res);
            }
        }

        private void parseOsuDB(string osuDbPath, string songsFolder)
        {
            var osuDbP = new OsuDbAPI.OsuDbFile(osuDbPath);
            currentTaskLabel.Text = "Processing beatmaps from osuDB...";
            currentTaskLabel.Refresh();
            var dict = new Dictionary<string, string>();
            for (int i = 0; i < osuDbP.Beatmaps.Count; ++i)
            {
                progressBar1.Value = (int)((100.0 / osuDbP.Beatmaps.Count) * i);
                OsuDbAPI.Beatmap dbBeatmap = osuDbP.Beatmaps[i];
                string beatmapPath = songsFolder + "\\" + dbBeatmap.FolderName + "\\" + dbBeatmap.OsuFile;
                dict.Add(dbBeatmap.Hash, beatmapPath);
            }
            progressBar1.Value = 100;
            mapsDB = dict;
            currentTaskLabel.Text = "Finished processing beatmaps from osuDB.";
            currentTaskLabel.Refresh();
        }

        private void saveResult(string res)
        {
            if (saveReportToFileCheckBox.Checked)
            {
                SaveFileDialog saveReportDialogue = new SaveFileDialog();
                saveReportDialogue.FileName = "Report " + DateTime.Now.ToString("MMMM dd, yyyy H-mm-ss") + ".txt";
                saveReportDialogue.Filter = "All Files|*.*;";

                if (saveReportDialogue.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveReportDialogue.FileName, res);
                }
            }
            if (alertOutputCheckBox.Checked)
            {
                reportDialog dialog = new reportDialog();
                dialog.textBox1.Text = res;
                //dialog.Size = new Size(500,500);
                dialog.textBox1.ScrollBars = ScrollBars.Vertical;
                dialog.Show();
            }
        }

        private void openOsuDBButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog osuFileDialogue = new OpenFileDialog();
            osuFileDialogue.Filter = "osudb (*.db)|*.db;";
            osuFileDialogue.Multiselect = false;

            if (osuFileDialogue.ShowDialog() == DialogResult.OK)
            {
                settings.pathOsuDB = osuFileDialogue.FileName;
                MessageBox.Show("Now select Songs folder.");

                FolderBrowserDialog songsFolderDialogue = new FolderBrowserDialog();
                songsFolderDialogue.SelectedPath = Path.GetDirectoryName(osuFileDialogue.FileName);

                if (songsFolderDialogue.ShowDialog() == DialogResult.OK)
                {
                    settings.pathSongs = songsFolderDialogue.SelectedPath;
                    parseOsuDB(osuFileDialogue.FileName, songsFolderDialogue.SelectedPath);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            settings.saveSettings();
        }

        private void analyzeReplaysButton_Click(object sender, EventArgs e)
        {
            string res = "";
            var pairs = new Dictionary<Beatmap, Replay>();

            currentTaskLabel.Text = "Analyzing replays in folder...";
            currentTaskLabel.Refresh();

            try
            {
                DirectoryInfo directory = new DirectoryInfo(settings.pathReplays);
                FileInfo[] files = directory.GetFiles();

                var replaysFiles = new List<string>();
                foreach (FileInfo file in files)
                {
                    if (file.Extension == ".osr")
                    {
                        replaysFiles.Add(file.FullName);
                    }
                }

                progressBar1.Value = 0;
                var replays = new List<Replay>();
                for (int i = 0; i < replaysFiles.Count; ++i)
                {
                    string path = replaysFiles[i];

                    progressBar1.Value = (int)((100.0 / replaysFiles.Count) * i);
                    Replay replay = new Replay(path, true);
                    Beatmap map = findBeatmapInOsuDB(replay);
                    res += osuDodgyMomentsFinder.Program.ReplayAnalyzing(map, replay).ToString();

                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
            finally
            {
                progressBar1.Value = 100;
                currentTaskLabel.Text = "Finished analyzing replays in folder...";
                currentTaskLabel.Refresh();
                saveResult(res);
            }
        }

        private void testButton_Click(object sender, EventArgs e)
        {
            /*Beatmap map = null;
            string res = "";
            for (int i = 0; i < replayFilePath.Length; ++i)
            {
                progressBar1.Value = (int)((100.0 / replayFilePath.Length) * i);
                Replay replay = new Replay(replayFilePath[i], true);

                map = findBeatmapInOsuDB(replay);

                if (map != null)
                    res += new ReplayAnalyzer(map, replay).outputDistances();
                else
                    res += replayFilePath[i] + " does not correspond to the selected map\n";
            }
            progressBar1.Value = 100;
            saveResult(res);*/
        }

        private string AvsBReplaysCompare(List<Replay> replaysA, List<Replay> replaysB)
        {
            string res = "";
            currentTaskLabel.Text = "Comparing replays...";
            currentTaskLabel.Refresh();
            progressBar1.Value = 0;
            int count = replaysA.Count * replaysB.Count;
            for (int i = 0; i < replaysA.Count; ++i)
            {
                for (int j = 0; j < replaysB.Count; ++j)
                {
                    ReplayComparator comparator = new ReplayComparator(replaysA[i], replaysB[j]);
                    double diff = comparator.compareReplays();
                    if (diff < 10)
                        res += "IDENTICAL REPLAY PAIR: ";
                    res += replaysA[i].Filename + " vs. " + replaysB[j].Filename + " = " + diff + "\n";
                    progressBar1.Value = (int)((100.0 / count) * i);
                }
            }
            progressBar1.Value = 100;
            currentTaskLabel.Text = "Finished comparing replays.";
            currentTaskLabel.Refresh();

            return res;
        }

        private string AllVSReplaysCompare(List<Replay> replays)
        {
            string res = "";
            currentTaskLabel.Text = "Comparing replays...";
            currentTaskLabel.Refresh();
            progressBar1.Value = 0;
            int count = replays.Count * (replays.Count - 1) / 2;
            for (int i = 0; i < replays.Count; ++i)
            {
                for (int j = i + 1; j < replays.Count; ++j)
                {
                    ReplayComparator comparator = new ReplayComparator(replays[i], replays[j]);
                    double diff = comparator.compareReplays();
                    if (diff < 10)
                        res += "IDENTICAL REPLAY PAIR: ";
                    res += replays[i].Filename + " vs. " + replays[j].Filename + " = " + diff + "\n";
                    progressBar1.Value = (int)((100.0 / count) * i);
                }
            }
            progressBar1.Value = 100;
            currentTaskLabel.Text = "Finished comparing replays.";
            currentTaskLabel.Refresh();

            return res;
        }

        private void compareReplaysButton_Click(object sender, EventArgs e)
        {
            saveResult(AllVSReplaysCompare(replays));
        }

        private void compareAllReplaysButton_Click(object sender, EventArgs e)
        {
            string res = "";

            var pairs = new Dictionary<Beatmap, Replay>();

            DirectoryInfo directory = new DirectoryInfo(settings.pathReplays);
            FileInfo[] files = directory.GetFiles();

            var replaysFiles = new List<string>();
            foreach (FileInfo file in files)
            {
                if (file.Extension == ".osr")
                {
                    replaysFiles.Add(file.FullName);
                }
            }

            currentTaskLabel.Text = "Comparing replays in folder...";
            currentTaskLabel.Refresh();
            progressBar1.Value = 0;
            var replays = new List<Replay>();
            for (int i = 0; i < replaysFiles.Count; ++i)
            {
                string path = replaysFiles[i];
                try
                {
                    progressBar1.Value = (int)((100.0 / replaysFiles.Count) * i);
                    Replay replay = new Replay(path, true);
                    replays.Add(replay);    
                }
                catch (Exception exception)
                {
                    res += "Failed to process " + path + " (" + exception.Message + ")\n";
                }
            }
            progressBar1.Value = 100;
            currentTaskLabel.Text = "Finished comparing replays in folder...";
            currentTaskLabel.Refresh();

            saveResult(res + AllVSReplaysCompare(replays));
        }

        private void compareOneVSFolderButton_Click(object sender, EventArgs e)
        {
            string res = "";

            var pairs = new Dictionary<Beatmap, Replay>();

            DirectoryInfo directory = new DirectoryInfo(settings.pathReplays);
            FileInfo[] files = directory.GetFiles();

            var replaysFiles = new List<string>();
            foreach (FileInfo file in files)
            {
                if (file.Extension == ".osr")
                {
                    replaysFiles.Add(file.FullName);
                }
            }

            progressBar1.Value = 0;
            var replays = new List<Replay>();
            for (int i = 0; i < replaysFiles.Count; ++i)
            {
                string path = replaysFiles[i];
                try
                {
                    progressBar1.Value = (int)((100.0 / replaysFiles.Count) * i);
                    Replay replay = new Replay(path, true);
                    replays.Add(replay);
                }
                catch (Exception exception)
                {
                    res += "Failed to process " + path + " (" + exception.Message + ")\n";
                }
            }

            saveResult(res + AvsBReplaysCompare(this.replays, replays));
        }

        private void chooseReplaysFolderButton_MouseHover(object sender, EventArgs e)
        {
            saveLabel();
            currentTaskLabel.Text = this.settings.pathReplays.Length > 0 ? this.settings.pathReplays : "Not selected";
        }

        private void chooseReplayButton_MouseHover(object sender, EventArgs e)
        {
            saveLabel();
            currentTaskLabel.Text = this.osuReplaysPath.Length > 0 ? this.osuReplaysPath : "Not selected";
        }

        private void chooseMapButton_MouseHover(object sender, EventArgs e)
        {
            saveLabel();
            currentTaskLabel.Text = this.osuFilePath.Length > 0 ? this.osuFilePath : "Not selected";
        }

        private void openOsuDBButton_MouseHover(object sender, EventArgs e)
        {
            saveLabel();
            currentTaskLabel.Text = this.settings.pathOsuDB.Length > 0 ? this.settings.pathOsuDB : "Not selected";
        }

        private void saveLabel()
        {
            cacheLabel = true;
            cachedLabelText = currentTaskLabel.Text;
        }

        private void restoreLabel()
        {
            if (cacheLabel)
                currentTaskLabel.Text = cachedLabelText;
        }

        private void chooseReplayButton_MouseLeave(object sender, EventArgs e)
        {
            restoreLabel();
        }

        private void chooseReplaysFolderButton_MouseLeave(object sender, EventArgs e)
        {
            restoreLabel();
        }

        private void chooseMapButton_MouseLeave(object sender, EventArgs e)
        {
            restoreLabel();
        }

        private void openOsuDBButton_MouseLeave(object sender, EventArgs e)
        {
            restoreLabel();
        }
    }
}
