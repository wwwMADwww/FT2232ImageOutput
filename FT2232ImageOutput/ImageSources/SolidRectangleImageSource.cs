using DSS.ILDA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.ImageSources;

public class SolidRectangleImageSource : IImageSource
{
    public SolidRectangleImageSource(ImageMaxValues maxValues)
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
        // {
        //
        //    new ImagePoint()
        //    {
        //        X = MaxValues.MinX,
        //        Y = MaxValues.MinY,
        //        Z = MaxValues.MaxZ,
        //
        //        R = MaxValues.MaxRGB,
        //        G = MaxValues.MaxRGB,
        //        B = MaxValues.MaxRGB,
        //
        //        Blanking = false
        //    },
        //
        //    new ImagePoint()
        //    {
        //        X = MaxValues.MaxX,
        //        Y = MaxValues.MinY,
        //        Z = MaxValues.MaxZ,
        //
        //        R = MaxValues.MaxRGB,
        //        G = MaxValues.MaxRGB,
        //        B = MaxValues.MaxRGB,
        //
        //        Blanking = false
        //    },
        //
        //
        //    new ImagePoint()
        //    {
        //        X = MaxValues.MaxX,
        //        Y = MaxValues.MaxY,
        //        Z = MaxValues.MaxZ,
        //
        //        R = MaxValues.MaxRGB,
        //        G = MaxValues.MaxRGB,
        //        B = MaxValues.MaxRGB,
        //
        //        Blanking = false
        //    },
        //
        //
        //    new ImagePoint()
        //    {
        //        X = MaxValues.MinX,
        //        Y = MaxValues.MaxY,
        //        Z = MaxValues.MaxZ,
        //
        //        R = MaxValues.MaxRGB,
        //        G = MaxValues.MaxRGB,
        //        B = MaxValues.MaxRGB,
        //
        //        Blanking = false
        //    }
        //};

        for (int y = MaxValues.MinY; y <= MaxValues.MaxY; y++)
        {
            for (int x = MaxValues.MinX; x <= MaxValues.MaxX; x++)
            {
                var point = new ImagePoint()
                {
                    X = x,
                    Y = y,
                    Z = MaxValues.MaxZ,
        
                    R = MaxValues.MaxRGB,
                    G = MaxValues.MaxRGB,
                    B = MaxValues.MaxRGB,
        
                    Blanking = y >= MaxValues.MaxY - 2 && x >= MaxValues.MaxX - 2
                };
        
        
                list.Add(point);
        
            }
        }

        return list;

    }

}
