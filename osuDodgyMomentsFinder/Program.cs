using System;
using System.IO;
using BMAPI.v1;
using ReplayAPI;
using BMAPI.v1.HitObjects;
using System.Collections.Generic;

namespace osuDodgyMomentsFinder
{
    class Program
    {
        static List<KeyValuePair<Beatmap, Replay>> AssociateMapsReplays(bool osuDB)
        {
            DirectoryInfo directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            FileInfo[] files = directory.GetFiles();

            var replaysFiles = new List<string>();
            var mapsFiles = new List<string>();
            foreach (FileInfo file in files)
            {
                if (file.Extension == ".osr")
                {
                    replaysFiles.Add(file.Name);
                }
                if (file.Extension == ".osu")
                {
                    mapsFiles.Add(file.Name);
                }
            }
            var replays = replaysFiles.ConvertAll((path) => new Replay(path, true));

            var osuDBmaps = MainControlFrame.Instance.osuDbP.Beatmaps;

            var maps = mapsFiles.ConvertAll((path) => new Beatmap(path));

            var result = new List<KeyValuePair<Beatmap, Replay>>();
            var dict = new Dictionary<string, Beatmap>();

            //Add all the beatmaps from the current folder
            foreach(var map in maps)
            {
                dict.Add(map.BeatmapHash, map);
            }

            //Add all the beatmaps from the osu DB
            if (osuDB)
            {
                foreach (OsuDbAPI.Beatmap dbBeatmap in osuDBmaps)
                {
                    string beatmapPath = MainControlFrame.Instance.pathSongs + dbBeatmap.FolderName + "\\" + dbBeatmap.OsuFile;
                    Beatmap map = new Beatmap(beatmapPath);
                    if (!dict.ContainsKey(map.BeatmapHash))
                        dict.Add(map.BeatmapHash, map);
                }

                foreach (var replay in replays)
                {

                    if (dict.ContainsKey(replay.MapHash))
                    {
                        result.Add(new KeyValuePair<Beatmap, Replay>(dict[replay.MapHash], replay));
                    }
                    else
                    {
                        Console.WriteLine("WARNING! A corresponding map wasn't found for " + replay.ToString());
                    }
                }
            }

            return result;
        }


        static string ReplayAnalyzing(Beatmap beatmap, Replay replay)
        {
            string res = "";

            res += ("BEATMAP: " + beatmap.ToString() + "\n");
            res += ("REPLAY: " + replay.ToString() + "\n");

            ReplayAnalyzer analyzer = new ReplayAnalyzer(beatmap, replay);
            res += analyzer.MainInfo() + "\n";
            res += analyzer.PixelPerfectInfo() + "\n";
            res += analyzer.OveraimsInfo() + "\n";

            return res;
        }


        static void ReplayComparison(string[] args)
        {
            DirectoryInfo directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            FileInfo[] files = directory.GetFiles();

            Console.Clear();
            var replaysFiles = new List<string>();
            foreach (FileInfo file in files)
            {
                if (file.Extension == ".osr")
                {
                    replaysFiles.Add(file.Name);
                }
            }

            CompareReplays(replaysFiles, Double.MaxValue);

            Console.ReadKey();
        }

        static void CompareReplays(List<string> replaysFiles, double threshold)
        {
            var replays = replaysFiles.ConvertAll((path) => new Replay(path, true));

            for (int i = 0; i < replays.Count; ++i)
            {
                for (int j = i + 1; j < replays.Count; ++j)
                {
                    ReplayComparator comparator = new ReplayComparator(replays[i], replays[j]);
                    //Console.WriteLine(replaysFiles[i] + " vs. " + replaysFiles[j]);
                    double diff = comparator.compareReplays();
                    if (diff <= threshold)
                        Console.WriteLine(replaysFiles[i] + " vs. " + replaysFiles[j] + " = " + diff);
                }
            }
        }

        static void CompareReplays(string[] args)
        {
            var list = new List<string>();
            for (int i = 0; i < args.Length; ++i)
                list.Add(args[i]);
            CompareReplays(list, Double.MaxValue);
            Console.ReadKey();
        }

        static void ReplayAnalyzingAll(bool oneFile = true)
        {
            var pairs = AssociateMapsReplays(true);

            string res = "";
            foreach (var pair in pairs)
            {
                string result = ReplayAnalyzing(pair.Key, pair.Value);
                if (oneFile)
                    res += result;
                else
                    File.WriteAllText(pair.Value.ToString() + ".osi", result);
            }
            if (oneFile)
                File.WriteAllText("FullAnalysis.osi", res);

        }

        static string ReplayAnalyzing(Replay replay)
        {
            var maps = MainControlFrame.Instance.osuDbP.Beatmaps;

            string beatmapPath = "";
            foreach (OsuDbAPI.Beatmap dbBeatmap in maps)
            {
                if (dbBeatmap.Hash == replay.MapHash)
                {
                    beatmapPath = MainControlFrame.Instance.pathSongs + dbBeatmap.FolderName + "\\" + dbBeatmap.OsuFile;
                    break;
                }
            }

            Beatmap beatmap = new Beatmap(beatmapPath);

            string res = "";

            res += ("BEATMAP: " + beatmap.ToString() + "\n");
            res += ("REPLAY: " + replay.ToString() + "\n");

            ReplayAnalyzer analyzer = new ReplayAnalyzer(beatmap, replay);
            res += analyzer.MainInfo() + "\n";
            res += analyzer.PixelPerfectInfo() + "\n";
            res += analyzer.OveraimsInfo() + "\n";

            return res;
        }


        static void Main(string[] args)
        {
            if (File.Exists(MainControlFrame.Instance.pathSettings))
            {
                MainControlFrame.Instance.LoadSettings();
            }
            else
            {
                string[] settings = new string[]
                {
                    @"# Lines starting with a # are ignored",
                    @"# Do not change the order of the settings",
                    @"",
                    @"# Path to osu!.db",
                    @"C:\\osu!\\osu!.db",
                    @"",
                    @"# Path to your osu! song folder",
                    @"C:\\osu!\\songs\\",
                    @"",
                    @"# Path to a replay folder",
                    @"C:\\osu!\\replays\\"
                };
                File.WriteAllLines(MainControlFrame.Instance.pathSettings, settings);
                Console.WriteLine("A settings file has been created for you to link to your songs folder.");
                return;
            }


            if (args.Length == 0)
            {
                Console.WriteLine("Welcome the firedigger's replay analyzer. Use one of 3 options");
                Console.WriteLine("-i for getting info about a certain replay");
                Console.WriteLine("-c for comparing all the replays in the current folder against each other");
                Console.WriteLine("-cr for comparing the replays from command line args");
                Console.ReadKey();
                return;
            }
            if (args[0] == "-ia")
            {
                ReplayAnalyzingAll(true);
            }
            if (args[0] == "-i")
            {
                //if (args.Length == 1)
                //    args = UIUtils.getArgsFromUser();
                Console.WriteLine(ReplayAnalyzing(new Replay(args[1], true)));
            }
            if (args[0] == "-c")
                ReplayComparison(args.SubArray(1));
            if (args[0] == "-cr")
                CompareReplays(args.SubArray(1));
            if (args[0] == "-s")
            {
                if (args.Length == 1)
                    args = UIUtils.getArgsFromUser();
                CursorSpeed(new Beatmap(args[1]), new Replay(args[2], true));
            }

        }

        static void CursorSpeed(Beatmap beatmap, Replay replay)
        {
            string res = "";

            Console.WriteLine("BEATMAP: " + beatmap.ToString() + "\n");
            Console.WriteLine("REPLAY: " + replay.ToString() + "\n");

            ReplayAnalyzer analyzer = new ReplayAnalyzer(beatmap, replay);
            res += analyzer.outputAcceleration() + "\r\n";
            res += analyzer.outputTime() + "\r\n";

            File.WriteAllText(replay.ToString() + ".accelerations", res);
        }
    }
}
