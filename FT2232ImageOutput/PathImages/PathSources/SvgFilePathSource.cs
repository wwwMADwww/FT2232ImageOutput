using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Svg;
using Svg.Pathing;
using ManuPath;

namespace FT2232ImageOutput.PathImages.PathSources;

public class SvgFilePathSource: IPathSource
{
    private readonly string _filename;

    private readonly SvgDocument _svgDocument;

    Path[][] _frames = null;


    public SvgFilePathSource(string filename)
    {
        _filename = filename;
        _svgDocument = SvgDocument.Open(_filename);
        ReadFrames();
    }

    public Vector2 Size => new Vector2(_svgDocument.ViewBox.Width, _svgDocument.ViewBox.Height);

    public bool Streaming => false;

    public IEnumerable<IEnumerable<Path>> GetFrames()
    {

         return _frames;

    }

    void ReadFrames()
    {
        var paths = new List<Path>();

        // TODO: multiple frames

        var elements = _svgDocument.Children.FindSvgElementsOf<SvgGroup>() // layers, groups
            .SelectMany(g => g.Children.FindSvgElementsOf<SvgPath>()) // paths
            .ToArray();


        foreach (var e in elements)
        {
            var primitives = new List<IPathPrimitive>();

            foreach (var path in e.PathData)
            {
                if (path is SvgCubicCurveSegment cb)
                {
                    var p = new CubicBezier(
                        new Vector2(cb.Start.X, cb.Start.Y),
                        new Vector2(cb.End.X, cb.End.Y),
                        new Vector2(cb.FirstControlPoint.X, cb.FirstControlPoint.Y),
                        new Vector2(cb.SecondControlPoint.X, cb.SecondControlPoint.Y)
                        );

                    primitives.Add(p);
                }
                else if (path is SvgLineSegment line)
                {
                    var p = new Segment(new Vector2(line.Start.X, line.Start.Y), new Vector2(line.End.X, line.End.Y));
                    primitives.Add(p);
                }
                else if (path is SvgMoveToSegment move)
                {

                }
                else if (path is SvgClosePathSegment closePath)
                {
                    if (!primitives.Any()) // occurs when path is not line or bezier
                        continue;
                    var p = new Segment(primitives.Last().LastPoint, primitives.First().FirstPoint);
                    primitives.Add(p);
                }
                else
                {
                    Console.WriteLine($"path type '{path.GetType().Name}' not supported. svg element id {e.ID}");
                }
            }

            var fillColor = ((SvgColourServer)e.Fill).Colour;
            var strokeColor = ((SvgColourServer)e.Stroke).Colour;

            paths.Add(new Path()
            {
                Id = e.ID,

                Primitives = primitives,

                FillRule = e.FillRule == SvgFillRule.EvenOdd
                    ? PathFillRule.EvenOdd
                    : PathFillRule.NonZeroWinding,

                FillColor = e.Fill != SvgPaintServer.None
                    ? Color.FromArgb((byte)(255 * e.FillOpacity), fillColor.R, fillColor.G, fillColor.B)
                    : (Color?)null,

                StrokeColor = e.Stroke != SvgPaintServer.None
                    ? Color.FromArgb((byte)(255 * e.StrokeOpacity), strokeColor.R, strokeColor.G, strokeColor.B)
                    : (Color?)null,

            });

        }

        // new Vector2(svg.ViewBox.Width, svg.ViewBox.Height)

        _frames = new Path[][] { paths.ToArray() };
    }

}
