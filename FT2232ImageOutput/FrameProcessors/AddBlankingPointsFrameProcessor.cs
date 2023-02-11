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

            res.Points = GetPoints(frame.Points);

            return res;

        }


        IEnumerable<ImagePoint> GetPoints(IEnumerable<ImagePoint> originalPoints)
        {
            var firstpoint = originalPoints.First();
            foreach (var _ in Enumerable.Range(0, _pointsAfter))
            {
                var newpoint = firstpoint.Clone();
                newpoint.Blanking = true;
                yield return newpoint;
            }


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
                        var newpoint = point.Clone();
                        newpoint.Blanking = true;
                        yield return newpoint;
                    }
                    continue;
                }

                yield return point.Clone();
            }

            if (!_savePrevPointsState)
                _prevblanked = false;

            yield break;
        }


    }

}
