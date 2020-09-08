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

            // if (!frame.Points.Any())
            // {
            //     res.Points = new ImagePoint[0];
            //     return res;
            // }


            res.Points = GetPoints(frame.Points);
        
            return res;
        
        }

        IEnumerable<ImagePoint> GetPoints(IEnumerable<ImagePoint> originalPoints)
        {
            foreach (var point in originalPoints)
            {
                var newPoint = point.Clone();

                if (_mode.HasFlag(MirrorFrameProcessorMode.Horizontal))
                    newPoint.X = _maxValues.MaxX - (point.X - _maxValues.MinX);

                if (_mode.HasFlag(MirrorFrameProcessorMode.Vertical))
                    newPoint.Y = _maxValues.MaxY - (point.Y - _maxValues.MinY);

                yield return newPoint;
            }

            yield break;
        }

    }

    [Flags]
    public enum MirrorFrameProcessorMode { Vertical = 1, Horizontal = 2}

}
