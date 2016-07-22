using System;
using System.IO;
using BMAPI.v1;
using ReplayAPI;
using BMAPI.v1.HitObjects;

namespace osuDodgyMomentsFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
                args = UIUtils.getArgsFromUser();

            Beatmap beatmap = new Beatmap(args[0]);
            Replay replay = new Replay(args[1], true);

            Console.WriteLine("BEATMAP: " + args[0]);
            Console.WriteLine("REPLAY: " + args[1]);

            ReplayAnalyzer analyzer = new ReplayAnalyzer(beatmap, replay);
            analyzer.PrintMainInfo();
            analyzer.PrintPixelPerfect();
            analyzer.PrintOveraims();
            
            Console.ReadKey();
        }
        
    }
}
