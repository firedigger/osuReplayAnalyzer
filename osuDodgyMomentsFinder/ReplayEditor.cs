using ReplayAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osuDodgyMomentsFinder
{
    class ReplayEditor
    {
        public Replay replay { get; private set; }

        public ReplayEditor(Replay replay)
        {
            this.replay = replay;
        }

        public void Shift(double X, double Y)
        {
            foreach(var frame in replay.ReplayFrames)
            {
                frame.X += (float)X;
                frame.Y += (float)Y;
            }
        }


        public void mixReplay(Replay other)
        {

        }

        public void Save(string path)
        {
            replay.Save(path);
        }

    }
}
