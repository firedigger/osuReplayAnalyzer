using System;
using System.IO;
using BMAPI.v1;
using ReplayAPI;
using System.Collections.Generic;
using System.Text;

namespace osuDodgyMomentsFinder
{
    public class Program
    {
        private static MainControlFrame settings = new MainControlFrame();

        public static Dictionary<string, Beatmap> processOsuDB(OsuDbAPI.OsuDbFile osuDB, string songsFolder)
        {
            Dictionary<string, Beatmap> dict = new Dictionary<string, Beatmap>();
            foreach(OsuDbAPI.Beatmap dbBeatmap in osuDB.Beatmaps)
            {
                string beatmapPath = songsFolder + dbBeatmap.FolderName + "\\" + dbBeatmap.OsuFile;
                Beatmap map = new Beatmap(beatmapPath);
                dict.Add(map.BeatmapHash, map);
            }

            return dict;
        }


        public static List<KeyValuePair<Beatmap, Replay>> AssociateMapsReplays(bool osuDB)
        {
            DirectoryInfo directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            FileInfo[] files = directory.GetFiles();

            var replaysFiles = new List<string>();
            var mapsFiles = new List<string>();
            foreach(FileInfo file in files)
            {
                if(file.Extension == ".osr")
                {
                    replaysFiles.Add(file.Name);
                }
                if(file.Extension == ".osu")
                {
                    mapsFiles.Add(file.Name);
                }
            }
            var replays = replaysFiles.ConvertAll((path) => new Replay(path, true));

            var osuDBmaps = settings.osuDbP.Beatmaps;

            var maps = mapsFiles.ConvertAll((path) => new Beatmap(path));

            var result = new List<KeyValuePair<Beatmap, Replay>>();
            var dict = new Dictionary<string, Beatmap>();

            //Add all the beatmaps from the current folder
            foreach(var map in maps)
            {
                dict.Add(map.BeatmapHash, map);
            }

            //Add all the beatmaps from the osu DB
            if(osuDB)
            {
                foreach(OsuDbAPI.Beatmap dbBeatmap in osuDBmaps)
                {
                    string beatmapPath = settings.pathSongs + dbBeatmap.FolderName + "\\" + dbBeatmap.OsuFile;
                    Beatmap map = new Beatmap(beatmapPath);
                    if(!dict.ContainsKey(map.BeatmapHash))
                        dict.Add(map.BeatmapHash, map);
                }

                foreach(var replay in replays)
                {

                    if(dict.ContainsKey(replay.MapHash))
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

        public static StringBuilder ReplayDataCollecting(Beatmap beatmap, Replay replay)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("BEATMAP: " + beatmap.ToString());
            sb.AppendLine("REPLAY: " + replay.ToString());

            ReplayAnalyzer analyzer = new ReplayAnalyzer(beatmap, replay);
            sb.AppendLine("Pixel perfect factors," + analyzer.PixelPerfectRawData().ToString());
            sb.AppendLine("Time frame differences," + analyzer.TimeFramesRawData().ToString());
            sb.AppendLine("Travelled distance differences," + analyzer.TravelledDistanceDiffRawData().ToString());
            sb.AppendLine("Speed," + analyzer.SpeedRawData().ToString());
            sb.AppendLine("Acceleration," + analyzer.AccelerationRawData().ToString());
            sb.AppendLine("Hit errors," + analyzer.HitErrorRawData().ToString());
            sb.AppendLine("Press key time lengths," + analyzer.PressKeyIntevalsRawData().ToString());

            return sb;
        }

        public static StringBuilder ReplayAnalyzing(Beatmap beatmap, Replay replay)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("BEATMAP: " + beatmap.ToString());
            sb.AppendLine("REPLAY: " + replay.ToString());
            sb.AppendLine();

            ReplayAnalyzer analyzer = new ReplayAnalyzer(beatmap, replay);
            sb.AppendLine(analyzer.MainInfo().ToString());
            sb.AppendLine();
            sb.AppendLine(analyzer.PixelPerfectInfo().ToString());
            sb.AppendLine();
            sb.AppendLine(analyzer.OveraimsInfo().ToString());
            sb.AppendLine();
            sb.AppendLine(analyzer.TeleportsInfo().ToString());
            sb.AppendLine();
            sb.AppendLine(analyzer.SingletapsInfo().ToString());
            sb.AppendLine();
            sb.AppendLine(analyzer.ExtraHitsInfo().ToString());

            return sb;
        }


        public static void ReplayComparison(string[] args)
        {
            DirectoryInfo directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            FileInfo[] files = directory.GetFiles();

            Console.Clear();
            var replaysFiles = new List<string>();
            foreach(FileInfo file in files)
            {
                if(file.Extension == ".osr")
                {
                    replaysFiles.Add(file.Name);
                }
            }

            CompareReplays(replaysFiles, Double.MaxValue);

            Console.ReadKey();
        }

        public static void CompareReplays(List<string> replaysFiles, double threshold)
        {
            var replays = replaysFiles.ConvertAll((path) => new Replay(path, true));

            for(int i = 0; i < replays.Count; ++i)
            {
                for(int j = i + 1; j < replays.Count; ++j)
                {
                    ReplayComparator comparator = new ReplayComparator(replays[i], replays[j]);
                    double diff = comparator.compareReplays();
                    if(diff <= threshold)
                        Console.WriteLine(replaysFiles[i] + " vs. " + replaysFiles[j] + " = " + diff);
                }
            }
        }

        public static void CompareReplays(string[] args)
        {
            var list = new List<string>();
            for(int i = 0; i < args.Length; ++i)
                list.Add(args[i]);
            CompareReplays(list, Double.MaxValue);
            Console.ReadKey();
        }

        public static void ReplayAnalyzingAll(bool oneFile = true)
        {
            var pairs = AssociateMapsReplays(true);

            string res = "";
            foreach(var pair in pairs)
            {
                string result = ReplayAnalyzing(pair.Key, pair.Value).ToString();
                if(oneFile)
                    res += result;
                else
                    File.WriteAllText(pair.Value.ToString() + ".txt", result);
            }
            if(oneFile)
                File.WriteAllText("Report " + DateTime.Now.ToString("MMMM dd, yyyy H-mm-ss") + ".txt", res);

        }

        public static string ReplayAnalyzing(Replay replay)
        {
            var maps = settings.osuDbP.Beatmaps;

            string beatmapPath = "";
            foreach(OsuDbAPI.Beatmap dbBeatmap in maps)
            {
                if(dbBeatmap.Hash == replay.MapHash)
                {
                    beatmapPath = settings.pathSongs + dbBeatmap.FolderName + "\\" + dbBeatmap.OsuFile;
                    break;
                }
            }

            Beatmap beatmap = new Beatmap(beatmapPath);

            return ReplayAnalyzing(beatmap, replay).ToString();
        }


        public static void Main(string[] args)
        {
            if(File.Exists(settings.pathSettings))
            {
                Console.WriteLine(settings.pathSettings + " found. Parsing settings.");
                settings.LoadSettings();
            }
            else
            {
                string[] settings_help = new string[]
                {
                    @"# Lines starting with a # are ignored",
                    @"",
                    @"# Path to osu!.db",
                    @"pathOsuDB=C:\\osu!\\osu!.db",
                    @"",
                    @"# Path to your osu! song folder",
                    @"pathSongs=C:\\osu!\\songs\\",
                    @"",
                    @"# Path to a replay folder",
                    @"pathReplays=C:\\osu!\\replays\\"
                };
                File.WriteAllLines(settings.pathSettings, settings_help);
                Console.WriteLine("A settings file has been created for you to link to your songs folder.");
                return;
            }


            if(args.Length == 0)
            {
                Console.WriteLine("Welcome the firedigger's replay analyzer. Use one of 3 options");
                Console.WriteLine("-i (path to replay) for getting info about a certain replay");
                Console.WriteLine("-ia for getting info about all replays in the current folder");
                Console.WriteLine("-c for comparing all the replays in the current folder against each other");
                Console.WriteLine("-cr [paths to replays] for comparing the replays from command line args");
                Console.ReadKey();
                return;
            }
            if(args[0] == "-ia")
            {
                ReplayAnalyzingAll(true);
            }
            if(args[0] == "-i")
            {
                Console.WriteLine(ReplayAnalyzing(new Replay(args[1], true)));
            }
            if(args[0] == "-c")
                ReplayComparison(args.SubArray(1));
            if(args[0] == "-cr")
                CompareReplays(args.SubArray(1));
            if(args[0] == "-s")
            {
                if(args.Length == 1)
                    args = UIUtils.getArgsFromUser();
                CursorSpeed(new Beatmap(args[1]), new Replay(args[2], true));
            }

        }

        public static void CursorSpeed(Beatmap beatmap, Replay replay)
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
