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
        static string createNoHDPath(string path)
        {
            return path.Replace(".osr", ".noHD.osr");
        }


        static void ReplayAnalyzing(string[] args)
        {
            if (args.Length == 0)
                args = UIUtils.getArgsFromUser();

            Beatmap beatmap = new Beatmap(args[0]);
            Replay replay = new Replay(args[1], true);

            if (replay.Mods.HasFlag(Mods.Hidden))
            {
                replay.Mods = replay.Mods ^ Mods.Hidden;
                replay.Save(createNoHDPath(args[1]));
            }

            Console.WriteLine("BEATMAP: " + args[0]);
            Console.WriteLine("REPLAY: " + args[1]);

            ReplayAnalyzer analyzer = new ReplayAnalyzer(beatmap, replay);
            analyzer.PrintMainInfo();
            analyzer.PrintPixelPerfect();
            analyzer.PrintOveraims();

            Console.ReadKey();
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

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Welcome the firedigger's replay analyzer. Use one of 3 options");
                Console.WriteLine("-i for getting info about a certain replay");
                Console.WriteLine("-c for comparing all the replays in the current folder against each other");
                Console.WriteLine("-cr for comparing the replays from command line args");
                Console.ReadKey();
                return;
            }
            if (args[0] == "-i")
                ReplayAnalyzing(args.SubArray(1));
            if (args[0] == "-c")
                ReplayComparison(args.SubArray(1));
            if (args[0] == "-cr")
                CompareReplays(args.SubArray(1));
        }
        
    }
}
