using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FT2232ImageOutput.Utils;
using WolframFourierArtParser;

namespace FT2232ImageOutput.ImageSources;


public class WolframFourierArtImageSource : IImageSource
{
    private readonly string _filePath;

    private readonly float _argumentIncrement;
    private readonly float _dotDistanceMin;
    private readonly float _dotDistanceMax;

    private ImageFrame[] _frames;

    public WolframFourierArtImageSource(
        string filePath, 
        float argumentIncrement,
        float dotDistanceMin,
        float dotDistanceMax,
        ImageMaxValues maxValues)
    {            
        _filePath = Path.GetFullPath(filePath);

        if (!File.Exists(_filePath))
            throw new FileNotFoundException($"File '{_filePath}' does not exists.");

        if (Directory.Exists(_filePath))
            throw new ArgumentException($"Path '{_filePath}' is a directory.");

        _argumentIncrement = argumentIncrement;
        _dotDistanceMin = dotDistanceMin;
        _dotDistanceMax = dotDistanceMax;
        MaxValues = maxValues;
    }


    public ImageType ImageType => ImageType.Vector;

    public bool Streaming => false;

    public ImageMaxValues MaxValues { get; set; }

    public void Init()
    {
        var isJson = Path.GetExtension(_filePath).ToLower() == ".json";

        FourierSeries[] series;

        if (isJson)
        {
            using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
            series = JsonSerializer.Deserialize<FourierSeries[]>(fs);
        }
        else
        {
            series = new FourierSeriesParser().ParseFile(_filePath);
        }

        var funcs = series.Select(s => new FourierFunc(s)).ToArray();

        var minX = double.PositiveInfinity;
        var maxX = double.NegativeInfinity;
                   
        var minY = double.PositiveInfinity;
        var maxY = double.NegativeInfinity;

        for (double t = 0.0; t <= 1.0; t += _argumentIncrement)
        {
            foreach (var func in funcs)
            {
                var v = func.Calculate(t);

                if (!v.HasValue) continue;

                if (!double.IsNaN(v.Value.X))
                { 
                    minX = Math.Min(minX, v.Value.X);
                    maxX = Math.Max(maxX, v.Value.X);
                }

                if (!double.IsNaN(v.Value.Y))
                {
                    minY = Math.Min(minY, v.Value.Y);
                    maxY = Math.Max(maxY, v.Value.Y);
                }
            }
        }

        var koeff = MathUtils.AspectRatio(maxX - minX, maxY - minY);

        var framePoints = new List<ImagePoint>();

        Vector2? lastPoint = null;

        foreach (var func in funcs)
        {
            var coords = ManuPath.Maths.CommonMath.CurveToEquidistantDots(
                0.0f, 1.0f,
                _dotDistanceMin, _dotDistanceMax,
                _argumentIncrement,
                lastPoint,
                t => {
                    var v = func.Calculate(t);
                    return v.HasValue
                        ? new Vector2(
                            (float)MathUtils.ConvertRange(minX, maxX, MaxValues.MinX, MaxValues.MaxX * koeff.x, v.Value.X), 
                            (float)MathUtils.ConvertRange(minY, maxY, MaxValues.MinY, MaxValues.MaxY * koeff.y, v.Value.Y))
                        : null;
                } );

            if (coords.Length == 0) continue;

            foreach (var coord in coords)
            {
                var point = new ImagePoint()
                {
                    X = (int) coord.X,
                    Y = (int) coord.Y,
                    Z = MaxValues.MaxZ,

                    R = MaxValues.MaxRGB,
                    G = MaxValues.MaxRGB,
                    B = MaxValues.MaxRGB,

                    Blanking = false
                };

                framePoints.Add(point);
            }

            lastPoint = coords[^1];

            var blank = framePoints.Last().Clone();
            blank.Blanking = true;
            framePoints.Add(blank);
        }


        _frames = new[] { new ImageFrame()
        {
            Duration = -1,
            Number = 1,
            Points = framePoints.ToArray()
        } };
    }

    public IEnumerable<ImageFrame> GetFrames()
    {
        return _frames;
    }

}
