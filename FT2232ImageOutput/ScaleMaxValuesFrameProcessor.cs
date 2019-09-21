using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput
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
            // var points = new List<ImagePoint>(frame.Points.Count());

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
                    X = ConvertRange(_maxValues.MinX, _maxValues.MaxX, _targetMaxValues.MinX, _targetMaxValues.MaxX, point.X),
                    Y = ConvertRange(_maxValues.MinY, _maxValues.MaxY, _targetMaxValues.MinY, _targetMaxValues.MaxY, point.Y),
                    Z = ConvertRange(_maxValues.MinZ, _maxValues.MaxZ, _targetMaxValues.MinZ, _targetMaxValues.MaxZ, point.Z),

                    R = ConvertRange(0, _maxValues.MaxRGB, 0, _targetMaxValues.MaxRGB, point.R),
                    G = ConvertRange(0, _maxValues.MaxRGB, 0, _targetMaxValues.MaxRGB, point.G),
                    B = ConvertRange(0, _maxValues.MaxRGB, 0, _targetMaxValues.MaxRGB, point.B),

                    Blanking = point.Blanking
                };

                // points.Add(newPoint);
                yield return newPoint;
            }

            yield break;
        }

        // public ImageFrame Process(ImageFrame frame)
        // {
        //     foreach (var point in frame.Points)
        //     {
        //         point.X = ConvertRange(_maxValues.MinX, _maxValues.MaxX, _targetMaxValues.MinX, _targetMaxValues.MaxX, point.X);
        //         point.Y = ConvertRange(_maxValues.MinY, _maxValues.MaxY, _targetMaxValues.MinY, _targetMaxValues.MaxY, point.Y);
        //         point.Z = ConvertRange(_maxValues.MinZ, _maxValues.MaxZ, _targetMaxValues.MinZ, _targetMaxValues.MaxZ, point.Z);
        // 
        //         point.R = ConvertRange(0, _maxValues.MaxRGB, 0, _targetMaxValues.MaxRGB, point.R);
        //         point.G = ConvertRange(0, _maxValues.MaxRGB, 0, _targetMaxValues.MaxRGB, point.G);
        //         point.B = ConvertRange(0, _maxValues.MaxRGB, 0, _targetMaxValues.MaxRGB, point.B);
        // 
        //         point.Blanking = point.Blanking;
        //     }
        // 
        //     return frame;
        // 
        // }



        // https://stackoverflow.com/questions/4229662/convert-numbers-within-a-range-to-numbers-within-another-range
        int ConvertRange(
            int originalStart, int originalEnd, // original range
            int newStart, int newEnd, // desired range
            int value) // value to convert
        {
            double scale = (double)(newEnd - newStart) / (originalEnd - originalStart);
            return (int)(newStart + ((value - originalStart) * scale));
        }




    }
}
