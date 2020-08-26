using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using RT.KitchenSink;
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

        const int w = 14;
        const int h = 16;
        const int vs = w * (h + 1); // the index in the array where the vertical lines start
        const int fullSize = vs + (w + 1) * h;

        private static void outputSolution(bool?[] grid, int[] highlight = null, bool[] intendedSolution = null)
        {
            var arr = Ut.NewArray((4 * w + 1) * (2 * h + 1), _ => new ConsoleColoredChar(' ', null));
            // Horiz
            for (var x = 0; x < w; x++)
                for (var y = 0; y <= h; y++)
                    for (var i = 0; i < 3; i++)
                        arr[4 * x + 1 + i + 2 * y * (4 * w + 1)] = new ConsoleColoredChar(
                            grid[x + w * y] == true ? '═' : grid[x + w * y] == false ? ' ' : '─',
                            grid[x + w * y] == true ? ConsoleColor.White : grid[x + w * y] == false ? ConsoleColor.Black : ConsoleColor.DarkGray,
                            (ConsoleColor) ((intendedSolution != null && grid[x + w * y] != null && grid[x + w * y] != intendedSolution[x + w * y] ? 1 : 0) + (highlight != null && highlight.Contains(x + w * y) ? 2 : 0)));
            // Vert
            for (var x = 0; x <= w; x++)
                for (var y = 0; y < h; y++)
                    arr[4 * x + (2 * y + 1) * (4 * w + 1)] = new ConsoleColoredChar(
                        grid[vs + x + (w + 1) * y] == true ? '║' : grid[vs + x + (w + 1) * y] == false ? ' ' : '│',
                        grid[vs + x + (w + 1) * y] == true ? ConsoleColor.White : grid[vs + x + (w + 1) * y] == false ? ConsoleColor.Black : ConsoleColor.DarkGray,
                        (ConsoleColor) ((intendedSolution != null && grid[vs + x + (w + 1) * y] != null && grid[vs + x + (w + 1) * y] != intendedSolution[vs + x + (w + 1) * y] ? 1 : 0) + (highlight != null && highlight.Contains(vs + x + (w + 1) * y) ? 2 : 0)));

            for (var y = 0; y < 2 * h + 1; y++)
                ConsoleUtil.WriteLine(new ConsoleColoredString(Enumerable.Range(0, 4 * w + 1).Select(x => arr[x + (4 * w + 1) * y]).ToArray()));
            Console.WriteLine();
        }

        public static void SolveNonorooms()
        {
            var (horizClues, vertClues, intendedSolution, definiteGivens) = getClues();

            static IEnumerable<bool[]> generate(int[] clue, int size, int clueIx, int left)
            {
                if (clueIx == clue.Length)
                {
                    yield return new bool[size];
                    yield break;
                }
                var leeway = size - left - (clue.Skip(clueIx).Sum() + clue.Length - clueIx - 1);
                for (var i = 0; i <= leeway; i++)
                {
                    foreach (var subsolution in generate(clue, size, clueIx + 1, left + i + clue[clueIx] + 1))
                    {
                        for (var j = 0; j < clue[clueIx]; j++)
                            subsolution[left + i + j] = true;
                        yield return subsolution;
                    }
                }
            }

            IEnumerable<bool[]> recurse(bool?[] sofar, bool[][][] horizCandidates, bool[][][] vertCandidates, int level)
            {
                startAgain:
                var massiveDebugging = false;
                var anyDeduction = false;
                var ix = -1;
                for (var i = 0; i < sofar.Length; i++)
                {
                    if (sofar[i] != null)
                        continue;
                    if (ix == -1)
                        ix = i;

                    // Gather info for loose-end deductions, and perform candidate deductions
                    var surroundings = Ut.NewArray(2, _ => new List<bool?>());
                    var isHoriz = i < vs;
                    var x = (isHoriz ? i : i - vs) % (isHoriz ? w : w + 1);
                    var y = (isHoriz ? i : i - vs) / (isHoriz ? w : w + 1);

                    if (isHoriz)
                    {
                        if (horizCandidates[y].Length == 0)
                            yield break;

                        if (horizCandidates[y].All(c => c[x] == horizCandidates[y][0][x]))
                        {
                            sofar[i] = horizCandidates[y][0][x];
                            if (massiveDebugging)
                                outputSolution(sofar, new[] { i });
                            anyDeduction = true;
                        }

                        // Left end
                        if (x > 0)
                            surroundings[0].Add(sofar[i - 1]);
                        if (y > 0)
                            surroundings[0].Add(sofar[vs + x + (w + 1) * (y - 1)]);
                        if (y < h)
                            surroundings[0].Add(sofar[vs + x + (w + 1) * y]);

                        // Right end
                        if (x < w - 1)
                            surroundings[1].Add(sofar[i + 1]);
                        if (y > 0)
                            surroundings[1].Add(sofar[vs + x + 1 + (w + 1) * (y - 1)]);
                        if (y < h)
                            surroundings[1].Add(sofar[vs + x + 1 + (w + 1) * y]);
                    }
                    else
                    {
                        if (vertCandidates[x].Length == 0)
                            yield break;

                        if (vertCandidates[x].All(c => c[y] == vertCandidates[x][0][y]))
                        {
                            sofar[i] = vertCandidates[x][0][y];
                            if (massiveDebugging)
                                outputSolution(sofar, new[] { i });
                            anyDeduction = true;
                        }

                        // Top end
                        if (y > 0)
                            surroundings[0].Add(sofar[i - (w + 1)]);
                        if (x > 0)
                            surroundings[0].Add(sofar[x - 1 + w * y]);
                        if (x < w)
                            surroundings[0].Add(sofar[x + w * y]);

                        // Bottom end
                        if (y < h - 1)
                            surroundings[1].Add(sofar[i + w + 1]);
                        if (x > 0)
                            surroundings[1].Add(sofar[x - 1 + w * (y + 1)]);
                        if (x < w)
                            surroundings[1].Add(sofar[x + w * (y + 1)]);
                    }

                    // Perform loose-end deductions
                    if (surroundings.Any(surr => surr.Count(s => s == true) == 1 && surr.Count(s => s == null) == 0))   // Must continue a loose end
                    {
                        if (sofar[i] == false)  // Candidate deduction has already concluded one way, thus contradiction
                            yield break;
                        sofar[i] = true;
                        anyDeduction = true;
                    }
                    else if (surroundings.Any(surr => surr.All(s => s == false)))    // Can’t create a loose end
                    {
                        if (sofar[i] == true)  // Candidate deduction has already concluded one way, thus contradiction
                            yield break;
                        sofar[i] = false;
                        anyDeduction = true;
                    }

                    if (sofar[i] != null)
                    {
                        if (isHoriz)
                            horizCandidates[y] = horizCandidates[y].Where(c => c[x] == sofar[i].Value).ToArray();
                        else
                            vertCandidates[x] = vertCandidates[x].Where(c => c[y] == sofar[i].Value).ToArray();
                    }
                }

                if (ix == -1)
                {
                    yield return sofar.Select(b => b.Value).ToArray();
                    yield break;
                }

                if (anyDeduction)
                    goto startAgain;

                var ixIsHoriz = ix < vs;
                var ixX = (ixIsHoriz ? ix : ix - vs) % (ixIsHoriz ? w : w + 1);
                var ixY = (ixIsHoriz ? ix : ix - vs) / (ixIsHoriz ? w : w + 1);

                foreach (var val in new[] { false, true })
                {
                    sofar[ix] = val;
                    foreach (var solution in recurse(sofar.ToArray(),
                        ixIsHoriz ? horizCandidates.Select((hc, y) => y != ixY ? hc : hc.Where(c => c[ixX] == val).ToArray()).ToArray() : horizCandidates.ToArray(),
                        ixIsHoriz ? vertCandidates.ToArray() : vertCandidates.Select((vc, x) => x != ixX ? vc : vc.Where(c => c[ixY] == val).ToArray()).ToArray(), level + 1))
                        yield return solution;
                }
            }

            outputSolution(intendedSolution.Select(b => b.Nullable()).ToArray());

            for (var seed = 1; seed <= 100; seed++)
            {
                //Console.WriteLine($"Seed: {seed}");
                var horizCandidates = horizClues.Select(clue => generate(clue, w, 0, 0).ToArray()).ToArray();
                var vertCandidates = vertClues.Select(clue => generate(clue, h, 0, 0).ToArray()).ToArray();
                var requiredGivens = Ut.ReduceRequiredSet(Enumerable.Range(0, w * (h + 1) + (w + 1) * h).Except(definiteGivens).ToArray().Shuffle(new Random(seed)), skipConsistencyTest: true, test: state =>
                {
                    var grid = new bool?[w * (h + 1) + (w + 1) * h];
                    foreach (var ix in state.SetToTest.Concat(definiteGivens))
                        grid[ix] = intendedSolution[ix];
                    //ConsoleUtil.WriteLine(grid.Split(2).Select(ch => " ▌▐█"[(ch.First() == null ? 0 : 1) + (ch.Last() == null ? 0 : 2)]).JoinString().Color(ConsoleColor.White, ConsoleColor.DarkBlue));
                    return !recurse(grid,
                        horizCandidates.Select((hc, y) => hc.Where(c => Enumerable.Range(0, w).All(x => grid[x + w * y] == null || grid[x + w * y] == c[x])).ToArray()).ToArray(),
                        vertCandidates.Select((vc, x) => vc.Where(c => Enumerable.Range(0, h).All(y => grid[vs + x + (w + 1) * y] == null || grid[vs + x + (w + 1) * y] == c[y])).ToArray()).ToArray(),
                        0).Skip(1).Any();
                }).ToArray();

                Console.WriteLine($"Seed {seed} requires {requiredGivens.Length} givens: {requiredGivens.JoinString(", ")}.");
                outputSolution(intendedSolution.Select((b, ix) => requiredGivens.Contains(ix) ? b.Nullable() : null).ToArray());
                Console.ReadLine();
            }
        }

        public static void SolveSlitherRooms()
        {
            var (_, _, intendedSolution, definiteGivens) = getClues();
            var slitherClues = Ut.NewArray(w * h, i =>
            {
                var x = i % w;
                var y = i / w;
                return
                    (intendedSolution[x + w * y] ? 1 : 0) +
                    (intendedSolution[x + w * (y + 1)] ? 1 : 0) +
                    (intendedSolution[vs + x + (w + 1) * y] ? 1 : 0) +
                    (intendedSolution[vs + x + 1 + (w + 1) * y] ? 1 : 0);
            });
            Console.WriteLine(slitherClues.Split(w).Select(row => row.JoinString(" ")).JoinString("\n"));
            Console.WriteLine();

            IEnumerable<bool[]> recurse(bool?[] sofar, int?[] clues, int level)
            {
                startAgain:
                var anyDeduction = false;

                // LOOSE-END DEDUCTIONS
                for (var i = 0; i < sofar.Length; i++)
                {
                    var surroundings = Ut.NewArray(2, _ => new List<bool?>());
                    var isHoriz = i < vs;
                    var x = (isHoriz ? i : i - vs) % (isHoriz ? w : w + 1);
                    var y = (isHoriz ? i : i - vs) / (isHoriz ? w : w + 1);

                    if (isHoriz)
                    {
                        // Left end
                        if (x > 0)
                            surroundings[0].Add(sofar[i - 1]);
                        if (y > 0)
                            surroundings[0].Add(sofar[vs + x + (w + 1) * (y - 1)]);
                        if (y < h)
                            surroundings[0].Add(sofar[vs + x + (w + 1) * y]);

                        // Right end
                        if (x < w - 1)
                            surroundings[1].Add(sofar[i + 1]);
                        if (y > 0)
                            surroundings[1].Add(sofar[vs + x + 1 + (w + 1) * (y - 1)]);
                        if (y < h)
                            surroundings[1].Add(sofar[vs + x + 1 + (w + 1) * y]);
                    }
                    else
                    {
                        // Top end
                        if (y > 0)
                            surroundings[0].Add(sofar[i - (w + 1)]);
                        if (x > 0)
                            surroundings[0].Add(sofar[x - 1 + w * y]);
                        if (x < w)
                            surroundings[0].Add(sofar[x + w * y]);

                        // Bottom end
                        if (y < h - 1)
                            surroundings[1].Add(sofar[i + w + 1]);
                        if (x > 0)
                            surroundings[1].Add(sofar[x - 1 + w * (y + 1)]);
                        if (x < w)
                            surroundings[1].Add(sofar[x + w * (y + 1)]);
                    }

                    // Must continue a loose end
                    if (surroundings.Any(surr => surr.Count(s => s == true) == 1 && surr.Count(s => s == null) == 0))
                    {
                        if (sofar[i] == false)
                            yield break;
                        else if (sofar[i] == null)
                        {
                            sofar[i] = true;
                            anyDeduction = true;
                        }
                    }
                    // Can’t create a loose end
                    else if (surroundings.Any(surr => surr.All(s => s == false)))
                    {
                        if (sofar[i] == true)
                            yield break;
                        else if (sofar[i] == null)
                        {
                            sofar[i] = false;
                            anyDeduction = true;
                        }
                    }
                }

                // SLITHER-CLUE DEDUCTIONS
                for (var y = 0; y < h; y++)
                    for (var x = 0; x < w; x++)
                        if (clues[x + w * y] != null)
                        {
                            var surroundingIxs = new[] { x + w * y, x + w * (y + 1), vs + x + (w + 1) * y, vs + x + 1 + (w + 1) * y };
                            var trues = surroundingIxs.Count(i => sofar[i] == true);
                            var falses = surroundingIxs.Count(i => sofar[i] == false);
                            var nulls = surroundingIxs.Count(i => sofar[i] == null);
                            var clue = clues[x + w * y].Value;
                            if (trues > clue || falses > 4 - clue)
                                yield break;    // Found a contradiction between the loose-end deduction and a slither clue
                            if (nulls > 0 && trues + nulls == clue)
                            {
                                foreach (var sIx in surroundingIxs)
                                    if (sofar[sIx] == null)
                                        sofar[sIx] = true;
                                anyDeduction = true;
                            }
                            if (nulls > 0 && falses + nulls == 4 - clue)
                            {
                                foreach (var sIx in surroundingIxs)
                                    if (sofar[sIx] == null)
                                        sofar[sIx] = false;
                                anyDeduction = true;
                            }
                        }

                if (anyDeduction)
                    goto startAgain;

                var ix = sofar.IndexOf(v => v == null);
                if (ix == -1)
                {
                    yield return sofar.Select(b => b.Value).ToArray();
                    yield break;
                }

                foreach (var val in new[] { false, true })
                {
                    sofar[ix] = val;
                    foreach (var solution in recurse(sofar.ToArray(), clues, level + 1))
                        yield return solution;
                }
            }

            //var grid = new bool?[fullSize];
            //foreach (var ix in definiteGivens)
            //    grid[ix] = intendedSolution[ix];
            //foreach (var solution in recurse(grid, slitherClues.Select(b => b.Nullable()).ToArray(), 0))
            //    outputSolution(solution.Select(b => b.Nullable()).ToArray(), intendedSolution: intendedSolution);

            var potentialGivens = Enumerable.Range(0, w * h).ToArray();
            var actualGivens = Ut.ReduceRequiredSet(potentialGivens, test: set =>
            {
                var grid = new bool?[fullSize];
                foreach (var ix in definiteGivens)
                    grid[ix] = intendedSolution[ix];
                var clues = new int?[w * h];
                foreach (var ix in set.SetToTest)
                    clues[ix] = slitherClues[ix];
                ConsoleUtil.WriteLine(clues.Split(2).Select(ch => " ▌▐█"[(ch.First() == null ? 0 : 1) + (ch.Last() == null ? 0 : 2)]).JoinString().Color(ConsoleColor.White, ConsoleColor.DarkBlue));
                var ambiguous = recurse(grid, clues, 0).Skip(1).Any();
                return !ambiguous;
            });

            var finishedClues = new int?[w * h];
            foreach (var ix in actualGivens)
                finishedClues[ix] = slitherClues[ix];
            Console.WriteLine(finishedClues.Select(cl => cl == null ? " " : cl.ToString()).Split(w).Select(row => row.JoinString(" ")).JoinString("\n"));

            var xml = XDocument.Parse(File.ReadAllText(@"D:\c\Qoph\DataFiles\Signal Link\Maze (v5, 4 logic puzzle prep).svg"));
            var p = xml.Root.ElementsI("path").FirstOrDefault();
            XName n(string name) => XName.Get(name, p.Name.NamespaceName);
            foreach (var ix in actualGivens)
                xml.Root.Add(new XElement(n("text"), new XAttribute("font-family", "Work Sans"), new XAttribute("text-anchor", "middle"), new XAttribute("x", 20 + 20 * (ix % w)), new XAttribute("y", 24 + 20 * (ix / w)), slitherClues[ix].ToString()));
            File.WriteAllText(@"D:\c\Qoph\DataFiles\Signal Link\Maze (v5, 5 logic puzzle with clues from algorithm).svg", xml.ToString(SaveOptions.DisableFormatting));
        }

        private static (int[][] horizClues, int[][] vertClues, bool[] intendedSolution, int[] definiteGivens) getClues()
        {
            var xml = XDocument.Parse(File.ReadAllText(@"D:\c\Qoph\DataFiles\Signal Link\Maze (v5, 4 logic puzzle prep).svg"));
            var intendedSolution = new bool[fullSize];
            var definiteGivens = new List<int>();
            foreach (var path in xml.Root.Descendants())
            {
                if (path.Name.LocalName != "path" || path.AttributeI("fill") == null)
                    continue;
                var fill = path.AttributeI("fill").Value;
                if (fill == "#fff")
                {
                    var pieces = DecodeSvgPath.DecodePieces(path.AttributeI("d").Value);
                    foreach (var (start, end) in pieces.Where(p => p.Points != null).SelectMany(p => p.Points).ConsecutivePairs(true))
                    {
                        if (start.Y == end.Y)
                        {
                            // Horizontal
                            var leftX = ((int) Math.Round(start.X > end.X ? end.X : start.X) / 10 - 1) / 2;
                            var rightX = ((int) Math.Round(start.X > end.X ? start.X : end.X) / 10 - 1) / 2;
                            var y = ((int) Math.Round(start.Y) / 10 - 1) / 2;
                            for (var x = leftX; x < rightX; x++)
                                intendedSolution[x + w * y] = true;
                        }
                        else if (start.X == end.X)
                        {
                            // Vertical
                            var topY = ((int) Math.Round(start.Y > end.Y ? end.Y : start.Y) / 10 - 1) / 2;
                            var bottomY = ((int) Math.Round(start.Y > end.Y ? start.Y : end.Y) / 10 - 1) / 2;
                            var x = ((int) Math.Round(start.X) / 10 - 1) / 2;
                            for (var y = topY; y < bottomY; y++)
                                intendedSolution[vs + x + (w + 1) * y] = true;
                        }
                    }
                }
                else if (fill != "none" && fill != "#dedede")
                {
                    var pieces = DecodeSvgPath.DecodePieces(path.AttributeI("d").Value);
                    var points = pieces.Where(p => p.Points != null).SelectMany(p => p.Points).Select(p => (x: Math.Round(p.X, 5), y: Math.Round(p.Y, 5))).Distinct().ToArray();
                    var (x, y, num) = points.Aggregate((x: 0.0, y: 0.0, num: 0), (prev, next) => (x: prev.x + next.x, y: prev.y + next.y, num: prev.num + 1));

                    var rx = (int) Math.Round(x / num) / 10 - 2;
                    var ry = (int) Math.Round(y / num) / 10 - 2;
                    if (rx % 2 == 0)
                        // Horiz
                        definiteGivens.Add(rx / 2 + w * (ry + 1) / 2);
                    else
                        // Vert
                        definiteGivens.Add(vs + (rx + 1) / 2 + (w + 1) * ry / 2);
                }
            }

            var hc = Enumerable.Range(0, h + 1).Select(y => intendedSolution.Skip(y * w).Take(w).GroupConsecutive().Where(group => group.Key).Select(group => group.Count).ToArray()).ToArray();
            var vc = Enumerable.Range(0, w + 1).Select(x => Enumerable.Range(0, h).Select(y => intendedSolution[vs + x + (w + 1) * y]).GroupConsecutive().Where(group => group.Key).Select(group => group.Count).ToArray()).ToArray();
            return (hc, vc, intendedSolution, definiteGivens.Order().ToArray());
        }

        public static void SvgWhitening()
        {
            var xml = XDocument.Parse(File.ReadAllText(@"D:\c\Qoph\DataFiles\Signal Link\Maze (v4, 5 logic puzzle 2).svg"));
            var newPaths = new List<XElement>();
            foreach (var path in xml.Root.Descendants())
            {
                if (path.Name.LocalName != "path")
                    continue;
                var d = DecodeSvgPath.DecodePieces(path.AttributeI("d").Value).ToArray();
                if (d.Count(piece => piece.Type == DecodeSvgPath.PathPieceType.End) < 2)
                    continue;
                var ix = d.IndexOf(piece => piece.Type == DecodeSvgPath.PathPieceType.End);
                path.AttributeI("d").Value = d.Subarray(0, ix + 1).JoinString(" ");
                newPaths.Add(new XElement(path.Name, new XAttribute("fill", "#fff"), new XAttribute("d", d.Subarray(ix + 1).JoinString(" "))));
            }
            foreach (var path in newPaths)
                xml.Root.Add(path);
            File.WriteAllText(@"D:\c\Qoph\DataFiles\Signal Link\Maze (v4, 5 logic puzzle 3).svg", xml.ToString(SaveOptions.DisableFormatting));
        }
    }
}
