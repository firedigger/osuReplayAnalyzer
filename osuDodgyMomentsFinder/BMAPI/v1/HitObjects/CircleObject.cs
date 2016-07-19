using System;

namespace BMAPI.v1.HitObjects
{
    public class CircleObject
    {
        public CircleObject() { }
        public CircleObject(CircleObject baseInstance)
        {
            //Copy from baseInstance
            Location = baseInstance.Location;
            Radius = baseInstance.Radius;
            StartTime = baseInstance.StartTime;
            Type = baseInstance.Type;
            Effect = baseInstance.Effect;
        }

        public Point2 Location = new Point2(0, 0);
        public float Radius = 80;
        public float StartTime { get; set; }
        public HitObjectType Type { get; set; }
        public EffectType Effect = EffectType.None;

        public virtual bool ContainsPoint(Point2 Point)
        {
            return Math.Sqrt(Math.Pow(Point.X - Location.X, 2) + Math.Pow(Point.Y - Location.Y, 2)) <= Radius;
        }
    }
}
