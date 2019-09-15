using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput
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
            var points = new List<ImagePoint>(frame.Points.Count());

            if (!frame.Points.Any())
            {
                res.Points = points;
                return res;
            }

            ImagePoint pointOld = null;

            foreach (var point in frame.Points)
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
                    ) : true)
                ))
                {
                    var newPoint = new ImagePoint()
                    {
                        X = point.X,
                        Y = point.Y,
                        Z = point.Z,

                        R = point.R,
                        G = point.G,
                        B = point.B,

                        Blanking = point.Blanking
                    };
                    points.Add(newPoint);
                }
                
                pointOld = point;

            }

            res.Points = points;

            return res;

        }

    }

    [Flags]
    public enum DuplicateReduceFrameProcessorFlags { 
        CoordXY = 1, 
        CoordZ = 2, 
        Color = 4,
    
        Coords = CoordXY | CoordZ,
        All = Coords | Color
    }

}
