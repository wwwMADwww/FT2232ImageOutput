using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FT2232ImageOutput.FrameProcessors;

public class SineWaveFrameProcessor : IFrameProcessor
{
    private readonly float _amplitude;
    private readonly float _interval;
    private readonly float _increment;
    private readonly ImageMaxValues _maxValues;
    private readonly Timer _timer;
    float _angle = 0;



    public SineWaveFrameProcessor(TimeSpan incrementInterval, float amplitude, float increment, float interval, ImageMaxValues maxValues)
    {
        _amplitude = amplitude;
        _interval = interval;
        _increment = increment;
        _maxValues = maxValues;

        _timer = new Timer(IncrementAngle, null, incrementInterval, incrementInterval);
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


    void IncrementAngle(object x)
    {
        _angle += _increment;
        // if (_angle > 360)
        //     _angle = 360 - _angle;
    }

    IEnumerable<ImagePoint> GetPoints(IEnumerable<ImagePoint> originalPoints)
    {
        var angle = _angle;

        foreach (var point in originalPoints)
        {

            var newX = (int)(point.X + (Math.Sin(DegreeToRadian(angle+ (point.Y * _interval))) * _amplitude));

            var blanking = point.Blanking;

            if (_maxValues.MinX > newX)
            {
                newX = _maxValues.MinX;
                blanking = true;
            }

            if (newX > _maxValues.MaxX)
            {
                newX = _maxValues.MaxX;
                blanking = true;
            }

            var newPoint = point.Clone();
            newPoint.X = newX;
            newPoint.Blanking = blanking;

            yield return newPoint;
            
        }

        // _angle += _increment;


        yield break;

    }

    private double DegreeToRadian(double angle)
    {
        return Math.PI * angle / 180.0;
    }

}
