using BMAPI.v1.HitObjects;
using ReplayAPI;

namespace osuDodgyMomentsFinder
{
    public class HitFrame
    {
        public ReplayFrame frame { get; set; }
        public CircleObject note { get; set; }
        public Keys key { get; set; }

        public double perfectness()
        {
            return Utils.pixelPerfectHitFactor(frame, note);
        }

        public HitFrame(CircleObject note, ReplayFrame frame, Keys key)
        {
            this.frame = frame;
            this.note = note;
            this.key = key;
        }


        public override string ToString()
        {
            string res = note.ToString();
            res += " hit at " + frame.Time + "ms";
            //res += "(" + frame.keyCounter + ")";

            return res;

        }

    }
}
