using FT2232ImageOutput.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.FrameProcessors
{
    public class GrayscaleFrameProcessor : IFrameProcessor
    {
        private readonly ImageMaxValues _maxValues;
        private readonly GrayscaleFrameProcessorMapMode _mapMode;
        private readonly bool _invertResult;

        public GrayscaleFrameProcessor(ImageMaxValues maxValues, GrayscaleFrameProcessorMapMode mapMode, bool invertResult)
        {
            _maxValues = maxValues;
            _mapMode = mapMode;
            _invertResult = invertResult;
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

            res.Points = GetPoints(frame.Points); // points;

            return res;

        }

        IEnumerable<ImagePoint> GetPoints(IEnumerable<ImagePoint> originalPoints)
        {


            foreach (var point in originalPoints)
            {

                // almost fisrt from google
                int gray = (int)(.21 * point.R + .71 * point.G + .071 * point.B);

                if (_invertResult)
                    gray = _maxValues.MaxRGB - gray;

                var newPoint = point.Clone();

                switch (_mapMode)
                {

                    case GrayscaleFrameProcessorMapMode.Color:
                        newPoint.R = gray;
                        newPoint.G = gray;
                        newPoint.B = gray;
                        break;

                    case GrayscaleFrameProcessorMapMode.CoordZ:
                        newPoint.Z = MathUtils.ConvertRange(0, _maxValues.MaxRGB, _maxValues.MinZ, _maxValues.MaxZ, gray);
                        break;

                }

                yield return newPoint;
            }

            yield break;

        }


    }

    public enum GrayscaleFrameProcessorMapMode { Color, CoordZ }

}
