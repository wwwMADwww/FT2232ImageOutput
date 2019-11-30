using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.FrameProcessors
{
    public class DuplicatePointsFrameProcessor : IFrameProcessor
    {
        private readonly int _maxAmount;
        private readonly float _inc;

        float _angle = 0;
        int _currentAmount = 0;

        public DuplicatePointsFrameProcessor(int maxAmount, float inc)
        {
            _maxAmount = maxAmount;
            _inc = inc;
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

            _angle = _angle + _inc;
            _currentAmount = (int)Math.Floor((Math.Sin(DegreeToRadian((double)_angle)) * _maxAmount) + _maxAmount);

            // Console.WriteLine($"_angle {_angle}, _currentAmount {_currentAmount}");

            return res;

        }


        IEnumerable<ImagePoint> GetPoints(IEnumerable<ImagePoint> originalPoints)
        {

            foreach (var point in originalPoints)
            {
                yield return point.Clone();

                for (int i=0; i < _currentAmount; i++)
                    if (!point.Blanking)
                        yield return point.Clone();
            }

            yield break;
        }


        private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }


    }

}
