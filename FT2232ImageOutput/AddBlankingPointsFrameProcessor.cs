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

            res.Points = GetPoints(frame.Points); //points;

            return res;

        }


        IEnumerable<ImagePoint> GetPoints(IEnumerable<ImagePoint> originalPoints)
        {

            foreach (var point in originalPoints)
            {
                yield return Copy(point);
            }

            var p = Copy(originalPoints.Last());
            p.Blanking = true;

            yield return p;

            yield break;
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
