using FT2232ImageOutput.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace FT2232ImageOutput.PathImages
{
    public static class PathMath
    {


        public static List<Vector2> CubicBezierApproxWithConstDistance(
            CubicBezier bc,
            float pointDistanceMin, float pointDistanceMax,
            float initialDeltaT, float initialDeltaTKoeff, float deltaTKoeff2)
        {
            // TODO: decompose to ICurveApproximator or something

            var res = new List<Vector2>();

            // debug, statistics
            // var repeatEvent = 0;
            // var repeatTotal = 0;
            // var repeatMax = 0;

            res.Add(bc.P1);

            var oldx = bc.P1.X;
            var oldy = bc.P1.Y;


            var t = 0f;
            var dt = initialDeltaT;


            while (true)
            {

                var x = 0f;
                var y = 0f;

                var prevDiv = false;
                var prevMul = false;

                var dtkoeff2 = deltaTKoeff2;
                var dtkoeff = initialDeltaTKoeff;

                // var repeat = -1;

                // перебор значений t так, чтобы расстояние между точками было не менее r1 и не более r2
                while (true)
                {
                    // repeat++;

                    var t2 = t + dt;

                    if (t2 >= 1.0f)
                        t2 = 1.0f;

                    x = BezierCoordX(t2, bc);
                    y = BezierCoordY(t2, bc);

                    var d = MathUtils.Distance(oldx, oldy, x, y);

                    if (d < pointDistanceMin)
                    {
                        // если отрезок слишком короткий, но мы уже уперлись в последнюю точку, то пропускаем действие.
                        if (t2 < 1.0f)
                        {
                            if (prevDiv)
                                dtkoeff /= dtkoeff2;

                            dt *= dtkoeff;
                            prevMul = true;
                            prevDiv = false;

                            continue;
                        }
                    }
                    else if (d > pointDistanceMax)
                    {
                        if (prevMul)
                            dtkoeff *= dtkoeff2;

                        dt /= dtkoeff;
                        prevDiv = true;
                        prevMul = false;

                        continue;
                    }

                    t = t2;
                    // Console.WriteLine($"t {t}, d {d}");

                    // x4 less repeatTotal and x2 less repeatEvent when disabled
                    // dtkoeff = initialDeltaTKoeff;
                    // dtkoeff2 = deltaTKoeff2;
                    // dt = initialDeltaT;

                    prevDiv = prevMul = false;
                    break;

                } // while repeat


                // if (repeat > 0)
                // {
                //     repeatEvent++;
                //     repeatTotal += repeat;
                //     if (repeatMax < repeat)
                //         repeatMax = repeat;
                // }

                res.Add(new Vector2(x, y));

                oldx = x;
                oldy = y;

                if (t >= 1)
                    break;
            } // while elements

            res.Add(bc.P2);


            return res;
        }



        public static IEnumerable<Vector2> CubicBezierApproxWithN(CubicBezier cb, int n)
        {
            // TODO: decompose to ICurveApproximator or something

            var res = new List<Vector2>();

            for (var t = 0f; t <= 1.0f; t += 1.0f / n)
            {
                var x = BezierCoordX(t, cb);
                var y = BezierCoordY(t, cb);

                res.Add(new Vector2(x, y));

            }

            return res;
        }


        public static float BezierCoordX(float t, CubicBezier bc) => BezierCoord(t, bc.P1.X, bc.C1.X, bc.C2.X, bc.P2.X);
        public static float BezierCoordY(float t, CubicBezier bc) => BezierCoord(t, bc.P1.Y, bc.C1.Y, bc.C2.Y, bc.P2.Y);

        public static float BezierCoord(float t, float i0, float i1, float i2, float i3)
        {
            return (float)(
                i0 * Math.Pow((1 - t), 3) +
                i1 * 3 * t * Math.Pow((1 - t), 2) +
                i2 * 3 * Math.Pow(t, 2) * (1 - t) +
                i3 * Math.Pow(t, 3)
            );
        }




        public static IEnumerable<Vector2> SegmentDivideToSpecificLength(Segment s, float pointDistanceMin)
        {
            var res = new List<Vector2>();


            var segmentCount = (float)Math.Floor(s.Length() / pointDistanceMin);

            var dx = (s.P2.X - s.P1.X) / segmentCount;
            var dy = (s.P2.Y - s.P1.Y) / segmentCount;

            for (int i = 0; i <= segmentCount; i++)
                res.Add(new Vector2(s.P1.X + dx * i, s.P1.Y + dy * i));

            return res.ToArray();
        }


        public static IEnumerable<Vector2> SegmentDivideToN(Segment s, int n)
        {
            var res = new List<Vector2>();

            var dx = (s.P2.X - s.P1.X) / n;
            var dy = (s.P2.Y - s.P1.Y) / n;

            for (int i = 0; i <= n; i++)
                res.Add(new Vector2(s.P1.X + dx * i, s.P1.Y + dy * i));

            return res.ToArray();
        }




        public static bool IsPointInPolygon(IEnumerable<Segment> polygon, Vector2 point, PathFillRule rule)
        {

            int count = 0;

            foreach (var s in polygon)
            {
                if (s.IsZeroLength())
                    continue;

                // var (_, intersects, _) = GetIntersectionPoint(v1, v2, point, new Vector2f(vertexArray.Bounds.Left + vertexArray.Bounds.Width + 10, point.Y));
                var intersects = IsRightRayIntersectsWithLine(s, point); // faster
                if (intersects)
                {
                    switch (rule)
                    {
                        case PathFillRule.EvenOdd:
                            count++;
                            break;

                        case PathFillRule.NonZeroWinding:

                            // casting ray from point to right
                            // on clockwise - line goes down
                            // on counter clockwise - line goes down

                            var dy = s.P2.Y - s.P1.Y;

                            if (dy < 0)
                                count--; // clockwise
                            else if (dy > 0)
                                count++; // counter clockwise

                            // on dy == 0 line is parallel to ray, does not count

                            break;

                    }
                }

            }


            return rule switch
            {
                PathFillRule.EvenOdd => count % 2 == 1,
                PathFillRule.NonZeroWinding => count != 0,
                _ => throw new Exception("dafuq r u doin?"),
            };
        }



        public static bool IsSegmentIntersectsWithPoly(IEnumerable<Segment> polygon, Segment s)
        {

            if (s.IsZeroLength())
                return false;

            foreach (var ps in polygon)
            {

                if (ps.IsZeroLength())
                    continue;

                if (GetIntersectionPoint(ps, s).intersects)
                    return true;

            }

            return false;

        }



        public static bool IsRightRayIntersectsWithLine(Segment s, Vector2 rayStart)
        {
            // casting ray from rayStart to right

            //  lineX1  lineX2
            //    v?.x  v?.x
            //     .     .
            //  P--.--P--.-P-> miss 1  
            // - - + - - + - - lineY1, v?.y
            //     .    /.    
            //     . P-+-.---> cross 2
            //     .  /  . P-> miss 2   
            //  P--.-+---.---> cross 1
            //     ./  P-.---> miss 3
            // - - + - - + - - lineY2, v?.y
            //  P--.--P--.-P-> miss 1  
            //     .     .

            var lineY1 = Math.Min(s.P1.Y, s.P2.Y);
            var lineY2 = Math.Max(s.P1.Y, s.P2.Y);


            var lineX1 = Math.Min(s.P1.X, s.P2.X);
            var lineX2 = Math.Max(s.P1.X, s.P2.X);


            if (lineY1 <= rayStart.Y && rayStart.Y <= lineY2)
            {
                if (rayStart.X <= lineX1)
                {
                    // cross 1
                    return true;
                }
                else if (rayStart.X > lineX2)
                {
                    // miss 2
                    return false;
                }
                else
                {
                    var k = (s.P2.Y - s.P1.Y) / (rayStart.Y - s.P1.Y);
                    var lx = (s.P2.X - s.P1.X) / k + s.P1.X;

                    if (rayStart.X <= lx)
                    {
                        // cross 2
                        return true;
                    }
                    else
                    {
                        // miss 3
                        return false;
                    }
                }
            }
            else
            {
                // miss 1
                return false;
            }
        }



        public static (Vector2? point, bool intersects, bool parallel) GetIntersectionPoint(Segment s1, Segment s2)
        {

            static bool IsRangesOverlapping(float r1start, float r1end, float r2start, float r2end)
            {
                if (r1start > r1end) (r1start, r1end) = (r1end, r1start);
                if (r2start > r2end) (r2start, r2end) = (r2end, r2start);

                return (Math.Max(r1start, r2start) - Math.Min(r1end, r2end)) <= 0;
            }

            static bool IsPointInRange(float start, float end, float p)
            {
                return (start <= end)
                    ? start <= p && p <= end
                    : start >= p && p >= end;
            }

            static bool IsPointOnBothSegments(Segment s1, Segment s2, Vector2 p)
            {
                return
                    (IsPointInRange(s1.P1.X, s1.P2.X, p.X) && IsPointInRange(s1.P1.Y, s1.P2.Y, p.Y)) &&
                    (IsPointInRange(s2.P1.X, s2.P2.X, p.X) && IsPointInRange(s2.P1.Y, s2.P2.Y, p.Y));
            }



            var vaVertical = s1.P1.X == s1.P2.X;
            var vbVertical = s2.P1.X == s2.P2.X;

            var kb = vbVertical ? 0 : (s2.P2.Y - s2.P1.Y) / (s2.P2.X - s2.P1.X);
            var ba = s2.P1.Y - kb * s2.P1.X;

            var ka = vaVertical ? 0 : (s1.P2.Y - s1.P1.Y) / (s1.P2.X - s1.P1.X);
            var bb = s1.P1.Y - ka * s1.P1.X;

            if (vaVertical && vbVertical) // both vertical hence parallel and maybe on same line
            {
                if (s1.P1.X == s2.P1.X)
                    return (null, IsRangesOverlapping(s1.P1.X, s1.P2.X, s2.P1.X, s2.P2.X), true); // on same line. check overlapping
                else
                    return (null, false, true); // just parallel
            }



            if (s1.P1.Y == s1.P2.Y && s2.P1.Y == s2.P2.Y) // both horizontal hence parallel and maybe on same line
            {
                if (s1.P1.Y == s2.P1.Y)
                    return (null, IsRangesOverlapping(s1.P1.Y, s1.P2.Y, s2.P1.Y, s2.P2.Y), true); // on same line. check overlapping
                else
                    return (null, false, true); // just parallel
            }




            if (vaVertical) // va vertical, vb is not
            {
                if (IsRangesOverlapping(s2.P1.X, s2.P2.X, s1.P1.X, s1.P2.X)) // va.x in vb horizontal range
                {
                    var pby = kb * s1.P1.X + ba;

                    if (IsPointOnBothSegments(s1, s2, new Vector2(s1.P1.X, pby))) // intersection point lays on both segments
                        return (new Vector2(s1.P1.X, pby), true, false);
                    else
                        return (null, false, false); // intersection point out of one or both segments
                }
                else
                    return (null, false, false); // va.x out of vb horizontal range
            }



            if (vbVertical) // vb vertical, va is not
            {
                if (IsRangesOverlapping(s1.P1.X, s1.P2.X, s2.P1.X, s2.P2.X)) // vb.x in va horizontal range
                {
                    var pay = ka * s2.P1.X + bb;

                    if (IsPointOnBothSegments(s1, s2, new Vector2(s2.P1.X, pay))) // intersection point lays on both segments
                        return (new Vector2(s2.P1.X, pay), true, false);
                    else
                        return (null, false, false); // intersection point out of one or both segments
                }
                else
                    return (null, false, false); // vb.x out of va horizontal range
            }



            // both lines has slope

            var px = Math.Abs(bb - ba) / Math.Abs(ka - kb);
            var py = kb * px + ba;

            var p = new Vector2(px, py);

            if (IsPointOnBothSegments(s1, s2, p)) // intersection point lays on both segments
                return (p, true, false);
            else
                return (null, false, false);  // intersection point out of one or both segments


        }




        public static Vector2 ScaleAndShiftV2(Vector2 v, Vector2 scale, Vector2 shift)
        {
            v.X = v.X * scale.X + shift.Y;
            v.Y = v.Y * scale.Y + shift.Y;
            return v;
        }

        public static RectangleF ScaleAndShiftRect(RectangleF r, Vector2 scale, Vector2 shift)
        {
            r.X = r.X * scale.X + shift.Y;
            r.Y = r.Y * scale.Y + shift.Y;
            r.Width = r.Width * scale.X + shift.Y;
            r.Height = r.Height * scale.Y + shift.Y;
            return r;
        }







    }

}
