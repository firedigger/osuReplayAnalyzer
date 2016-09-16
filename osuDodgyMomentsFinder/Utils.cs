using BMAPI.v1.HitObjects;
using ReplayAPI;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace osuDodgyMomentsFinder
{
    static class Utils
    {
        public static double derivative(double a, double b, double c, double h)
        {
            return (-a + 2 * b - c) / h / h;
        }

        public static void outputValues<T>(List<T> values, string fileName)
        {
            string res = "";
            foreach (var value in values)
            {
                res += value + ",";
            }
            if (res.Length > 0)
                res.Remove(res.Length - 1);

            File.WriteAllText(fileName, res);
        }

        //Assumes sorted
        public static double median(List<double> values)
        {
            if(values.Count % 2 == 0)
                return (values[values.Count / 2 - 1] + values[values.Count / 2]) / 2;
            else
                return values[values.Count / 2];
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static T[] SubArray<T>(this T[] data, int index)
        {
            int length = data.Length - index;
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static int sign(int value)
        {
            return value > 0 ? 1 : 0;
        }


        public static double sqr(double x)
        {
            return x * x;
        }

        public static double dist(ReplayFrame frame1, ReplayFrame frame2)
        {
            return dist(frame1.X, frame1.Y, frame2.X, frame2.Y);
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

        public static double variance(List<double> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
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
                if(file.Extension == ".osu")
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
            foreach(FileInfo file in files)
            {
                if(file.Extension == ".osr")
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
