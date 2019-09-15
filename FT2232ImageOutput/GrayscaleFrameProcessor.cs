using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput
{
    public class GrayscaleFrameProcessor : IFrameProcessor
    {
        private readonly ImageMaxValues _maxValues;
        private readonly GrayscaleFrameProcessorMapMode _mapMode;

        public GrayscaleFrameProcessor(ImageMaxValues maxValues, GrayscaleFrameProcessorMapMode mapMode)
        {
            _maxValues = maxValues;
            _mapMode = mapMode;
        }


        public ImageFrame Process(ImageFrame frame)
        {
            var res = new ImageFrame();
            res.Duration = frame.Duration;
            res.Number = frame.Number;
            var points = new List<ImagePoint>(frame.Points.Count());
            
            foreach (var point in frame.Points)
            {

                // almost fisrt from google
                int gray = (byte)(.21 * point.R + .71 * point.G + .071 * point.B);

                var newPoint = new ImagePoint()
                {
                    X = point.X,
                    Y = point.Y,
                    Z = point.Z,

                    Blanking = point.Blanking
                };

                switch (_mapMode)
                {

                    case GrayscaleFrameProcessorMapMode.Color:
                        newPoint.R = gray;
                        newPoint.G = gray;
                        newPoint.B = gray;
                        break;

                    case GrayscaleFrameProcessorMapMode.CoordZ:
                        newPoint.Z = gray;
                        break;

                }

                points.Add(newPoint);
            }

            res.Points = points;

            return res;

        }

    }

    public enum GrayscaleFrameProcessorMapMode { Color, CoordZ }

}
