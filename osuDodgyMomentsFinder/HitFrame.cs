using BMAPI.v1.HitObjects;
using ReplayAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osuDodgyMomentsFinder
{
    class HitFrame
    {
        public ReplayFrame frame { get; set; }
        public CircleObject note { get; set; }

        public HitFrame(CircleObject note, ReplayFrame frame)
        {
            this.frame = frame;
            this.note = note;
        }

       
        public override string ToString() 
        {
            string res = "";
            if ((note.Type.HasFlag(BMAPI.v1.HitObjectType.Circle)))
            {
                res += "Circle";
            }
            if ((note.Type.HasFlag(BMAPI.v1.HitObjectType.Slider)))
            {
                res += "Slider";
            }
            res += " at ";
            res += note.StartTime + "ms";
            res += " hit at " + frame.Time + "ms";
            res += "(" + frame.keyCounter + ")";

            return res;

        }

    }
}
