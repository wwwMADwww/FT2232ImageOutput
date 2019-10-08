using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.FrameProcessors
{
    public class DuplicateReduceFrameProcessor : IFrameProcessor
    {
        private readonly DuplicateReduceFrameProcessorFlags _reduceMode;

        public DuplicateReduceFrameProcessor(DuplicateReduceFrameProcessorFlags reduceMode)
        {
            _reduceMode = reduceMode;
        }


        public ImageFrame Process(ImageFrame frame)
        {
            var res = new ImageFrame();
            res.Duration = frame.Duration;
            res.Number = frame.Number;
            // var points = new List<ImagePoint>(frame.Points.Count());

            if (!frame.Points.Any())
            {
                res.Points = new ImagePoint[0]; //points;
                return res;
            }


            res.Points = GetPoints(frame.Points); // points;

            return res;

        }

        IEnumerable<ImagePoint> GetPoints(IEnumerable<ImagePoint> originalPoints)
        {

            ImagePoint pointOld = null;

            foreach (var point in originalPoints)
            {

                if (pointOld == null || !(
                    (_reduceMode.HasFlag(DuplicateReduceFrameProcessorFlags.CoordXY) ? (
                        point.X == pointOld.X &&
                        point.Y == pointOld.Y
                    ) : true) &&
                    (_reduceMode.HasFlag(DuplicateReduceFrameProcessorFlags.CoordZ) ? (
                        point.Z == pointOld.Z
                    ) : true) &&
                    (_reduceMode.HasFlag(DuplicateReduceFrameProcessorFlags.Color) ? (
                        point.R == pointOld.R &&
                        point.G == pointOld.G &&
                        point.B == pointOld.B
                    ) : true) &&
                    (_reduceMode.HasFlag(DuplicateReduceFrameProcessorFlags.Blanking) ? (
                        point.Blanking && pointOld.Blanking
                    ) : true)
                ))
                {
                    yield return point.Clone();
                }

                pointOld = point;

            }


            yield break;

        }

    }

    [Flags]
    public enum DuplicateReduceFrameProcessorFlags { 
        CoordXY = 1, 
        CoordZ = 2, 
        Color = 4,
        Blanking = 8,

        Coords = CoordXY | CoordZ,
        All = Coords | Color | Blanking
    }

}
