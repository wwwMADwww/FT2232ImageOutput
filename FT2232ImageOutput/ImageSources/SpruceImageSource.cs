using DSS.ILDA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.ImageSources;

public class SpruceImageSource : IImageSource
{
    public SpruceImageSource(ImageMaxValues maxValues)
    {
        MaxValues = maxValues;
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
        var list = new List<ImagePoint>();

        var xcenter = (MaxValues.MaxX - MaxValues.MinX) / 2;
        var ycenter = (MaxValues.MaxY - MaxValues.MinY) / 2;

        for (int i = MaxValues.MinX; i <= MaxValues.MaxX; i++)
        {

            if (i % 2 == 0)
            {

                list.Add(new ImagePoint()
                {
                    X = i,
                    Y = ycenter,
                    // X = xcenter,
                    // Y = i,
                    Z = MaxValues.MaxZ,

                    R = MaxValues.MaxRGB,
                    G = MaxValues.MaxRGB,
                    B = MaxValues.MaxRGB,

                    Blanking = false
                });

            }
            else
            {

                list.Add(new ImagePoint()
                {
                    X = i,
                    Y = ycenter + (i / 2),
                    // X = xcenter + (i / 2),
                    // Y = i,
                    Z = MaxValues.MaxZ,

                    R = MaxValues.MaxRGB,
                    G = MaxValues.MaxRGB,
                    B = MaxValues.MaxRGB,

                    Blanking = false
                });


                list.Add(new ImagePoint()
                {
                    X = i,
                    Y = ycenter - (i / 2),
                    // X = xcenter - (i / 2),
                    // Y = i,
                    Z = MaxValues.MaxZ,

                    R = MaxValues.MaxRGB,
                    G = MaxValues.MaxRGB,
                    B = MaxValues.MaxRGB,

                    Blanking = false
                });

            }

        }

        return list;

    }

}
