using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using RT.KitchenSink;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace Qoph
{
    public static partial class Md
    {
        public static IEnumerable<Pt> Bézier(Pt start, Pt control1, Pt control2, Pt end, int steps)
        {
            return Enumerable.Range(0, steps)
                .Select(i => (double) i / (steps - 1))
                .Select(t => pow(1 - t, 3) * start + 3 * pow(1 - t, 2) * t * control1 + 3 * (1 - t) * t * t * control2 + pow(t, 3) * end);
        }

        public static IEnumerable<PointD> Bézier(PointD start, PointD control1, PointD control2, PointD end, int steps)
        {
            return Enumerable.Range(0, steps)
                .Select(i => (double) i / (steps - 1))
                .Select(t => pow(1 - t, 3) * start + 3 * pow(1 - t, 2) * t * control1 + 3 * (1 - t) * t * t * control2 + pow(t, 3) * end);
        }

        public static Pt[][] BézierPatch(Pt p00, Pt p10, Pt p20, Pt p30, Pt p01, Pt p11, Pt p21, Pt p31, Pt p02, Pt p12, Pt p22, Pt p32, Pt p03, Pt p13, Pt p23, Pt p33, int steps)
        {
            return Ut.NewArray(steps, steps, (a, b) =>
            {
                var u = (double) a / (steps - 1);
                return bé(bé(p00, p01, p02, p03, u), bé(p10, p11, p12, p13, u), bé(p20, p21, p22, p23, u), bé(p30, p31, p32, p33, u), (double) b / (steps - 1));
            });
        }

        public static Pt[][] BézierPatch(Pt[][] controlPoints, int steps)
        {
            return Ut.NewArray(steps, steps, (a, b) => ((double) a / (steps - 1)).Apply(u => ((double) b / (steps - 1)).Apply(v =>
                bé(
                    bé(controlPoints[0][0], controlPoints[1][0], controlPoints[2][0], controlPoints[3][0], u),
                    bé(controlPoints[0][1], controlPoints[1][1], controlPoints[2][1], controlPoints[3][1], u),
                    bé(controlPoints[0][2], controlPoints[1][2], controlPoints[2][2], controlPoints[3][2], u),
                    bé(controlPoints[0][3], controlPoints[1][3], controlPoints[2][3], controlPoints[3][3], u),
                    v))));
        }

        private static Pt bé(Pt start, Pt c1, Pt c2, Pt end, double t) => Math.Pow((1 - t), 3) * start + 3 * (1 - t) * (1 - t) * t * c1 + 3 * (1 - t) * t * t * c2 + Math.Pow(t, 3) * end;
        private static PointD bé(PointD start, PointD c1, PointD c2, PointD end, double t) => Math.Pow((1 - t), 3) * start + 3 * (1 - t) * (1 - t) * t * c1 + 3 * (1 - t) * t * t * c2 + Math.Pow(t, 3) * end;

        public static IEnumerable<PointD> SmoothBézier(PointD start, PointD c1, PointD c2, PointD end, double smoothness)
        {
            yield return start;

            var stack = new Stack<Tuple<double, double>>();
            stack.Push(Tuple.Create(0d, 1d));

            while (stack.Count > 0)
            {
                var elem = stack.Pop();
                var p1 = bé(start, c1, c2, end, elem.Item1);
                var p2 = bé(start, c1, c2, end, elem.Item2);
                var midT = (elem.Item1 + elem.Item2) / 2;
                var midCurve = bé(start, c1, c2, end, midT);
                var dist = new EdgeD(p1, p2).Distance(midCurve);
                if (dist <= smoothness)
                    yield return p2;
                else
                {
                    stack.Push(Tuple.Create(midT, elem.Item2));
                    stack.Push(Tuple.Create(elem.Item1, midT));
                }
            }
        }

        static Pt ifZero(this Pt pt, Pt alt) { return Math.Abs(pt.X) <= double.Epsilon && Math.Abs(pt.Y) <= double.Epsilon && Math.Abs(pt.Z) <= double.Epsilon ? alt : pt; }

        public static double pi = Math.PI;
        public static double sin(double x) => Math.Sin(x * pi / 180);
        public static double cos(double x) => Math.Cos(x * pi / 180);
        public static double tan(double x) => Math.Tan(x * pi / 180);
        public static double arcsin(double x) => Math.Asin(x) / Math.PI * 180;
        public static double arccos(double x) => Math.Acos(x) / Math.PI * 180;
        public static double pow(double x, double y) => Math.Pow(x, y);
        public static Pt pt(double x, double y, double z) => new Pt(x, y, z);
        public static Pt ptp(double r, double a, double y) => pt(r * cos(a), y, r * sin(a));
        public static PointD p(double x, double y) => new PointD(x, y);

        public static Pt[] MoveX(this Pt[] face, double x) { return face.Select(p => p.Add(x: x)).ToArray(); }
        public static Pt[] MoveY(this Pt[] face, double y) { return face.Select(p => p.Add(y: y)).ToArray(); }
        public static Pt[] MoveZ(this Pt[] face, double z) { return face.Select(p => p.Add(z: z)).ToArray(); }
        public static Pt[] Move(this Pt[] face, Pt by) { return face.Select(p => p + by).ToArray(); }

        public static IEnumerable<Pt> MoveX(this IEnumerable<Pt> face, double x) { return face.Select(p => p.Add(x: x)); }
        public static IEnumerable<Pt> MoveY(this IEnumerable<Pt> face, double y) { return face.Select(p => p.Add(y: y)); }
        public static IEnumerable<Pt> MoveZ(this IEnumerable<Pt> face, double z) { return face.Select(p => p.Add(z: z)); }
        public static IEnumerable<Pt> Move(this IEnumerable<Pt> face, Pt by) { return face.Select(p => p + by); }

        public static Pt[][] MoveX(this Pt[][] faces, double x) { return faces.Select(face => MoveX(face, x)).ToArray(); }
        public static Pt[][] MoveY(this Pt[][] faces, double y) { return faces.Select(face => MoveY(face, y)).ToArray(); }
        public static Pt[][] MoveZ(this Pt[][] faces, double z) { return faces.Select(face => MoveZ(face, z)).ToArray(); }
        public static Pt[][] Move(this Pt[][] faces, Pt by) { return faces.Select(face => Move(face, by)).ToArray(); }

        public static IEnumerable<Pt[]> MoveX(this IEnumerable<Pt[]> faces, double x) { return faces.Select(face => MoveX(face, x)); }
        public static IEnumerable<Pt[]> MoveY(this IEnumerable<Pt[]> faces, double y) { return faces.Select(face => MoveY(face, y)); }
        public static IEnumerable<Pt[]> MoveZ(this IEnumerable<Pt[]> faces, double z) { return faces.Select(face => MoveZ(face, z)); }
        public static IEnumerable<Pt[]> Move(this IEnumerable<Pt[]> faces, Pt by) { return faces.Select(face => Move(face, by)); }

        public static Pt RotateX(this Pt p, double angle) { return pt(p.X, p.Y * cos(angle) - p.Z * sin(angle), p.Y * sin(angle) + p.Z * cos(angle)); }
        public static Pt RotateY(this Pt p, double angle) { return pt(p.X * cos(angle) - p.Z * sin(angle), p.Y, p.X * sin(angle) + p.Z * cos(angle)); }
        public static Pt RotateZ(this Pt p, double angle) { return pt(p.X * cos(angle) - p.Y * sin(angle), p.X * sin(angle) + p.Y * cos(angle), p.Z); }

        public static Pt[] RotateX(this Pt[] face, double angle) { return face.Select(p => RotateX(p, angle)).ToArray(); }
        public static Pt[] RotateY(this Pt[] face, double angle) { return face.Select(p => RotateY(p, angle)).ToArray(); }
        public static Pt[] RotateZ(this Pt[] face, double angle) { return face.Select(p => RotateZ(p, angle)).ToArray(); }

        public static IEnumerable<Pt> RotateX(this IEnumerable<Pt> face, double angle) { return face.Select(p => RotateX(p, angle)); }
        public static IEnumerable<Pt> RotateY(this IEnumerable<Pt> face, double angle) { return face.Select(p => RotateY(p, angle)); }
        public static IEnumerable<Pt> RotateZ(this IEnumerable<Pt> face, double angle) { return face.Select(p => RotateZ(p, angle)); }

        public static Pt[][] RotateX(this Pt[][] faces, double angle) { return faces.Select(face => RotateX(face, angle)).ToArray(); }
        public static Pt[][] RotateY(this Pt[][] faces, double angle) { return faces.Select(face => RotateY(face, angle)).ToArray(); }
        public static Pt[][] RotateZ(this Pt[][] faces, double angle) { return faces.Select(face => RotateZ(face, angle)).ToArray(); }

        public static IEnumerable<Pt[]> RotateX(this IEnumerable<Pt[]> faces, double angle) { return faces.Select(face => RotateX(face, angle)); }
        public static IEnumerable<Pt[]> RotateY(this IEnumerable<Pt[]> faces, double angle) { return faces.Select(face => RotateY(face, angle)); }
        public static IEnumerable<Pt[]> RotateZ(this IEnumerable<Pt[]> faces, double angle) { return faces.Select(face => RotateZ(face, angle)); }

        public static IEnumerable<TResult> SelectManyConsecutivePairs<T, TResult>(this IEnumerable<T> source, bool closed, Func<T, T, IEnumerable<TResult>> selector) => source.SelectConsecutivePairs(closed, selector).SelectMany(x => x);
        public static IEnumerable<T> RemoveConsecutiveDuplicates<T>(this IEnumerable<T> source, bool closed) where T : IEquatable<T>
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return removeConsecutiveDuplicatesImpl(closed, source);
        }
        private static IEnumerable<T> removeConsecutiveDuplicatesImpl<T>(bool closed, IEnumerable<T> source) where T : IEquatable<T>
        {
            using (var e = source.GetEnumerator())
            {
                if (!e.MoveNext())
                    yield break;
                T first = e.Current;
                if (!closed)
                    yield return first;
                T last = first;
                while (e.MoveNext())
                {
                    if (!e.Current.Equals(last))
                        yield return e.Current;
                    last = e.Current;
                }
                if (closed && !first.Equals(last))
                    yield return first;
            }
        }

        public static IEnumerable<TResult> Select<T, TResult>(this IEnumerable<T> source, Func<T, bool, bool, TResult> selector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            return selectIterator(source, (elem, ix, f, l) => selector(elem, f, l));
        }

        public static IEnumerable<TResult> Select<T, TResult>(this IEnumerable<T> source, Func<T, int, bool, bool, TResult> selector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            return selectIterator(source, selector);
        }

        private static IEnumerable<TResult> selectIterator<T, TResult>(IEnumerable<T> source, Func<T, int, bool, bool, TResult> selector)
        {
            var index = 0;
            var isFirst = true;
            T elem;
            using var e = source.GetEnumerator();
            if (!e.MoveNext())
                yield break;
            elem = e.Current;
            while (e.MoveNext())
            {
                yield return selector(elem, index, isFirst, false);
                isFirst = false;
                elem = e.Current;
                index++;
            }
            yield return selector(elem, index, isFirst, true);
        }

        public static IEnumerable<PointD[]> Triangulate(this IEnumerable<PointD> face)
        {
            var pgon = face.RemoveConsecutiveDuplicates(true).ToList();

            while (pgon.Count > 3)
            {
                // Find an ear
                int bi = pgon.Count - 1;
                PointD a, b, c;
                while (true)
                {
                    a = pgon[(bi + pgon.Count - 1) % pgon.Count];
                    b = pgon[bi];
                    c = pgon[(bi + 1) % pgon.Count];

                    // If the angle ABC is concave, the triangle is not an ear.
                    if ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X) < 0)
                    {
                        // If any other point lies inside the triangle ABC, it is not an ear.
                        var isEar = true;
                        var triangle = new PolygonD(a, b, c);
                        for (int i = 0; i < pgon.Count - 3 && isEar; i++)
                            if (!triangle.Vertices.Contains(pgon[(bi + 2 + i) % pgon.Count]) && triangle.ContainsPoint(pgon[(bi + 2 + i) % pgon.Count]))
                                isEar = false;
                        if (isEar)
                            break;
                    }
                    bi--;
                    if (bi < 0)
                        throw new InvalidOperationException();
                }

                yield return new[] { a, b, c };
                pgon.RemoveAt(bi);
            }

            yield return pgon.ToArray();
        }

        public static PointD[][] Triangulate(this IEnumerable<IEnumerable<PointD>> polygons, bool failNegative = false)
        {
            var result = new List<PointD[]>();
            var remaining = polygons.Select(p => new PolygonD(p.RemoveConsecutiveDuplicates(true))).ToList();
            while (remaining.Count > 0)
            {
                var polyIx = remaining.IndexOf(poly => poly.Area() > 0);
                if (polyIx == -1)
                {
                    if (!failNegative)
                        return Triangulate(polygons.Select(poly => poly.Reverse()), failNegative: true);
                    throw new InvalidOperationException("There are only negative polygons left.");
                }

                var polygon = remaining[polyIx];
                remaining.RemoveAt(polyIx);
                int holeIx;
                while ((holeIx = remaining.IndexOf(poly => poly.Area() < 0 && poly.Vertices.Any(polygon.ContainsPoint))) != -1)
                {
                    // This polygon has a hole in the shape of another polygon.
                    var hole = remaining[holeIx];
                    remaining.RemoveAt(holeIx);

                    // Find a pair of adjacent points on the hole and a closeby pair of adjacent points on the polygon where we can “cut through”
                    var candidate = hole.Vertices
                        .SelectMany((v, i) => polygon.Vertices.Select((v2, i2) => new { Vertex = v, Index = i, Nearest = new { Vertex = v2, Index = i2 } }))
                        .Where(inf => !new EdgeD(inf.Vertex, inf.Nearest.Vertex).IntersectsWith(new EdgeD(hole.Vertices[(inf.Index + 1) % hole.Vertices.Count], polygon.Vertices[(inf.Nearest.Index + polygon.Vertices.Count - 1) % polygon.Vertices.Count]), true))

                        .Where(inf => !hole.ToEdges().Any(e => new EdgeD(inf.Vertex, inf.Nearest.Vertex).IntersectsWith(e, true)))
                        .Where(inf => !polygon.ToEdges().Any(e => new EdgeD(inf.Vertex, inf.Nearest.Vertex).IntersectsWith(e, true)))
                        .Where(inf => remaining.All(rem => !rem.ToEdges().Any(e => new EdgeD(inf.Vertex, inf.Nearest.Vertex).IntersectsWith(e, true))))

                        .Where(inf => !hole.ToEdges().Any(e => new EdgeD(hole.Vertices[(inf.Index + 1) % hole.Vertices.Count], polygon.Vertices[(inf.Nearest.Index + polygon.Vertices.Count - 1) % polygon.Vertices.Count]).IntersectsWith(e, true)))
                        .Where(inf => !polygon.ToEdges().Any(e => new EdgeD(hole.Vertices[(inf.Index + 1) % hole.Vertices.Count], polygon.Vertices[(inf.Nearest.Index + polygon.Vertices.Count - 1) % polygon.Vertices.Count]).IntersectsWith(e, true)))
                        .Where(inf => remaining.All(rem => !rem.ToEdges().Any(e => new EdgeD(hole.Vertices[(inf.Index + 1) % hole.Vertices.Count], polygon.Vertices[(inf.Nearest.Index + polygon.Vertices.Count - 1) % polygon.Vertices.Count]).IntersectsWith(e, true))))

                        .MinElement(inf => inf.Vertex.Distance(inf.Nearest.Vertex));

                    // Create the quadrilateral that “cuts through”
                    result.Add(new[] { candidate.Vertex, hole.Vertices[(candidate.Index + 1) % hole.Vertices.Count], polygon.Vertices[(candidate.Nearest.Index + polygon.Vertices.Count - 1) % polygon.Vertices.Count] });
                    result.Add(new[] { candidate.Vertex, polygon.Vertices[(candidate.Nearest.Index + polygon.Vertices.Count - 1) % polygon.Vertices.Count], candidate.Nearest.Vertex });

                    // Fix up the current polygon
                    polygon.Vertices.InsertRange(candidate.Nearest.Index, hole.Vertices.Skip(candidate.Index + 1).Concat(hole.Vertices.Take(candidate.Index + 1)));
                }

                // We should have a holeless polygon — triangulate that
                var pgon = polygon.Vertices;
                while (pgon.Count > 3)
                {
                    // Find an ear
                    int bi = pgon.Count - 1;
                    PointD a, b, c;
                    while (true)
                    {
                        a = pgon[(bi + pgon.Count - 1) % pgon.Count];
                        b = pgon[bi];
                        c = pgon[(bi + 1) % pgon.Count];

                        // If the angle ABC is concave, the triangle is not an ear.
                        if ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X) > 0)
                        {
                            // If any other point lies inside the triangle ABC, it is not an ear.
                            var isEar = true;
                            var triangle = new PolygonD(a, b, c);
                            for (int i = 0; i < pgon.Count - 3 && isEar; i++)
                                if (!triangle.Vertices.Contains(pgon[(bi + 2 + i) % pgon.Count]) && triangle.ContainsPoint(pgon[(bi + 2 + i) % pgon.Count]))
                                    isEar = false;
                            if (isEar)
                                break;
                        }
                        bi--;
                        if (bi < 0)
                            throw new InvalidOperationException();
                    }

                    result.Add(new[] { a, b, c });
                    pgon.RemoveAt(bi);
                }

                result.Add(pgon.ToArray());
            }
            return result.ToArray();
        }

        public static string PathToSvg(IEnumerable<PointD> ptsArr)
        {
            var minX = ptsArr.Min(p => p.X);
            var maxX = ptsArr.Max(p => p.X);
            var minY = ptsArr.Min(p => p.Y);
            var maxY = ptsArr.Max(p => p.Y);
            return $@"
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='{minX} {minY} {maxX - minX} {maxY - minY}'>
                    <path d='{ptsArr.Select((p, i) => $"{(i == 0 ? "M" : i == 1 ? "L" : "")}{p.X},{p.Y}").JoinString(" ")} z' stroke='#000' stroke-width='.01' fill='none' />
                </svg>
            ";
        }
    }
}
