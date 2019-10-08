using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput
{

    public class ImageFrame
    {
        public IEnumerable<ImagePoint> Points { get; set; }

        public int Number { get; set; }

        public int Duration { get; set; }

    }

    public class ImagePoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public bool Blanking { get; set; }



        public ImagePoint Clone()
        {
            return new ImagePoint()
            {
                X = X,
                Y = Y,
                Z = Z,

                R = R,
                G = G,
                B = B,

                Blanking = Blanking
            };
        }
    }
}
