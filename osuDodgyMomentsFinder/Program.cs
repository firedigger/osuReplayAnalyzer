using System;
using System.IO;
using BMAPI.v1;
using ReplayAPI;
using BMAPI.v1.HitObjects;

namespace osuDodgyMomentsFinder
{
    class Program
    {

        static void MainInfo(ReplayAnalyzer analyzer)
        {
            Console.WriteLine("Unstable rate = " + analyzer.unstableRate());

            Console.WriteLine("The best CS value = " + analyzer.bestCSValue());

            Console.WriteLine("The best pixel perfect hit " + analyzer.findBestPixelHit());

            var pixelPerfectHits = analyzer.findSortedPixelPerfectHits(10);

            Console.WriteLine("Pixel perfect hits: " + pixelPerfectHits.Count);
            foreach (var hit in pixelPerfectHits)
            {
                Console.WriteLine(hit.Key + " at " + hit.Value + "ms");
            }


            //Console.WriteLine(analyzer.hits.Count);

            /*int i = 0;
            foreach(var hit in analyzer.hits)
            {
                ++i;
                if (i > 10)
                    break;
                Console.WriteLine(Utils.printInfo(hit.Value, hit.Key));
            }*/

            //beatmap.Save("new.osu");

        }


        static void Main(string[] args)
        {
            Beatmap beatmap = new Beatmap(args[0]);
            Replay replay = new Replay(args[1], true);

            ReplayAnalyzer analyzer = new ReplayAnalyzer(beatmap, replay);

            MainInfo(analyzer);

            var overAims = analyzer.findOverAimHits();
            Console.WriteLine("Over:Aim hits " + overAims.Count);
            foreach (var hit in overAims)
            {
                Console.WriteLine("at " + hit + "ms");
            }



            //MainInfo(analyzer);

            Console.ReadKey();
        }
        
    }
}
