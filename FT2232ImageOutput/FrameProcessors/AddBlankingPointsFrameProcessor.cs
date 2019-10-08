using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.FrameProcessors
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
                yield return point.Clone();
            }

            var p = originalPoints.Last().Clone();
            p.Blanking = true;

            yield return p;

            yield break;
        }


    }

}
