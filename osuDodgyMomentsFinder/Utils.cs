﻿using BMAPI.v1.HitObjects;
using ReplayAPI;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osuDodgyMomentsFinder
{
    static class Utils
    {
        public static double sqr(double x)
        {
            return x * x;
        }

        public static double dist(float x1, float y1, float x2, float y2)
        {
            return Math.Sqrt(sqr(x1 - x2) + sqr(y1 - y2));
        }

        public static string printInfo(ReplayFrame frame, CircleObject obj)
        {
            return "Hit " + obj.Type.ToString() + " (starting at " + obj.StartTime + ")" + " at " + frame.Time;
        }

        public static double pixelPerfectHitFactor(ReplayFrame frame, CircleObject obj)
        {
            return dist(frame.X, frame.Y, obj.Location.X, obj.Location.Y) / obj.Radius;
        }
    }

    static class UIUtils
    {
        public static string[] getArgsFromUser()
        {
            DirectoryInfo directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            

            // get beatmap
            Console.WriteLine("SONG LIST: ");

            FileInfo[] files = directory.GetFiles();
            List<string> beatmaps = new List<string>();
            int counter = 1;
            foreach(FileInfo file in files)
            {
                if (file.Extension == ".osu")
                {
                    beatmaps.Add(file.Name);
                    Console.WriteLine(counter.ToString() + "\t" + file.Name);
                    ++counter;
                }
            }

            Console.Write("Pick number: ");
            int songID = int.Parse(Console.ReadLine());
            

            // get replay
            Console.Clear();
            Console.WriteLine("PLAY LIST: ");
            List<string> replays = new List<string>();
            counter = 1;
            foreach (FileInfo file in files)
            {
                if (file.Extension == ".osr")
                {
                    replays.Add(file.Name);
                    Console.WriteLine(counter.ToString() + "\t" + file.Name);
                    ++counter;
                }
            }

            Console.Write("Pick number: ");
            int playID = int.Parse(Console.ReadLine());

            Console.Clear();
            return new string[] { beatmaps[songID - 1], replays[playID - 1] };
        }
    }
}
