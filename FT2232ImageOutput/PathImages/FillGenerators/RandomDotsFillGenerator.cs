using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Text;

namespace FT2232ImageOutput.PathImages.FillGenerators
{
    public class RandomDotsFillGenerator : IPrimitiveFillGenerator
    {
        private readonly int _dotCount;
        private readonly bool _dotCountExactly;
        private readonly bool _allowIntersections;

        public RandomDotsFillGenerator(int dotCount, bool dotCountExactly, bool allowIntersections)
        {
            _dotCount = dotCount;
            _dotCountExactly = dotCountExactly;
            _allowIntersections = allowIntersections;
        }



        public ElementInfo GenerateFill(ElementInfo preparedFillPath)
        {
            if (preparedFillPath.Path.Any(p => !(p is Segment)))
                throw new ArgumentException("path must contain only Segments");

            var res = new List<Vector2>();

            var random = new Random(DateTime.Now.Millisecond);

            Vector2? p1 = null;

            int c = 0;

            for (int i = 0; i < _dotCount; i++)
            {
                var p = new Vector2();

                do
                {
                    p.X = (float)random.NextDouble() * preparedFillPath.Bounds.Right;
                    p.Y = (float)random.NextDouble() * preparedFillPath.Bounds.Bottom;

                    if (PathMath.IsPointInPolygon(preparedFillPath.Path.OfType<Segment>(), p, preparedFillPath.FillRule))
                    {
                        if (!p1.HasValue)
                            p1 = p;


                        if (_allowIntersections)
                        {
                            c++;
                            res.Add(p);
                            p1 = p;
                            break;
                        }
                        else
                        {
                            if (!PathMath.IsSegmentIntersectsWithPoly(preparedFillPath.Path.OfType<Segment>(), new Segment(p1.Value, p)))
                            {
                                c++;
                                res.Add(p);
                                p1 = p;
                                break;
                            }
                            else
                                continue;

                        }

                    }

                } while (_dotCountExactly);

            }


            return new ElementInfo()
            {
                Bounds = preparedFillPath.Bounds,
                HasStroke = true,
                StrokeColor = preparedFillPath.StrokeColor,
                Path = res.Select(v => new Dot(v)).ToArray()
            };

        }

    }
}
