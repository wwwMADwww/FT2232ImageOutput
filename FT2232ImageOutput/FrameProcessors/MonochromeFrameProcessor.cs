using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.FrameProcessors;

public class MonochromeFrameProcessor : IFrameProcessor
{
    private readonly MonochromeFrameProcessorSourceColor _sourceColor;

    public MonochromeFrameProcessor(MonochromeFrameProcessorSourceColor sourceColor)
    {
        _sourceColor = sourceColor;
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

            int color = 0;

            switch (_sourceColor)
            {

                case MonochromeFrameProcessorSourceColor.Red:
                    color = point.R;
                    break;

                case MonochromeFrameProcessorSourceColor.Green:
                    color = point.G;
                    break;

                case MonochromeFrameProcessorSourceColor.Blue:
                    color = point.B;
                    break;
            }

            var newPoint = point.Clone();

            newPoint.R = color;
            newPoint.G = color;
            newPoint.B = color;

            yield return newPoint;
        }


        yield break;

    }


}

public enum MonochromeFrameProcessorSourceColor { Red, Green, Blue }
