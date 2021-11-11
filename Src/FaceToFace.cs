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
using Qoph.Modeling;
using RT.KitchenSink;
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
        private static readonly Polyhedron _polyhedron = parse(@"D:\c\Qoph\DataFiles\Face To Face\Txt\LpentagonalIcositetrahedron.txt");

        private static readonly (string word, int clueSize, string clue, int[] cells)[] _crosswordLights = Ut.NewArray(
            ("NEED", 18, "Require\n(4)", new[] { 2, 14, 13, 5 }),
            ("DEAL", 9, "Agreement or dish\nout cards (4)", new[] { 5, 4, 9, 10 }),
            ("ONSET", 12, "Beginning\nphase (5)", new[] { 3, 23, 22, 4, 7 }),
            ("SHARING", 14, "Apportioning\n(7)", new[] { 22, 8, 11, 0, 1, 17, 16 }),
            ("NUN", 16, "Sister (3)", new[] { 23, 20, 2 }),
            ("TEN", 14, "X in\nRome (3)", new[] { 21, 14, 15 }),
            ("ROUTE", 14, "Itinerary\n(5)", new[] { 0, 3, 20, 21, 13 }),
            ("HATE", 17, "Loathe\n(4)", new[] { 8, 9, 7, 6 }),
            ("EGG", 18, "Ovum (3)", new[] { 6, 16, 19 }),
            ("ALONG", 11, "For the length\nof (5)", new[] { 11, 10, 18, 17, 12 }),
            ("GOING", 11, "Leaving or\nfunctioning (5)", new[] { 19, 18, 1, 15, 12 }));

        private struct DistrInfo
        {
            public string Puzzle;
            public string Clue;
            public (string word, int[] faces, int? color)[] Distribution;
            public DistrInfo(string puzzle, string clue, params (string word, int[] faces, int? color)[] distribution)
            {
                Puzzle = puzzle;
                Clue = clue;
                Distribution = distribution;
            }
        }

        private static readonly DistrInfo _carpetColors = new DistrInfo("Carpet colors", "LAMP BROS",
                (word: "Q", faces: new[] { 22 }, color: 0),
                (word: "LAMP", faces: new[] { 8, 11, 0, 1 }, color: 1),
                (word: "BROS", faces: new[] { 15, 12, 16, 19 }, color: 2),
                (word: "EDFGHJKNIYTVUXZ", faces: new[] { 2, 3, 4, 5, 7, 9, 10, 13, 14, 17, 18, 6, 20, 21, 23 }, color: null));
        private static readonly DistrInfo _smashChars = new DistrInfo("Smash Bros characters", "CYAN SUM",
                (word: "Q", faces: new[] { 22 }, color: 0),
                (word: "CYAN", faces: new[] { 8, 11, 0, 1 }, color: 1),
                (word: "SUM", faces: new[] { 15, 12, 16 }, color: 2),
                (word: "TKHDLVOBRJZGXWPF", faces: new[] { 2, 3, 5, 7, 10, 13, 14, 6, 17, 18, 20, 21, 23, 19, 9, 4 }, color: null));
        private static readonly DistrInfo _cyanSums = new DistrInfo("Vertex sums (cyan numbers)", "LYRICS NEXT WORD",
                (word: "Q", faces: new[] { 22 }, color: 0),
                (word: "LYRICS", faces: new[] { 8, 11, 0, 1, 15, 12 }, color: 1),
                (word: "NEXT", faces: new[] { 14, 13, 5, 4 }, color: 2),
                (word: "WOD", faces: new[] { 9, 10, 3 }, color: 3),
                (word: "JGHFKMPVUZ", faces: new[] { 2, 6, 7, 16, 17, 18, 19, 20, 21, 23 }, color: null));
        private static readonly DistrInfo _musicSnippets = new DistrInfo("Lyrics", "GASHLYCRUMB TINS",
                (word: "Q", faces: new[] { 22 }, color: 0),
                (word: "GASHLYCRUMB", faces: new[] { 8, 11, 0, 1, 15, 12, 16, 19, 9, 4, 5 }, color: 1),
                (word: "TIN", faces: new[] { 21, 20, 3 }, color: 2),
                (word: "DJKOPVWXZ", faces: new[] { 6, 7, 10, 13, 14, 17, 18, 2, 23 }, color: null));
        private static readonly DistrInfo _gashlycrumbTinies = new DistrInfo("Gashlycrumb Tinies", "LOCK IS BAR",
                (word: "Q", faces: new[] { 22 }, color: 0),
                (word: "LOCK", faces: new[] { 8, 11, 0, 1 }, color: 1),
                (word: "IS", faces: new[] { 15, 12 }, color: 2),
                (word: "BAR", faces: new[] { 16, 19, 9 }, color: 3),
                (word: "EFGHJMNPUVWXYZ", faces: new[] { 2, 3, 4, 5, 6, 7, 10, 13, 14, 17, 18, 20, 21, 23 }, color: null));
        private static readonly DistrInfo _crosswordAfterOffset = new DistrInfo("Crossword", "CARPET INDEX",
                (word: "Q", faces: new[] { 22 }, color: 0),
                (word: "CARPET", faces: new[] { 8, 11, 0, 1, 15, 12 }, color: 1),
                (word: "INDX", faces: new[] { 23, 3, 2, 17 }, color: 2),
                (word: "FGHKJLMSUVWYZ", faces: new[] { 4, 5, 6, 7, 9, 10, 13, 14, 16, 18, 19, 20, 21 }, color: null));

        private static readonly DistrInfo[] _distributions = new[] { _carpetColors, _smashChars, _cyanSums, _musicSnippets, _gashlycrumbTinies, _crosswordAfterOffset };

        private static readonly string[] _carpetColorNames = "WHITE,AQUA,AZURE,FUCHSIA,JADE,VIOLET,ONYX,PINK,GAMBOGE".Split(',');

        private static readonly (string author, string title, string givenLyrics, string nextWord)?[] _songTitles = Ut.NewArray<(string title, string author, string givenLyrics, string nextWord)?>(
            ("Fools Garden", "Lemon Tree", "I’m sitting here in a boring room, it’s just another rainy Sunday...", "Afternoon"),
            ("The Weeknd", "Blinding Lights", "So I hit the road in overdrive...", "Baby"),
            ("Jonathan Coulton", "Still Alive", "... there’s no sense crying over every mistake. You just keep on trying till you run out of...", "Cake"),
            ("John Lennon", "Imagine", "... sharing all the world, yoo-hoo... You may say I’m a...", "Dreamer"),
            null,
            null,
            ("David Bowie", "Space Oddity", "Take your protein pills and put your helmet on...", "Ground"),
            ("Bonnie Tyler", "Holding Out for a Hero", "And where are all the Gods? Where’s the streetwise...", "Hercules"),
            ("Aqua", "Barbie Girl", "You can brush my hair, undress me everywhere...", "Imagination"),
            ("Lorde", "Royals", "We’re driving Cadillacs in our dreams, but everybody’s like Cristal, Maybach, diamonds on your timepiece...", "Jet"),
            ("Toto", "Africa", "I know that I must do what’s right, as sure as...", "Kilimanjaro"),
            ("Kanye West", "Stronger", "Bow in the presence of greatness, ’cause right now thou hast forsaken us. You should be honored by my...", "Lateness"),
            ("Ed Sheeran", "Shape of You", "I’m in love with the shape of you. We push and pull like a...", "Magnet"),
            ("Tears for Fears", "Everybody Wants to Rule the World", "Acting on your best behaviour, turn your back on mother...", "Nature"),
            ("Queen", "Bohemian Rhapsody", "... caught in a landslide, no escape from reality...", "Open"),
            ("Elton John", "I'm Still Standing", "After all this time, picking up the...", "Pieces"),
            ("Boney M.", "Rasputin", "Ra-Ra-Rasputin, lover of the Russian...", "Queen"),
            ("Electric Light Orchestra", "Mr. Blue Sky", "Hey you with the pretty face, welcome to the human...", "Race"),
            ("Playboi Carti", "Magnolia", "In New York I Milly Rock, hide it in my...", "Sock"),
            ("Drake", "God's Plan", "She say, “Do you love me?” I...", "Tell"),
            ("The Bangles", "Eternal Flame", "Can you feel my heart beating? Do you...", "Understand"),
            ("John Denver", "Take Me Home, Country Roads", "... I belong, West...", "Virginia"),
            ("Robbie Williams", "Angels", "... whether I’m right or wrong. And down the...", "Waterfall"),
            ("Tom Lehrer", "The Elements", "... also mendelevium, einsteinium, nobelium, and argon, krypton, neon, radon, ...", "Xenon"),
            ("The Beatles", "Yellow Submarine", "And we lived beneath the waves in our...", "Yellow"),
            ("Spice Girls", "Wannabe", "... wanna (ha), I wanna (ha), I wanna (ha), I wanna (ha), I wanna really really really wanna...", "Zigazig"));

        public static readonly string[] _gashlycrumbTiniesObjects = Ut.NewArray(
            "a piece of banister",
            "a plush toy",
            "a pile of trash",
            "a sled",
            "a bucket of orange paint",
            "a worm",
            "a doormat",
            "a copy of the album \"So Much Fun\"",
            "a painting of Queen Victoria",
            "a beaker of alkaline solution",
            "a body spray",
            "a nail",
            "a bottle of salty water",
            "a copy of the funnies with their beginning and end torn off and the rest scrambled",
            "a piercing",
            "a copy of the third Super Smash Bros. game",
            "a pile of mud",
            "a box of matches",
            "a bottle of anticonvulsants",
            "a floppy disk",
            "a kitchen sink",
            "a toy railway car",
            "a frozen cube",
            "a rat",
            "a book of jokes",
            "a bottle of spirits");

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
            Func<int, int, int, int, EdgeD, string> edgeSvg = null,
            Func<int, int, PointD, string> vertexSvg = null,
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
                // Put first vertex at origin
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
                        var best = values.IndexOf(tup => ((tup.face[2] - tup.face[1]) * (tup.face[0] - tup.face[1])).Apply(nrml => Math.Abs(nrml.X) < closeness && Math.Abs(nrml.Y) < closeness && nrml.Z < 0));
                        if (best == -1)
                            throw new InvalidOperationException(@"No suitable angle found.");
                        q.Enqueue((toFaceIx, rotatedPolyhedron.Select(face => face.Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], values[best].angle)).ToArray()).ToArray()));
                    }
                    else
                    {
                        var p1 = polygons[fromFaceIx][fromEdgeIx];
                        var p2 = polygons[fromFaceIx][(fromEdgeIx + 1) % polygons[fromFaceIx].Length];
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

                            svg.Append($@"<path stroke='cornflowerblue' fill='none' id='curve-{fromFaceIx}-{fromEdgeIx}' d='{(
                                (p2m - p1m).Distance() < .5 ? $"M {p1m.X},{p1m.Y} L {p2m.X},{p2m.Y}" :
                                l1 >= 0 && l1 <= 1 && l2 >= 0 && l2 <= 1 ? $"M {p1m.X},{p1m.Y} C {intersect.X},{intersect.Y} {intersect.X},{intersect.Y} {p2m.X},{p2m.Y}" :
                                $"M {p1m.X},{p1m.Y} C {p1c.X},{p1c.Y} {p2c.X},{p2c.Y} {p2m.X},{p2m.Y}")}' />");
                        }

                        if (edgeSvg != null)
                            svgExtras.Append(edgeSvg(fromFaceIx, fromEdgeIx, toFaceIx, toEdgeIx, new EdgeD(p1, p2)));
                        if (vertexSvg != null)
                        {
                            svgExtras.Append(vertexSvg(fromFaceIx, fromEdgeIx, p1));
                            svgExtras.Append(vertexSvg(fromFaceIx, (fromEdgeIx + 1) % polygons[fromFaceIx].Length, p2));
                        }
                    }
                }
            }

            var xMin = polygons.Min(pg => pg?.Min(p => p.X)).Value;
            var yMin = polygons.Min(pg => pg?.Min(p => p.Y)).Value;
            var xMax = polygons.Max(pg => pg?.Max(p => p.X)).Value;
            var yMax = polygons.Max(pg => pg?.Max(p => p.Y)).Value;
            return (svg: $@"<svg class='polyhedron' xmlns='http://www.w3.org/2000/svg' viewBox='{xMin - .5} {yMin - .5} {xMax - xMin + 1} {yMax - yMin + 1}' stroke-width='{(xMax - xMin + 1) / 360}' font-family='Work Sans'>{svgFaces}{svg}{svgExtras}</svg>", polygons);
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

        public static void GenerateTemplate()
        {
            var polyhedron = parse(@"D:\c\Qoph\DataFiles\Face To Face\Txt\LpentagonalIcositetrahedron.txt");
            File.WriteAllText(@"D:\c\Qoph\DataFiles\Face To Face\Template.svg",
                //stroke='white' stroke-width='.05' paint-order='stroke'
                generateNet(polyhedron, faceColor: f => "#def", faceSvg: (f, x, y) => $"<text x='{x}' y='{y + .06}' fill='black' font-size='.2' text-anchor='middle'>{f}</text>").svg);
        }

        public static void Planning()
        {
            var p = parse(@"D:\c\Qoph\DataFiles\Face To Face\Txt\LpentagonalIcositetrahedron.txt");

            Func<int, double, double, string> textTagger((string word, int[] faces, int? color)[] data) => (face, x, y) =>
            {
                var (word, faces, color) = data.Where(tup => tup.faces.Contains(face)).FirstOrDefault(("?", new[] { face }, null));
                return $"<text x='{x}' y='{y + .06}' fill='black' font-size='.2' text-anchor='middle'>{word[faces.IndexOf(face)]}</text>";
            };
            var markingColors = new[] { "#faa", "#afa", "#ffa", "#adf" };
            Func<int, string> faceColorer((string word, int[] faces, int? color)[] data) => face => data.Where(tup => tup.faces.Contains(face)).Select(tup => tup.color == null ? "#fff" : markingColors[tup.color.Value]).FirstOrDefault("#fff");

            object makePiece(DistrInfo distrInfo) => new DIV { class_ = "piece" }._(
                new H1(Enumerable.Range(0, 26).Select(i => (char) ('A' + i)).Where(ch => distrInfo.Distribution.All(tup => !tup.word.Contains(ch))).JoinString()),
                new H2(distrInfo.Puzzle),
                new H3(distrInfo.Clue),
                Enumerable.Range(0, 24).Where(face => distrInfo.Distribution.All(tup => !tup.faces.Contains(face))).ToArray().Apply(missingFaces =>
                    missingFaces.Length == 0 ? null : new H4($"({missingFaces.JoinString(", ")})")),
                new RawTag(generateNet(p, faceSvg: textTagger(distrInfo.Distribution), faceColor: faceColorer(distrInfo.Distribution)).svg));

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
                            _distributions.Select(makePiece),
                            new DIV { class_ = "piece" }._(new RawTag(generateNet(p, faceSvg: (f, x, y) => $"<text x='{x}' y='{y + .06}' fill='black' font-size='.2' text-anchor='middle'>{f}</text>").svg))
                        )
                    )
                ).ToString()
            );
        }

        private static int findEdgeThatLeadsTo(int fromFace, int toFace)
        {
            var f1 = _polyhedron.Faces[fromFace];
            var f2 = _polyhedron.Faces[toFace];
            return Enumerable.Range(0, 5).Single(e => f2.Contains(f1[e]) && f2.Contains(f1[(e + 1) % 5]));
        }

        private static (int face, int edge)[] getLocked()
        {
            var edges = new List<(int face, int edge)>();
            for (var f = 0; f < _polyhedron.Faces.Length; f++)
                for (var e = 0; e < 5; e++)
                    edges.Add((f, e));

            foreach (var (word, clueSize, clue, light) in _crosswordLights)
            {
                foreach (var (f1Ix, f2Ix) in light.ConsecutivePairs(false))
                {
                    edges.Remove((f1Ix, findEdgeThatLeadsTo(f1Ix, f2Ix)));
                    edges.Remove((f2Ix, findEdgeThatLeadsTo(f2Ix, f1Ix)));
                }
            }

            return edges.ToArray();
        }

        public static (string letters, (int[] cells, string word)[] lights) GenerateCrossword()
        {
            var lights = Ut.NewArray(
                new[] { 5, 13, 14, 2 },
                new[] { 5, 4, 9, 10 },
                new[] { 3, 23, 22, 4, 7 },
                new[] { 22, 8, 11, 0, 1, 17, 16 },
                new[] { 23, 20, 2 },
                new[] { 21, 14, 15 },
                new[] { 13, 21, 20, 3, 0 },
                new[] { 6, 7, 9, 8 },
                new[] { 6, 16, 19 },
                new[] { 12, 17, 18, 10, 11 },
                new[] { 12, 15, 1, 18, 19 });

            var words = File.ReadAllLines(@"D:\Daten\Wordlists\VeryCommonWords.txt")
                .Except("DIE,LAD,YER,THE,THEN,SLAVE,HIM,EGO,RAPE,DROWN,TOMB,SATIN,ETC,HMM,EERIE,WARFARE,IDIOT,ARSON,HER,RNA,SHH,LOO,ARSE,ASS,HELL,WHALING".Split(','))
                .ToArray();

            int[][] getWords(int len) => words.Where(w => w.Length == len).SelectMany(w => new[] { w, w.Reverse().JoinString() }).Select(w => w.Select(ch => ch - 'A' + 1).ToArray()).ToArray();

            var puzzle = new Puzzle(24, 1, 26);
            foreach (var light in lights)
                puzzle.AddConstraint(new CombinationsConstraint(light, getWords(light.Length)));
            foreach (var faceIx in Enumerable.Range(0, 24))
                puzzle.AddConstraint(new OneCellLambdaConstraint(faceIx, v => Math.Abs(v - getFaceValue(faceIx, _crosswordAfterOffset)) < 17));

            var bestSolutionRating = int.MaxValue;
            int[] bestSolution = null;

            foreach (var solution in puzzle.Solve())
            {
                if (lights.Select(light => words.Contains(light.Select(cell => (char) (solution[cell] + 'A' - 1)).JoinString()) ? light[0] : light.Last()).Distinct().Count() < lights.Count())
                    continue;

                var rating = Enumerable.Range(0, 24).Sum(face => Math.Abs(solution[face] - getFaceValue(face, _crosswordAfterOffset)));
                if (rating < bestSolutionRating)
                {
                    var wordsUsed = new HashSet<string>();
                    foreach (var light in lights)
                    {
                        var word = light.Select(cell => (char) (solution[cell] + 'A' - 1)).JoinString();
                        if (words.Contains(word))
                            wordsUsed.Add(word);
                        else
                            wordsUsed.Add(word.Reverse().JoinString());
                    }
                    if (wordsUsed.Count < lights.Length)
                        continue;
                    Console.WriteLine(wordsUsed.JoinString("\n"));
                    Console.WriteLine($"Rating: {rating}");
                    Console.WriteLine();
                    bestSolutionRating = rating;
                    bestSolution = solution;
                }
            }
            if (bestSolution == null)
            {
                Console.WriteLine("No solution found.");
                return default;
            }
            else
            {
                File.WriteAllText(@"D:\c\Qoph\DataFiles\Face To Face\Generated Crossword.svg",
                    generateNet(_polyhedron, faceColor: f => "#def", faceSvg: (f, x, y) => $"<text x='{x}' y='{y + .06}' fill='black' font-size='.2' text-anchor='middle'>{(char) (bestSolution[f] - 1 + 'A')}</text>").svg);

                var results = new List<(int[] cells, string word)>();
                foreach (var light in lights)
                {
                    var word = light.Select(cell => (char) (bestSolution[cell] + 'A' - 1)).JoinString();
                    if (words.Contains(word))
                        results.Add((light, word));
                    else
                        results.Add((light.Reverse().ToArray(), word.Reverse().JoinString()));
                }

                foreach (var (cells, word) in results)
                    Console.WriteLine($@"(""{word}"", new[] {{ {cells.JoinString(", ")} }}),");
                Console.WriteLine();

                return (bestSolution.Select(val => (char) (val + 'A' - 1)).JoinString(), results.ToArray());
            }
        }

        public sealed class EdgeInfo
        {
            public int AdjacentFace;
            public int AdjacentEdge;
            public bool Locked;
            public int? CyanNumber;  // in the corner where this edge starts (clockwise from the door)
            public string CrosswordInfo;    // could be crossword clue or offset
            public int CrosswordInfoFontSize;
            public EdgeD Edge;
        }

        public sealed class FaceInfo
        {
            public EdgeInfo[] Edges = new EdgeInfo[5];
            public string CarpetColor;
            public int CarpetColorIndex;
            public string GashlycrumbTiniesObject;
            public string SongFilename;
            public string SongTitle;
            public string SongLyrics;
            public string SongNextWord;
            public int LampBro;

            public static readonly string[] LampBroNames = { "Mario", "Donkey Kong", "Link", "Samus", "Yoshi", "Kirby", "Fox", "Pikachu", "Luigi", "Ness", "Captain Falcon", "Jigglypuff", "Peach", "Bowser", "Ice Climbers", "Sheik", "Zelda", "Dr. Mario", "Pichu", "Falco", "Marth", "Young Link", "Ganondorf", "Mewtwo", "Roy Fire Emblem", "Mr. Game & Watch" };
            public string LampBroName => LampBroNames[LampBro - 1];
        }

        private static int getFaceValue(int face, DistrInfo distr) => distr.Distribution.Select(d => new { Data = d, Ix = d.faces.IndexOf(face) }).Where(inf => inf.Ix != -1).Single().Apply(inf => inf.Data.word[inf.Ix] - 'A' + 1);

        public static void GatherAllData()
        {
            var faceInfos = GetFaceData();

            var maplePieces = new List<string>();
            var cyanCorners = new List<((int faceIx, int vertexIx)[] adj, int cyanNumber)>();
            for (var faceIx = 0; faceIx < faceInfos.Length; faceIx++)
            {
                var face = faceInfos[faceIx];
                for (var vertexIx = 0; vertexIx < face.Edges.Length; vertexIx++)
                {
                    var edge = face.Edges[vertexIx];
                    if (edge.CyanNumber == null)
                        continue;
                    var vertex = _polyhedron.Faces[faceIx][vertexIx];
                    var adjoiningFaces = _polyhedron.Faces.SelectIndexWhere(f => f.Contains(vertex)).ToArray();
                    if (faceIx != adjoiningFaces.Min())
                        continue;
                    maplePieces.Add($"{adjoiningFaces.Select(ix => $"f{ix}").JoinString("+")}={edge.CyanNumber.Value}");
                    cyanCorners.Add((adjoiningFaces.Select(f => (f, _polyhedron.Faces[f].IndexOf(vertex))).ToArray(), edge.CyanNumber.Value));
                }
            }
            //Clipboard.SetText($"solve({{{maplePieces.JoinString(", ")}}}, {{{Enumerable.Range(0, 24).Select(i => $"f{i}").JoinString(", ")}}});");
            //Debugger.Break();

            // C# declaration for Unity project
            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Data.cs", @"/\*Faces-start\*/", @"/\*Faces-end\*/",
                $@"new[] {{ {faceInfos.Select(fi => $@"new FaceData {{ " +
                    $@"CarpetColor = ""{fi.CarpetColor.ToLowerInvariant().CLiteralEscape()}"", " +
                    $@"CarpetLength = {fi.CarpetColorIndex}, " +
                    $@"ItemInBox = ""{fi.GashlycrumbTiniesObject.CLiteralEscape()}"", " +
                    $@"LampBro = {fi.LampBro}, " +
                    $@"Edges = new[] {{ {fi.Edges.Select(e => $@"new Edge {{ {Ut.NewArray(
                        e.CyanNumber.NullOr(p => $"CyanNumber = {p}"),
                        e.Locked && e.CrosswordInfo != null ? $@"Label = ""{e.CrosswordInfo.CLiteralEscape()}"", LabelFontSize = {e.CrosswordInfoFontSize}" : null,
                        e.Locked ? null : $"Face = {e.AdjacentFace}").Where(str => str != null).JoinString(", ")} }}").JoinString(", ")} }} }}").JoinString(", ")} }}");

            // JS declaration for solution page
            General.ReplaceInFile(@"D:\c\Qoph\EnigmorionFiles\Solutions\face-to-face.html", @"/\*Faces-start\*/", @"/\*Faces-end\*/",
                $@"[{"\n\t"}{faceInfos.Select((fi, fIx) => $@"{{ " +
                    $@"c: [ {_distributions.Select(dist => dist.Distribution.First(d => d.faces.Contains(fIx)).Apply(tup => tup.color == null ? "null" : tup.color.ToString())).JoinString(", ")} ], " +
                    $@"v: [ {_distributions.Select(dist => getFaceValue(fIx, dist)).JoinString(", ")} ], " +
                    $@"cc: ""{fi.CarpetColor.ToUpperInvariant().CLiteralEscape()}"", " +
                    $@"ci: {fi.CarpetColorIndex}, " +
                    $@"lb: {fi.LampBro}, " +
                    $@"s: {{ t: ""{fi.SongTitle.CLiteralEscape()}"", l: ""{fi.SongLyrics}"", n: ""{fi.SongNextWord}"" }}, " +
                    $@"item: ""{Regex.Replace(fi.GashlycrumbTiniesObject, @"""(.*?)""", m => $"“{m.Groups[1].Value}”").CLiteralEscape()}"", " +
                    $@"e: [ {fi.Edges.Select(e => $@"{{ {Ut.NewArray(
                        e.CyanNumber.NullOr(p => $"pn: {p}"),
                        e.CrosswordInfo.NullOr(c => $@"cm: ""{("+-".Contains(e.CrosswordInfo[0]) ? e.CrosswordInfo.Replace("-", "−") : "(clue)").CLiteralEscape()}"""),
                        e.CrosswordInfo.NullOr(c => $"cmx: {e.Edge.Midpoint.X:.00}"),
                        e.CrosswordInfo.NullOr(c => $"cmy: {e.Edge.Midpoint.Y:.00}"),
                        e.CrosswordInfo.NullOr(c => $"cma: {e.Edge.AngleDeg:.00}"),
                        $"face: {e.AdjacentFace}").Where(str => str != null).JoinString(", ")} }}").JoinString(", ")} ] }}").JoinString(",\n\t")}]");

            General.ReplaceInFile(@"D:\c\Qoph\EnigmorionFiles\Solutions\face-to-face.html", @"<!--CyanCorners-start-->", @"<!--CyanCorners-end-->",
                cyanCorners.SelectMany(corner => corner.adj
                    .Select(inf => (p: faceInfos[inf.faceIx].Edges[inf.vertexIx].Edge.Start, prevP: faceInfos[inf.faceIx].Edges[(inf.vertexIx + 4) % 5].Edge.Start, nextP: faceInfos[inf.faceIx].Edges[inf.vertexIx].Edge.End))
                    .Select(inf => (inf.p, vector: (inf.prevP - inf.p).Unit() + (inf.nextP - inf.p).Unit()))
                    .Select(inf => (p: inf.p + .1 * inf.vector.Unit(), angle: inf.vector.Theta() * 180 / Math.PI))
                    .Select(inf => $"<text transform='translate({inf.p.X:.00} {inf.p.Y:.00}) rotate({inf.angle - 90:.00})' y='.06'>{corner.cyanNumber}</text>")).JoinString());

            General.ReplaceInFile(@"D:\c\Qoph\EnigmorionFiles\Solutions\face-to-face.html", @"/\*DoorInfo-start\*/", @"/\*DoorInfo-end\*/",
                $@"[ {_crosswordLights.Select(cl => $@"{{ w: ""{cl.word}"", l: ""{cl.clue.Replace("\n", " ").CLiteralEscape()}"", c: [ {cl.cells.JoinString(", ")} ], e: {faceInfos[cl.cells[0]].Edges.IndexOf(e => e.CrosswordInfo == cl.clue)} }}").JoinString(", ")} ]");

            // Song files in Unity project
            for (var i = 0; i < faceInfos.Length; i++)
                File.Copy($@"D:\c\Qoph\DataFiles\Face To Face\SongRadiofied\{faceInfos[i].SongFilename}.mp3", $@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\RadioSongs\radio{i}.mp3", overwrite: true);

            // Google Sheets
            Clipboard.SetText(faceInfos.Select((f, ix) => $"{ix}\t{Enumerable.Range(0, 5).Select(edge => $"{(f.Edges[edge].Locked ? f.Edges[edge].CrosswordInfo.Apply(ci => string.IsNullOrWhiteSpace(ci) ? "" : !"+-".Contains(ci[0]) ? ci.Replace("\n", " ") : $"‘{ci}’") : f.Edges[edge].AdjacentFace.ToString())}\t{(f.Edges[edge].Locked ? "" : f.Edges[edge].AdjacentEdge.ToString())}").JoinString("\t")}\t{Enumerable.Range(0, 5).Select(edge => f.Edges[edge].CyanNumber).JoinString("\t")}\t{f.CarpetColor}\t{f.CarpetColorIndex}\t{f.SongTitle}\t{f.GashlycrumbTiniesObject}\t{f.LampBroName}").JoinString("\n"));
        }

        // Cached here because generating this takes several seconds. Generated from random seed 47, git commit 4ebc757c65e5165e2dac6ba5440ed8d24f98fa3f
        private static readonly int?[][] _cyanNumbers = new int?[][]
        {
            new int?[] { 41, 40, null, null, 47 },
            new int?[] { 41, 22, null, 33, 40 },
            new int?[] { 41, 36, 46, null, 22 },
            new int?[] { 41, 47, null, null, 36 },
            new int?[] { 59, null, 49, 55, null },
            new int?[] { 59, 36, 50, null, null },
            new int?[] { 59, 21, 32, 31, 36 },
            new int?[] { 59, null, null, 30, 21 },
            new int?[] { null, 55, 49, 55, null },
            new int?[] { null, null, null, null, 55 },
            new int?[] { null, null, null, 44, null },
            new int?[] { null, null, null, 47, null },
            new int?[] { 41, 31, 32, 36, null },
            new int?[] { 41, 40, 50, 36, 31 },
            new int?[] { 41, null, 46, 57, 40 },
            new int?[] { 41, null, null, 22, null },
            new int?[] { 46, 36, 32, 21, 30 },
            new int?[] { 46, 33, null, null, 36 },
            new int?[] { 46, 44, null, 40, 33 },
            new int?[] { 46, 30, null, null, 44 },
            new int?[] { 86, 57, 46, 36, null },
            new int?[] { 86, null, 50, 40, 57 },
            new int?[] { 86, 55, 49, null, null },
            new int?[] { 86, null, null, null, 55 }
        };

        public static FaceInfo[] GetFaceData()
        {
            var edges = new Dictionary<(int face, int vertex), EdgeD>();
            generateNet(_polyhedron,
                edgeSvg: (f1Ix, e1Ix, f2Ix, e2Ix, e) =>
                {
                    edges[(f1Ix, e1Ix)] = e;
                    if (!edges.ContainsKey((f2Ix, e2Ix)))
                        edges[(f2Ix, e2Ix)] = new EdgeD(e.End, e.Start);
                    return null;
                });

            var locked = getLocked();
            var faceInfos = new List<FaceInfo>();
            for (var faceIx = 0; faceIx < _polyhedron.Faces.Length; faceIx++)
            {
                var face = _polyhedron.Faces[faceIx];
                if (face.Length != 5)
                    throw new InvalidOperationException();

                var carpetLetter = (char) ('A' + getFaceValue(faceIx, _carpetColors) - 1);
                var carpetColor = _carpetColorNames.First(cn => cn.Contains(carpetLetter));
                var (songAuthor, songTitle, songGivenLyrics, songNextWord) = _songTitles[getFaceValue(faceIx, _musicSnippets) - 1].Value;
                var inf = new FaceInfo
                {
                    SongFilename = songTitle,
                    SongTitle = $"{songTitle.Replace("'", "’")} ({songAuthor})",
                    SongLyrics = songGivenLyrics,
                    SongNextWord = songNextWord,
                    GashlycrumbTiniesObject = _gashlycrumbTiniesObjects[getFaceValue(faceIx, _gashlycrumbTinies) - 1],
                    CarpetColor = carpetColor,
                    CarpetColorIndex = carpetColor.IndexOf(carpetLetter) + 1
                };

                for (var edgeIx = 0; edgeIx < 5; edgeIx++)
                {
                    var adjacentFace = _polyhedron.Faces.Single(f => f != face && f.Contains(face[edgeIx]) && f.Contains(face[(edgeIx + 1) % 5]));
                    var adjacentFaceIx = _polyhedron.Faces.IndexOf(adjacentFace);

                    inf.Edges[edgeIx] = new EdgeInfo
                    {
                        Locked = locked.Contains((faceIx, edgeIx)),
                        AdjacentFace = adjacentFaceIx,
                        AdjacentEdge = adjacentFace.IndexOf(face[(edgeIx + 1) % 5]),
                        Edge = edges[(faceIx, edgeIx)]
                    };
                }
                faceInfos.Add(inf);
            }

            // Crossword
            foreach (var (word, clueSize, clue, cells) in _crosswordLights)
            {
                var firstFace = cells[0];
                var firstEdge = findEdgeThatLeadsTo(cells[0], cells[1]);
                var barEdge = locked.First(tup => tup.face == firstFace && (tup.edge == (firstEdge + 2) % 5 || tup.edge == (firstEdge + 3) % 5)).edge;
                faceInfos[firstFace].Edges[barEdge].CrosswordInfo = clue;
                faceInfos[firstFace].Edges[barEdge].CrosswordInfoFontSize = clueSize;
            }
            for (var faceIx = 0; faceIx < 24; faceIx++)
            {
                var letter = _crosswordLights.Select(tup => new { Tup = tup, Ix = tup.cells.IndexOf(faceIx) }).Where(inf => inf.Ix != -1).Select(inf => inf.Tup.word[inf.Ix]).First();
                var barEdge = locked.First(tup => tup.face == faceIx && faceInfos[faceIx].Edges[tup.edge].CrosswordInfo == null).edge;
                var offset = getFaceValue(faceIx, _crosswordAfterOffset) - (letter - 'A' + 1);
                faceInfos[faceIx].Edges[barEdge].CrosswordInfo = $@"{(offset >= 0 ? "+" : "-")}{Math.Abs(offset)}";
                faceInfos[faceIx].Edges[barEdge].CrosswordInfoFontSize = 32;
            }

            // Lamp Bros
            for (var faceIx = 0; faceIx < 24; faceIx++)
                faceInfos[faceIx].LampBro = getFaceValue(faceIx, _smashChars);

            // Cyan numbers (vertex sums)
            var rnd = new Random(47);
            if (_cyanNumbers == null)
            {
                var cyanCluesRaw =
                    from faceIx in Enumerable.Range(0, _polyhedron.Faces.Length)
                    from edgeIx in Enumerable.Range(0, 5)
                    let vertex = _polyhedron.Faces[faceIx][edgeIx]
                    let adjoiningFaces = _polyhedron.Faces.SelectIndexWhere(f => f.Contains(vertex))
                    select (faces: adjoiningFaces.ToArray(), sum: adjoiningFaces.Sum(f => getFaceValue(f, _cyanSums)));
                var cyanClues = cyanCluesRaw.Where(cl => cl.faces[0] == cl.faces.Min()).ToArray();

                Puzzle makePuzzle(IEnumerable<int> clues)
                {
                    var puzzle = new Puzzle(_polyhedron.Faces.Length, 0, 50);
                    foreach (var i in clues)
                        puzzle.AddConstraint(new SumConstraint(cyanClues[i].sum, cyanClues[i].faces));
                    return puzzle;
                }
                if (makePuzzle(Enumerable.Range(0, cyanClues.Length)).Solve().Skip(1).Any())
                    Debugger.Break();

                var requiredCyanClues = Ut.ReduceRequiredSet(Enumerable.Range(0, cyanClues.Length).ToArray().Shuffle(rnd), skipConsistencyTest: true, test: state =>
                {
                    Console.WriteLine(Enumerable.Range(0, cyanClues.Length).Select(i => state.SetToTest.Contains(i) ? "█" : "░").JoinString());
                    return !makePuzzle(state.SetToTest).Solve().Skip(1).Any();
                })
                    .Select(ix => cyanClues[ix])
                    .ToArray();

                foreach (var (faces, sum) in requiredCyanClues)
                {
                    var commonVertex = Enumerable.Range(0, _polyhedron.Vertices.Length).Single(vIx => faces.All(f => _polyhedron.Faces[f].Contains(vIx)));
                    for (var faceIx = 0; faceIx < _polyhedron.Faces.Length; faceIx++)
                        if (_polyhedron.Faces[faceIx].Contains(commonVertex))
                            faceInfos[faceIx].Edges[_polyhedron.Faces[faceIx].IndexOf(commonVertex)].CyanNumber = sum;
                }
            }
            else
                for (var fIx = 0; fIx < faceInfos.Count; fIx++)
                    for (var eIx = 0; eIx < faceInfos[fIx].Edges.Length; eIx++)
                        faceInfos[fIx].Edges[eIx].CyanNumber = _cyanNumbers[fIx][eIx];

            return faceInfos.ToArray();
        }

        public static void GenerateModels()
        {
            var poly = generateNet(_polyhedron).polygons[0];
            const double ceilingHeight = .425;
            const double doorHeight = .35;
            const double doorWidth = .09;
            const double frameWidth = .015;
            const double frameDepth = .015;
            const double cameraDistance = .28;
            const double inCameraDistance = .5;
            double[] cameraPos = { .38, .5, .5, .5, .62 };
            const double cameraHeight = .22;
            const double cameraHeightLook = .2;
            const double inCameraHeight = .275;
            const double inCameraHeightLook = .13;
            const double cyanNumberHeight = .365;

            var outCameras = new List<(Pt from, Pt to)>();
            var inCameras = new List<(Pt from, Pt to)>();
            var cyanNumbers = new List<(Pt from, Pt to)>();
            var cyanNumbers1 = new List<(Pt from, Pt to)>();
            var cyanNumbers2 = new List<(Pt from, Pt to)>();
            var doors = new List<(Pt from, Pt to)>();
            var wallPositions = new List<Pt>();

            var ix = 0;
            foreach (var (p1, p2) in poly.ConsecutivePairs(true))
            {
                //var mid = (p1 + p2) / 2;
                var mid = p1 * cameraPos[ix] + p2 * (1 - cameraPos[ix]);
                var right = mid - (p2 - p1).Unit() * doorWidth;
                var left = mid + (p2 - p1).Unit() * doorWidth;

                File.WriteAllText($@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Objects\Wall{ix}.obj",
                    GenerateObjFile(Ut.NewArray(
                        new[] { pt(p2.X, 0, p2.Y), pt(left.X, 0, left.Y), pt(left.X, ceilingHeight, left.Y), pt(p2.X, ceilingHeight, p2.Y) },
                        new[] { pt(left.X, doorHeight, left.Y), pt(right.X, doorHeight, right.Y), pt(right.X, ceilingHeight, right.Y), pt(left.X, ceilingHeight, left.Y) },
                        new[] { pt(right.X, 0, right.Y), pt(p1.X, 0, p1.Y), pt(p1.X, ceilingHeight, p1.Y), pt(right.X, ceilingHeight, right.Y) }
                    )
                        .Select(face => face.Select(p => p.WithTexture((p.X - p1.X) / (p2.X - p1.X), p.Y / ceilingHeight)).ToArray()).ToArray(), $"Wall{ix}", AutoNormal.Flat));

                var nx = (p2 - p1).Unit() * frameWidth / 2;
                var ny = (p2 - p1).Normal().Unit() * frameDepth / 2;
                var lfm = right - nx;
                var rfm = left + nx;

                File.WriteAllText($@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Objects\Frame{ix}.obj",
                    GenerateObjFile(Ut.NewArray(
                        // Right
                        new[] { (lfm + ny - nx).h(doorHeight + frameWidth), (lfm + ny + nx).h(doorHeight), (lfm + ny + nx).h(0), (lfm + ny - nx).h(0) },    // front
                        new[] { (lfm - ny - nx).h(doorHeight + frameWidth), (lfm + ny - nx).h(doorHeight + frameWidth), (lfm + ny - nx).h(0), (lfm - ny - nx).h(0) },   // right
                        new[] { (lfm + ny + nx).h(doorHeight), (lfm - ny + nx).h(doorHeight), (lfm - ny + nx).h(0), (lfm + ny + nx).h(0) }, // left

                        // Left
                        new[] { (rfm + ny - nx).h(doorHeight), (rfm + ny + nx).h(doorHeight + frameWidth), (rfm + ny + nx).h(0), (rfm + ny - nx).h(0) },    // front
                        new[] { (rfm - ny - nx).h(doorHeight), (rfm + ny - nx).h(doorHeight), (rfm + ny - nx).h(0), (rfm - ny - nx).h(0) }, // right
                        new[] { (rfm + ny + nx).h(doorHeight + frameWidth), (rfm - ny + nx).h(doorHeight + frameWidth), (rfm - ny + nx).h(0), (rfm + ny + nx).h(0) },   // left

                        // Top
                        new[] { (rfm + ny + nx).h(doorHeight + frameWidth), (rfm + ny - nx).h(doorHeight), (lfm + ny + nx).h(doorHeight), (lfm + ny - nx).h(doorHeight + frameWidth) }, // front
                        new[] { (rfm + ny + nx).h(doorHeight + frameWidth), (lfm + ny - nx).h(doorHeight + frameWidth), (lfm - ny - nx).h(doorHeight + frameWidth), (rfm - ny + nx).h(doorHeight + frameWidth) },   // top
                        new[] { (rfm - ny - nx).h(doorHeight), (lfm - ny + nx).h(doorHeight), (lfm + ny + nx).h(doorHeight), (rfm + ny - nx).h(doorHeight) }    // bottom
                    ), $"Frame{ix}", AutoNormal.Flat));

                outCameras.Add((
                    (p1 * cameraPos[ix] + p2 * (1 - cameraPos[ix]) - (p2 - p1).Normal().Unit() * cameraDistance).h(cameraHeight),
                    (p1 * cameraPos[ix] + p2 * (1 - cameraPos[ix]) + (p2 - p1).Normal().Unit() * cameraDistance).h(cameraHeightLook)));
                inCameras.Add((
                    (p1 * cameraPos[ix] + p2 * (1 - cameraPos[ix]) + (p2 - p1).Normal().Unit() * inCameraDistance).h(inCameraHeight),
                    (p1 * cameraPos[ix] + p2 * (1 - cameraPos[ix]) - (p2 - p1).Normal().Unit() * inCameraDistance).h(inCameraHeightLook)));
                var cyanMid = .49 * p2 + .51 * p1;

                // cyan number on the right side of the wall
                cyanNumbers1.Add(((p1 + (p2 - p1).Normal().Unit() * .0001).h(cyanNumberHeight), (p1 - (p2 - p1).Normal().Unit() * .1).h(cyanNumberHeight)));
                // cyan number on the left side of the wall
                cyanNumbers2.Add(((p2 + (p2 - p1).Normal().Unit() * .0001).h(cyanNumberHeight), (p2 - (p2 - p1).Normal().Unit() * .1).h(cyanNumberHeight)));

                doors.Add((left.h(doorHeight / 2), (left - (p2 - p1).Normal().Unit()).h(doorHeight / 2)));
                wallPositions.Add(pt(-mid.X, 0, mid.Y));
                ix++;
            }

            // Note that cyanNumbers2 are on the left side of wall x. But we want them on the vertex on the right side of the wall (because edges go widdershins), so cycle them over by one
            cyanNumbers2.Insert(0, cyanNumbers2.Last());
            cyanNumbers2.RemoveAt(cyanNumbers2.Count - 1);

            File.WriteAllText(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Objects\Floor.obj",
                GenerateObjFile(new[] { poly.Select(p => pt(p.X, 0, p.Y).WithTexture(p.X, p.Y)).ToArray() }, "Floor", AutoNormal.Flat));
            File.WriteAllText(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Objects\Ceiling.obj",
                GenerateObjFile(new[] { poly.Select(p => pt(p.X, ceilingHeight, p.Y).WithTexture(p.X, p.Y)).Reverse().ToArray() }, "Ceiling", AutoNormal.Flat));

            static Pt invX(Pt p) => pt(-p.X, p.Y, p.Z);
            static string makeArray(List<(Pt from, Pt to)> list) => $@"new[] {{ {list.Select(tup => $@"new PosAndDir {{ From = vec{invX(tup.from)}, To = vec{invX(tup.to)} }}").JoinString(", ")} }}";
            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Data.cs", @"/\*CameraPositions-start\*/", @"/\*CameraPositions-end\*/", makeArray(outCameras));
            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Data.cs", @"/\*InCameraPositions-start\*/", @"/\*InCameraPositions-end\*/", makeArray(inCameras));
            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Data.cs", @"/\*CyanNumbersPositions-start\*/", @"/\*CyanNumbersPositions-end\*/", makeArray(cyanNumbers));
            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Data.cs", @"/\*CyanNumbers1Positions-start\*/", @"/\*CyanNumbers1Positions-end\*/", makeArray(cyanNumbers1));
            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Data.cs", @"/\*CyanNumbers2Positions-start\*/", @"/\*CyanNumbers2Positions-end\*/", makeArray(cyanNumbers2));
            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Data.cs", @"/\*DoorPositions-start\*/", @"/\*DoorPositions-end\*/", makeArray(doors));
            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Data.cs", @"/\*WallPositions-start\*/", @"/\*WallPositions-end\*/", wallPositions.Select(p => $"vec{p}").JoinString(", "));

            var doorKnobCurve = DecodeSvgPath.Do(@"M 100,-35 H 80 v 25 C 60,-10 60,-35 35,-35 15.670034,-35 0,-25 0,0", .1).Select(p => p.ToArray()).First();
            const int revSteps = 36;
            File.WriteAllText(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Objects\Doorknob.obj",
                GenerateObjFile(CreateMesh(true, false, Enumerable.Range(0, revSteps)
                    .Select(i => 360.0 / revSteps * i)
                    .Select(angle => doorKnobCurve
                        .Select((p, pIx) => pt(p.X, 0, p.Y).RotateX(angle)
                            .Apply(point => pIx == doorKnobCurve.Length - 1
                                ? point.WithMeshInfo(-1, 0, 0)
                                : point.WithMeshInfo(Normal.Average, Normal.Average, pIx < 3 ? Normal.Mine : Normal.Average, pIx < 3 ? Normal.Mine : Normal.Average)))
                        .Reverse()
                        .ToArray())
                    .ToArray()), "Doorknob"));

            const int revStepsRope = 6;
            const double r = .1;
            const double x1 = -.5;
            const double x2 = .4;
            var ropeCurve = new[] { pt(x1, 0, x1 + r * Math.Sqrt(2)) }
                .Concat(Enumerable.Range(0, revStepsRope).Select(a => 135 - 90d * a / (revStepsRope - 1)).Select(angle => pt(r * cos(angle), 0, r * sin(angle))))
                .Concat(new[] { pt(x2, 0, -x2 + r * Math.Sqrt(2)) });
            File.WriteAllText(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Objects\Signrope.obj",
                GenerateObjFile(TubeFromCurve(ropeCurve, .005, revSteps), "Signrope"));

            var net = generateNet(_polyhedron).polygons;

            // Calculate the rotations to transform the current room into an adjoining room
            var rotateAbout = new (int v1, int v2)[] { (0, 0), (2, 2), (2, 2), (3, 4), (0, 0) }.Select(tup => (net[0][tup.v1] + net[0][tup.v2]) / 2).ToArray();
            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Data.cs", @"/\*RotateRoomAbout-start\*/", @"/\*RotateRoomAbout-end\*/",
                rotateAbout.Select(p => $"new Vector3({-p.X}f, 0, {p.Y}f)").JoinString(", "));

            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Data.cs", @"/\*RotateRoomBy-start\*/", @"/\*RotateRoomBy-end\*/",
                new (PointD c, PointD a, PointD b)?[] { (net[0][0], net[0][1], net[1][1]), (net[0][2], net[0][1], net[18][1]), (net[0][2], net[0][3], net[10][3]), null, (net[0][0], net[0][4], net[3][4]) }
                    .Select(tup =>
                    {
                        if (tup == null)
                            return "180f";
                        var (c, a, b) = tup.Value;
                        var v1 = a - c;
                        var v2 = b - c;
                        return $"{Math.Atan2(v1.X * v2.Y - v1.Y * v2.X, v1.X * v2.X + v1.Y * v2.Y) / Math.PI * 180}f";
                    })
                    .JoinString(", "));

            // Calculate the tilt angle
            static Pt p(int face, int vx) => _polyhedron.Vertices[_polyhedron.Faces[face][vx]];
            var n1 = (p(0, 1) - p(0, 0)) * (p(0, 4) - p(0, 0));
            var n2 = (p(1, 1) - p(1, 0)) * (p(1, 4) - p(1, 0));
            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Data.cs", @"/\*TiltAngle-start\*/", @"/\*TiltAngle-end\*/", $"{-arcsin((n1 * n2).Length)}f");

            PointD pn(int face, int vx) => net[face][vx];
            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Data.cs", @"/\*TiltRoomAbout-start\*/", @"/\*TiltRoomAbout-end\*/",
                new[] { pn(0, 1) - pn(0, 0), pn(0, 2) - pn(0, 1), pn(0, 3) - pn(0, 2), pn(0, 4) - rotateAbout[3], pn(0, 0) - pn(0, 4) }.Select(p => $"new Vector3({-p.X}f, 0, {p.Y}f)").JoinString(", "));

            // Calculate the room’s midpoint
            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Data.cs", @"/\*Midpoint-start\*/", @"/\*Midpoint-end\*/",
                (net[0].Aggregate(new PointD(0, 0), (p, n) => p + n) / net[0].Length).Apply(p => $"new Vector3({-p.X}f, {inCameraHeight}f, {p.Y}f)"));

            File.WriteAllText(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\Cylinder.obj", GenerateObjFile(LooseModels.Cylinder(0, 1.4, .3, 36), "Cylinder"));
            File.WriteAllText(@"D:\c\Qoph\DataFiles\Face To Face\Unity\Face To Face\Assets\LampCylinder.obj", GenerateObjFile(LooseModels.Cylinder(0, 1.4, .3, 36), "Cylinder"));
        }

        private static Pt h(this PointD p, double y) => pt(p.X, y, p.Y);
    }
}
