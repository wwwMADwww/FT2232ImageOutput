using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace FT2232ImageOutput.PathImages.FillGenerators
{
    public class IntervalDotsFillGenerator: IPrimitiveFillGenerator
    {
        private readonly int _intervalMin;
        private readonly int _intervalMax;
        private readonly bool _randomShift;
        private readonly bool _invert;

        public IntervalDotsFillGenerator(int intervalMin, int intervalMax, bool invert, bool randomShift)
        {
            _intervalMin = intervalMin;
            _intervalMax = intervalMax;
            _randomShift = randomShift;
            _invert = invert;
        }


        public ElementInfo GenerateFill(ElementInfo filledPoly)
        {
            if (filledPoly.Path.Any(p => !(p is Segment)))
                throw new ArgumentException("path must contain only Segments");

            var res = new List<Vector2>();

            var random = new Random();

            var intensity = _invert
                ? 255 - filledPoly.FillColor.A
                : filledPoly.FillColor.A;

            var interval = (int)(_intervalMin + (_intervalMax - ((_intervalMax - _intervalMin) * (intensity / 255f))));

            for (var x = filledPoly.Bounds.Left; x < filledPoly.Bounds.Right; x += interval)
            {
                for (var y = filledPoly.Bounds.Top; y < filledPoly.Bounds.Bottom; y += interval)
                {
                    var sx = _randomShift
                        ? (float)random.NextDouble() * interval
                        : 0;

                    var sy = _randomShift
                        ? (float)random.NextDouble() * interval
                        : 0;


                    var p = new Vector2(x + sx, y + sy);

                    if (PathMath.IsPointInPolygon(filledPoly.Path.OfType<Segment>(), p, filledPoly.FillRule))
                        res.Add(p);
                }
            }

            return new ElementInfo()
            {
                Bounds = filledPoly.Bounds,
                HasStroke = true,
                StrokeColor = filledPoly.FillColor,
                Path = res.Select(v => new Dot(v)).ToArray()
            };

        }

    }
}
