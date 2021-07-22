using FT2232ImageOutput.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;

namespace FT2232ImageOutput.PathImages
{
    public static class PathUtils
    {


        public static PathImageInfo ScalePathImage(PathImageInfo image, Vector2 scale, Vector2 shift)
        {

            var res = new List<ElementInfo>();

            foreach (var e in image.Elements)
            {
                var r = new List<IPathPrimitive>();

                foreach (var path in e.Path)
                {
                    if (path is CubicBezier cb)
                    {
                        var p = new CubicBezier(
                            PathMath.ScaleAndShiftV2(cb.P1, scale, shift),
                            PathMath.ScaleAndShiftV2(cb.P2, scale, shift),
                            PathMath.ScaleAndShiftV2(cb.C1, scale, shift),
                            PathMath.ScaleAndShiftV2(cb.C2, scale, shift)
                            );

                        r.Add(p);
                    }
                    else if (path is Segment line)
                    {
                        var p = new Segment(
                            PathMath.ScaleAndShiftV2(line.P1, scale, shift),
                            PathMath.ScaleAndShiftV2(line.P2, scale, shift)
                            );
                        r.Add(p);
                    }
                    else if (path is MoveTo move)
                    {
                        r.Add(move);
                    }
                    else if (path is ClosePath closePath)
                    {
                        r.Add(closePath);
                    }
                    else
                    {
                        Console.WriteLine($"path type '{path.GetType().Name}' not supported yet");
                    }
                }


                res.Add(new ElementInfo()
                {
                    Path = r,

                    Bounds = PathMath.ScaleAndShiftRect(e.Bounds, scale, shift),

                    HasFill = e.HasFill,
                    FillRule = e.FillRule,

                    FillColor = e.FillColor,
                    HasStroke = e.HasStroke,
                    StrokeColor = e.StrokeColor

                });

            }

            var imageSize = PathMath.ScaleAndShiftV2(new Vector2(image.Width, image.Height), scale, shift);

            return new PathImageInfo()
            {
                Elements = res,
                Height = imageSize.X,
                Width = imageSize.X
            };
        }



        public static IEnumerable<Dot> PathToDots(
            ElementInfo path,
            Vector2 scale, Vector2 shift,
            float distanceMin, float distanceMax
            )
        {
            var dotPath = new List<Vector2>();

            Vector2? pathStart = null;
            Vector2? lastPoint = null;

            foreach (var primitive in path.Path)
            {
                IEnumerable<Vector2> dots = null;

                if (primitive is Segment s)
                {
                    dots = PathMath.SegmentDivideToSpecificLength(s, distanceMin);

                    if (!pathStart.HasValue)
                        pathStart = s.P1;
                    lastPoint = s.P2;
                }
                else if (primitive is CubicBezier cb)
                {
                    dots = PathMath.CubicBezierApproxWithConstDistance(cb, distanceMin, distanceMax, 0.1f, 2f, 1.2f);

                    if (!pathStart.HasValue)
                        pathStart = cb.P1;
                    lastPoint = cb.P2;
                }
                else if (primitive is MoveTo moveto)
                {

                }
                else if (primitive is ClosePath closePath)
                {
                    if (pathStart.HasValue && lastPoint.HasValue)
                    {
                        var seg = new Segment(lastPoint.Value, pathStart.Value);

                        if (seg.Length() > distanceMin)
                            dots = PathMath.SegmentDivideToSpecificLength(seg, distanceMin);
                        else
                            dots = new[] { lastPoint.Value, pathStart.Value };
                        lastPoint = null;
                        pathStart = null;
                    }
                }
                else
                {
                    throw new Exception($"primitive type is {primitive.GetType().Name}");
                }

                if (dots?.Any() ?? false)
                {
                    // var dotlist = dots.ToList();
                    //dotlist.RemoveAt(dotlist.Count - 1);

                    dotPath.AddRange(dots);
                }
            }


            return dotPath.Select(v => new Dot(v)).ToArray();
        }



        public static IEnumerable<Segment> PathToSegments(
            ElementInfo elementInfo,
            Vector2 scale, Vector2 shift,
            float distanceMin, float distanceMax
            )
        {
            var segmentPath = new List<Segment>();

            Vector2? pathStart = null;
            Vector2? lastPoint = null;

            foreach (var primitive in elementInfo.Path)
            {
                IEnumerable<Vector2> dots = null;

                if (primitive is Segment s)
                {
                    dots = new[] { s.P1, s.P2 };

                    if (!pathStart.HasValue)
                        pathStart = s.P1;
                    lastPoint = s.P2;
                }
                else if (primitive is CubicBezier cb)
                {
                    dots = PathMath.CubicBezierApproxWithConstDistance(cb, distanceMin, distanceMax, 0.1f, 2f, 1.2f);

                    if (!pathStart.HasValue)
                        pathStart = cb.P1;
                    lastPoint = cb.P2;
                }
                else if (primitive is MoveTo lineBreak)
                {
                    if (pathStart.HasValue && lastPoint.HasValue)
                    {
                        dots = new[] { lastPoint.Value, pathStart.Value };
                        lastPoint = null;
                        pathStart = null;
                    }
                }
                else if (primitive is ClosePath closePath)
                {
                    if (pathStart.HasValue && lastPoint.HasValue)
                    {
                        dots = new[] { lastPoint.Value, pathStart.Value };
                        lastPoint = null;
                        pathStart = null;
                    }
                }
                else
                {
                    throw new Exception($"primitive type is {primitive.GetType().Name}");
                }

                if (!dots?.Any() ?? true)
                    continue;

                var segs = new List<Segment>();

                Vector2? p1 = null;

                foreach (var dot in dots)
                {
                    if (p1.HasValue)
                        segs.Add(new Segment(p1.Value, dot));

                    p1 = dot;
                }

                segmentPath.AddRange(segs);
            }

            if (pathStart.HasValue && lastPoint.HasValue)
            {
                var seg = new Segment(lastPoint.Value, pathStart.Value);

                segmentPath.Add(seg);
                lastPoint = null;
                pathStart = null;
            }

            return segmentPath.ToArray();

        }



    }

}
