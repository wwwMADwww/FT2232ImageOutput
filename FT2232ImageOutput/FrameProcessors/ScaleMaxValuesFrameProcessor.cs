using FT2232ImageOutput.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.FrameProcessors
{
    public class ScaleMaxValuesFrameProcessor : IFrameProcessor
    {
        private readonly ImageMaxValues _maxValues;
        private readonly ImageMaxValues _targetMaxValues;

        public ScaleMaxValuesFrameProcessor(ImageMaxValues maxValues, ImageMaxValues targetMaxValues)
        {
            _maxValues = maxValues;
            _targetMaxValues = targetMaxValues;
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


            res.Points = GetPoints(frame.Points); // points;
        
            return res;
        
        }

        IEnumerable<ImagePoint> GetPoints(IEnumerable<ImagePoint> originalPoints)
        {
            foreach (var point in originalPoints)
            {
                var newPoint = new ImagePoint()
                {
                    X = MathUtils.ConvertRange(_maxValues.MinX, _maxValues.MaxX, _targetMaxValues.MinX, _targetMaxValues.MaxX, point.X),
                    Y = MathUtils.ConvertRange(_maxValues.MinY, _maxValues.MaxY, _targetMaxValues.MinY, _targetMaxValues.MaxY, point.Y),
                    Z = MathUtils.ConvertRange(_maxValues.MinZ, _maxValues.MaxZ, _targetMaxValues.MinZ, _targetMaxValues.MaxZ, point.Z),
                        
                    R = MathUtils.ConvertRange(0, _maxValues.MaxRGB, 0, _targetMaxValues.MaxRGB, point.R),
                    G = MathUtils.ConvertRange(0, _maxValues.MaxRGB, 0, _targetMaxValues.MaxRGB, point.G),
                    B = MathUtils.ConvertRange(0, _maxValues.MaxRGB, 0, _targetMaxValues.MaxRGB, point.B),

                    Blanking = point.Blanking
                };

                yield return newPoint;
            }

            yield break;
        }
            




    }
}
