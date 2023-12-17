using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput;

public interface IImageSource
{
    IEnumerable<ImageFrame> GetFrames();

    ImageType ImageType { get; }

    ImageMaxValues MaxValues { get; }

    bool Streaming { get; }

}

public enum ImageType { Vector, Raster }

public class ImageMaxValues
{ 
    public int MinX { get; set; }
    public int MaxX { get; set; }

    public int MinY { get; set; }
    public int MaxY { get; set; }

    public int MinZ { get; set; }
    public int MaxZ { get; set; }

    public int MaxRGB { get; set; }


    public int Width  => MaxX - MinX;
    public int Height => MaxY - MinY;


    public int CenterX => MinX + (Width / 2);
    public int CenterY => MinY + (Height / 2);

}
