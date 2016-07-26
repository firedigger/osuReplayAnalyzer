using BMAPI.v1;
using BMAPI.v1.Events;
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
        public List<BreakEvent> breaks { get; private set; }
        public CursorMovement cursorData { get; private set; }
        public List<ReplayFrame> times { get; private set; }

        private void applyHardrock()
        {
            replay.AxisFlip = true;
            beatmap.CircleSize = beatmap.CircleSize * 1.3f;
            if (beatmap.CircleSize > 10)
                beatmap.CircleSize = 10;
            this.replay.ReplayFrames.ForEach((t) => t.Y = 384 - t.Y);
        }

        private void selectBreaks()
        {
            foreach(var event1 in this.beatmap.Events)
            {
                if (event1.GetType() == typeof(BreakEvent))
                {
                    this.breaks.Add((BreakEvent)event1);
                }
            }
        }

        private void associateHits()
        {
            int misses = 0;

            int keyIndex = 0;
            bool pressReady = true;
            Keys lastKey = Keys.None;
            KeyCounter keyCounter = new KeyCounter();

            if ((replay.Mods & Mods.HardRock) > 0)
            {
                applyHardrock();
            }

            int breakIndex = 0;
            int combo = 0;

            for (int i = 0; i < beatmap.HitObjects.Count; ++i)
            {
                CircleObject note = beatmap.HitObjects[i];
                bool flag = false;
                
                if ((note.Type & HitObjectType.Spinner) > 0)
                    continue;

                for (int j = keyIndex; j < replay.ReplayFrames.Count; ++j)
                {
                    ReplayFrame frame = replay.ReplayFrames[j];

                    if (((frame.Keys & lastKey) ^ frame.Keys) > 0)
                        pressReady = true;

                    if (breakIndex < breaks.Count && frame.Time > breaks[breakIndex].EndTime)
                    {
                        ++breakIndex;
                    }

                    if (frame.Time >= beatmap.HitObjects[0].StartTime - hitTimeWindow && (breakIndex >= breaks.Count || frame.Time < this.breaks[breakIndex].StartTime - hitTimeWindow))
                    {
                        keyCounter.Update(lastKey, frame.Keys);
                    }

                    frame.keyCounter = new KeyCounter(keyCounter);

                    if (frame.Keys != Keys.None && Math.Abs(frame.Time - note.StartTime) <= hitTimeWindow && note.ContainsPoint(new BMAPI.Point2(frame.X, frame.Y)) && pressReady)
                    {
                        ++combo;
                        frame.combo = combo;
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

                    frame.combo = combo;

                }

                if (!flag)
                {
                    ++misses;
                    miss.Add(note);
                }

            }
        }

        private List<double> calcSliderHoldTime()
        {
            List<double> result = new List<double>();

            foreach(var hit in this.hits)
            {
                if (hit.note.Type == HitObjectType.Slider && ((SliderObject)hit.note).RepeatCount == 1)
                {
                    int i = this.replay.ReplayFrames.FindIndex(x => x == hit.frame);

                    if (i >= 4250)
                    {
                        int u = 0;
                    }

                    Keys key = getKey(this.replay.ReplayFrames[i - 1].Keys, this.replay.ReplayFrames[i].Keys);

                    if (key == Keys.None)
                        throw new Exception();
                    
                    while(replay.ReplayFrames[i].Keys.HasFlag(key))
                    {
                        ++i;
                    }

                    SliderObject hitObjectAsSlider = (SliderObject)hit.note;
                    double objectLength = (hitObjectAsSlider.TotalLength / hitObjectAsSlider.Velocity) * hitObjectAsSlider.RepeatCount;
                    //double objectLength = ((hitObjectAsSlider.SegmentEndTime - hitObjectAsSlider.StartTime) * hitObjectAsSlider.RepeatCount);
                    double delta = replay.ReplayFrames[i].Time - hitObjectAsSlider.StartTime - objectLength;

                    if (delta > 100)
                    {
                        int u = 0;
                    }

                    if (!double.IsNaN(delta))
                    {
                        result.Add(delta);
                    }
                }
            }

            return result;
        }

        public double calcSliderHoldTimeVariance()
        {
            var list = calcSliderHoldTime();
            return Utils.variance(list);
        }

        private Keys getKey(Keys last, Keys current)
        {
            if (!last.HasFlag(Keys.M1) && current.HasFlag(Keys.M1) && !current.HasFlag(Keys.K1)) return Keys.M1;
            if (!last.HasFlag(Keys.M2) && current.HasFlag(Keys.M2) && !current.HasFlag(Keys.K2)) return Keys.M2;
            if (!last.HasFlag(Keys.K1) && current.HasFlag(Keys.K1)) return Keys.K1;
            if (!last.HasFlag(Keys.K2) && current.HasFlag(Keys.K2)) return Keys.K2;
            return Keys.None;
        }

        private List<double> calcPressIntervals()
        {
            List<double> result = new List<double>();

            bool k1 = false, k2 = false;
            double k1_timer = 0, k2_timer = 0;
            foreach (var frame in this.replay.ReplayFrames)
            {
                var hit = this.hits.Find(x => x.frame.Equals(frame));
                
                if (hit != null && hit.note.Type == HitObjectType.Circle)
                {
                    if (!k1 && frame.Keys.HasFlag(Keys.K1))
                        k1 = true;

                    if (!k2 && frame.Keys.HasFlag(Keys.K2))
                        k2 = true;
                }

                //k1
                if (k1 && frame.Keys.HasFlag(Keys.K1))
                {
                    k1_timer += frame.TimeDiff;
                }

                if (k1 && !frame.Keys.HasFlag(Keys.K1))
                {
                    k1 = false;
                    result.Add(k1_timer);
                    k1_timer = 0;
                }

                //k2
                if (k2 && frame.Keys.HasFlag(Keys.K2))
                {
                    k2_timer += frame.TimeDiff;
                }

                if (k2 && !frame.Keys.HasFlag(Keys.K2))
                {
                    k2 = false;
                    result.Add(k2_timer);
                    k2_timer = 0;
                }
            }

            return result;
        }

        public double averagePressIntervals()
        {
            return calcPressIntervals().Average();
        }


        public bool isCheating()
        {
            if (this.replay.PlayerName == "Axarious" || this.replay.PlayerName == "Bikko")
                return true;
            return false;
        }

        public double calculateAverageFrameTimeDiff()
        {
            return this.times.ConvertAll(x => x.TimeDiff).Average();
        }

        
        private void calculateCursorSpeed()
        {
            double distance = 0;

            this.times = replay.ReplayFrames.Where(x => x.TimeDiff > 0 && x.TimeDiff <= 40).ToList();

            times[0].travelledDistance = distance;
            for (int i = 0; i < times.Count - 1; ++i)
            {
                ReplayFrame from = times[i], to = times[i + 1];
                distance += Utils.dist(from.X, from.Y, to.X, to.Y);
                to.travelledDistance = distance;
            }

            times[0].speed = 0;
            for (int i = 1; i < times.Count - 1; ++i)
            {
                ReplayFrame from = times[i - 1], to = times[i + 1], current = times[i];

                double V = (to.travelledDistance - from.travelledDistance) / (to.TimeDiff + current.TimeDiff);
                current.speed = V;
            }
            times.Last().speed = 0;

            times[0].acceleration = 0;
            for (int i = 1; i < times.Count - 1; ++i)
            {
                ReplayFrame from = times[i - 1], to = times[i + 1], current = times[i];

                double A = (to.speed - from.speed) / (to.TimeDiff + current.TimeDiff);
                current.acceleration = A;
            }
            times.Last().acceleration = 0;

            /*int hitIndex = 0;
            for(int i = 0; i < this.beatmap.HitObjects.Count - 1; ++i)
            {
                CircleObject note = this.beatmap.HitObjects[i];

                while (this.replay.ReplayFrames[hitIndex].Time < note.StartTime)
                    ++hitIndex;

                List<>

            }*/

            /*this.cursorData = new CursorMovement();
            double h = this.replay.ReplayFrames.Where(x => x.TimeDiff > 0 && x.TimeDiff <= 40).Average(x => x.TimeDiff);

            double[] values = new double[(int)(replay.ReplayFrames.Last().TimeDiff / h)];
            values[0] = this.replay.ReplayFrames[0].travelledDistance;
            for(int i = 1; i < values.Length; ++i)
            {
                int frameBefore = i;
                int frameAfter = i;
                while (replay.ReplayFrames[frameBefore].Time > i * h)
                    --frameBefore;
                while (replay.ReplayFrames[frameBefore].Time < (i + 1) * h)
                    ++frameAfter;

                values[i] = (replay.ReplayFrames[frameAfter].travelledDistance - replay.ReplayFrames[frameBefore].travelledDistance) / (replay.ReplayFrames[frameAfter].Time - replay.ReplayFrames[frameBefore].Time) * (i*h - replay.ReplayFrames[frameBefore].Time);
            }

            double offset = 2 * h;
            double[] speedValues = new double[values.Length - 4];
            for(int i = 2; i < values.Length - 2; ++i)
            {
                double S1 = values[i - 1] - values[i - 2]; 
                double S2 = values[i] - values[i - 1];
                double S3 = values[i + 1] - values[i];
                double S4 = values[i + 2] - values[i + 1];

                double V2 = Utils.derivative(S1, S2, S3, h);
                double V3 = Utils.derivative(S2, S3, S4, h);

                double V = (V3 + V2) / 2;
                speedValues[i - 2] = V;
            }

            cursorData.h = h;
            cursorData.offset = offset;
            cursorData.speed = values;*/
        }

        public List<double> speedList()
        {
            return this.times.ConvertAll(x => x.speed);
        }

        public List<double> accelerationList()
        {
            return this.times.ConvertAll(x => x.acceleration);
        }

        public string outputSpeed()
        {
            string res = "";
            foreach(var value in speedList())
            {
                res += value + ",";
            }
            return res.Remove(res.Length - 1);
        }

        public string outputAcceleration()
        {
            string res = "";
            foreach (var value in this.times.ConvertAll(x => x.acceleration))
            {
                res += value + ",";
            }
            return res.Remove(res.Length - 1);
        }

        public string outputTime()
        {
            string res = "";
            foreach (var value in this.times.ConvertAll(x => x.Time))
            {
                res += value + ",";
            }
            return res.Remove(res.Length - 1);
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

        public double calcAccelerationVariance()
        {
            return Utils.variance(accelerationList());
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

            hits = new List<HitFrame>();
            miss = new List<CircleObject>();
            breaks = new List<BreakEvent>();

            selectBreaks();
            associateHits();
            calculateCursorSpeed();
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
            List<double> values = this.hits.ConvertAll((pair) => (double)pair.frame.Time - pair.note.StartTime);
            ur = 10 * Utils.variance(values);
            return ur;
        }


        public string MainInfo()
        {
            string res = "";
            res += "GENERIC INFO\n";

            res += "Unstable rate = " + unstableRate() + "\n";
            if (unstableRate() < 50)
                res += "WARNING! Unstable rate is too low (auto)\n";
            res += "The best CS value = " + bestCSValue() + "\n";

            res += "Cursor acceleration variance = " + calcAccelerationVariance() + "\n";

            double averageFrameTimeDiff = calculateAverageFrameTimeDiff();
            res += "Average frame time difference = " + averageFrameTimeDiff + "ms\n";
            if (replay.Mods.HasFlag(Mods.DoubleTime) && averageFrameTimeDiff < 17)
            {
                res += "WARNING! Average frame time difference is not consistent with the speed-modifying gameplay mods(timewarp)";
            }
            if (!replay.Mods.HasFlag(Mods.HalfTime) && averageFrameTimeDiff < 12)
            {
                res += "WARNING! Average frame time difference is not consistent with the speed-modifying gameplay mods(timewarp)";
            }

            res += "Average Key press time interval = " + averagePressIntervals() + "ms\n";
            //res += "Slider hold time variance = " + calcSliderHoldTimeVariance() + "ms\n";
            return res;
        }

        public string PixelPerfectInfo()
        {
            string res = "";
            res += "PIXEL PERFECT\n";

            var pixelPerfectHits = findSortedPixelPerfectHits(1000, 0.98);
            double bestPxPerfect = findBestPixelHit();
            res += "The best pixel perfect hit = " + bestPxPerfect + "\n";
            if (bestPxPerfect < 0.6)
                res += "WARNING! Player is clicking into the center of the note too consistently (auto)\n";
            res += "Pixel perfect hits: " + pixelPerfectHits.Count + "\n";
            foreach (var hit in pixelPerfectHits)
                res += "* " + hit.Key +  " " + hit.Value + "\n";
            var LOLpixelPerfectHits = findSortedPixelPerfectHits(100, 0.99);
            if (LOLpixelPerfectHits.Count > 40)
                res += "WARNING! Player is constantly doing pixel perfect hits (relax)\n";

            return res;
        }
        
        public string OveraimsInfo()
        {
            string res = "";
            res += "OVER-AIM\n";

            var overAims = findOverAimHits();
            res += "Over-aim count: " + overAims.Count + "\n";
            foreach (var hit in overAims)
                res += "* " + hit + "\n";

            return res;
        }
    }
}
