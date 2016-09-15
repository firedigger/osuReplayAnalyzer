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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI
{
    public partial class Form1 : Form
    {
        string[] replayFilePath;
        string osuFilePath;
        Dictionary<string, string> mapsDB;

        MainControlFrame settings;

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
                //settings.pathReplays = replayFileDialogue.FileName.;
                replayFilePath = replayFileDialogue.FileNames;
            }

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
            for (int i = 0; i < replayFilePath.Length; ++i)
            {
                progressBar1.Value = (int)((100.0 / replayFilePath.Length) * i);
                Replay replay = new Replay(replayFilePath[i], true);

                map = findBeatmapInOsuDB(replay);

                if (map != null)
                    res += osuDodgyMomentsFinder.Program.ReplayAnalyzing(map, replay);
                else
                    res += replayFilePath[i] + " does not correspond to the selected map\n";
            }
            progressBar1.Value = 100;
            saveResult(res);
        }

        private void parseOsuDB(string osuDbPath, string songsFolder)
        {
            var osuDbP = new OsuDbAPI.OsuDbFile(osuDbPath);
            currentTaskLabel.Text = "Processing beatmaps from osuDB...";
            var dict = new Dictionary<string, string>();
            for (int i = 0; i < osuDbP.Beatmaps.Count; ++i)
            {
                progressBar1.Value = (int)((100.0 / osuDbP.Beatmaps.Count) * i);
                OsuDbAPI.Beatmap dbBeatmap = osuDbP.Beatmaps[i];
                string beatmapPath = songsFolder + "\\" + dbBeatmap.FolderName + "\\" + dbBeatmap.OsuFile;
                //Beatmap map = new Beatmap(beatmapPath);
                dict.Add(dbBeatmap.Hash, beatmapPath);
            }
            progressBar1.Value = 100;
            mapsDB = dict;
            currentTaskLabel.Text = "Finished processing beatmaps from osuDB...";
        }

        private void saveResult(string res)
        {
            if (saveReportToFileCheckBox.Checked)
            {
                SaveFileDialog saveReportDialogue = new SaveFileDialog();
                saveReportDialogue.FileName = "report " + DateTime.Now.ToString("MMMM dd, yyyy H-mm-ss") + ".txt";
                saveReportDialogue.Filter = "All Files|*.*;";

                if (saveReportDialogue.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveReportDialogue.FileName, res);
                }
            }
            if (alertOutputCheckBox.Checked)
                MessageBox.Show(res);
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
            for(int i = 0; i < replaysFiles.Count; ++i)
            {
                string path = replaysFiles[i];
                try
                {
                    progressBar1.Value = (int)((100.0 / replaysFiles.Count) * i);
                    Replay replay = new Replay(path, true);
                    Beatmap map = findBeatmapInOsuDB(replay);
                    res += osuDodgyMomentsFinder.Program.ReplayAnalyzing(map, replay);
                }
                catch (Exception exception)
                {
                    res += "Failed to process " + path +  " (" + exception.Message + ")\n";
                }
            }
            progressBar1.Value = 100;
            saveResult(res);
        }
    }
}
