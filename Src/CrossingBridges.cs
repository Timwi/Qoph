using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace Qoph
{
    static class CrossingBridges
    {
        public static void RunHashiwokakeroSolver()
        {
            const int w = 15;

            var islands = new Dictionary<int, int>
            {
                [9 + w * 0] = 2,
                [6 + w * 0] = 4,
                [4 + w * 0] = 4,
                [6 + w * 2] = 1,
                [2 + w * 0] = 4,
                [0 + w * 0] = 3,
                [0 + w * 2] = 2,
                [14 + w * 1] = 4,
                [9 + w * 1] = 7,
                [14 + w * 3] = 3,
                [7 + w * 1] = 3,
                [9 + w * 3] = 5,
                [7 + w * 3] = 3,
                [5 + w * 1] = 4,
                [1 + w * 1] = 4,
                [5 + w * 3] = 4,
                [1 + w * 5] = 4,
                [13 + w * 2] = 3,
                [10 + w * 2] = 3,
                [13 + w * 4] = 3,
                [10 + w * 5] = 1,
                [4 + w * 2] = 2,
                [2 + w * 2] = 3,
                [2 + w * 4] = 3,
                [0 + w * 4] = 2,
                [14 + w * 5] = 3,
                [9 + w * 6] = 5,
                [3 + w * 3] = 2,
                [3 + w * 5] = 2,
                [13 + w * 6] = 5,
                [8 + w * 4] = 3,
                [6 + w * 4] = 4,
                [8 + w * 7] = 4,
                [4 + w * 4] = 1,
                [6 + w * 6] = 3,
                [2 + w * 6] = 4,
                [0 + w * 10] = 4,
                [14 + w * 7] = 3,
                [5 + w * 5] = 1,
                [1 + w * 7] = 3,
                [11 + w * 6] = 4,
                [13 + w * 10] = 3,
                [9 + w * 8] = 2,
                [2 + w * 8] = 2,
                [14 + w * 9] = 3,
                [12 + w * 9] = 2,
                [5 + w * 7] = 3,
                [8 + w * 9] = 3,
                [3 + w * 7] = 2,
                [3 + w * 10] = 4,
                [1 + w * 9] = 1,
                [7 + w * 8] = 2,
                [4 + w * 8] = 3,
                [4 + w * 11] = 6,
                [2 + w * 10] = 5,
                [14 + w * 11] = 3,
                [6 + w * 9] = 2,
                [6 + w * 11] = 3,
                [3 + w * 14] = 7,
                [11 + w * 10] = 3,
                [9 + w * 10] = 3,
                [7 + w * 10] = 3,
                [7 + w * 12] = 1,
                [2 + w * 13] = 2,
                [0 + w * 12] = 2,
                [12 + w * 11] = 3,
                [10 + w * 11] = 5,
                [8 + w * 11] = 2,
                [8 + w * 13] = 1,
                [10 + w * 13] = 4,
                [4 + w * 13] = 2,
                [1 + w * 11] = 2,
                [1 + w * 14] = 4,
                [14 + w * 12] = 4,
                [11 + w * 12] = 2,
                [14 + w * 14] = 3,
                [12 + w * 13] = 2,
                [11 + w * 14] = 4,
                [9 + w * 14] = 5,
                [9 + w * 12] = 1,
                [7 + w * 14] = 3,
                [5 + w * 18] = 4,
                [5 + w * 20] = 6,
                [8 + w * 18] = 2,
                [6 + w * 16] = 3,
                [6 + w * 15] = 2,
                [9 + w * 15] = 2,
                [8 + w * 16] = 4,
                [5 + w * 22] = 4,
                [4 + w * 19] = 4,
                [4 + w * 21] = 1,
                [7 + w * 20] = 3,
                [4 + w * 17] = 4,
                [6 + w * 19] = 2,
                [9 + w * 19] = 4,
                [7 + w * 22] = 5,
                [7 + w * 17] = 2,
                [9 + w * 17] = 2,
                [10 + w * 16] = 3,
                [10 + w * 18] = 3,
                [11 + w * 18] = 2,
                [9 + w * 22] = 4,
                [10 + w * 22] = 2,
                [4 + w * 15] = 1,
                [3 + w * 19] = 2,
            };

            var sb = new StringBuilder();
            foreach (var kvp in islands)
                sb.AppendLine($"[{kvp.Key % w}+w*{kvp.Key / w}]={kvp.Value},");
            Clipboard.SetText(sb.ToString());

            GenerateHashiwokakeroSvg(w, null, islands, "puzzle");

            var count = 0;
            foreach (var (solution, deductions) in SolveHashiwokakero(w, islands))
            {
                count++;
                var topLeft = "M -0.5 6.5 h 2 v -3 h 1 v 2 h 4 v -1 h -3 v -1 h 3 v -4 h -3 v 1 h -3 v -1 h -1 v 2 h 4 v 1 h -3 v 3 h -1 z";
                var topRight = "M 7.5 -.5 h 3 v 1 h 1 v -1 h 3 v 2 h -3 v 4 h 2 v 1 h -5 v -3 h 1 v -2 h -2 z";
                var bottomLeft = "M 4.5 9.5 h 2 v 5 h -1 v -4 h -1 v 3 h -1 v -3 h -1 v 3 h -1 v -3 h -1 v 2 h -1 v -4 h 1 v 1 h 1 v -2 h 1 v 2 h 1 v -2 h 1 z";
                var bottomRight = "M 9.5 13.5 h 4 v -1 h -3 v -1 h 4 v -4 h -1 v 3 h -3 v -1 h 2 v -2 h -2 v 1 h -3 v 1 h 2 v 1 h -2 v 3 h 1 v -2 h 1 z";
                var bottomBottom = "M 6.5 20.5 h -2 v -2 h 2 v -1 h -3 v -1 h 4 v -1 h 1 v 2 h 2 v 1 h -3 v 1 h 3 v 3 h -4 z";
                GenerateHashiwokakeroSvg(w, solution, islands, "solution",
                    svgExtra: $"<path d='{topLeft}{topRight}{bottomLeft}{bottomRight}{bottomBottom}' fill='none' stroke='green' stroke-width='.1' />");
                Console.WriteLine(deductions.JoinString("\n"));
                Console.WriteLine();
            }
            ConsoleUtil.WriteLine($"{count} solutions found.".Color(count == 1 ? ConsoleColor.White : ConsoleColor.Yellow, count == 1 ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed));
        }

        public static IEnumerable<((int fromCell, int toCell, int numBridges)[] solution, List<string> deductions)> SolveHashiwokakero(int w, Dictionary<int, int> islands)
        {
            bool validWithNoInBetween(int cell1, int cell2) => cell1 != cell2 &&
                ((cell1 % w == cell2 % w && Enumerable.Range(Math.Min(cell1 / w, cell2 / w) + 1, Math.Abs(cell1 / w - cell2 / w) - 1).All(row => !islands.ContainsKey(cell1 % w + w * row))) ||
                (cell1 / w == cell2 / w && Enumerable.Range(Math.Min(cell1 % w, cell2 % w) + 1, Math.Abs(cell1 % w - cell2 % w) - 1).All(col => !islands.ContainsKey(col + w * (cell1 / w)))));
            var allBridgeCandidates = islands.Keys.UniquePairs().Select(pair => (fromCell: pair.Item1, toCell: pair.Item2)).Where(pair => validWithNoInBetween(pair.fromCell, pair.toCell)).ToList();
            return SolveHashiwokakero(w, new List<(int fromCell, int toCell, int numBridges)>(), allBridgeCandidates, islands.ToDictionary(), new List<string>());
        }

        private static IEnumerable<((int fromCell, int toCell, int numBridges)[] solution, List<string> deductions)> SolveHashiwokakero(int w, List<(int fromCell, int toCell, int numBridges)> bridgesSoFar, List<(int fromCell, int toCell)> bridgesAvailable, Dictionary<int, int> remainingIslands, List<string> deductions)
        {
            if (remainingIslands.Values.Sum() > 4 * bridgesAvailable.Count)
                yield break;

            void dec(int cell, int amount)
            {
                var prevVal = remainingIslands[cell];
                if (prevVal == amount)
                {
                    remainingIslands.Remove(cell);
                    bridgesAvailable.RemoveAll(bridge => bridge.fromCell == cell || bridge.toCell == cell);
                }
                else if (prevVal > amount)
                    remainingIslands[cell] = prevVal - amount;
                else
                    throw new InvalidOperationException();
            }

            keepGoing:
            List<(int fromCell, int toCell)> fewestPossibilities = null;
            var fewestPossibilitiesCell = -1;
            foreach (var (cell, count) in remainingIslands)
            {
                // Find bridges that are forced
                var av = bridgesAvailable.Where(tup => tup.fromCell == cell || tup.toCell == cell).ToList();

                if (av.Count * 2 < count)   // This island is impossible to satisfy
                    yield break;

                if (av.Count * 2 == count)
                {
                    foreach (var (fromCell, toCell) in av)
                    {
                        bridgesSoFar.Add((fromCell, toCell, 2));
                        if (remainingIslands[fromCell] < 2 || remainingIslands[toCell] < 2)     // impossible to satisfy
                            yield break;
                        dec(fromCell, 2);
                        dec(toCell, 2);
                        bridgesAvailable.RemoveAll(br => wouldCross(w, br.fromCell, br.toCell, fromCell, toCell));
                    }
                    if (remainingIslands.ContainsKey(cell))
                        Debugger.Break();
                    goto keepGoing;
                }

                if (av.Count == 1 && count == 1)
                {
                    var (fromCell, toCell) = av[0];
                    deductions.Add($"{fromCell}→{toCell} is forced");
                    bridgesSoFar.Add((fromCell, toCell, 1));
                    dec(fromCell, 1);
                    dec(toCell, 1);
                    bridgesAvailable.RemoveAll(br => wouldCross(w, br.fromCell, br.toCell, fromCell, toCell));
                    if (remainingIslands.ContainsKey(cell))
                        Debugger.Break();
                    goto keepGoing;
                }

                // Find the island with the fewest remaining possible bridges
                if (fewestPossibilities == null || av.Count < fewestPossibilities.Count)
                {
                    fewestPossibilities = av;
                    fewestPossibilitiesCell = cell;
                }
            }

            if (bridgesAvailable.Count == 0)
            {
                if (remainingIslands.Count == 0)
                    yield return (bridgesSoFar.ToArray(), deductions.ToList());
                yield break;
            }

            var bridge = fewestPossibilities[0];
            deductions.Add($"Examining: {bridge.fromCell}→{bridge.toCell}");
            bridgesAvailable.Remove(bridge);

            // Try without this bridge
            foreach (var result in SolveHashiwokakero(w, bridgesSoFar.ToList(), bridgesAvailable.ToList(), remainingIslands.ToDictionary(), deductions.ToList()))
                yield return result;

            // Try with a single bridge
            dec(bridge.fromCell, 1);
            dec(bridge.toCell, 1);
            bridgesSoFar.Add((bridge.fromCell, bridge.toCell, 1));
            bridgesAvailable.RemoveAll(br => wouldCross(w, br.fromCell, br.toCell, bridge.fromCell, bridge.toCell));
            foreach (var result in SolveHashiwokakero(w, bridgesSoFar.ToList(), bridgesAvailable.ToList(), remainingIslands.ToDictionary(), deductions.ToList()))
                yield return result;

            // Try with a double bridge
            if (remainingIslands.ContainsKey(bridge.fromCell) && remainingIslands.ContainsKey(bridge.toCell))
            {
                bridgesSoFar.RemoveAt(bridgesSoFar.Count - 1);
                dec(bridge.fromCell, 1);
                dec(bridge.toCell, 1);
                bridgesSoFar.Add((bridge.fromCell, bridge.toCell, 2));
                foreach (var result in SolveHashiwokakero(w, bridgesSoFar.ToList(), bridgesAvailable.ToList(), remainingIslands.ToDictionary(), deductions.ToList()))
                    yield return result;
            }
        }

        public static void GenerateHashiwokakero()
        {
            //var bridges = GenerateHashiwokakero(267, size);   // This seems to produce a Hashiwokakero where all islands/bridges are connected
        }

        private static bool wouldCross(int w, int from1, int to1, int from2, int to2)
        {
            if (from2 % w == to2 % w)
                return from1 / w == to1 / w &&
                    Math.Min(from1 % w, to1 % w) < from2 % w && Math.Max(from1 % w, to1 % w) > from2 % w &&
                    Math.Min(from2 / w, to2 / w) < from1 / w && Math.Max(from2 / w, to2 / w) > from1 / w;
            else if (from2 / w == to2 / w)
                return from1 % w == to1 % w &&
                    Math.Min(from1 / w, to1 / w) < from2 / w && Math.Max(from1 / w, to1 / w) > from2 / w &&
                    Math.Min(from2 % w, to2 % w) < from1 % w && Math.Max(from2 % w, to2 % w) > from1 % w;
            throw new InvalidOperationException();
        }

        private static void GenerateHashiwokakeroSvg(int w, (int fromCell, int toCell, int numBridges)[] bridgesSolution, Dictionary<int, int> islands, string filenameExtra = null, string svgExtra = null)
        {
            var svg = new StringBuilder();
            svg.Append($"<rect stroke='none' fill='#bdf' x='-.5' y='-.5' width='7' height='7' />");
            svg.Append($"<rect stroke='none' fill='#bdf' x='7.5' y='-.5' width='7' height='7' />");
            svg.Append($"<rect stroke='none' fill='#bdf' x='-.5' y='7.5' width='7' height='7' />");
            svg.Append($"<rect stroke='none' fill='#bdf' x='7.5' y='7.5' width='7' height='7' />");
            svg.Append($"<rect stroke='none' fill='#bdf' x='3.5' y='15.5' width='7' height='7' />");
            if (bridgesSolution != null)
            {
                // Find disjointed pieces
                var pieces = new List<List<(int fromCell, int toCell, int numBridges)>>();
                var remaining = bridgesSolution.ToList();
                while (remaining.Count > 0)
                {
                    var start = remaining[0];
                    var piece = new List<(int fromCell, int toCell, int numBridges)> { start };
                    remaining.RemoveAt(0);
                    while (remaining.Count > 0)
                    {
                        var other = remaining.SelectIndexWhere(tup1 => piece.Any(tup2 => tup1.fromCell == tup2.fromCell || tup1.fromCell == tup2.toCell || tup1.toCell == tup2.fromCell || tup1.toCell == tup2.toCell)).FirstOrNull();
                        if (other == null)
                            break;
                        piece.Add(remaining[other.Value]);
                        remaining.RemoveAt(other.Value);
                    }
                    pieces.Add(piece);
                }

                foreach (var (fromCell, toCell, numBridges) in bridgesSolution)
                {
                    for (var b = 0; b < numBridges; b++)
                    {
                        var xOffset = fromCell % w == toCell % w ? -(numBridges - 1) * .07 + b * .14 : 0;
                        var yOffset = fromCell / w == toCell / w ? -(numBridges - 1) * .07 + b * .14 : 0;
                        svg.Append($"<line x1='{fromCell % w + xOffset}' y1='{fromCell / w + yOffset}' x2='{toCell % w + xOffset}' y2='{toCell / w + yOffset}' stroke='hsl({360 * pieces.IndexOf(p => p.Contains((fromCell, toCell, numBridges))) / pieces.Count}, 75%, 50%)' />");
                    }
                }
            }
            foreach (var cell in islands.Keys)
            {
                svg.Append($"<circle fill='white' cx='{cell % w}' cy='{cell / w}' r='.3' />");
                if (bridgesSolution == null)
                    svg.Append($"<text x='{cell % w}' y='{cell / w + .2}' stroke='none' fill='black'>{islands[cell]}</text>");
                else
                {
                    svg.Append($"<text x='{cell % w}' y='{cell / w + .1}' stroke='none' fill='black' font-size='.4'>{islands[cell]}</text>");
                    svg.Append($"<text x='{cell % w}' y='{cell / w + .2}' stroke='none' fill='black' font-size='.1'>{cell % w} + w * {cell / w}</text>");
                }
            }
            File.WriteAllText($@"D:\c\Qoph\DataFiles\Objectionable Ranking\Crossing Bridges\Hashiwokakero{(filenameExtra == null ? "" : $" ({filenameExtra})")}.svg",
                $@"<svg xmlns='http://www.w3.org/2000/svg' viewBox='-1 -1 16 24' fill='none' stroke='black' stroke-width='.04' font-size='.5' font-family='Presquire' text-anchor='middle'>{svg}{svgExtra}</svg>");
        }

        // Warning: this algorithm does not guarantee that all bridges are connected in the final output
        public static IEnumerable<(int fromCell, int toCell, int numBridges)> GenerateHashiwokakero(int w, int seed)
        {
            var locker = new object();

            var islands = new HashSet<int>();
            var rnd = new Random(seed);

            // Step 1: place islands in random places
            for (var i = 0; i < w * w; i++)
                if (i % w != i / w && ((i % 2 == 1) ^ (i % w > i / w)) && rnd.Next(0, 4) != 0)
                    islands.Add(i);

            // Step 2: connect islands with bridges at random
            bool validWithNoInBetween(int cell1, int cell2) => cell1 != cell2 &&
                ((cell1 % w == cell2 % w && Enumerable.Range(Math.Min(cell1 / w, cell2 / w) + 1, Math.Abs(cell1 / w - cell2 / w) - 1).All(row => !islands.Contains(cell1 % w + w * row))) ||
                (cell1 / w == cell2 / w && Enumerable.Range(Math.Min(cell1 % w, cell2 % w) + 1, Math.Abs(cell1 % w - cell2 % w) - 1).All(col => !islands.Contains(col + w * (cell1 / w)))));

            // All pairs of bridges that could potentially form a connection
            var allPairs = islands.UniquePairs().Select(pair => (fromCell: pair.Item1, toCell: pair.Item2)).Where(pair => validWithNoInBetween(pair.fromCell, pair.toCell)).ToList();

            // Bridges we’ve placed
            var bridges = new List<(int fromCell, int toCell, int numBridges)>();

            // Determines if two islands are already indirectly connected through multiple bridges
            HashSet<int> getConnectedCells(int cell)
            {
                var q = new Queue<int>();
                q.Enqueue(cell);
                var already = new HashSet<int>();
                while (q.Count > 0)
                {
                    var item = q.Dequeue();
                    if (!already.Add(item))
                        continue;
                    foreach (var (fromCell, toCell, numBridges) in bridges)
                        if (fromCell == item)
                            q.Enqueue(toCell);
                        else if (toCell == item)
                            q.Enqueue(fromCell);
                }
                return already;
            }

            // Keep choosing valid pairs of islands and put a bridge between them
            while (allPairs.Count > 0)
            {
                // Prefer to connect an island that has only one possible connection left. This avoids (but does not completely prevent) unconnected islands.
                var preferredCells = islands.Where(cell => allPairs.Count(tup => tup.fromCell == cell || tup.toCell == cell) == 1).ToArray();
                var preferredPairs = allPairs.SelectIndexWhere(tup => preferredCells.Contains(tup.fromCell) || preferredCells.Contains(tup.toCell)).ToArray();
                var pairIx = preferredPairs.Length == 0 ? rnd.Next(0, allPairs.Count) : preferredPairs.PickRandom(rnd);
                var (fromCell, toCell) = allPairs[pairIx];
                allPairs.RemoveAt(pairIx);
                // If they are already indirectly connected, we don’t _need_ to connect them, but we can
                var numBridges = getConnectedCells(fromCell).Contains(toCell) ? rnd.Next(0, 3) : rnd.Next(1, 3);
                if (numBridges > 0)
                {
                    bridges.Add((fromCell, toCell, numBridges));
                    allPairs.RemoveAll(pair => wouldCross(w, fromCell, toCell, pair.fromCell, pair.toCell));
                }
            }

            //// Find all connected groups. This should find one large connected groups and a splattering of individual unconnected clusters
            //var connectedGroups = new List<HashSet<int>>();
            //var islandsCopy = islands.ToList();
            //while (islandsCopy.Count > 0)
            //{
            //    var connectedCells = getConnectedCells(islandsCopy[0]);
            //    connectedGroups.Add(connectedCells);
            //    islandsCopy.RemoveAll(cell => connectedCells.Contains(cell));
            //}
            //connectedGroups.Sort(CustomComparer<HashSet<int>>.By(h => -h.Count));

            return bridges;
        }
    }
}