using System;
using RT.Util.ExtensionMethods;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using RT.Util;
using RT.Util.Consoles;

namespace PuzzleStuff.BombDisposal
{
    static class CrossingBridges
    {
        public static void HashiwokakeroExperiment()
        {
            const int size = 15;
            var bridges = GenerateHashiwokakero(267, size);   // This seems to produce a Hashiwokakero where all islands/bridges are connected

            static string generateSvg(string lineColor, IEnumerable<(int fromCell, int toCell, int numBridges)> bridges)
            {
                var svg = new StringBuilder();
                var cells = new HashSet<int>();
                foreach (var (fromCell, toCell, numBridges) in bridges)
                {
                    for (var b = 0; b < numBridges; b++)
                    {
                        var xOffset = fromCell % size == toCell % size ? -(numBridges - 1) * .07 + b * .14 : 0;
                        var yOffset = fromCell / size == toCell / size ? -(numBridges - 1) * .07 + b * .14 : 0;
                        svg.Append($"<line x1='{fromCell % size + xOffset}' y1='{fromCell / size + yOffset}' x2='{toCell % size + xOffset}' y2='{toCell / size + yOffset}' stroke='{lineColor}' />");
                    }
                    cells.Add(fromCell);
                    cells.Add(toCell);
                }
                foreach (var cell in cells)
                    svg.Append($"<circle fill='white' cx='{cell % size}' cy='{cell / size}' r='.3' />");
                return svg.ToString();
            }

            var svg = new StringBuilder();
            svg.Append($"<rect stroke='none' fill='#bdf' x='-.5' y='-.5' width='{size / 2}' height='{size / 2}' />");
            svg.Append($"<rect stroke='none' fill='#bdf' x='{size / 2 + .5}' y='-.5' width='{size / 2}' height='{size / 2}' />");
            svg.Append($"<rect stroke='none' fill='#bdf' x='-.5' y='{size / 2 + .5}' width='{size / 2}' height='{size / 2}' />");
            svg.Append($"<rect stroke='none' fill='#bdf' x='{size / 2 + .5}' y='{size / 2 + .5}' width='{size / 2}' height='{size / 2}' />");
            svg.Append(generateSvg($"black", bridges));
            File.WriteAllText(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Objectionable Ranking\Crossing Bridges\Hashiwokakero.svg", $@"<svg xmlns='http://www.w3.org/2000/svg' viewBox='-1 -1 19 19' fill='none' stroke='black' stroke-width='.04'>{svg}</svg>");

            foreach (var (fromCell, toCell, numBridges) in bridges)
                Console.WriteLine($"({fromCell}, {toCell}, {numBridges}),");
        }

        // Warning: this algorithm does not guarantee that all bridges are connected in the final output
        public static IEnumerable<(int fromCell, int toCell, int numBridges)> GenerateHashiwokakero(int seed, int size)
        {
            var locker = new object();

            var islands = new HashSet<int>();
            var rnd = new Random(seed);

            // Step 1: place islands in random places
            for (var i = 0; i < size * size; i++)
                if (i % size != i / size && ((i % 2 == 1) ^ (i % size > i / size)) && rnd.Next(0, 4) != 0)
                    islands.Add(i);

            // Step 2: connect islands with bridges at random
            bool validWithNoInBetween(int cell1, int cell2) => cell1 != cell2 &&
                ((cell1 % size == cell2 % size && Enumerable.Range(Math.Min(cell1 / size, cell2 / size) + 1, Math.Abs(cell1 / size - cell2 / size) - 1).All(row => !islands.Contains(cell1 % size + size * row))) ||
                (cell1 / size == cell2 / size && Enumerable.Range(Math.Min(cell1 % size, cell2 % size) + 1, Math.Abs(cell1 % size - cell2 % size) - 1).All(col => !islands.Contains(col + size * (cell1 / size)))));

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

                    // Remove all pairs that would now cross over that new bridge
                    if (fromCell % size == toCell % size)
                        allPairs.RemoveAll(pair => pair.fromCell / size == pair.toCell / size &&
                            Math.Min(pair.fromCell % size, pair.toCell % size) < fromCell % size && Math.Max(pair.fromCell % size, pair.toCell % size) > fromCell % size &&
                            Math.Min(fromCell / size, toCell / size) < pair.fromCell / size && Math.Max(fromCell / size, toCell / size) > pair.fromCell / size);
                    else if (fromCell / size == toCell / size)
                        allPairs.RemoveAll(pair => pair.fromCell % size == pair.toCell % size &&
                            Math.Min(pair.fromCell / size, pair.toCell / size) < fromCell / size && Math.Max(pair.fromCell / size, pair.toCell / size) > fromCell / size &&
                            Math.Min(fromCell % size, toCell % size) < pair.fromCell % size && Math.Max(fromCell % size, toCell % size) > pair.fromCell % size);
                    else
                        Debugger.Break();
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