using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FT2232ImageOutput.FrameProcessors;

public class RotateFrameProcessor : IFrameProcessor
{
    private readonly Point _center;
    private readonly float _angleIncrement;
    private readonly int _incrementInterval;
    private readonly ImageMaxValues _maxValues;
    private float _angle;
    private readonly Timer _timer;

    public RotateFrameProcessor(ImageMaxValues maxValues, float angle, Point center, float angleIncrement, int incrementInterval)
    {
        _center = center;
        _angleIncrement = angleIncrement;
        _incrementInterval = incrementInterval;
        _maxValues = maxValues;
        _angle = angle;

        if (_center == null)
        {
            _center = new Point()
            {
                X = _maxValues.CenterX,
                Y = _maxValues.CenterY,
            };
        }

        if (_incrementInterval > 0)
            _timer = new Timer(IncrementAngle, null, _incrementInterval, _incrementInterval);

    }


    void IncrementAngle(object x)
    {
        _angle += _angleIncrement;
        // if (_angle > 360)
        //     _angle = 360 - _angle;
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
            var rotated = RotatePoint(new Point(point.X, point.Y), _center, _angle);

            if ((_maxValues.MinX <= rotated.X && rotated.X <= _maxValues.MaxX) &&
                (_maxValues.MinY <= rotated.Y && rotated.Y <= _maxValues.MaxY))
            {
                var newPoint = point.Clone();
                newPoint.X = rotated.X;
                newPoint.Y = rotated.Y;

                yield return newPoint;
            }
        }

        yield break;
    }

    // https://stackoverflow.com/questions/13695317/rotate-a-point-around-another-point
    /// <summary>
    /// Rotates one point around another
    /// </summary>
    /// <param name="pointToRotate">The point to rotate.</param>
    /// <param name="centerPoint">The center point of rotation.</param>
    /// <param name="angleInDegrees">The rotation angle in degrees.</param>
    /// <returns>Rotated point</returns>
    Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
    {
        double angleInRadians = angleInDegrees * (Math.PI / 180);
        double cosTheta = Math.Cos(angleInRadians);
        double sinTheta = Math.Sin(angleInRadians);
        return new Point
        {
            X =
                (int)
                (cosTheta * (pointToRotate.X - centerPoint.X) -
                sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
            Y =
                (int)
                (sinTheta * (pointToRotate.X - centerPoint.X) +
                cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
        };
    }

}

public class Point
{
    public Point() { }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; set; }
    public int Y { get; set; }
}
