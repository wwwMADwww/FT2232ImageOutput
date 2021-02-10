using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using Svg;
using Svg.Pathing;

namespace FT2232ImageOutput.PathImages.ImageSources
{
    public class SvgFilePathSource
    {
        private readonly string _filename;

        public SvgFilePathSource(string filename)
        {
            _filename = filename;
        }


        public IEnumerable<PathImageInfo> ReadSvg()
        {
            // TODO: multiple frames

            var res = new List<ElementInfo>();

            var svg = SvgDocument.Open(_filename);


            var elements = svg.Children.FindSvgElementsOf<SvgGroup>()
                .SelectMany(g => g.Children.FindSvgElementsOf<SvgPath>())
                .ToArray();


            foreach (var e in elements)
            {
                var r = new List<IPathPrimitive>();

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

                        r.Add(p);
                    }
                    else if (path is SvgLineSegment line)
                    {
                        var p = new Segment(new Vector2(line.Start.X, line.Start.Y), new Vector2(line.End.X, line.End.Y));
                        r.Add(p);
                    }
                    else if (path is SvgMoveToSegment move)
                    {
                        r.Add(new MoveTo());
                    }
                    else if (path is SvgClosePathSegment closePath)
                    {
                        r.Add(new ClosePath());
                    }
                    else
                    {
                        Console.WriteLine($"path type '{path.GetType().Name}' not supported yet");
                    }
                }

                var fillColor = ((SvgColourServer)e.Fill).Colour;
                var strokeColor = ((SvgColourServer)e.Stroke).Colour;

                res.Add(new ElementInfo()
                {
                    Path = r,

                    Bounds = e.Bounds,

                    HasFill = e.Fill != SvgPaintServer.None,
                    FillRule = e.FillRule == SvgFillRule.EvenOdd
                        ? PathFillRule.EvenOdd
                        : PathFillRule.NonZeroWinding,

                    FillColor = Color.FromArgb((byte)(255 * e.FillOpacity), fillColor.R, fillColor.G, fillColor.B),
                    HasStroke = e.Stroke != SvgPaintServer.None,
                    StrokeColor = Color.FromArgb((byte)(255 * e.StrokeOpacity), strokeColor.R, strokeColor.G, strokeColor.B)

                });

            }


            return new[] {new PathImageInfo()
            {
                Elements = res,
                Height = svg.ViewBox.Height,
                Width = svg.ViewBox.Width
            }
            };
        }


    }
}
