using DSS.ILDA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.ImageSources;

public class PointsImageSource : IImageSource
{
    private readonly int _count;

    public PointsImageSource(ImageMaxValues maxValues, int count)
    {
        MaxValues = maxValues;
        _count = count;
    }


    public ImageType ImageType => ImageType.Vector;

    public bool Streaming => false;



    public ImageMaxValues MaxValues { get; }



    public IEnumerable<ImageFrame> GetFrames()
    {

        var frames = new List<ImageFrame>();
        

        frames.Add(new ImageFrame()
        {
            Duration = -1,
            Number = 1,
            Points = GetPoints()
        });

        return frames;

    }

    IEnumerable<ImagePoint> GetPoints()
    {

        for (int i = 0; i < _count; i++)
        {
            if ((i % 2) == 1)
            {
                yield return new ImagePoint()
                {
                    X = MaxValues.MinX,
                    Y = MaxValues.MinY,
                    Z = MaxValues.MaxZ,

                    R = MaxValues.MaxRGB,
                    G = MaxValues.MaxRGB,
                    B = MaxValues.MaxRGB,

                    Blanking = false
                };
            }
            else
            {

                yield return new ImagePoint()
                {
                    X = MaxValues.MaxX,
                    Y = MaxValues.MaxY,
                    Z = MaxValues.MaxZ,

                    R = MaxValues.MaxRGB,
                    G = MaxValues.MaxRGB,
                    B = MaxValues.MaxRGB,

                    Blanking = false
                };
            }
        }
        yield break;


    }

}
