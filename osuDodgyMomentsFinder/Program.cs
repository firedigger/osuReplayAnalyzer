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
            Beatmap beatmap = new Beatmap(args[0]);
            Replay replay = new Replay(args[1], true);

            //Console.WriteLine(1);

            ReplayAnalyzer analyzer = new ReplayAnalyzer(beatmap, replay);

            Console.WriteLine(analyzer.unstableRate());

            /*var pixelPerfectHits = analyzer.findPixelPerfectHits(1);

            Console.WriteLine(pixelPerfectHits.Count);
            foreach (var hit in pixelPerfectHits)
            {
                Console.WriteLine(hit);
            }*/

            //Console.WriteLine(analyzer.hits.Count);

            /*int i = 0;
            foreach(var hit in analyzer.hits)
            {
                ++i;
                if (i > 10)
                    break;
                Console.WriteLine(Utils.printInfo(hit.Value, hit.Key));
            }*/

            Console.ReadKey();

        }
        
    }
}
