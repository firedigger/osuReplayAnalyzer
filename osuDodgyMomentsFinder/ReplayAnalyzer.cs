using BMAPI.v1;
using BMAPI.v1.HitObjects;
using ReplayAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osuDodgyMomentsFinder
{

    /* This class is a list of pair of a clickable object and a replay frame hit
     * Initializing the class is a task of associating every keypress with an object hit
     * After that all the procedural checks on suspicious moment become possible
     */
    class ReplayAnalyzer
    {
        //The beatmap
        private Beatmap beatmap;

        //The replay
        private Replay replay;


        //Circle radius
        private double circleRadius;

        //hit time window
        private double hitTimeWindow;


        //The list of pair of a <hit, object hit>
        public List<KeyValuePair<CircleObject, ReplayFrame>> hits { get; private set; }


        private void applyHardrock()
        {
            replay.AxisFlip = true;
            beatmap.CircleSize = beatmap.CircleSize * 1.3f;
            this.replay.ReplayFrames.ForEach((t) => t.Y = 384 - t.Y);
            Console.WriteLine(beatmap.CircleSize);
        }


        private void associateHits()
        {
            hits = new List<KeyValuePair<CircleObject, ReplayFrame>>();
            //int currentTime = 0;
            int keyIndex = 0;
            bool pressReady = true;
            Keys lastKey = Keys.None;

            if ((replay.Mods & Mods.HardRock) > 0)
            {
                applyHardrock();
            }


            for (int i = 0; i < beatmap.HitObjects.Count; ++i)
            {
                CircleObject note = beatmap.HitObjects[i];
                bool flag = false;

                if (i == 209)
                {
                    int u = 0;
                }

                if ((note.Type & HitObjectType.Spinner) > 0)
                    continue;

                for (int j = keyIndex; j < replay.ReplayFrames.Count; ++j)
                {
                    ReplayFrame frame = replay.ReplayFrames[j];

                    if (((frame.Keys & lastKey) ^ frame.Keys) > 0)
                        pressReady = true;

                    if (frame.Keys != Keys.None && Math.Abs(frame.Time - note.StartTime) <= hitTimeWindow && note.ContainsPoint(new BMAPI.Point2(frame.X, frame.Y)) && pressReady)
                    {
                        flag = true;
                        pressReady = false;
                        lastKey = frame.Keys;
                        hits.Add(new KeyValuePair<CircleObject, ReplayFrame>(note, frame));
                        keyIndex = j + 1;
                        break;
                    }

                    if (frame.Keys != Keys.None)
                        pressReady = false;

                    lastKey = frame.Keys;

                }

                if (!flag)
                {
                    Console.WriteLine("Panic! Didn't find the hit for " + i + " " + keyIndex);
                }


            }

            /*if (note.Type == HitObjectType.Slider)
            {
                SliderObject slider = (SliderObject)note;

                double startTime = note.StartTime;
                double endTime = slider.SegmentEndTime;
            }*/
            //}
        }

        private double calcTimeWindow(double OD)
        {
            return -12 * OD + 259.5;
        }

        public ReplayAnalyzer(Beatmap beatmap, Replay replay)
        {
            this.beatmap = beatmap;
            this.replay = replay;

            this.circleRadius = beatmap.HitObjects[0].Radius;
            this.hitTimeWindow = calcTimeWindow(beatmap.OverallDifficulty);

            associateHits();
        }

        public double findBestPixelHit()
        {
            return this.hits.Max((pair) => Utils.pixelPerfectHitFactor(pair.Value, pair.Key));
        }

        public List<double> findPixelPerfectHits(double threshold)
        {
            List<double> result = new List<double>();

            foreach (var pair in this.hits)
            {
                double factor = Utils.pixelPerfectHitFactor(pair.Value, pair.Key);

                if (factor >= threshold)
                {
                    result.Add(pair.Key.StartTime);
                }
            }


            return result;
        }

        public List<KeyValuePair<double, double>> findSortedPixelPerfectHits(int maxSize)
        {
            List<KeyValuePair<double, double>> result = new List<KeyValuePair<double, double>>();

            foreach (var pair in this.hits)
            {
                double factor = Utils.pixelPerfectHitFactor(pair.Value, pair.Key);
                result.Add(new KeyValuePair<double, double>(factor, pair.Key.StartTime));
            }
            result.Sort((a, b) => b.Key.CompareTo(a.Key));
            

            return result.GetRange(0,maxSize);
        }



        public double unstableRate()
        {

            List<float> values = this.hits.ConvertAll((pair) => pair.Value.Time - pair.Key.StartTime);

            double avg = values.Average();
            return 10 * Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
            /*double mean = 0;
            double mean_sq = 0;

            foreach (var pair in this.hits)
            {
                double diff = Math.Abs(pair.Value.Time - pair.Key.StartTime);
                mean += diff;
                mean_sq += Utils.sqr(diff);
            }

            Console.WriteLine(mean * mean);
            Console.WriteLine(mean_sq);

            return 10 * Math.Sqrt(mean_sq - Utils.sqr(mean));*/
        }

    }
}
