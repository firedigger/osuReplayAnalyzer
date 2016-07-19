using BMAPI.v1.HitObjects;
using ReplayAPI;
using System;
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
}
