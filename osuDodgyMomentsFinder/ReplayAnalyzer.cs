﻿using BMAPI.v1;
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
        public List<HitFrame> hits { get; private set; }
        public List<CircleObject> miss { get; private set; }


        private void applyHardrock()
        {
            replay.AxisFlip = true;
            beatmap.CircleSize = beatmap.CircleSize * 1.3f;
            if (beatmap.CircleSize > 10)
                beatmap.CircleSize = 10;
            this.replay.ReplayFrames.ForEach((t) => t.Y = 384 - t.Y);
        }

        private void associateHits()
        {
            hits = new List<HitFrame>();
            miss = new List<CircleObject>();

            int misses = 0;

            int keyIndex = 0;
            bool pressReady = true;
            Keys lastKey = Keys.None;
            KeyCounter keyCounter = new KeyCounter();

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

                    keyCounter.Update(lastKey, frame.Keys);

                    frame.keyCounter = new KeyCounter(keyCounter);

                    if (frame.Keys != Keys.None && Math.Abs(frame.Time - note.StartTime) <= hitTimeWindow && note.ContainsPoint(new BMAPI.Point2(frame.X, frame.Y)) && pressReady)
                    {
                        flag = true;
                        pressReady = false;
                        lastKey = frame.Keys;
                        hits.Add(new HitFrame(note, frame));
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


        public List<HitFrame> findOverAimHits()
        {
            var result = new List<HitFrame>();
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
                        if (hits[l].note.Equals(note) && hits[l].frame.Time > replay.ReplayFrames[keyIndex].Time)
                        {
                            noteIndex = l;
                            break;
                        }

                    if (noteIndex != -1)
                        result.Add(hits[noteIndex]);

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
            return this.hits.Max((pair) => Utils.pixelPerfectHitFactor(pair.frame, pair.note));
        }

        public List<double> findPixelPerfectHits(double threshold)
        {
            List<double> result = new List<double>();

            foreach (var pair in this.hits)
            {
                double factor = Utils.pixelPerfectHitFactor(pair.frame, pair.note);

                if (factor >= threshold)
                {
                    result.Add(pair.note.StartTime);
                }
            }


            return result;
        }


        public List<KeyValuePair<double, HitFrame>> findSortedPixelPerfectHits(int maxSize, double threshold)
        {
            var pixelPerfectHits = new List<KeyValuePair<double, HitFrame>>();

            foreach (var pair in this.hits)
            {
                double factor = Utils.pixelPerfectHitFactor(pair.frame, pair.note);
                if(factor >= threshold)
                    pixelPerfectHits.Add(new KeyValuePair<double, HitFrame>(factor, pair));
            }

            pixelPerfectHits.Sort((a, b) => b.Key.CompareTo(a.Key));

            return pixelPerfectHits.GetRange(0, Math.Min(maxSize, pixelPerfectHits.Count));
        }


        private double ur = -1;
        public double unstableRate()
        {
            if (ur >= 0)
                return ur;
            List<float> values = this.hits.ConvertAll((pair) => pair.frame.Time - pair.note.StartTime);
            double avg = values.Average();
            ur = 10 * Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
            return ur;
        }


        public void PrintMainInfo()
        {
            Console.WriteLine("\nGENERIC INFO");

            Console.WriteLine("Unstable rate = " + unstableRate());
            if (unstableRate() < 50)
                Console.WriteLine("WARNING! Unstable rate is too low (auto)");
            Console.WriteLine("The best CS value = " + bestCSValue());
        }

        public void PrintPixelPerfect()
        {
            Console.WriteLine("\nPIXEL PERFECT");

            var pixelPerfectHits = findSortedPixelPerfectHits(10, 0.9);
            double bestPxPerfect = findBestPixelHit();
            Console.WriteLine("The best pixel perfect hit = " + bestPxPerfect);
            if (bestPxPerfect < 0.6)
                Console.WriteLine("WARNING! Player is clicking into the center of the note too consistently (auto)");
            Console.WriteLine("Pixel perfect hits: " + pixelPerfectHits.Count);
            foreach (var hit in pixelPerfectHits)
                Console.WriteLine("* " + hit.Key +  " " + hit.Value);
            var LOLpixelPerfectHits = findSortedPixelPerfectHits(100, 0.99);
            if (LOLpixelPerfectHits.Count > 40)
                Console.WriteLine("WARNING! Player is constantly doing pixel perfect hits (relax)");
        }
        
        public void PrintOveraims()
        {
            Console.WriteLine("\nOVER-AIM");

            var overAims = findOverAimHits();
            Console.WriteLine("Over-aim count: " + overAims.Count);
            foreach (var hit in overAims)
                Console.WriteLine("* " + hit);
        }
    }
}