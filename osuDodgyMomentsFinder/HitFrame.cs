using BMAPI.v1.HitObjects;
using ReplayAPI;

namespace osuDodgyMomentsFinder
{
    public class HitFrame
    {
        public ReplayFrame frame { get; set; }
        public CircleObject note { get; set; }
        public Keys key { get; set; }
        public double perfectness { get; set; }

        public HitFrame(CircleObject note, ReplayFrame frame, Keys key)
        {
            this.frame = frame;
            this.note = note;
            this.key = key;
        }


        public override string ToString()
        {
            string res = "";
            if((note.Type.HasFlag(BMAPI.v1.HitObjectType.Circle)))
            {
                res += "Circle";
            }
            if((note.Type.HasFlag(BMAPI.v1.HitObjectType.Slider)))
            {
                res += "Slider";
            }
            res += " at ";
            res += note.StartTime + "ms";
            res += " hit at " + frame.Time + "ms";
            res += "(" + frame.keyCounter + ")";
            //res += "(" + frame.combo + "x)";

            return res;

        }

    }
}
