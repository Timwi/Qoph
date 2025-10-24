using System.Diagnostics;
using System.Xml.Linq;
using RT.Geometry;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace Qoph
{
    static class SignalLink
    {
        struct Cubelet
        {
            public int U, B, L, F, R, D;    // bit order is CW from TL (MSB) to BL (LSB)

            public Cubelet(int u, int b, int l, int f, int r, int d)
            {
                U = u; B = b; L = l; F = f; R = r; D = d;
            }

            /// <summary>Performs a rotation in which the UP face rotates clockwise.</summary>
            public Cubelet Ucw => new Cubelet(rot(U), L, F, R, B, rot(D, 3));
            /// <summary>Performs a rotation in which the FRONT face rotates clockwise.</summary>
            public Cubelet Fcw => new Cubelet(rot(L), rot(B, 3), rot(D), rot(F), rot(U), rot(R));

            public Cubelet[] AllRotations => Ut.NewArray(
                // UP face is up
                this, Ucw, Ucw.Ucw, Ucw.Ucw.Ucw,
                // LEFT face is up
                Fcw, Fcw.Ucw, Fcw.Ucw.Ucw, Fcw.Ucw.Ucw.Ucw,
                // FRONT face is up
                Ucw.Fcw, Ucw.Fcw.Ucw, Ucw.Fcw.Ucw.Ucw, Ucw.Fcw.Ucw.Ucw.Ucw,
                // RIGHT face is up
                Ucw.Ucw.Fcw, Ucw.Ucw.Fcw.Ucw, Ucw.Ucw.Fcw.Ucw.Ucw, Ucw.Ucw.Fcw.Ucw.Ucw.Ucw,
                // BACK face is up
                Ucw.Ucw.Ucw.Fcw, Ucw.Ucw.Ucw.Fcw.Ucw, Ucw.Ucw.Ucw.Fcw.Ucw.Ucw, Ucw.Ucw.Ucw.Fcw.Ucw.Ucw.Ucw,
                // DOWN face is up
                Fcw.Fcw, Fcw.Fcw.Ucw, Fcw.Fcw.Ucw.Ucw, Fcw.Fcw.Ucw.Ucw.Ucw);

            private int rot(int value, int repetitions = 1)
            {
                for (var i = 0; i < repetitions; i++)
                    value = ((value & 0b11101110) >> 1) | ((value & 0b00010001) << 3);
                return value;
            }

            public Cubelet WithU(int u) => new Cubelet(u, B, L, F, R, D);
            public Cubelet WithB(int b) => new Cubelet(U, b, L, F, R, D);
            public Cubelet WithL(int l) => new Cubelet(U, B, l, F, R, D);
            public Cubelet WithF(int f) => new Cubelet(U, B, L, f, R, D);
            public Cubelet WithR(int r) => new Cubelet(U, B, L, F, r, D);
            public Cubelet WithD(int d) => new Cubelet(U, B, L, F, R, d);

            public override string ToString() => $"{U} {B} {L} {F} {R} {D}";
        }

        private static readonly Random rnd = new Random(47);
        private static int r(int allow) => (rnd.Next(0, 0b100000000) & ~allow & ~((allow >> 1) | ((allow & 1) << 3))) | 0b100000000;
        private static int ru => r(0b1000);
        private static int rr => r(0b0100);
        private static int rd => r(0b0010);
        private static int rl => r(0b0001);
        private static int re => r(0);
        private static int flipped(int v) => ((v & 0b10001000) >> 3) | ((v & 0b01000100) >> 1) | ((v & 0b00100010) << 1) | ((v & 0b00010001) << 3) | (v & 0b100000000);
        private static int mirrored(int v) => ((v & 0b10101010) >> 1) | ((v & 0b01010101) << 1) | (v & 0b100000000);

        public static void SolveCubeArrangement()
        {
            var allCubelets = Ut.NewArray(
                // FRONT PLANE
                new Cubelet(0b11010000, rr, 0b00010000, 0b10010000, rl, ru),
                new Cubelet(0b00010000, ru, rr, 0b11010000, rl, ru),
                new Cubelet(0b10110000, ru, rr, 0b01110000, 0b11000000, ru),

                new Cubelet(rd, rr, 0b01010000, 0b10010000, rl, rl),
                new Cubelet(rd, re, rr, 0b00100000, rl, ru),
                new Cubelet(rd, rl, rr, 0b10100000, 0b01110000, rr),

                new Cubelet(rl, rr, 0b01100000, 0b01010000, rl, 0b10100000),
                new Cubelet(rd, rd, rr, 0b10010000, rd, 0b10100000),
                new Cubelet(rr, re, rd, 0b10010000, 0b01110000, 0b10100000),

                // MIDDLE PLANE
                new Cubelet(0b00010000, ru, 0b11110000, rl, ru, rl),
                new Cubelet(0b01000000, ru, ru, ru, ru, re),
                new Cubelet(0b10000000, rl, ru, ru, 0b00110000, rr),

                new Cubelet(rl, rr, 0b10000000, rl, re, rl),
                new Cubelet(re, re, re, re, re, re),
                new Cubelet(rr, rl, re, rr, 0b01000000, rr),

                new Cubelet(rl, rr, 0b01000000, rl, rd, 0b10010000),
                new Cubelet(re, rd, rd, rd, rd, 0b10000000),
                new Cubelet(rr, rd, rd, re, 0b00010000, 0b10010000),

                // BACK PLANE
                new Cubelet(0b01000000, 0b11110000, 0b10100000, ru, rr, rd),
                new Cubelet(0b01010000, 0b11010000, rl, ru, rr, rd),
                new Cubelet(0b00110000, 0b11000000, rl, rr, 0b11010000, rr),

                new Cubelet(ru, 0b01010000, 0b10100000, rl, rr, rl),
                new Cubelet(ru, 0b01010000, rl, re, rr, rd),
                new Cubelet(rr, 0b11000000, rl, rr, 0b01010000, rd),

                new Cubelet(rl, 0b01110000, 0b10000000, rl, rd, 0b10010000),
                new Cubelet(ru, 0b01000000, rd, rd, rr, 0b11010000),
                new Cubelet(ru, 0b00110000, rl, rd, 0b01100000, 0b11010000)
            );

            // Make sure that faces that touch each other have matching patterns
            for (var i = 0; i < 27; i++)
            {
                if (i % 3 > 0)
                    allCubelets[i] = allCubelets[i].WithL(mirrored(allCubelets[i - 1].R));
                if ((i / 3) % 3 > 0)
                    allCubelets[i] = allCubelets[i].WithU(flipped(allCubelets[i - 3].D));
                if (i / 3 / 3 > 0)
                    allCubelets[i] = allCubelets[i].WithF(mirrored(allCubelets[i - 9].B));
            }

            // Generate SVG
            var xml = XDocument.Parse(File.ReadAllText(@"D:\c\Qoph\DataFiles\Signal Link\Maze (v5, 1 after fixing loop bug).svg"));
            var p = xml.Root.ElementsI("path").FirstOrDefault(e => e.AttributeI("id").Value == "path1739");
            var txt = xml.Root.ElementsI("g").FirstOrDefault(e => e.AttributeI("id").Value == "g1708").ElementI("text");
            const int xOffset = -70;
            const int yOffset = 320;
            const int xSpacing = 110;
            const int ySpacing = 180;
            const int zSpacingX = 30;
            const int zSpacingY = -50;
            for (var i = 0; i < allCubelets.Length; i++)
            {
                void drawPattern(int x, int y, int z, int dx, int dy, int v)
                {
                    if ((v & 0b100000000) != 0)
                    {
                        void makePath(int ddx, int ddy, int lx, int ly)
                        {
                            xml.Root.Add(new XElement(p.Name, new XAttribute(p.AttributeI("d").Name, $"M {xOffset + xSpacing * x + zSpacingX * z + dx + ddx} {yOffset + ySpacing * y + zSpacingY * z + dy + ddy} l {lx} {ly}"), new XAttribute(p.AttributeI("style").Name, p.AttributeI("style").Value)));
                        }
                        if ((v & 0b10000000) != 0)
                            makePath(0, 0, 10, 10);
                        if ((v & 0b01000000) != 0)
                            makePath(20, 0, -10, 10);
                        if ((v & 0b00100000) != 0)
                            makePath(20, 20, -10, -10);
                        if ((v & 0b00010000) != 0)
                            makePath(0, 20, 10, -10);
                        if ((v & 0b00001000) != 0)
                            makePath(0, 10, 10, -10);
                        if ((v & 0b00000100) != 0)
                            makePath(10, 0, 10, 10);
                        if ((v & 0b00000010) != 0)
                            makePath(20, 10, -10, 10);
                        if ((v & 0b00000001) != 0)
                            makePath(10, 20, -10, -10);
                    }
                    //xml.Root.Add(new XElement(txt.Name,
                    //    new XAttribute(txt.AttributeI("x").Name, $"{xOffset + xSpacing * x + zSpacingX * z + dx + 10}"),
                    //    new XAttribute(txt.AttributeI("y").Name, $"{yOffset + ySpacing * y + zSpacingY * z + dy + 13}"),
                    //    new XAttribute(txt.AttributeI("style").Name, txt.AttributeI("style").Value.Replace("text-anchor:start", "text-anchor:middle").Replace("fill:#47c040", "fill:#2266aa")),
                    //    v.ToString()));
                }

                drawPattern(i % 3, (i / 3) % 3, i / 3 / 3, 40, 0, allCubelets[i].U);
                drawPattern(i % 3, (i / 3) % 3, i / 3 / 3, 0, 20, allCubelets[i].B);
                drawPattern(i % 3, (i / 3) % 3, i / 3 / 3, 20, 20, allCubelets[i].L);
                drawPattern(i % 3, (i / 3) % 3, i / 3 / 3, 40, 20, allCubelets[i].F);
                drawPattern(i % 3, (i / 3) % 3, i / 3 / 3, 60, 20, allCubelets[i].R);
                drawPattern(i % 3, (i / 3) % 3, i / 3 / 3, 40, 40, allCubelets[i].D);
            }
            File.WriteAllText(@"D:\c\Qoph\DataFiles\Signal Link\Maze (v5, 2 after algorithm added lines).svg", xml.ToString(SaveOptions.DisableFormatting));

            // Now remove the marker bit that tells the above code which lines to draw
            for (var i = 0; i < 27; i++)
                allCubelets[i] = allCubelets[i].Apply(c => new Cubelet(c.U & 0b11111111, c.B & 0b11111111, c.L & 0b11111111, c.F & 0b11111111, c.R & 0b11111111, c.D & 0b11111111));

            var allCubeletsAllOrientations = allCubelets.SelectMany((c, ix) => c.AllRotations.Select((cl, ori) => (cubeletId: ix, orientation: ori, cubelet: cl))).ToArray();

            IEnumerable<(int cubeletId, int orientation, Cubelet cubelet)[]> recurse((int cubeletId, int orientation, Cubelet cubelet)?[] sofar, (int cubeletId, int orientation, Cubelet cubelet)[][] candidates, int level)
            {
                if (sofar.All(v => v != null))
                {
                    yield return sofar.Select(tup => tup.Value).ToArray();
                    yield break;
                }

                var cIx = 13; // middle center cubelet
                var bestIxSize = int.MaxValue;
                for (var i = 0; i < 27; i++)
                    if (sofar[i] == null && candidates[i] != null && candidates[i].Length < bestIxSize)
                    {
                        cIx = i;
                        bestIxSize = candidates[i].Length;
                        if (bestIxSize == 0)
                            yield break;
                    }
                if (cIx == -1)
                    Debugger.Break();

                foreach (var tup in candidates[cIx] ?? allCubeletsAllOrientations)
                {
                    sofar[cIx] = tup;
                    var (cubeletId, orientation, cubelet) = tup;
                    var cx = cIx % 3;
                    var cy = (cIx / 3) % 3;
                    var cz = cIx / 3 / 3;
                    foreach (var solution in recurse(sofar, candidates.Select((arr, ix) => sofar[ix] != null ? null : (arr ?? allCubeletsAllOrientations).Where(tup =>
                            tup.cubeletId != cubeletId &&
                            (ix % 3 != cx - 1 || (ix / 3) % 3 != cy || ix / 3 / 3 != cz || tup.cubelet.R == mirrored(cubelet.L)) &&
                            (ix % 3 != cx + 1 || (ix / 3) % 3 != cy || ix / 3 / 3 != cz || tup.cubelet.L == mirrored(cubelet.R)) &&
                            (ix % 3 != cx || (ix / 3) % 3 != cy - 1 || ix / 3 / 3 != cz || tup.cubelet.D == flipped(cubelet.U)) &&
                            (ix % 3 != cx || (ix / 3) % 3 != cy + 1 || ix / 3 / 3 != cz || tup.cubelet.U == flipped(cubelet.D)) &&
                            (ix % 3 != cx || (ix / 3) % 3 != cy || ix / 3 / 3 != cz - 1 || tup.cubelet.B == mirrored(cubelet.F)) &&
                            (ix % 3 != cx || (ix / 3) % 3 != cy || ix / 3 / 3 != cz + 1 || tup.cubelet.F == mirrored(cubelet.B))).ToArray()).ToArray(), level + 1))
                        yield return solution;
                }
                sofar[cIx] = null;
            }

            var solutions = new List<(int cubeletId, int orientation, Cubelet cubelet)[]>();
            foreach (var solution in recurse(new (int cubeletId, int orientation, Cubelet cubelet)?[27], new (int cubeletId, int orientation, Cubelet cubelet)[27][], 0))
            {
                solutions.Add(solution);
                ConsoleUtil.WriteLine($"SOLUTION #{solutions.Count}".Color(ConsoleColor.White));
                foreach (var inf in solution)
                    ConsoleUtil.WriteLine($"    {inf}".Color(ConsoleColor.DarkCyan));
            }
            Console.WriteLine();
            ConsoleUtil.WriteLine($"Solutions found: {solutions.Count}".Color(ConsoleColor.Green));
        }

        public static void SvgWhitening()
        {
            var xml = XDocument.Parse(File.ReadAllText(@"D:\c\Qoph\DataFiles\Signal Link\Maze (v4, 5 logic puzzle 2).svg"));
            var newPaths = new List<XElement>();
            foreach (var path in xml.Root.Descendants())
            {
                if (path.Name.LocalName != "path")
                    continue;
                var d = SvgPath.Decode(path.AttributeI("d").Value).ToArray();
                if (d.Count(piece => piece.Type == SvgPieceType.End) < 2)
                    continue;
                var ix = d.IndexOf(piece => piece.Type == SvgPieceType.End);
                path.AttributeI("d").Value = d.Subarray(0, ix + 1).JoinString(" ");
                newPaths.Add(new XElement(path.Name, new XAttribute("fill", "#fff"), new XAttribute("d", d.Subarray(ix + 1).JoinString(" "))));
            }
            foreach (var path in newPaths)
                xml.Root.Add(path);
            File.WriteAllText(@"D:\c\Qoph\DataFiles\Signal Link\Maze (v4, 5 logic puzzle 3).svg", xml.ToString(SaveOptions.DisableFormatting));
        }
    }
}
