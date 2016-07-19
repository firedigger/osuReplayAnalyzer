using System;
using System.Collections.Generic;
using System.Linq;

namespace BMAPI.v1.HitObjects
{
    public class SliderObject : CircleObject
    {
        public SliderObject() { }
        public SliderObject(CircleObject baseInstance) : base(baseInstance) { }

        public new SliderType Type = SliderType.Linear;
        public List<Point2> Points = new List<Point2>();
        public int RepeatCount { get; set; }
        private float _PixelLength = 0f;
        public float PixelLength
        {
            get { return this._PixelLength; }
            set { this._PixelLength = value; }
        }
        private float _TotalLength = -1;
        public float TotalLength
        {
            get
            {
                return this._TotalLength;
            }
        }
        private float _SegmentEndTime = -1;
        public float SegmentEndTime
        {
            get
            {
                if (this._SegmentEndTime < 0)
                {
                    this._SegmentEndTime = this.StartTime + this.TotalLength / this.Velocity;
                }
                return this._SegmentEndTime;
            }
        }
        public float Velocity { get; set; }
        public float MaxPoints { get; set; }
    }      
}
