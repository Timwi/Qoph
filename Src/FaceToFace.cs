using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CsQuery;
using PuzzleSolvers;
using RT.TagSoup;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;
using RT.Util.Text;

namespace Qoph
{
    using static Md;

    static class FaceToFace
    {
        public static void DownloadFiles()
        {
            foreach (var htmlFile in @"
http://dmccooey.com/polyhedra/Platonic.html
http://dmccooey.com/polyhedra/KeplerPoinsot.html
http://dmccooey.com/polyhedra/VersiRegular.html
http://dmccooey.com/polyhedra/Archimedean.html
http://dmccooey.com/polyhedra/Catalan.html
http://dmccooey.com/polyhedra/PrismAntiprism.html
http://dmccooey.com/polyhedra/DipyramidTrapezohedron.html
http://dmccooey.com/polyhedra/StarPrismAntiprism.html
http://dmccooey.com/polyhedra/StarDipyramidTrapezohedron.html
http://dmccooey.com/polyhedra/Hull.html
http://dmccooey.com/polyhedra/Propellor.html
http://dmccooey.com/polyhedra/BiscribedNonChiral.html
http://dmccooey.com/polyhedra/BiscribedChiral.html
http://dmccooey.com/polyhedra/TruncatedArchimedean.html
http://dmccooey.com/polyhedra/RectifiedArchimedean.html
http://dmccooey.com/polyhedra/TruncatedCatalan.html
http://dmccooey.com/polyhedra/Chamfer.html
http://dmccooey.com/polyhedra/JohnsonPage1.html
http://dmccooey.com/polyhedra/JohnsonPage2.html
http://dmccooey.com/polyhedra/JohnsonPage3.html
http://dmccooey.com/polyhedra/JohnsonPage4.html
http://dmccooey.com/polyhedra/JohnsonPage5.html
http://dmccooey.com/polyhedra/Derived.html
http://dmccooey.com/polyhedra/GeodesicIcosahedra.html
http://dmccooey.com/polyhedra/GeodesicIcosahedraPage2.html
http://dmccooey.com/polyhedra/DualGeodesicIcosahedra.html
http://dmccooey.com/polyhedra/DualGeodesicIcosahedraPage2.html
http://dmccooey.com/polyhedra/GeodesicCubes.html
http://dmccooey.com/polyhedra/GeodesicCubesPage2.html
http://dmccooey.com/polyhedra/GeodesicRTs.html
http://dmccooey.com/polyhedra/GeodesicRTsPage2.html
http://dmccooey.com/polyhedra/HighOrderGeodesics.html
http://dmccooey.com/polyhedra/GreaterSelfDual.html
http://dmccooey.com/polyhedra/ToroidalRegularHexagonal.html
http://dmccooey.com/polyhedra/ToroidalRegularTriangular.html
http://dmccooey.com/polyhedra/ToroidalRegularTetragonal.html
http://dmccooey.com/polyhedra/ToroidalNonRegular.html
http://dmccooey.com/polyhedra/HigherGenus.html
http://dmccooey.com/polyhedra/Other.html".Replace("\r", "").Split('\n').Where(url => !string.IsNullOrWhiteSpace(url) && !url.StartsWith("#")))
            {
                var response = new HClient().Get(htmlFile);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    ConsoleUtil.WriteLine("{0/Red} ({1/DarkRed} {2/DarkRed})".Color(ConsoleColor.DarkRed).Fmt(htmlFile, (int) response.StatusCode, response.StatusCode));
                    continue;
                }
                ConsoleUtil.WriteLine(htmlFile.Color(ConsoleColor.Green));

                var doc = CQ.CreateDocument(response.DataString);
                var lockObj = new object();
                doc["a"].ParallelForEach(elem =>
                {
                    var href = elem.Attributes["href"];
                    if (href == null || href.StartsWith("http") || !href.EndsWith(".html"))
                        return;
                    var filename = href.Replace(".html", ".txt");
                    var targetPath = $@"D:\c\Qoph\DataFiles\Face To Face\Txt\{filename}";
                    if (File.Exists(targetPath))
                        return;
                    var resp = new HClient().Get($"http://dmccooey.com/polyhedra/{filename}");
                    if (resp.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        lock (lockObj)
                            ConsoleUtil.WriteLine(" • {0/Red} ({1/DarkRed} {2/DarkRed})".Color(ConsoleColor.DarkGray).Fmt(href, (int) resp.StatusCode, resp.StatusCode));
                        return;
                    }
                    lock (lockObj)
                    {
                        File.WriteAllText(targetPath, resp.DataString);
                        ConsoleUtil.WriteLine(" • {0/DarkGreen}".Color(ConsoleColor.DarkGray).Fmt(href));
                    }
                });
            }
        }

        public sealed class Polyhedron
        {
            public string Filename;
            public string Name;
            public Pt[] Vertices;
            public int[][] Faces;

            public double XOffset = 0;
            public double YOffset = 0;
            public double Rotation = 0;

            public IEnumerable<int> FindAdjacent(int face)
            {
                for (var i = 0; i < Faces.Length; i++)
                {
                    if (i == face)
                        continue;
                    for (var j = 0; j < Faces[i].Length; j++)
                        for (var k = 0; k < Faces[face].Length; k++)
                            if (Faces[i][j] == Faces[face][(k + 1) % Faces[face].Length] && Faces[i][(j + 1) % Faces[i].Length] == Faces[face][k])
                            {
                                yield return i;
                                goto next;
                            }
                    next:;
                }
            }
        }

        public static Polyhedron parse(string path)
        {
            var data = File.ReadAllText(path);
            var lines = data.Replace("\r", "").Split('\n');
            var nameMatch = Regex.Match(lines[0], @"^(.*?)$");   // (?: with|$)
            var name = nameMatch.Groups[1].Value.Replace(" (canonical)", "");
            var matches = lines.Skip(1).Select(line => new
            {
                Line = line,
                CoordinateMatch = Regex.Match(line, @"^C(\d+) *= *(-?\d*\.?\d+) *(?:=|$)"),
                VertexMatch = Regex.Match(line, @"^V(\d+) *= *\( *((?<m1>-?)C(?<c1>\d+)|(?<n1>-?\d*\.?\d+)) *, *((?<m2>-?)C(?<c2>\d+)|(?<n2>-?\d*\.?\d+)) *, *((?<m3>-?)C(?<c3>\d+)|(?<n3>-?\d*\.?\d+)) *\) *$"),
                FaceMatch = Regex.Match(line, @"^ *\{ *(\d+ *(, *\d+ *)*)\} *$")
            });

            var coords = matches.Where(m => m.CoordinateMatch.Success)
                .GroupBy(m => int.Parse(m.CoordinateMatch.Groups[1].Value))
                .Select(gr =>
                {
                    var ix = gr.Key;
                    var values = gr.Select(m => double.Parse(m.CoordinateMatch.Groups[2].Value)).ToArray();
                    if (values.Skip(1).All(v => v == values[0]))
                        return (index: ix, value: values[0]);
                    Debugger.Break();
                    throw new InvalidOperationException();
                })
                .OrderBy(tup => tup.index)
                .Select((tup, ix) => { if (tup.index != ix) Debugger.Break(); return tup.value; })
                .ToArray();

            double resolveCoord(Group minus, Group coordIx, Group number) => number.Success ? double.Parse(number.Value) : (minus.Value == "-" ? -1 : 1) * coords[int.Parse(coordIx.Value)];

            var vertices = matches
                .Where(m => m.VertexMatch.Success)
                .Select(m => (index: int.Parse(m.VertexMatch.Groups[1].Value), vertex: m.VertexMatch.Groups.Apply(g => new Pt(x: resolveCoord(g["m1"], g["c1"], g["n1"]), y: resolveCoord(g["m2"], g["c2"], g["n2"]), z: resolveCoord(g["m3"], g["c3"], g["n3"])))))
                .OrderBy(inf => inf.index)
                .Select((inf, ix) => { if (inf.index != ix) Debugger.Break(); return inf.vertex; })
                .ToArray();

            var faces = matches.Where(m => m.FaceMatch.Success).Select(m => m.FaceMatch.Groups[1].Value.Split(',').Select(str => int.Parse(str.Trim())).ToArray()).ToArray();
            return new Polyhedron { Filename = Path.GetFileName(path), Name = name, Vertices = vertices, Faces = faces };
        }

        public static void Test()
        {
            const int extraFaces = 1;

            var polyhedra = getPolyhedra();

            foreach (var p in polyhedra)
                if (p.Faces.Length == 24)
                    Console.WriteLine(p.Name);
            Console.WriteLine();

            polyhedra = polyhedra.Where(p => p.Faces.Length <= 25 && !"{}".Any(ch => p.Name.Contains(ch))).ToList();

            var loopLen = 6;
            IEnumerable<int[]> recurse(int[] sofar)
            {
                if (sofar.Length == loopLen)
                {
                    yield return sofar;
                    yield break;
                }

                for (var i = 0; i < polyhedra.Count; i++)
                {
                    if (sofar.Contains(i))
                        continue;
                    var polyhedron = polyhedra[i];
                    var nameLen1 = polyhedron.Name.ToUpperInvariant().Count(ch => ch >= 'A' && ch <= 'Z');
                    var nameLen2 = Regex.Replace(polyhedron.Name, @"\s*\(.*\)\s*$", "").ToUpperInvariant().Count(ch => ch >= 'A' && ch <= 'Z');
                    var joinsUp = true;
                    if (sofar.Length == loopLen - 1)
                    {
                        var oNameLen1 = polyhedra[sofar[0]].Name.ToUpperInvariant().Count(ch => ch >= 'A' && ch <= 'Z');
                        var oNameLen2 = Regex.Replace(polyhedra[sofar[0]].Name, @"\s*\(.*\)\s*$", "").ToUpperInvariant().Count(ch => ch >= 'A' && ch <= 'Z');
                        joinsUp = polyhedra[i].Faces.Length - extraFaces == oNameLen1 || polyhedra[i].Faces.Length - extraFaces == oNameLen2;
                    }
                    if (joinsUp && (sofar.Length == 0 || polyhedra[sofar[sofar.Length - 1]].Faces.Length - extraFaces == nameLen1 || polyhedra[sofar[sofar.Length - 1]].Faces.Length - extraFaces == nameLen2))
                        foreach (var solution in recurse(sofar.Insert(sofar.Length, i)))
                            yield return solution;
                }
            }

            var count = 0;
            var best = int.MaxValue;
            var bestCount = 0;
            foreach (var solution in recurse(new int[0]))
            {
                count++;

                var totalFaces = solution.Sum(ix => polyhedra[ix].Faces.Length);
                if (totalFaces < best)
                {
                    best = totalFaces;
                    var tt = new TextTable { ColumnSpacing = 2 };

                    tt.SetCell(4, 0, "V".Color(ConsoleColor.Green));
                    tt.SetCell(5, 0, "F".Color(ConsoleColor.Cyan));

                    var row = 1;
                    for (var i = 0; i < solution.Length; i++)
                    {
                        var polyhedron = polyhedra[solution[i]];
                        var nameLen1 = polyhedron.Name.ToUpperInvariant().Count(ch => ch >= 'A' && ch <= 'Z');
                        var nameLen2 = Regex.Replace(polyhedron.Name, @"\s*\(.*\)\s*$", "").ToUpperInvariant().Count(ch => ch >= 'A' && ch <= 'Z');
                        tt.SetCell(0, row, polyhedron.Filename.Color(ConsoleColor.DarkYellow));
                        tt.SetCell(1, row, polyhedron.Name.Color(ConsoleColor.Yellow));
                        tt.SetCell(2, row, nameLen1.ToString().Color(ConsoleColor.Magenta));
                        tt.SetCell(3, row, nameLen2.ToString().Color(ConsoleColor.Magenta));
                        tt.SetCell(4, row, polyhedron.Vertices.Length.ToString().Color(ConsoleColor.Green));
                        tt.SetCell(5, row, polyhedron.Faces.Length.ToString().Color(ConsoleColor.Cyan));
                        row++;
                    }
                    tt.SetCell(5, row, totalFaces.ToString().Color(ConsoleColor.White));
                    tt.WriteToConsole();
                    Console.WriteLine();
                    bestCount = 0;
                }
                else if (totalFaces == best)
                    bestCount++;
            }
            Console.WriteLine(count);
            Console.WriteLine(bestCount);
        }

        private static List<Polyhedron> getPolyhedra()
        {
            var polyhedra = new List<Polyhedron>();
            var files = new DirectoryInfo(@"D:\c\Qoph\DataFiles\Face To Face\Txt").EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly).ToArray();
            files.ParallelForEach(4, file =>
            {
                var polyhedron = parse(file.FullName);
                lock (polyhedra)
                {
                    polyhedra.Add(polyhedron);
                    Console.CursorTop = 0;
                    Console.CursorLeft = 0;
                    var percentage = polyhedra.Count * 100 / files.Length;
                    ConsoleUtil.WriteLine($"{new string('█', percentage).Color(ConsoleColor.Cyan)}{new string('░', 100 - percentage).Color(ConsoleColor.Cyan)} {(percentage + "%").Color(ConsoleColor.Yellow)}", null);
                }
            });
            return polyhedra;
        }

        public static void GenerateNets()
        {
            var polyhedra = getPolyhedra();
            File.WriteAllText(@"D:\c\Qoph\DataFiles\Face To Face\Polyhedral Puzzle.html",
                $@"
<html>
    <head>
        <style>
            div.poly-wrap {{
                display: inline-block;
                border: 1px solid black;
                margin: 1cm;
                width: 5.5cm;
                text-align: center;
            }}
            div.filename {{ font-size: 8pt; }}
            svg.polyhedron {{
                width: 5cm;
            }}
        </style>
    </head>
    <body>{polyhedra.Where(p => p.Faces.Length == 24 || p.Faces.Length == 28).OrderBy(p => p.Faces.Length).ThenBy(p => p.Name).Select(p =>
                {
                    try { return $"<div class='poly-wrap'>{generateNet(p).svg}<div class='filename'>{p.Filename}</div><div class='name'><a href='http://dmccooey.com/polyhedra/{Path.GetFileNameWithoutExtension(p.Filename)}.html'>{p.Name}</a> ({p.Faces.Length} faces)</div></div>"; }
                    catch (Exception e) { return $"<div>Error - {p.Filename} - <a href='http://dmccooey.com/polyhedra/{Path.GetFileNameWithoutExtension(p.Filename)}.html'>{p.Name}</a> - {e.Message} - {e.GetType().FullName}</div>"; }
                }).JoinString()}</body></html>");
        }

        /// <summary>
        /// Generates SVG showing the net of a polyhedron.
        /// </summary>
        /// <param name="polyhedron">Polyhedron to generate net for.</param>
        /// <param name="edgeStrokeWidth">Determines the stroke width of an edge. Parameters are: Face 1 index, Face 2 index.</param>
        /// <param name="edgeSvg">Extra SVG to add for each edge. Parameters are: Face 1 index, Face 2 index, X, Y.</param>
        /// <param name="vertexSvg">Extra SVG to add for each vertex. Parameters are: Vertex index, X, Y.</param>
        /// <param name="faceSvg">Extra SVG to add for each face. Parameters are: Face index, X, Y.</param>
        /// <returns></returns>
        private static (string svg, PointD[][] polygons) generateNet(Polyhedron polyhedron,
            Func<int, int, double, double, string> edgeSvg = null,
            Func<int, double, double, string> vertexSvg = null,
            Func<int, int, double> edgeStrokeWidth = null,
            Func<int, double, double, string> faceSvg = null,
            Func<int, string> faceColor = null)
        {
            // Numbers closer than this are considered equal
            const double closeness = .00001;
            static bool sufficientlyClose(Pt p1, Pt p2) => Math.Abs(p1.X - p2.X) < closeness && Math.Abs(p1.Y - p2.Y) < closeness && Math.Abs(p1.Z - p2.Z) < closeness;

            // Take a full copy
            var faces = polyhedron.Faces.Select(face => face.Select(vIx => polyhedron.Vertices[vIx]).ToArray()).ToArray();

            var svg = new StringBuilder();
            var svgFaces = new StringBuilder();
            var svgExtras = new StringBuilder();

            // Restricted variable scope
            {
                var vx = faces[0][0];
                // Put first vertex at origin and apply rotation
                for (int i = 0; i < faces.Length; i++)
                    for (int j = 0; j < faces[i].Length; j++)
                        faces[i][j] = faces[i][j] - vx;

                // Rotate so that first face is on the X/Y plane
                var normal = (faces[0][2] - faces[0][1]) * (faces[0][0] - faces[0][1]);
                var rot = normal.Normalize() * new Pt(0, 0, 1);
                if (Math.Abs(rot.X) < closeness && Math.Abs(rot.Y) < closeness && Math.Abs(rot.Z) < closeness)
                {
                    // the face is already on the X/Y plane
                }
                else
                {
                    var newFaces1 = faces.Select(f => f.Select(p => p.Rotate(pt(0, 0, 0), rot, arcsin(rot.Length))).ToArray()).ToArray();
                    var newFaces2 = faces.Select(f => f.Select(p => p.Rotate(pt(0, 0, 0), rot, -arcsin(rot.Length))).ToArray()).ToArray();
                    faces = newFaces1[0].Sum(p => p.Z * p.Z) < newFaces2[0].Sum(p => p.Z * p.Z) ? newFaces1 : newFaces2;
                }

                // If polyhedron is now *below* the x/y plane, rotate it 180° so it’s above
                if (faces.Sum(f => f.Sum(p => p.Z)) < 0)
                    faces = faces.Select(f => f.Select(p => pt(-p.X, p.Y, -p.Z)).ToArray()).ToArray();

                // Finally, apply rotation and offset
                var offsetPt = new Pt(polyhedron.XOffset, polyhedron.YOffset, 0);
                for (int i = 0; i < faces.Length; i++)
                    for (int j = 0; j < faces[i].Length; j++)
                        faces[i][j] = faces[i][j].RotateZ(polyhedron.Rotation) + offsetPt;
            }

            var q = new Queue<(int newFaceIx, Pt[][] rotatedSolid)>();

            // Keeps track of the polygons in the net and also which faces have already been processed during the following algorithm (unvisited ones are null).
            var polygons = new PointD[faces.Length][];

            // Remembers which faces have already been encountered (through adjacent edges) but not yet processed.
            var seen = new HashSet<int> { 0 };

            q.Enqueue((0, faces));
            while (q.Count > 0)
            {
                var (fromFaceIx, rotatedPolyhedron) = q.Dequeue();
                polygons[fromFaceIx] = rotatedPolyhedron[fromFaceIx].Select(pt => p(pt.X, pt.Y)).ToArray();

                if (faceColor != null)
                    svgFaces.Append($@"<path id='outline-{fromFaceIx}' d='M{polygons[fromFaceIx].Select(p => $"{p.X},{p.Y}").JoinString(" ")}z' fill='{faceColor(fromFaceIx)}' />");
                if (faceSvg != null)
                    svgExtras.Append((polygons[fromFaceIx].Aggregate(new PointD(), (p, n) => p + n) / polygons[fromFaceIx].Length).Apply(mid => faceSvg(fromFaceIx, mid.X, mid.Y)));

                for (int fromEdgeIx = 0; fromEdgeIx < rotatedPolyhedron[fromFaceIx].Length; fromEdgeIx++)
                {
                    int toEdgeIx = -1;
                    // Find another face that has the same edge
                    var toFaceIx = rotatedPolyhedron.IndexOf(fc =>
                    {
                        toEdgeIx = fc.IndexOf(p => sufficientlyClose(p, rotatedPolyhedron[fromFaceIx][(fromEdgeIx + 1) % rotatedPolyhedron[fromFaceIx].Length]));
                        return toEdgeIx != -1 && sufficientlyClose(fc[(toEdgeIx + 1) % fc.Length], rotatedPolyhedron[fromFaceIx][fromEdgeIx]);
                    });
                    if (toEdgeIx == -1 || toFaceIx == -1)
                        throw new InvalidOperationException(@"Something went wrong");

                    if (seen.Add(toFaceIx))
                    {
                        // Rotate about the edge so that the new face is on the X/Y plane (i.e. “roll” the polyhedron)
                        var toFace = rotatedPolyhedron[toFaceIx];
                        var normal = (toFace[2] - toFace[1]) * (toFace[0] - toFace[1]);
                        var rot = normal.Normalize() * pt(0, 0, 1);
                        var asin = arcsin(rot.Length);
                        var values = Ut.NewArray<(Pt[] face, double angle)>(
                            (toFace.Take(3).Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], asin)).ToArray(), asin),
                            (toFace.Take(3).Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], -asin)).ToArray(), -asin),
                            (toFace.Take(3).Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], 180 + asin)).ToArray(), 180 + asin),
                            (toFace.Take(3).Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], 180 - asin)).ToArray(), 180 - asin));
                        var best = values.FirstOrDefault(tup => ((tup.face[2] - tup.face[1]) * (tup.face[0] - tup.face[1])).Apply(nrml => Math.Abs(nrml.X) < closeness && Math.Abs(nrml.Y) < closeness && nrml.Z < 0));
                        if (best.face == null)
                            throw new InvalidOperationException(@"No suitable angle found.");

                        q.Enqueue((toFaceIx, rotatedPolyhedron.Select(face => face.Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], best.angle)).ToArray()).ToArray()));
                    }
                    else
                    {
                        var p1 = polygons[fromFaceIx][fromEdgeIx];
                        var p2 = polygons[fromFaceIx][(fromEdgeIx + 1) % polygons[fromFaceIx].Length];
                        IEnumerable<string> classes = new[] { $"face-{fromFaceIx}", $"face-{toFaceIx}", $"edge-{fromFaceIx}-{fromEdgeIx}", $"edge-{toFaceIx}-{toEdgeIx}" };
                        svg.Append($@"<path id='edge-{fromFaceIx}-{fromEdgeIx}' d='M {p1.X},{p1.Y} L {p2.X},{p2.Y}' stroke='black'{(edgeStrokeWidth == null ? null : $" stroke-width='{edgeStrokeWidth(fromFaceIx, toFaceIx)}'")} />");

                        if (polygons[toFaceIx] != null)
                        {
                            var controlPointFactor = 1;

                            var p11 = polygons[fromFaceIx][fromEdgeIx];
                            var p12 = polygons[fromFaceIx][(fromEdgeIx + 1) % polygons[fromFaceIx].Length];
                            var p1m = p((p11.X + p12.X) / 2, (p11.Y + p12.Y) / 2);
                            var p1c = p(p1m.X - (p1m.Y - p11.Y) * controlPointFactor, p1m.Y + (p1m.X - p11.X) * controlPointFactor);
                            var p21 = polygons[toFaceIx][toEdgeIx];
                            var p22 = polygons[toFaceIx][(toEdgeIx + 1) % polygons[toFaceIx].Length];
                            var p2m = p((p21.X + p22.X) / 2, (p21.Y + p22.Y) / 2);
                            var p2c = p(p2m.X - (p2m.Y - p21.Y) * controlPointFactor, p2m.Y + (p2m.X - p21.X) * controlPointFactor);

                            var edge1 = new EdgeD(p1m, p1c);
                            var edge2 = new EdgeD(p2c, p2m);
                            Intersect.LineWithLine(ref edge1, ref edge2, out var l1, out var l2);
                            var intersect = edge1.Start + l1 * (edge1.End - edge1.Start);

                            classes = classes.Concat("decor");
                            //switch (adj & Adjacency.ConnectionMask)
                            //{
                            //    case Adjacency.Portaled:
                            //        var ch = polyhedron.GetPortalLetter(fromFaceIx, fromEdgeIx);
                            //        sendText($"portal-letter-{fromFaceIx}-{fromEdgeIx}", classes, .5, p1c.X, p1c.Y, ch.ToString(), "#000", edgeData);
                            //        sendText($"portal-letter-{toFaceIx}-{toEdgeIx}", classes, .5, p2c.X, p2c.Y, ch.ToString(), "#000", edgeData);
                            //        sendPath($"portal-marker-{fromFaceIx}-{fromEdgeIx}", classes, edgeData, $"M {(p11.X + p1m.X) / 2},{(p11.Y + p1m.Y) / 2} {(p1c.X + p1m.X) / 2},{(p1c.Y + p1m.Y) / 2} {(p12.X + p1m.X) / 2},{(p12.Y + p1m.Y) / 2} z", fill: "#888");
                            //        sendPath($"portal-marker-{toFaceIx}-{toEdgeIx}", classes, edgeData, $"M {(p21.X + p2m.X) / 2},{(p21.Y + p2m.Y) / 2} {(p2c.X + p2m.X) / 2},{(p2c.Y + p2m.Y) / 2} {(p22.X + p2m.X) / 2},{(p22.Y + p2m.Y) / 2} z", fill: "#888");
                            //        break;

                            //    case Adjacency.Curved:
                            svg.Append($@"<path stroke='cornflowerblue' fill='none' id='curve-{fromFaceIx}-{fromEdgeIx}' d='{(
                                (p2m - p1m).Distance() < .5 ? $"M {p1m.X},{p1m.Y} L {p2m.X},{p2m.Y}" :
                                l1 >= 0 && l1 <= 1 && l2 >= 0 && l2 <= 1 ? $"M {p1m.X},{p1m.Y} C {intersect.X},{intersect.Y} {intersect.X},{intersect.Y} {p2m.X},{p2m.Y}" :
                                $"M {p1m.X},{p1m.Y} C {p1c.X},{p1c.Y} {p2c.X},{p2c.Y} {p2m.X},{p2m.Y}")}' />");
                            //        break;
                            //}
                        }

                        if (edgeSvg != null)
                            svgExtras.Append(edgeSvg(fromFaceIx, toFaceIx, (p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2));
                        if (vertexSvg != null)
                            svgExtras.Append(vertexSvg(polyhedron.Faces[fromFaceIx][fromEdgeIx], p1.X, p1.Y));
                    }
                }
            }

            var xMin = polygons.Min(pg => pg?.Min(p => p.X)).Value;
            var yMin = polygons.Min(pg => pg?.Min(p => p.Y)).Value;
            var xMax = polygons.Max(pg => pg?.Max(p => p.X)).Value;
            var yMax = polygons.Max(pg => pg?.Max(p => p.Y)).Value;
            return (svg: $@"<svg class='polyhedron' xmlns='http://www.w3.org/2000/svg' viewBox='{xMin - .5} {yMin - .5} {xMax - xMin + 1} {yMax - yMin + 1}' stroke-width='{(xMax - xMin + 1) / 360}' font-family='Work Sans'>{svgFaces}{svg}{svgExtras}</svg>", polygons);
        }

        public static void GenerateCrossword()
        {
            var polyhedron = parse(@"D:\c\Qoph\DataFiles\Face To Face\Txt\SelfDualIcosioctahedron4.txt");
            var (svg, polygons) = generateNet(polyhedron);
            var polyMidPoints = polygons.Select(poly => new PointD(poly.Average(p => p.X), poly.Average(p => p.Y))).ToArray();
            var xMin = polygons.Min(pg => pg.Min(p => p.X));
            var yMin = polygons.Min(pg => pg.Min(p => p.Y));
            var xMax = polygons.Max(pg => pg.Max(p => p.X));
            var yMax = polygons.Max(pg => pg.Max(p => p.Y));

            var minGridSize = 1;
            var maxGridSize = 1000;
            (double x, double y)[] coords = null;
            while (Math.Abs(maxGridSize - minGridSize) > 1)
            {
                var gridSize = (maxGridSize + minGridSize) / 2;
                coords = polyMidPoints.Select(p => (x: Math.Floor((p.X - xMin) / (xMax - xMin) * gridSize), y: Math.Floor((p.Y - yMin) / (yMax - yMin) * gridSize))).ToArray();
                if (coords.UniquePairs().Any(tup => tup.Item1 == tup.Item2))
                    minGridSize = gridSize;
                else
                    maxGridSize = gridSize;
            }

            var cellW = (xMax - xMin) / maxGridSize;
            var cellH = (yMax - yMin) / maxGridSize;

            var facePins = coords.Select((c, ix) => $"<circle cx='{c.x * cellW + xMin}' cy='{c.y * cellH + yMin}' r='{Math.Min(cellW, cellH) * .2}' /><line stroke-width='.03' stroke='red' x1='{c.x * cellW + xMin}' y1='{c.y * cellH + yMin}' x2='{polyMidPoints[ix].X}' y2='{polyMidPoints[ix].Y}' />").JoinString();
            var faceLabels = polygons.Select((f, faceIx) => $"<text font-size='.2' text-anchor='middle' fill='#080' x='{f.Average(p => p.X)}' y='{f.Average(p => p.Y)}'>{faceIx}</text>").JoinString();

            Console.WriteLine("Spaces:");
            Console.WriteLine(coords.Select((c, ix) => $"{ix} = {c}").JoinString("\n"));
            Console.WriteLine();
            Console.WriteLine("Lights:");
            var path = @"D:\c\Qoph\DataFiles\Face To Face\Polyhedral Puzzle Crossword (lights).txt";
            var lights = @"19 9 17 15 4 13 14 2 18,6 19 20 0 12,13 11 23 21,2 26 7 6 27 9,11 25 1 3 27 9,2 26 8,16 10 20 18 7,10 24 0,11 25 4 5 24 10,22 23 1 15,3 21 22 8,14 12 5 16 17"
                .Split(',')
                .Select(str => str.Split(' ').Select(int.Parse).ToArray())
                .ToArray();
            File.WriteAllText(path, lights
                .Select(light => light.Select(f => coords[f]).Select(c => $"{c.x} {c.y}").JoinString("\r\n"))
                .Concat(coords.Select(c => $"{c.x} {c.y}").JoinString("\r\n"))
                .JoinString("\r\n\r\n"));
            Console.WriteLine($"Written to {path}");
            Console.WriteLine();

            var faceLetters = new char?[polyhedron.Faces.Length];
            var words = @"SREVRESBO<,TSAEL<,EDIA<,BETTER,DEEPER,BEE,TSAOT<,SHE,DERAHS<,LIEV<,PALE,SLATE".Split(',');
            var clues = @"Watchers,To the smallest degree,Helper,Superior,Less shallow,Hive dweller,Tribute or food,That woman,Mutual,Bride’s garment,Lose color,Roofing material".Split(',');
            var targetLetters = @"DFAQSBWXFTRENGUZYHPVCDOMLIJK";
            for (var w = 0; w < words.Length; w++)
            {
                for (var c = 0; c < words[w].Length; c++)
                {
                    if (c == words[w].Length - 1 && words[w][c] == '<')
                        continue;
                    if (faceLetters[lights[w][c]] == null)
                        faceLetters[lights[w][c]] = words[w][c];
                    else if (faceLetters[lights[w][c]].Value != words[w][c])
                        Debugger.Break();
                }
            }

            File.WriteAllText(@"D:\c\Qoph\DataFiles\Face To Face\Polyhedral Puzzle Crossword.svg",
                generateNet(polyhedron,
                    edgeStrokeWidth: (f1, f2) => lights.Any(l => l.IndexOf(f1).Apply(p1 => p1 != -1 && ((p1 > 0 && l[p1 - 1] == f2) || (p1 < l.Length - 1 && l[p1 + 1] == f2)))) ? .01 : .05,
                    faceSvg: (fx, x, y) =>
                        //lights.Select((l, wx) => (l[0] == fx && !words[wx].EndsWith("<")) || (l.Last() == fx && words[wx].EndsWith("<")) ? clues[wx] : null).Where(clue => clue != null).ToArray().Apply(clues =>
                        //    clues.Length > 0 ? $"<text x='{x}' y='{y + .025}' stroke='white' stroke-width='.02' paint-order='stroke' fill='{(clues.Length > 1 ? "red" : "black")}' font-size='.1' text-anchor='middle'>{clues.JoinString("/")}</text>" : null) +
                        $"<text x='{x}' y='{y + .025}' stroke='white' stroke-width='.02' paint-order='stroke' fill='{(lights.Count(l => l[0] == fx || l.Last() == fx) > 1 ? "red" : "black")}' font-size='.25' text-anchor='middle'>{faceLetters[fx]}</text>" +
                        (targetLetters[fx] - faceLetters[fx]).NullOr(offset => $"<text x='{x}' y='{y + .125}' stroke='white' stroke-width='.01' paint-order='stroke' fill='{(lights.Count(l => l[0] == fx || l.Last() == fx) > 1 ? "red" : "black")}' font-size='.1' text-anchor='middle'>{(offset > 0 ? "+" : offset < 0 ? "−" : "")}{Math.Abs(offset)}</text>")
                ).svg);
            Console.WriteLine(faceLetters.JoinString());
        }

        enum LtGtClue { Equal, LessThan, GreaterThan };

        public static void GenerateLessThanGreaterThanPuzzle_OBSOLETE()
        {
            // This puzzle doesn’t work because less-than/greater-than on every edge is not enough to uniquely define every face

            var polyhedron = parse(@"D:\c\Qoph\DataFiles\Face To Face\Txt\SnubSquareAntiprism.txt");
            var n = polyhedron.Faces.Length;
            var solution = Enumerable.Range(0, n).ToArray().Shuffle();
            Console.WriteLine($"Solution: {solution.JoinString(", ")}");
            var minValue = solution.Min();
            var maxValue = solution.Max();
            var adjacents = Enumerable.Range(0, n).Select(fIx => polyhedron.FindAdjacent(fIx).ToArray()).ToArray();

            // Clues that would be given in the puzzle
            var ltGtClues = Enumerable.Range(0, n).SelectMany(f1Ix => adjacents[f1Ix].Select(f2Ix => (f1Ix, f2Ix, op: solution[f1Ix] > solution[f2Ix] ? -1 : solution[f1Ix] < solution[f2Ix] ? 1 : 0))).ToArray();
            var givens = solution.Select((sol, i) => (fIx: i, value: sol)).ToArray().Shuffle().Take(n - 3).ToArray();

            //var originalClues = Ut.NewArray(n * n, ix =>
            //{
            //    var f1Ix = ix % n;
            //    var f2Ix = ix / n;
            //    return !adjacents[f1Ix].Contains(f2Ix) ? (int?) null : solution[f1Ix] > solution[f2Ix] ? -1 : solution[f1Ix] < solution[f2Ix] ? 1 : 0;
            //});


            // SOLVER STARTS HERE

            // Compute a sort of transitive closure of the given clues
            var transitiveClues = new int?[n * n];
            foreach (var (f1Ix, f2Ix, op) in ltGtClues)
            {
                transitiveClues[f1Ix + n * f2Ix] = op;
                transitiveClues[f2Ix + n * f1Ix] = -op;
            }
            while (true)
            {
                var anyChanges = false;

                for (var i = 0; i < n; i++)
                    for (var j = 0; j < n; j++)
                        if (j != i && transitiveClues[i + j * n] is int ij)
                            for (var k = 0; k < n; k++)
                                if (k != i && k != j && transitiveClues[j + k * n] is int jk)
                                    if ((ij >= 0 && jk >= 0 && (transitiveClues[i + k * n] == null || transitiveClues[i + k * n].Value < ij + jk)) ||
                                        (ij <= 0 && jk <= 0 && (transitiveClues[i + k * n] == null || transitiveClues[i + k * n].Value > ij + jk)))
                                    {
                                        transitiveClues[i + k * n] = ij + jk;
                                        anyChanges = true;
                                    }

                if (!anyChanges)
                    break;
            }

            IEnumerable<int[]> solve(int?[] clues, int[] sofar, int[] minValues, int[] maxValues, bool[] used, bool[] filled)
            {
                var allFilled = true;
                int faceIx = 0;
                int bestConstraint = int.MaxValue;
                for (var i = 0; i < n; i++)
                    if (!filled[i])
                    {
                        allFilled = false;
                        if (maxValues[i] - minValues[i] < bestConstraint)
                        {
                            bestConstraint = maxValues[i] - minValues[i];
                            faceIx = i;
                        }
                    }

                if (allFilled)
                {
                    // Check if the original constraints still hold
                    foreach (var (f1Ix, f2Ix, op) in ltGtClues)
                        if ((op == 0 && sofar[f1Ix] != sofar[f2Ix]) ||
                            (op == -1 && !(sofar[f1Ix] > sofar[f2Ix])) ||
                            (op == 1 && !(sofar[f1Ix] < sofar[f2Ix])))
                            Debugger.Break();

                    yield return sofar.ToArray();
                    yield break;
                }

                for (var v = minValues[faceIx]; v <= maxValues[faceIx]; v++)
                {
                    // Use this only if values aren’t supposed to be all unique
                    if (used[v])
                        goto busted;

                    var mins = minValues;
                    var minsCopied = false;
                    var maxs = maxValues;
                    var maxsCopied = false;
                    for (var otherFaceIx = 0; otherFaceIx < n; otherFaceIx++)
                    {
                        if (otherFaceIx != faceIx && clues[faceIx + n * otherFaceIx] is int clue)
                        {
                            if (clue >= 0 && mins[otherFaceIx] < v + clue)
                            {
                                if (!minsCopied)
                                {
                                    mins = (int[]) minValues.Clone();
                                    minsCopied = true;
                                }
                                mins[otherFaceIx] = v + clue;
                            }
                            if (clue <= 0 && maxs[otherFaceIx] > v + clue)
                            {
                                if (!maxsCopied)
                                {
                                    maxs = (int[]) maxValues.Clone();
                                    maxsCopied = true;
                                }
                                maxs[otherFaceIx] = v + clue;
                            }
                            if (mins[otherFaceIx] > maxs[otherFaceIx])
                                goto busted;
                        }
                    }

                    sofar[faceIx] = v;
                    used[v] = true;
                    filled[faceIx] = true;
                    foreach (var sol in solve(clues, sofar, mins, maxs, used, filled))
                        yield return sol;
                    used[v] = false;
                    filled[faceIx] = false;

                    busted:;
                }
            }

            var newMinValues = Ut.NewArray(n, i => minValue + Enumerable.Range(0, n).Where(i2 => transitiveClues[i2 + n * i] is int clue && clue > 0).MaxOrDefault(i2 => transitiveClues[i2 + n * i].Value, 0));
            var newMaxValues = Ut.NewArray(n, i => maxValue + Enumerable.Range(0, n).Where(i2 => transitiveClues[i2 + n * i] is int clue && clue < 0).MinOrDefault(i2 => transitiveClues[i2 + n * i].Value, 0));
            foreach (var (fIx, value) in givens)
            {
                newMinValues[fIx] = value;
                newMaxValues[fIx] = value;
            }
            ConsoleUtil.WriteLine($"{"GIVENS:  ".Color(ConsoleColor.Cyan)} — {Enumerable.Range(0, n).Select(i => (givens.Where(tup => tup.fIx == i).FirstOrNull().NullOr(tup => tup.value.ToString()) ?? "").PadLeft(3)).JoinString(", ").Color(ConsoleColor.DarkCyan)}", null);
            ConsoleUtil.WriteLine($"{"MINS:    ".Color(ConsoleColor.Magenta)} — {newMinValues.Select(i => i.ToString().PadLeft(3)).JoinString(", ").Color(ConsoleColor.DarkMagenta)}", null);
            ConsoleUtil.WriteLine($"{"MAXS:    ".Color(ConsoleColor.Green)} — {newMaxValues.Select(i => i.ToString().PadLeft(3)).JoinString(", ").Color(ConsoleColor.DarkGreen)}", null);
            foreach (var sol in solve(transitiveClues, new int[n],
                minValues: newMinValues,
                maxValues: newMaxValues,
                new bool[n], new bool[n]))
                ConsoleUtil.WriteLine($"{"SOLUTION:".Color(ConsoleColor.White)} — {sol.Select(i => i.ToString().PadLeft(3)).JoinString(", ").Color(ConsoleColor.Yellow)}", null);
        }

        enum EdgeClueType { Sum, Difference, Product, Quotient, Rel }

        // Self-Dual Icosioctahedron #1 (28 faces)
        public static void GenerateEdgeClues()
        {
            var n = 24;
            foreach (var polyhedron in getPolyhedra())
            {
                if (!Ut.NewArray(
                    "Biscribed Pentagonal Icositetrahedron (dextro) with radius = 1",
                    "Klein Map {7,3}_8 with basic shape",
                    "Pentagonal Icositetrahedron (laevo)",
                    "Pentagonal Icositetrahedron (dextro)").Contains(polyhedron.Name))
                    continue;

                var rnd = new Random(2);
                var solution = Enumerable.Range(1, 26).Except(new[] { 1, 5 }).ToArray().Shuffle(rnd);
                if (solution.Length != n)
                    Debugger.Break();

                IEnumerable<(int face1, int face2, EdgeClueType type, int value)> generateClues(int face1, int face2)
                {
                    var list = new List<(int face1, int face2, EdgeClueType type, int value)>();
                    list.Add((face1, face2, EdgeClueType.Sum, solution[face1] + solution[face2]));
                    //list.Add((face1, face2, EdgeClueType.Difference, Math.Abs(solution[face1] - solution[face2])));
                    //var product = solution[face1] * solution[face2];
                    //if (new[] { 2, 3, 5, 7 }.Count(f => product % f == 0) >= 3)
                    //    list.Add((face1, face2, EdgeClueType.Product, product));
                    //list.Add((face1, face2, EdgeClueType.Rel, Math.Sign(solution[face2] - solution[face1])));
                    //if (solution[face1] != 0 && solution[face2] % solution[face1] == 0 && solution[face2] / solution[face1] <= 5)
                    //    list.Add((face1, face2, EdgeClueType.Quotient, solution[face2] / solution[face1]));
                    //else if (solution[face2] != 0 && solution[face1] % solution[face2] == 0 && solution[face1] / solution[face2] <= 5)
                    //    list.Add((face1, face2, EdgeClueType.Quotient, solution[face1] / solution[face2]));
                    //yield return list.PickRandom(rnd);
                    return list;
                }

                var minValue = 1;// solution.Min();
                var maxValue = 1000;// solution.Max();
                var maxLength = Math.Max(minValue.ToString().Length, maxValue.ToString().Length);
                ConsoleUtil.WriteLine($"SOLTN: {solution.Select(i => i.ToString().PadLeft(maxLength)).JoinString(", ").Color(ConsoleColor.Yellow)}", null);

                IEnumerable<int[]> recurse((int face1, int face2, EdgeClueType type, int value)[] clues, int[] sofar, bool[] used, int[][] possibleValues)
                {
                    var bestFace = 0;
                    var bestConstraint = int.MaxValue;
                    var allUsed = true;
                    for (var i = 0; i < n; i++)
                    {
                        if (!used[i])
                        {
                            allUsed = false;
                            if (possibleValues[i].Length < bestConstraint)
                            {
                                bestConstraint = possibleValues[i].Length;
                                bestFace = i;
                            }
                        }
                    }
                    if (allUsed)
                    {
                        yield return sofar;
                        yield break;
                    }

                    used[bestFace] = true;
                    foreach (var value in possibleValues[bestFace])
                    {
                        //for (var i = 0; i < n; i++)
                        //    if (i != bestFace && used[i] && sofar[i] == value)
                        //        goto busted;

                        sofar[bestFace] = value;
                        var newPossibleValues = (int[][]) possibleValues.Clone();
                        foreach (var clue in clues)
                        {
                            var (cFace1, cFace2, cType, cValue) = clue;
                            if (cFace1 == bestFace)
                            {
                            }
                            else if (cFace2 == bestFace)
                            {
                                var t = cFace1;
                                cFace1 = cFace2;
                                cFace2 = t;
                                if (cType == EdgeClueType.Rel)
                                    cValue = -cValue;
                            }
                            else
                                continue;

                            switch (cType)
                            {
                                case EdgeClueType.Sum: newPossibleValues[cFace2] = newPossibleValues[cFace2].Where(i => i + value == cValue).ToArray(); break;
                                case EdgeClueType.Difference: newPossibleValues[cFace2] = newPossibleValues[cFace2].Where(i => Math.Abs(i - value) == cValue).ToArray(); break;
                                case EdgeClueType.Product: newPossibleValues[cFace2] = newPossibleValues[cFace2].Where(i => i * value == cValue).ToArray(); break;
                                case EdgeClueType.Quotient: newPossibleValues[cFace2] = newPossibleValues[cFace2].Where(i => i == cValue * value || value == cValue * i).ToArray(); break;
                                case EdgeClueType.Rel: newPossibleValues[cFace2] = newPossibleValues[cFace2].Where(i => Math.Sign(i - value) == cValue).ToArray(); break;
                            }
                            if (newPossibleValues[cFace2].Length == 0)
                                goto busted;
                        }

                        foreach (var sol in recurse(clues, sofar, used, newPossibleValues))
                            yield return sol;

                        busted:;
                    }
                    used[bestFace] = false;
                }

                var allClues = polyhedron.Faces
                    .SelectMany((face, faceIx) => polyhedron.FindAdjacent(faceIx).Select(adjFaceIx => (face1: faceIx, face2: adjFaceIx)))
                    .Where(tup => tup.face1 < tup.face2)
                    .SelectMany(edge => generateClues(edge.face1, edge.face2))
                    .ToArray()
                    .Shuffle(rnd);

                if (recurse(allClues, new int[n], new bool[n], Ut.NewArray(n, _ => Enumerable.Range(minValue, maxValue - minValue + 1).ToArray())).Skip(1).Any())
                    // Puzzle is not unique
                    Debugger.Break();
                else
                {
                    File.WriteAllText(@"D:\c\Qoph\DataFiles\Face To Face\Edge Puzzle.svg",
                        generateNet(polyhedron, edgeSvg: (f1, f2, x, y) => $"<text x='{x}' y='{y + .06}' stroke='white' stroke-width='.05' paint-order='stroke' fill='black' font-size='.2' text-anchor='middle'>{solution[f1] + solution[f2]}</text>").svg);
                    Debugger.Break();
                }
            }
        }

        public static void TestVertexCluesPuzzle()
        {
            var n = 24;
            var rnd = new Random(47);
            var solution = Enumerable.Range(1, 26).Except(new[] { 2, 21 }).ToArray().Shuffle(rnd);
            if (solution.Length != n)
                Debugger.Break();

            foreach (var polyhedron in getPolyhedra())
            {
                IEnumerable<(int vx, VertexClueType type, int value)> generateClues(int vx)
                {
                    var list = new List<(int vx, VertexClueType type, int value)>();
                    list.Add((vx, VertexClueType.Sum, Enumerable.Range(0, n).Where(fx => polyhedron.Faces[fx].Contains(vx)).Sum(fx => solution[fx])));

                    //var product = Enumerable.Range(0, n).Where(fx => polyhedron.Faces[fx].Contains(vx)).Aggregate(1, (prev, fx) => prev * solution[fx]);
                    //if (new[] { 2, 3, 5, 7 }.Count(f => product % f == 0) >= 3)
                    //    list.Add((vx, VertexClueType.Product, product));

                    //list.Add((vx, VertexClueType.NumberOfEvens, Enumerable.Range(0, n).Where(fx => polyhedron.Faces[fx].Contains(vx)).Count(fx => solution[fx] % 2 == 0)));
                    //yield return list.PickRandom(rnd);
                    return list;
                }

                if (polyhedron.Faces.Length != n)
                    continue;
                //var polyhedron = parse(@"D:\c\Qoph\DataFiles\Face To Face\Txt\SelfDualIcosioctahedron4.txt");
                var allClues = Enumerable.Range(0, polyhedron.Vertices.Length)
                    .SelectMany(vx => generateClues(vx))
                    .ToArray();
                var eqs = allClues.Select((clue, clueIx) => $"{clue.value}={Enumerable.Range(0, polyhedron.Faces.Length).Where(f => polyhedron.Faces[f].Contains(clue.vx)).Select(f => $"f{f}").JoinString("+")}").JoinString(", ");
                Clipboard.SetText($"solve({{{eqs}}}, {{{Enumerable.Range(0, polyhedron.Faces.Length).Select(f => $"f{f}").JoinString(", ")}}});");
                Console.WriteLine(polyhedron.Name);
                Console.ReadLine();
            }
        }

        enum VertexClueType { Sum, Product, NumberOfEvens }

        public static void GenerateVertexClues()
        {
            var polyhedron = parse(@"D:\c\Qoph\DataFiles\Face To Face\Txt\SelfDualIcosioctahedron4.txt");
            Console.WriteLine(polyhedron.Faces.Length);
            var n = polyhedron.Faces.Length;

            var rnd = new Random(47);
            var solution = Enumerable.Range(1, 26).Concat(new[] { 2, 21 }).ToArray().Shuffle(rnd);
            if (solution.Length != n)
                Debugger.Break();

            IEnumerable<(int vx, VertexClueType type, int value)> generateClues(int vx)
            {
                var list = new List<(int vx, VertexClueType type, int value)>();
                list.Add((vx, VertexClueType.Sum, Enumerable.Range(0, n).Where(fx => polyhedron.Faces[fx].Contains(vx)).Sum(fx => solution[fx])));

                //var product = Enumerable.Range(0, n).Where(fx => polyhedron.Faces[fx].Contains(vx)).Aggregate(1, (prev, fx) => prev * solution[fx]);
                //if (new[] { 2, 3, 5, 7 }.Count(f => product % f == 0) >= 3)
                //    list.Add((vx, VertexClueType.Product, product));

                //list.Add((vx, VertexClueType.NumberOfEvens, Enumerable.Range(0, n).Where(fx => polyhedron.Faces[fx].Contains(vx)).Count(fx => solution[fx] % 2 == 0)));
                //yield return list.PickRandom(rnd);
                return list;
            }

            var minValue = 1;// solution.Min();
            var maxValue = 26; // solution.Max();
            var maxLength = Math.Max(minValue.ToString().Length, maxValue.ToString().Length);
            ConsoleUtil.WriteLine($"SOLTN: {solution.Select(i => i.ToString().PadLeft(maxLength)).JoinString(", ").Color(ConsoleColor.Yellow)}", null);

            IEnumerable<int[]> recurse((int vx, VertexClueType type, int value)[] clues, int[] sofar, bool[] used, int[][] possibleValues)
            {
                var bestFace = -1;
                var bestFaceScore = -1;
                for (var i = 0; i < n; i++)
                {
                    if (used[i])
                        continue;
                    var numClues = clues.Count(c => polyhedron.Faces[i].Contains(c.vx));
                    var score = (maxValue - minValue + 1) - possibleValues[i].Length + 2 * numClues;
                    if (score > bestFaceScore)
                    {
                        bestFaceScore = score;
                        bestFace = i;
                    }
                }
                if (bestFace == -1)
                {
                    yield return sofar;
                    yield break;
                }

                if (possibleValues[bestFace].Length == 0)
                    yield break;
                used[bestFace] = true;
                foreach (var value in possibleValues[bestFace])
                {
                    //for (var i = 0; i < n; i++)
                    //    if (i != bestFace && used[i] && sofar[i] == value)
                    //        goto busted;

                    sofar[bestFace] = value;
                    var newPossibleValues = (int[][]) possibleValues.Clone();
                    newPossibleValues[bestFace] = new[] { value };
                    foreach (var (cVx, cType, cValue) in clues)
                    {
                        if (!polyhedron.Faces[bestFace].Contains(cVx))
                            continue;
                        var faces = Enumerable.Range(0, n).Where(fx => polyhedron.Faces[fx].Contains(cVx)).ToArray();
                        var unusedFaces = faces.Where(f => !used[f]).ToArray();

                        switch (cType)
                        {
                            case VertexClueType.Sum:
                                if (unusedFaces.Length == 0 && faces.Sum(fx => sofar[fx]) != cValue)
                                    goto busted;
                                else if (unusedFaces.Length == 1)
                                {
                                    var required = cValue - faces.Sum(fx => used[fx] ? sofar[fx] : 0);
                                    if (!newPossibleValues[unusedFaces[0]].Contains(required))
                                        goto busted;
                                    newPossibleValues[unusedFaces[0]] = new[] { required };
                                }
                                else if (unusedFaces.Length == 2)
                                {
                                    var required = cValue - faces.Sum(fx => used[fx] ? sofar[fx] : 0);
                                    newPossibleValues[unusedFaces[0]] = newPossibleValues[unusedFaces[0]].Where(v => newPossibleValues[unusedFaces[1]].Any(v2 => v + v2 == required)).ToArray();
                                    newPossibleValues[unusedFaces[1]] = newPossibleValues[unusedFaces[1]].Where(v => newPossibleValues[unusedFaces[0]].Any(v2 => v + v2 == required)).ToArray();
                                    if (newPossibleValues[unusedFaces[0]].Length == 0 || newPossibleValues[unusedFaces[1]].Length == 0)
                                        goto busted;
                                }
                                // Check the smallest and largest possible sums
                                else
                                {
                                    foreach (var unusedFace in unusedFaces)
                                    {
                                        var min = cValue - faces.Sum(fx => used[fx] ? sofar[fx] : fx != unusedFace ? newPossibleValues[fx].Max() : 0);
                                        var max = cValue - faces.Sum(fx => used[fx] ? sofar[fx] : fx != unusedFace ? newPossibleValues[fx].Min() : 0);
                                        newPossibleValues[unusedFace] = newPossibleValues[unusedFace].Where(v => v >= min && v <= max).ToArray();
                                        if (newPossibleValues[unusedFace].Length == 0)
                                            goto busted;
                                    }
                                }

                                break;

                            case VertexClueType.Product:
                                var productSoFar = faces.Aggregate(1, (prev, fx) => used[fx] ? sofar[fx] * prev : prev);
                                if (cValue % productSoFar != 0)
                                    goto busted;
                                if (unusedFaces.Length == 1)
                                {
                                    var required = cValue / productSoFar;
                                    if (!newPossibleValues[unusedFaces[0]].Contains(required))
                                        goto busted;
                                    newPossibleValues[unusedFaces[0]] = new[] { required };
                                }
                                // Check the smallest and largest possible products
                                else if (faces.Aggregate(1, (prev, fx) => prev * possibleValues[fx].Min()) > cValue || faces.Aggregate(1, (prev, fx) => prev * possibleValues[fx].Max()) < cValue)
                                    goto busted;

                                break;

                            case VertexClueType.NumberOfEvens:
                                var usedEvenCount = faces.Count(fx => used[fx] && sofar[fx] % 2 == 0);
                                if (usedEvenCount > cValue)
                                    goto busted;
                                else if (usedEvenCount == cValue)
                                    foreach (var uf in unusedFaces)
                                    {
                                        newPossibleValues[uf] = newPossibleValues[uf].Where(v => v % 2 != 0).ToArray();
                                        if (newPossibleValues[uf].Length == 0)
                                            goto busted;
                                    }
                                else if (usedEvenCount == cValue - unusedFaces.Length)
                                    foreach (var uf in unusedFaces)
                                    {
                                        newPossibleValues[uf] = newPossibleValues[uf].Where(v => v % 2 == 0).ToArray();
                                        if (newPossibleValues[uf].Length == 0)
                                            goto busted;
                                    }
                                else if (usedEvenCount < cValue - unusedFaces.Length)
                                    goto busted;
                                break;
                        }
                    }

                    foreach (var sol in recurse(clues, sofar, used, newPossibleValues))
                        yield return sol;

                    busted:;
                }
                used[bestFace] = false;
            }

            var allClues = Enumerable.Range(0, polyhedron.Vertices.Length)
                .SelectMany(vx => generateClues(vx))
                .ToArray();
            var eqs = allClues.Select((clue, clueIx) => $"{clue.value}={Enumerable.Range(0, polyhedron.Faces.Length).Where(f => polyhedron.Faces[f].Contains(clue.vx)).Select(f => $"f{f}").JoinString("+")}").JoinString(", ");
            Clipboard.SetText($"solve({{{eqs}}}, {{{Enumerable.Range(0, polyhedron.Faces.Length).Select(f => $"f{f}").JoinString(", ")}}});");
            ConsoleColoredString colored((int vx, VertexClueType type, int value) clue) =>
                new ConsoleColoredString($"{clue.vx.ToString().Color(ConsoleColor.DarkCyan)} ({Enumerable.Range(0, n).Where(fx => polyhedron.Faces[fx].Contains(clue.vx)).JoinString(",")}) = {clue.type.ToString().Color(new[] { ConsoleColor.Green, ConsoleColor.Yellow, ConsoleColor.Magenta }[(int) clue.type])} {clue.value.ToString().Color(ConsoleColor.Red)}")
                    .ColorWhereNull(ConsoleColor.DarkGray);
            int[][] getInitialPossibleValues((int vx, VertexClueType type, int value)[] clues)
            {
                var initialPossibleValues = Enumerable.Range(0, n).Select(fx =>
                {
                    var values = Enumerable.Range(minValue, maxValue - minValue + 1);
                    foreach (var (vx, type, value) in allClues)
                    {
                        if (!polyhedron.Faces[fx].Contains(vx))
                            continue;
                        if (type == VertexClueType.Sum)
                            values = values.Where(v => v <= value);
                        else if (type == VertexClueType.Product && value == 0)
                            values = values.Where(v => v == 0);
                        else if (type == VertexClueType.Product && value != 0)
                            values = values.Where(v => v <= value && v != 0 && value % v == 0);
                        else if (type == VertexClueType.NumberOfEvens && value == 0)
                            values = values.Where(v => v % 2 != 0);
                        else if (type == VertexClueType.NumberOfEvens && value == polyhedron.Faces.Count(f => f.Contains(vx)))
                            values = values.Where(v => v % 2 == 0);
                    }
                    return values.ToArray();
                }).ToArray();
                return initialPossibleValues;
            }

            var count = 0;
            foreach (var sol in recurse(allClues, new int[n], new bool[n], getInitialPossibleValues(allClues)))
            {
                ConsoleUtil.WriteLine($"FOUND: {sol.Select(i => i.ToString().PadLeft(maxLength)).JoinString(", ").Color(ConsoleColor.Yellow)}", null);
                count++;
                if (count > 1)
                    break;
            }
            if (count != 1)
            {
                Console.WriteLine("ALL CLUES:");
                ConsoleUtil.WriteLine(allClues.Select(colored).JoinColoredString("\n"));
                Console.WriteLine();

                // Clues are ambiguous or impossible
                Debugger.Break();
            }

            if (recurse(allClues, new int[n], new bool[n], getInitialPossibleValues(allClues)).Skip(1).Any())
                // Puzzle is not unique
                Debugger.Break();

            //foreach (var sol in recurse(requiredClues, new int[n], new bool[n], getInitialPossibleValues(requiredClues)))
            //    ConsoleUtil.WriteLine($"FOUND: {sol.Select(i => i.ToString().PadLeft(maxLength)).JoinString(", ").Color(ConsoleColor.Yellow)}", null);

            File.WriteAllText(@"D:\c\Qoph\DataFiles\Face To Face\Vertex Puzzle.svg",
                generateNet(polyhedron, vertexSvg: (vx, x, y) => $"<text x='{x}' y='{y + .06}' stroke='white' stroke-width='.05' paint-order='stroke' fill='black' font-size='.2' text-anchor='middle'>{Enumerable.Range(0, polyhedron.Faces.Length).Where(fx => polyhedron.Faces[fx].Contains(vx)).Sum(fx => solution[fx])}</text>").svg);
        }

        enum FaceClueType { Sum, Product, NumberOfEvens }

        public static void GenerateFaceClues_OBSOLETE()
        {
            var polyhedron = parse(@"D:\c\Qoph\DataFiles\Face To Face\Txt\SelfDualIcosioctahedron4.txt");
            Console.WriteLine(polyhedron.Faces.Length);
            var n = polyhedron.Faces.Length;
            var adjs = Enumerable.Range(0, n).Select(fx => polyhedron.FindAdjacent(fx).ToArray()).ToArray();

            var rnd = new Random(47);
            var solution = Enumerable.Range(1, 26).Concat(new[] { 5, 9 }).ToArray().Shuffle(rnd);
            if (solution.Length != n)
                Debugger.Break();

            IEnumerable<(int fx, FaceClueType type, int value)> generateClues(int fx)
            {
                var list = new List<(int fx, FaceClueType type, int value)>();
                list.Add((fx, FaceClueType.Sum, adjs[fx].Sum(adjFx => solution[adjFx])));

                //var product = adjs[fx].Aggregate(1, (prev, adjFx) => prev * solution[adjFx]);
                //if (new[] { 2, 3, 5, 7 }.Count(f => product % f == 0) >= 3)
                //    list.Add((fx, FaceClueType.Product, product));

                //list.Add((fx, FaceClueType.NumberOfEvens, adjs[fx].Count(adjFx => solution[adjFx] % 2 == 0)));
                //yield return list.PickRandom(rnd);
                return list;
            }

            var minValue = solution.Min();
            var maxValue = solution.Max();
            var maxLength = Math.Max(minValue.ToString().Length, maxValue.ToString().Length);
            //ConsoleUtil.WriteLine($"INDEX: {Enumerable.Range(0, n).Select(i => i.ToString().PadLeft(maxLength)).JoinString(", ").Color(ConsoleColor.Blue)}", null);
            //ConsoleUtil.WriteLine($"SOLTN: {solution.Select(i => i.ToString().PadLeft(maxLength)).JoinString(", ").Color(ConsoleColor.Yellow)}", null);

            IEnumerable<int[]> recurse((int fx, FaceClueType type, int value)[] clues, int[] sofar, bool[] used, int[][] possibleValues)
            {
                var bestFace = -1;
                var bestFaceScore = -1;
                for (var i = 0; i < n; i++)
                {
                    if (used[i])
                        continue;
                    var score = (maxValue - minValue + 1) - possibleValues[i].Length + 2 * adjs[i].Length;
                    if (score > bestFaceScore)
                    {
                        bestFaceScore = score;
                        bestFace = i;
                    }
                }
                if (bestFace == -1)
                {
                    yield return sofar;
                    yield break;
                }

                if (possibleValues[bestFace].Length == 0)
                    yield break;
                used[bestFace] = true;
                foreach (var value in possibleValues[bestFace])
                {
                    //for (var i = 0; i < n; i++)
                    //    if (i != bestFace && used[i] && sofar[i] == value)
                    //        goto busted;

                    sofar[bestFace] = value;
                    var newPossibleValues = (int[][]) possibleValues.Clone();
                    newPossibleValues[bestFace] = new[] { value };
                    foreach (var (cFx, cType, cValue) in clues)
                    {
                        if (!adjs[cFx].Contains(bestFace))
                            continue;
                        var unusedFaces = adjs[cFx].Where(fx => !used[fx]).ToArray();

                        switch (cType)
                        {
                            case FaceClueType.Sum:
                                if (unusedFaces.Length == 0 && adjs[cFx].Sum(fx => sofar[fx]) != cValue)
                                    goto busted;
                                else if (unusedFaces.Length == 1)
                                {
                                    var required = cValue - adjs[cFx].Sum(fx => used[fx] ? sofar[fx] : 0);
                                    if (!newPossibleValues[unusedFaces[0]].Contains(required))
                                        goto busted;
                                    newPossibleValues[unusedFaces[0]] = new[] { required };
                                }
                                else if (unusedFaces.Length == 2)
                                {
                                    var required = cValue - adjs[cFx].Sum(fx => used[fx] ? sofar[fx] : 0);
                                    newPossibleValues[unusedFaces[0]] = newPossibleValues[unusedFaces[0]].Where(v => newPossibleValues[unusedFaces[1]].Any(v2 => v + v2 == required)).ToArray();
                                    newPossibleValues[unusedFaces[1]] = newPossibleValues[unusedFaces[1]].Where(v => newPossibleValues[unusedFaces[0]].Any(v2 => v + v2 == required)).ToArray();
                                    if (newPossibleValues[unusedFaces[0]].Length == 0 || newPossibleValues[unusedFaces[1]].Length == 0)
                                        goto busted;
                                }
                                // Check the smallest and largest possible sums
                                else
                                {
                                    foreach (var unusedFace in unusedFaces)
                                    {
                                        var min = cValue - adjs[cFx].Sum(fx => used[fx] ? sofar[fx] : fx != unusedFace ? newPossibleValues[fx].Max() : 0);
                                        var max = cValue - adjs[cFx].Sum(fx => used[fx] ? sofar[fx] : fx != unusedFace ? newPossibleValues[fx].Min() : 0);
                                        newPossibleValues[unusedFace] = newPossibleValues[unusedFace].Where(v => v >= min && v <= max).ToArray();
                                        if (newPossibleValues[unusedFace].Length == 0)
                                            goto busted;
                                    }
                                }

                                break;

                            case FaceClueType.Product:
                                var productSoFar = adjs[cFx].Aggregate(1, (prev, fx) => used[fx] ? sofar[fx] * prev : prev);
                                if (cValue % productSoFar != 0)
                                    goto busted;
                                if (unusedFaces.Length == 1)
                                {
                                    var required = cValue / productSoFar;
                                    if (!newPossibleValues[unusedFaces[0]].Contains(required))
                                        goto busted;
                                    newPossibleValues[unusedFaces[0]] = new[] { required };
                                }
                                // Check the smallest and largest possible products
                                else if (adjs[cFx].Aggregate(1, (prev, fx) => prev * possibleValues[fx].Min()) > cValue || adjs[cFx].Aggregate(1, (prev, fx) => prev * possibleValues[fx].Max()) < cValue)
                                    goto busted;

                                break;

                            case FaceClueType.NumberOfEvens:
                                var usedEvenCount = adjs[cFx].Count(fx => used[fx] && sofar[fx] % 2 == 0);
                                if (usedEvenCount > cValue)
                                    goto busted;
                                else if (usedEvenCount == cValue)
                                    foreach (var uf in unusedFaces)
                                    {
                                        newPossibleValues[uf] = newPossibleValues[uf].Where(v => v % 2 != 0).ToArray();
                                        if (newPossibleValues[uf].Length == 0)
                                            goto busted;
                                    }
                                else if (usedEvenCount == cValue - unusedFaces.Length)
                                    foreach (var uf in unusedFaces)
                                    {
                                        newPossibleValues[uf] = newPossibleValues[uf].Where(v => v % 2 == 0).ToArray();
                                        if (newPossibleValues[uf].Length == 0)
                                            goto busted;
                                    }
                                else if (usedEvenCount < cValue - unusedFaces.Length)
                                    goto busted;
                                break;
                        }
                    }

                    foreach (var sol in recurse(clues, sofar, used, newPossibleValues))
                        yield return sol;

                    busted:;
                }
                used[bestFace] = false;
            }

            var allClues = Enumerable.Range(0, polyhedron.Vertices.Length)
                .SelectMany(vx => generateClues(vx))
                .ToArray()
                .Shuffle(rnd);
            //ConsoleColoredString colored((int fx, FaceClueType type, int value) clue) =>
            //    new ConsoleColoredString($"{clue.fx.ToString().Color(ConsoleColor.DarkCyan)} ({adjs[clue.fx].JoinString(",")}) = {clue.type.ToString().Color(new[] { ConsoleColor.Green, ConsoleColor.Yellow, ConsoleColor.Magenta }[(int) clue.type])} {clue.value.ToString().Color(ConsoleColor.Red)}")
            //        .ColorWhereNull(ConsoleColor.DarkGray);

            int[][] getInitialPossibleValues((int fx, FaceClueType type, int value)[] clues)
            {
                var initialPossibleValues = Enumerable.Range(0, n).Select(fx =>
                {
                    var values = Enumerable.Range(minValue, maxValue - minValue + 1);
                    foreach (var (cFx, type, value) in allClues)
                    {
                        if (!adjs[fx].Contains(cFx))
                            continue;
                        if (type == FaceClueType.Sum)
                            values = values.Where(v => v <= value);
                        else if (type == FaceClueType.Product && value == 0)
                            values = values.Where(v => v == 0);
                        else if (type == FaceClueType.Product && value != 0)
                            values = values.Where(v => v <= value && v != 0 && value % v == 0);
                        else if (type == FaceClueType.NumberOfEvens && value == 0)
                            values = values.Where(v => v % 2 != 0);
                        else if (type == FaceClueType.NumberOfEvens && value == adjs[cFx].Length)
                            values = values.Where(v => v % 2 == 0);
                    }
                    return values.ToArray();
                }).ToArray();
                return initialPossibleValues;
            }

            Console.WriteLine($"Puzzle is {(recurse(allClues, new int[n], new bool[n], getInitialPossibleValues(allClues)).Skip(1).Any() ? "ambiguous" : "UNIQUE")}");

            //var eqs = allClues.Select((clue, clueIx) => $"{clue.value}={adjs[clue.fx].Select(f => $"f{f}").JoinString("+")}").JoinString(", ");
            //Clipboard.SetText($"solve({{{eqs}}}, {{{Enumerable.Range(0, polyhedron.Faces.Length).Select(f => $"f{f}").JoinString(", ")}}});");
            //Console.WriteLine(allClues.Length);
        }

        public static void FindColors()
        {
            var colors = File.ReadLines(@"D:\c\Qoph\DataFiles\Face To Face\Color names.txt")
                .Select(col => col.ToUpperInvariant().Where(c => c >= 'A' && c <= 'Z').JoinString())
                .Where(col => col.Length < 8)
                .ToArray();
            var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".OrderBy(ch => colors.Sum(str => str.Count(c => c == ch))).JoinString();

            var bestLength = int.MaxValue;

            IEnumerable<string[]> recurse(string[] sofar, string remainingLetters)
            {
                if (sofar.Length >= bestLength)
                    yield break;
                if (remainingLetters.Length == 0)
                {
                    bestLength = sofar.Length;
                    yield return sofar;
                    yield break;
                }
                foreach (var col in colors)
                    if (col.Contains(remainingLetters[0]))
                        foreach (var solution in recurse(sofar.Insert(sofar.Length, col), remainingLetters.Skip(1).Where(ch => !col.Contains(ch)).JoinString()))
                            yield return solution;
            }

            foreach (var solution in recurse(new string[0], letters))
                Console.WriteLine($"{solution.Length}: {solution.JoinString(" // ")}");
        }

        public static void GenerateBinairo()
        {
            var polyhedron = parse(@"D:\c\Qoph\DataFiles\Face To Face\Txt\SelfDualIcosioctahedron4.txt");
            var faces = polyhedron.Faces;
            var n = faces.Length;
            if (faces.Any(f => f.Length != 3 && f.Length != 4))
                Debugger.Break();
            var adjs = Enumerable.Range(0, n).Select(fx => polyhedron.FindAdjacent(fx).ToArray()).ToArray();

            IEnumerable<bool[]> recurse(bool?[] sofar, int ix)
            {
                retry:
                bool mustRetry = false;
                bool contradiction = false;
                void check(int face, bool value)
                {
                    if (sofar[face] == null)
                    {
                        sofar[face] = value;
                        mustRetry = true;
                    }
                    else if (sofar[face].Value == !value)
                        contradiction = true;
                }
                for (var f1 = 0; f1 < n; f1++)
                {
                    if (sofar[f1] == null)
                        continue;
                    foreach (var f2 in adjs[f1])
                    {
                        if (sofar[f2] != sofar[f1])
                            continue;

                        if (faces[f1].Length == 3)
                        {
                            foreach (var f in adjs[f1])
                                if (f != f2)
                                    check(f, !sofar[f1].Value);
                        }
                        else if (faces[f1].Length == 4)
                            check((Array.IndexOf(adjs[f1], f2) + 2) % 4, !sofar[f1].Value);
                        else
                            Debugger.Break();

                        if (contradiction)
                            yield break;
                    }
                }
                if (mustRetry)
                    goto retry;

                while (ix < sofar.Length && sofar[ix] != null)
                    ix++;
                if (ix == sofar.Length)
                {
                    yield return sofar.Select(b => b.Value).ToArray();
                    yield break;
                }

                foreach (var b in new[] { false, true })
                {
                    sofar[ix] = b;
                    foreach (var solution in recurse((bool?[]) sofar.Clone(), ix + 1))
                        yield return solution;
                }
            }

            var solution = recurse(new bool?[n], 0).First();
            var givens = Ut.ReduceRequiredSet(Enumerable.Range(0, n).ToList().Shuffle(), test: state =>
            {
                var sofar = new bool?[n];
                foreach (var f in state.SetToTest)
                    sofar[f] = solution[f];
                Console.WriteLine(sofar.Select(b => b != null ? "█" : "░").JoinString());
                return !recurse(sofar, 0).Skip(1).Any();
            });
            Console.WriteLine(givens.JoinString(", "));

            File.WriteAllText(@"D:\c\Qoph\DataFiles\Face To Face\Binairo Puzzle.svg",
                //stroke='white' stroke-width='.05' paint-order='stroke' 
                generateNet(polyhedron, faceSvg: (f, x, y) => givens.Contains(f) ? $"<text x='{x}' y='{y + .06}' fill='black' font-size='.2' text-anchor='middle'>{(solution[f] ? "1" : "0")}</text>" : null).svg);
        }

        public static void GenerateTemplate()
        {
            var polyhedron = parse(@"D:\c\Qoph\DataFiles\Face To Face\Txt\SelfDualIcosioctahedron4.txt");
            File.WriteAllText(@"D:\c\Qoph\DataFiles\Face To Face\Template.svg",
                //stroke='white' stroke-width='.05' paint-order='stroke' 
                generateNet(polyhedron, faceColor: f => "#def", faceSvg: (f, x, y) => $"<text x='{x}' y='{y + .06}' fill='black' font-size='.2' text-anchor='middle'>{f}</text>").svg);
        }

        public static void Planning()
        {
            var p = parse(@"D:\c\Qoph\DataFiles\Face To Face\Txt\LpentagonalIcositetrahedron.txt");

            Func<int, double, double, string> textTagger((string word, int[] faces, string color)[] data) => (face, x, y) =>
            {
                var (word, faces, color) = data.Where(tup => tup.faces.Contains(face)).FirstOrDefault(("?", new[] { face }, "#fff"));
                return $"<text x='{x}' y='{y + .06}' fill='black' font-size='.2' text-anchor='middle'>{word[faces.IndexOf(face)]}</text>";
            };
            Func<int, string> faceColorer((string word, int[] faces, string color)[] data) => face => data.Where(tup => tup.faces.Contains(face)).Select(tup => tup.color).FirstOrDefault("#fff");

            object makePiece(string puzzleType, string puzzleHint, params (string word, int[] faces, string color)[] data) => new DIV { class_ = "piece" }._(
                new H1(Enumerable.Range(0, 26).Select(i => (char) ('A' + i)).Where(ch => data.All(tup => !tup.word.Contains(ch))).JoinString()),
                new H2(puzzleType),
                new H3(puzzleHint),
                Enumerable.Range(0, 24).Where(face => data.All(tup => !tup.faces.Contains(face))).ToArray().Apply(missingFaces =>
                    missingFaces.Length == 0 ? null : new H4($"({missingFaces.JoinString(", ")})")),
                new RawTag(generateNet(p, faceSvg: textTagger(data), faceColor: faceColorer(data)).svg));

            File.WriteAllText(@"D:\c\Qoph\DataFiles\Face To Face\Planning.html",
                new HTML(
                    new HEAD(
                        new TITLE("Face to Face planning page"),
                        new META { httpEquiv = "Content-Type", content = "text/html; charset=utf-8" },
                        new STYLELiteral($@"
svg {{ width: 9cm; margin: 0 auto; display: block; }}
h1, h2, h3, h4 {{ text-align: center; }}
h1 {{ font-size: 20pt; }}
h2 {{ font-size: 17pt; }}
h3 {{ font-size: 14pt; }}
.piece {{ display: inline-block; vertical-align: top; }}")
                    ),
                    new BODY(
                        new DIV { id = "all" }._(
                            makePiece("Carpet colors", "CYAN SUM",
                                (word: "CYAN", faces: new[] { 8, 11, 0, 1 }, color: "#afa"),
                                (word: "SUM", faces: new[] { 15, 12, 6 }, color: "#ffa"),
                                (word: "Q", faces: new[] { 22 }, color: "#faa"),
                                (word: "BDFGHJKLOPRTVWXZ", faces: new[] { 2, 3, 4, 5, 7, 9, 10, 13, 14, 16, 17, 18, 19, 20, 21, 23 }, color: "#fff")),
                            makePiece("Edge sums (cyan)", "PINK SUM",
                                (word: "PINK", faces: new[] { 8, 11, 0, 1 }, color: "#afa"),
                                (word: "SUM", faces: new[] { 15, 12, 6 }, color: "#ffa"),
                                (word: "Q", faces: new[] { 22 }, color: "#faa"),
                                (word: "ABDEGHJLOFRVTZYX", faces: new[] { 2, 3, 4, 5, 7, 9, 10, 13, 14, 16, 17, 18, 19, 20, 21, 23 }, color: "#fff")),
                            makePiece("Vertex sums (pink)", "LYRICS NEXT WORD",
                                (word: "LYRICS", faces: new[] { 8, 11, 0, 1, 15, 12 }, color: "#afa"),
                                (word: "NEXT", faces: new[] { 14, 13, 5, 4 }, color: "#ffa"),
                                (word: "WOD", faces: new[] { 9, 10, 3 }, color: "#adf"),
                                (word: "Q", faces: new[] { 22 }, color: "#faa"),
                                (word: "JGHFKMPVUZ", faces: new[] { 2, 6, 7, 16, 17, 18, 19, 20, 21, 23 }, color: "#fff")),
                            makePiece("Lyrics", "GASHLYCRUMB TINS",
                                (word: "GASHLYCRUMB", faces: new[] { 8, 11, 0, 1, 15, 12, 16, 19, 9, 4, 5 }, color: "#afa"),
                                (word: "TIN", faces: new[] { 21, 20, 3 }, color: "#ffa"),
                                (word: "Q", faces: new[] { 22 }, color: "#faa"),
                                (word: "DJKOPVWXZ", faces: new[] { 6, 7, 10, 13, 14, 17, 18, 2, 23 }, color: "#fff")),
                            makePiece("Gashlycrumb Tinies", "LOCK IS BAR",
                                (word: "LOCK", faces: new[] { 8, 11, 0, 1 }, color: "#afa"),
                                (word: "IS", faces: new[] { 15, 12 }, color: "#ffa"),
                                (word: "BAR", faces: new[] { 16, 19, 9 }, color: "#adf"),
                                (word: "Q", faces: new[] { 22 }, color: "#faa"),
                                (word: "EFGHJMNPUVWXYZ", faces: new[] { 2, 3, 4, 5, 6, 7, 10, 13, 14, 17, 18, 20, 21, 23 }, color: "#fff")),
                            makePiece("Crossword", "CARPET INDEX",
                                (word: "CARPET", faces: new[] { 8, 11, 0, 1, 15, 12 }, color: "#afa"),
                                (word: "INDX", faces: new[] { 23, 3, 2, 17 }, color: "#ffa"),
                                (word: "Q", faces: new[] { 22 }, color: "#faa"),
                                (word: "FGHJKLMSUVWYZ", faces: new[] { 4, 5, 6, 7, 9, 10, 13, 14, 16, 18, 19, 20, 21 }, color: "#fff")),
                            //makePiece("Words from lyrics", "LOCKED MEANS BAR",
                            //    (word: "LOCKED", faces: new[] { 4, 5, 24, 10, 9, 27 }, color: "#afa"),
                            //    (word: "MEANS", faces: new[] { 12, 0, 20, 19, 6 }, color: "#ffa"),
                            //    (word: "BAR", faces: new[] { 14, 2, 18 }, color: "#acf"),
                            //    (word: "UGHIPJQTFVWXYZ", faces: new[] { 1, 3, 7, 8, 11, 13, 15, 16, 17, 21, 22, 23, 25, 26 }, color: "#fff")),
                            new DIV { class_ = "piece" }._(new RawTag(generateNet(p, faceSvg: (f, x, y) => $"<text x='{x}' y='{y + .06}' fill='black' font-size='.2' text-anchor='middle'>{f}</text>").svg))
                        )
                    )
                ).ToString()
            );
        }
    }
}
