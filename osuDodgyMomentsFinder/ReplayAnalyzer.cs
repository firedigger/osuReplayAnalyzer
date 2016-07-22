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

        //hit time window
        private double approachTimeWindow;

        //The list of pair of a <hit, object hit>
        public List<KeyValuePair<CircleObject, ReplayFrame>> hits { get; private set; }
        public List<CircleObject> miss { get; private set; }


        private void applyHardrock()
        {
            replay.AxisFlip = true;
            beatmap.CircleSize = beatmap.CircleSize * 1.3f;
            this.replay.ReplayFrames.ForEach((t) => t.Y = 384 - t.Y);
            //Console.WriteLine(beatmap.CircleSize);
        }


        private void associateHits()
        {
            hits = new List<KeyValuePair<CircleObject, ReplayFrame>>();
            miss = new List<CircleObject>();

            int misses = 0;

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

                if (i == 206)
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
                    ++misses;
                    miss.Add(note);
                }

            }
        }


        public List<double> findOverAimHits()
        {
            List<double> result = new List<double>();
            int keyIndex = 0;
            for (int i = 0; i < beatmap.HitObjects.Count; ++i)
            {
                CircleObject note = beatmap.HitObjects[i];
                bool hover = false;

                //searches for init circle object hover
                for (int j = keyIndex; j < replay.ReplayFrames.Count; ++j)
                {
                    ReplayFrame frame = replay.ReplayFrames[j];
                    if (note.ContainsPoint(new BMAPI.Point2(frame.X, frame.Y)) && Math.Abs(frame.Time - note.StartTime) <= approachTimeWindow)
                    {
                        hover = true;
                        keyIndex = j + 1;
                        break;
                    }

                }
                if (hover)
                {
                    //searches for leaving of the object (dehover)
                    for (int j = keyIndex; j < replay.ReplayFrames.Count; ++j)
                    {
                        ReplayFrame frame = replay.ReplayFrames[j];
                        if (!note.ContainsPoint(new BMAPI.Point2(frame.X, frame.Y)))
                        {
                            keyIndex = j + 1;
                            break;
                        }
                    }

                    if (keyIndex >= replay.ReplayFrames.Count)
                        return result;

                    //Check whether the hit happened BEFORE the dehover
                    int noteIndex = -1;
                    for (int l = 0; l < this.hits.Count; ++l)
                        if (hits[l].Key.Equals(note) && hits[l].Value.Time > replay.ReplayFrames[keyIndex].Time)
                        {
                            noteIndex = l;
                            break;
                        }
                    
                    if (noteIndex != -1)
                        result.Add(note.StartTime);

                }
            }
            return result;
        }




        //Recalculate the highest CS value for which the player would still have the same amount of misses
        public double bestCSValue()
        {
            double pixelPerfect = findBestPixelHit();

            double y = pixelPerfect * circleRadius;

            double x = (54.42 - y) / 4.48;

            return x;
        }


        public string outputMisses()
        {
            string res = "";
            this.miss.ForEach((note) => res += "Didn't find the hit for " + note.StartTime);
            return res;
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

            this.approachTimeWindow = 1800 - 120 * beatmap.ApproachRate;

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
            

            return result.GetRange(0, maxSize);
        }


        private double ur = -1;
        public double unstableRate()
        {
            if(ur >= 0)
                return ur;
            List<float> values = this.hits.ConvertAll((pair) => pair.Value.Time - pair.Key.StartTime);
            double avg = values.Average();
            ur = 10 * Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
            return ur;
        }

    }
}
