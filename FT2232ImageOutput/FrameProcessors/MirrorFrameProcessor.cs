using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.FrameProcessors
{
    public class MirrorFrameProcessor : IFrameProcessor
    {
        private readonly ImageMaxValues _maxValues;
        private readonly MirrorFrameProcessorMode _mode;

        public MirrorFrameProcessor(ImageMaxValues maxValues, MirrorFrameProcessorMode mode)
        {
            _maxValues = maxValues;
            _mode = mode;
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
                    X = _mode.HasFlag(MirrorFrameProcessorMode.Horizontal) 
                        ? _maxValues.MaxX - (point.X - _maxValues.MinX)
                        : point.X,

                    Y = _mode.HasFlag(MirrorFrameProcessorMode.Vertical)
                        ? _maxValues.MaxY - (point.Y - _maxValues.MinY)
                        : point.Y,

                    Z = point.Z,

                    R = point.R,
                    G = point.G,
                    B = point.B,

                    Blanking = point.Blanking
                };

                yield return newPoint;
            }

            yield break;
        }

    }

    [Flags]
    public enum MirrorFrameProcessorMode { Vertical = 1, Horizontal = 2}

}
