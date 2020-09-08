using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.FrameProcessors
{
    public class AddBlankingPointsFrameProcessor : IFrameProcessor
    {
        private readonly int _pointsBefore;
        private readonly int _pointsAfter;
        private readonly bool _savePrevPointsState;

        bool _prevblanked = false;

        public AddBlankingPointsFrameProcessor(int pointsBefore, int pointsAfter, bool savePrevPointsState)
        {
            _pointsBefore = pointsBefore;
            _pointsAfter = pointsAfter;
            _savePrevPointsState = savePrevPointsState;
        }


        public ImageFrame Process(ImageFrame frame)
        {
            var res = new ImageFrame();
            res.Duration = frame.Duration;
            res.Number = frame.Number;

            // if (!frame.Points.Any())
            // {
            //     res.Points = new ImagePoint[0];
            //     return res;
            // }

            res.Points = GetPoints(frame.Points); //points;

            return res;

        }


        IEnumerable<ImagePoint> GetPoints(IEnumerable<ImagePoint> originalPoints)
        {


            foreach (var point in originalPoints)
            {
                if (!_prevblanked && point.Blanking)
                {
                    _prevblanked = true;
                    foreach (var _ in Enumerable.Range(0, _pointsBefore))
                    {
                        yield return point.Clone();
                    }
                    continue;
                }

                if (_prevblanked && !point.Blanking)
                {
                    _prevblanked = false;
                    foreach (var _ in Enumerable.Range(0, _pointsAfter))
                    {
                        yield return point.Clone();
                    }
                    continue;
                }

                yield return point.Clone();
            }

            // var p = originalPoints.Last().Clone();
            // p.Blanking = true;

            // yield return p;

            if (!_savePrevPointsState)
                _prevblanked = false;

            yield break;
        }


    }

}
