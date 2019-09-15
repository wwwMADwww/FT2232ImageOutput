using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput
{
    public class AddBlankingPointsFrameProcessor : IFrameProcessor
    {

        public AddBlankingPointsFrameProcessor()
        {
        }


        public ImageFrame Process(ImageFrame frame)
        {
            var res = new ImageFrame();
            res.Duration = frame.Duration;
            res.Number = frame.Number;

            if (!frame.Points.Any())
            {
                res.Points = new ImagePoint[0];
                return res;
            }

            var points = new List<ImagePoint>(frame.Points.Count() + 2);
            
            foreach (var point in frame.Points)
            {
                points.Add(Copy(point));
            }

            var p = Copy(frame.Points.Last());
            p.Blanking = true;
            points.Add(p);
            
            res.Points = points;

            return res;

        }

        ImagePoint Copy(ImagePoint point)
        {
            return new ImagePoint()
            {
                X = point.X,
                Y = point.Y,
                Z = point.Z,

                R = point.R,
                G = point.G,
                B = point.B,

                Blanking = point.Blanking
            };
        }

    }

}
