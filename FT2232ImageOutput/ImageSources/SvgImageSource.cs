using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using ManuPath;
using ManuPath.DotGenerators;
using ManuPath.Figures;
using ManuPath.Svg;
using ManuPath.Transforms;
using FT2232ImageOutput.Utils;
using FT2232ImageOutput.Extensions;

namespace FT2232ImageOutput.ImageSources;


public enum ColorChannel { None, Red, Green, Blue, Alpha, Grayscale }

public class SvgImageSource : IImageSource
{
    private readonly string _filepath;
    private ManuPathImage _svgImage;
    private IFigure[] _figures;

    private IDotGenerator[] _strokeGenerators;
    private readonly Func<IFigure, IDotGenerator> _strokeGeneratorFactory;

    private IDotGenerator[] _fillGenerators;
    private readonly Func<IFigure, IDotGenerator> _fillGeneratorFactory;

    private readonly ColorChannel _strokeIntensityChannel;
    private readonly ColorChannel _strokeBrightnessChannel;
    private readonly ColorChannel _fillIntensityChannel;
    private readonly ColorChannel _fillBrightnessChannel;


    public SvgImageSource(
        string filepath,
        Func<IFigure, IDotGenerator> strokeGeneratorFactory,
        Func<IFigure, IDotGenerator> fillGeneratorFactory,
        ColorChannel strokeIntensityChannel,
        ColorChannel strokeBrightnessChannel,
        ColorChannel fillIntensityChannel,
        ColorChannel fillBrightnessChannel,
        ImageMaxValues maxValues)
    {
        _filepath = System.IO.Path.GetFullPath(filepath);

        _strokeGeneratorFactory = strokeGeneratorFactory;
        _fillGeneratorFactory = fillGeneratorFactory;

        _strokeIntensityChannel = strokeIntensityChannel;
        _strokeBrightnessChannel = strokeBrightnessChannel;

        _fillIntensityChannel = fillIntensityChannel;
        _fillBrightnessChannel = fillBrightnessChannel;

        MaxValues = maxValues;
    }

    public ImageType ImageType => ImageType.Vector;


    public ImageMaxValues MaxValues { get; protected set; }


    public bool Streaming => true;

    public void Init()
    {
        _svgImage = LoadImage(_filepath);

        _figures = TransformFigures(_svgImage.Figures, _svgImage.Size);

        _figures = SortFigures(_figures);

        _strokeGenerators = _figures
            .Where(f => f.Stroke?.Color.A > 0)
            .Select(_strokeGeneratorFactory)
            .ToArray();

        _fillGenerators = _figures
            .Where(f => f.Fill?.Color.A > 0)
            .Select(_fillGeneratorFactory)
            .ToArray();
    }

    public IEnumerable<ImageFrame> GetFrames()
    {
        var points = new List<ImagePoint>();

        var sw = new Stopwatch();
        sw.Start();
        foreach (var strokeGenerator in _strokeGenerators.Concat(_fillGenerators))
        {
            var generatedDots = strokeGenerator.Generate();

            foreach (var generatedDot in generatedDots)
            {
                foreach (var dot in generatedDot.Dots)
                {
                    var imagePoint = ToImagePoint(dot, GetChannelValue(generatedDot.Color));
                    points.Add(imagePoint);
                }
            }

            var lastpoint = points.Last();
            lastpoint.Blanking = true;
            points.Add(lastpoint);
        }
        sw.Stop();

        return new[]
        {
            new ImageFrame()
            {
                Duration = 0,
                Number = 1,
                Points = points.ToArray()
            }
        };

    }

    IFigure[] TransformFigures(IFigure[] figures, Vector2 imageSize)
    {
        var translate = new TranslateTransform(new Vector2(-MaxValues.MinX, -MaxValues.MinY));

        float scaleKoeff = imageSize.X > imageSize.Y
            ? MaxValues.Width / imageSize.X
            : MaxValues.Height / imageSize.Y;

        var scale = new ScaleTransform(new Vector2(scaleKoeff));

        figures = figures
            .Each(f => f.Transforms = f.Transforms.Concat(new ITransform[] { translate, scale }).ToArray())
            .Select(f => f.Transform())
            .ToArray();

        return figures;
    }

    IFigure[] SortFigures(IFigure[] figures)
    {
        var startFigure = figures.First();
        var sortedFigures = new List<IFigure>() { startFigure };
        var unsortedFigures = figures.Skip(1).ToList();

        while (unsortedFigures.Any())
        {
            var sortedLast = sortedFigures.Last().LastPoint;

            var ordered = unsortedFigures.Select(b =>
            {
                var bFirst = b.FirstPoint;
                var bLast = b.LastPoint;

                var distanceStraight = Math.Sqrt(Math.Pow(bFirst.X - sortedLast.X, 2) + Math.Pow(bFirst.Y - sortedLast.Y, 2));

                if (bFirst == bLast)
                {
                    return (path: b, distance: distanceStraight, reverse: false);
                }

                var distanceReverse = Math.Sqrt(Math.Pow(bLast.X - sortedLast.X, 2) + Math.Pow(bLast.Y - sortedLast.Y, 2));

                if (distanceStraight > distanceReverse)
                {
                    return (path: b, distance: distanceStraight, reverse: false);
                }
                else
                {
                    return (path: b, distance: distanceReverse, reverse: true);
                }
            })
            .OrderBy(x => x.distance)
            .ToArray();

            var p = ordered.First();
            if (p.reverse)
            {
                p.path.Reverse();
            }
            var closestPath = p.path;

            sortedFigures.Add(closestPath);
            unsortedFigures.Remove(closestPath);
        }

        return sortedFigures.ToArray();
    }

    ManuPathImage LoadImage(string filepath)
    {
        var image = SvgImageReader.ReadSvgFile(filepath);
        return image;
    }

    byte GetChannelValue(Color color) => _strokeBrightnessChannel switch
    {
        ColorChannel.None => 0,
        ColorChannel.Red => color.R,
        ColorChannel.Green => color.G,
        ColorChannel.Blue => color.B,
        ColorChannel.Alpha => color.A,
        ColorChannel.Grayscale => (byte)(.21f * color.R + .71f * color.G + .071f * color.B),
        _ => throw new ArgumentException("channelZ")
    };


    ImagePoint ToImagePoint(Vector2 p, int z)
    {
        return new ImagePoint()
        {
            Blanking = false,
            X = (int)p.X,
            Y = (int)p.Y,
            Z = MathUtils.ConvertRange(0, 255, MaxValues.MinZ, MaxValues.MaxZ, z),
            R = 255,
            G = 255,
            B = 255
        };
    }


}
