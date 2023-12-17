using DSS.ILDA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.ImageSources;

public class RawNumberImageSource : IImageSource
{
    private readonly int _x;
    private readonly int _y;
    private readonly int _z;

    public RawNumberImageSource(ImageMaxValues maxValues, int x, int y, int z)
    {
        MaxValues = maxValues;
        _x = x;
        _y = y;
        _z = z;
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
        
        list.Add(new ImagePoint()
        {
            Y = _y,
            X = _x,
            Z = _z,

            R = MaxValues.MaxRGB,
            G = MaxValues.MaxRGB,
            B = MaxValues.MaxRGB,

            Blanking = false
        });

        return list;

    }

}
