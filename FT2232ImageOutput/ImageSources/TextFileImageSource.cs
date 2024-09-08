using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FT2232ImageOutput.Utils;

namespace FT2232ImageOutput.ImageSources;


public class TextFileImageSource : IImageSource
{
    private readonly string _filePath;
    private List<ImageFrame> _frames;

    public TextFileImageSource(string filePath, ImageMaxValues maxValues)
    {            
        _filePath = Path.GetFullPath(filePath);

        if (!File.Exists(_filePath))
            throw new FileNotFoundException($"File '{_filePath}' does not exists.");

        if (Directory.Exists(_filePath))
            throw new ArgumentException($"Path '{_filePath}' is a directory.");

        MaxValues = maxValues;
    }


    public ImageType ImageType => ImageType.Vector;

    public bool Streaming => false;

    public ImageMaxValues MaxValues { get; set; }

    public void Init()
    { 
        // col delimiter \t
        // row delimiter \n or \r\n

        var coordList = new List<Vector2>();

        using (var sr = new StreamReader(new FileStream(_filePath, FileMode.Open, FileAccess.Read)))
        {
            while (true)
            {
                var line = sr.ReadLine();

                if (line == null) break;

                var coords = line.Split('\t');

                var x = float.Parse(coords[0]);
                var y = float.Parse(coords[1]);

                coordList.Add(new Vector2(x, y));
            }
        }

        _frames = new List<ImageFrame>();

        var framePoints = new List<ImagePoint>();

        var minX = coordList.MinBy(v => v.X).X;
        var maxX = coordList.MaxBy(v => v.X).X;

        var minY = coordList.MinBy(v => v.Y).Y;
        var maxY = coordList.MaxBy(v => v.Y).Y;

        var koeff = MathUtils.AspectRatio(maxX - minX, maxY - minY);

        foreach (var coord in coordList)
        { 
            var point = new ImagePoint()
            {
                X = (int)MathUtils.ConvertRange(minX, maxX, MaxValues.MinX, MaxValues.MaxX * koeff.x, coord.X),
                Y = (int)MathUtils.ConvertRange(minY, maxY, MaxValues.MinY, MaxValues.MaxY * koeff.y, coord.Y),
                Z = MaxValues.MaxZ,

                R = MaxValues.MaxRGB,
                G = MaxValues.MaxRGB,
                B = MaxValues.MaxRGB,

                Blanking = false
            };

            framePoints.Add(point);
        }

        _frames.Add(new ImageFrame()
        {
            Duration = -1,
            Number = 1,
            Points = framePoints.ToArray()
        });
        
    }

    public IEnumerable<ImageFrame> GetFrames()
    {
        return _frames;
    }

}
