using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace FT2232ImageOutput.PathImages
{

    public class PathImageInfo
    {
        public IEnumerable<ElementInfo> Elements { get; set; }
        public float Height { get; set; }
        public float Width { get; set; }
    }

    public class ElementInfo: ICloneable
    {
        public IEnumerable<IPathPrimitive> Path { get; set; }
        public RectangleF Bounds { get; set; }

        public bool HasFill { get; set; }
        public Color FillColor { get; set; }
        public PathFillRule FillRule { get; set; }

        public bool HasStroke { get; set; }
        public Color StrokeColor { get; set; }

        public object Clone()
        {
            return new ElementInfo()
            {
                Path = Path,
                Bounds = Bounds,

                HasFill = HasFill,
                FillColor = FillColor,
                FillRule = FillRule,

                HasStroke = HasStroke,
                StrokeColor = StrokeColor,
            };
        }
    }

    public class MoveTo : IPathPrimitive { }
    public class ClosePath : IPathPrimitive { }

    public enum PathFillRule { EvenOdd, NonZeroWinding }



    public interface IPathPrimitive { }

    public class Dot : IPathPrimitive
    {
        public Dot(Vector2 pos)
        {
            Pos = pos;
        }

        public Vector2 Pos { get; set; }
    }

    public class Segment : IPathPrimitive
    {
        public Segment(Vector2 p1, Vector2 p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public Vector2 P1 { get; set; }
        public Vector2 P2 { get; set; }

        public bool IsZeroLength() => P1 - P2 == Vector2.Zero;

        public float Length()
        {
            return (float)Math.Sqrt(Math.Pow(P2.X - P1.X, 2) + Math.Pow(P2.Y - P1.Y, 2));
        }
    }


    public class CubicBezier : IPathPrimitive
    {
        public CubicBezier(Vector2 p1, Vector2 p2, Vector2 c1, Vector2 c2)
        {
            P1 = p1;
            P2 = p2;
            C1 = c1;
            C2 = c2;
        }

        public Vector2 P1 { get; set; }
        public Vector2 P2 { get; set; }
        public Vector2 C1 { get; set; }
        public Vector2 C2 { get; set; }
    }

}
