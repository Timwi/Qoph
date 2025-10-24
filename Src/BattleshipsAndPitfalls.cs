using System.Diagnostics;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace Qoph
{
    static class BattleshipsAndPitfalls
    {
        public static void DoStatuePark()
        {
            var grid = Ut.NewArray<bool?>(7, 7);
            //grid[5][6] = false;
            //grid[4][6] = false;

            var count = 0;
            bool?[][] commonalities = null;
            foreach (var solution in StatueParkSolver.Solve(grid, ["#.#/###", "####/.#", "###/.#", "####/#", "..#/###", "#/##"], allowFlip: false))
            {
                Console.WriteLine(solution.Select(row => row.Select(cell => cell ? "██" : "··").JoinString()).JoinString("\n"));
                Console.WriteLine();
                count++;

                if (commonalities == null)
                    commonalities = solution.Select(row => row.Select(b => b.Nullable()).ToArray()).ToArray();
                else
                    for (var y = 0; y < 7; y++)
                        for (var x = 0; x < 7; x++)
                            if (commonalities[y][x] != solution[y][x])
                                commonalities[y][x] = null;
            }
            Console.WriteLine(count);
            Console.WriteLine();
            if (commonalities != null)
                Console.WriteLine(commonalities.Select(row => row.Select(cell => cell == null ? "??" : cell == true ? "██" : "··").JoinString()).JoinString("\n"));
            Console.WriteLine();
        }

        public static void DoFillomino()
        {
            var grid = @"5	2	2	4	4	4	3
5	3	3	3	1	4	3
5	4	4	4	2	2	3
5	4	2	2	6	6	6
5	6	6	6	2	6	6
1	6	2	1	2	6	5
6	6	2	5	5	5	5".Replace("\r", "").Split('\n').SelectMany(row => row.Split('\t').Select(cell => cell == "" ? (int?) null : int.Parse(cell))).ToArray();

            const int w = 7;
            const int h = 7;

            static IEnumerable<int> getAdj(int cell)
            {
                if (cell % w != 0)
                    yield return cell - 1;
                if (cell % w != w - 1)
                    yield return cell + 1;
                if (cell / w != 0)
                    yield return cell - w;
                if (cell / w != h - 1)
                    yield return cell + w;
            }

            IEnumerable<int[]> enumeratePolyominos(int?[] grid, int[] incompletePolyomino, int desiredSize, int[] usedCells)
            {
                if (incompletePolyomino.Length > desiredSize)
                    yield break;
                if (incompletePolyomino.Length == desiredSize)
                {
                    if (!incompletePolyomino.Any(cell => getAdj(cell).Any(adj => !incompletePolyomino.Contains(adj) && grid[adj] == desiredSize)))
                        yield return incompletePolyomino.ToArray();
                    yield break;
                }

                var used = new List<int>(usedCells);
                foreach (var adj in incompletePolyomino.SelectMany(cell => getAdj(cell)).Distinct().Where(cell => !usedCells.Contains(cell) && !incompletePolyomino.Contains(cell) && (grid[cell] == null || grid[cell] == desiredSize)))
                {
                    used.Add(adj);
                    foreach (var poly in enumeratePolyominos(grid, incompletePolyomino.Insert(incompletePolyomino.Length, adj), desiredSize, used.ToArray()))
                        yield return poly;
                }
            }

            int[] findPolyomino(int?[] grid, int startCell)
            {
                if (grid[startCell] is null or 0)
                    Debugger.Break();
                var poly = new List<int> { startCell };
                while (true)
                {
                    var prevCount = poly.Count;
                    poly.AddRange(poly.SelectMany(cell => getAdj(cell)).Distinct().Where(cell => !poly.Contains(cell) && grid[cell] == grid[startCell]).ToArray());
                    if (poly.Count == prevCount)
                        return poly.ToArray();
                }
            }

            IEnumerable<int[][]> recurseFillomino(int?[] grid, int[][] polysSoFar, List<(int[] cells, int desiredSize)> allPolys)
            {
                shortcut:
                if (polysSoFar.Length > 0)
                {
                    var orderedLast = polysSoFar.Last().Order().ToArray();
                    if (polysSoFar.SkipLast(1).Any(p => p.Order().SequenceEqual(orderedLast)))
                    {
                        foreach (var polyomino in polysSoFar)
                        {
                            Console.WriteLine(polyomino.JoinString(", "));
                            ConsoleUtil.WriteLine(Enumerable.Range(0, w * h).Select(cell => { var poly = polysSoFar.FirstOrDefault(p => p.Contains(cell)); return poly == null ? "??" : (polyomino.Contains(cell) ? "██" : "▒▒").Color((ConsoleColor) (poly.Length)); }).Split(w).Select(row => row.JoinColoredString()).JoinColoredString("\n"));
                            Console.WriteLine();
                        }
                        Debugger.Break();
                    }
                }

                if (polysSoFar.Sum(ar => ar.Length) == w * h)
                {
                    yield return polysSoFar.ToArray();
                    yield break;
                }

                if (allPolys.Count == 0)
                {
                    var firstEmptyCell = grid.IndexOf(ch => ch == null);
                    if (firstEmptyCell == -1)
                    {
                        foreach (var polyomino in polysSoFar)
                        {
                            Console.WriteLine(polyomino.JoinString(", "));
                            ConsoleUtil.WriteLine(Enumerable.Range(0, w * h).Select(cell => { var poly = polysSoFar.FirstOrDefault(p => p.Contains(cell)); return poly == null ? "??" : (polyomino.Contains(cell) ? "██" : "▒▒").Color((ConsoleColor) (poly.Length)); }).Split(w).Select(row => row.JoinColoredString()).JoinColoredString("\n"));
                            Console.WriteLine();
                        }
                        Console.WriteLine(polysSoFar.Sum(p => p.Length));
                        Console.WriteLine(w * h);
                        Console.WriteLine();
                    }
                    for (var size = 1; size <= 20; size++)
                    {
                        if (getAdj(firstEmptyCell).All(cell => grid[cell] == null || grid[cell].Value != size))
                        {
                            var newGrid = grid.ToArray();
                            newGrid[firstEmptyCell] = size;
                            foreach (var solution in recurseFillomino(newGrid, polysSoFar, [(new[] { firstEmptyCell }, size)]))
                                yield return solution;
                        }
                    }
                    yield break;
                }

                int[] bestPolyomino = null;
                var bestPolySize = 0;
                int[][] bestPossibleCompletions = null;
                var bestIx = -1;

                for (var polyIx = 0; polyIx < allPolys.Count; polyIx++)
                {
                    var (polyomino, polySize) = allPolys[polyIx];
                    var poss = enumeratePolyominos(grid, polyomino, polySize, []).ToArray();
                    if (poss.Length == 0)
                        yield break;
                    if (poss.Length == 1)
                    {
                        polysSoFar = polysSoFar.Insert(polysSoFar.Length, poss[0]);
                        foreach (var possCell in poss[0])
                            grid[possCell] = polySize;
                        allPolys.RemoveAt(polyIx);
                        allPolys.RemoveAll(poly => poly.cells.Any(cell => poss[0].Contains(cell)));
                        goto shortcut;
                    }
                    if (polyIx == 0 || poss.Length < bestPossibleCompletions.Length)
                    {
                        bestPolyomino = polyomino;
                        bestPolySize = polySize;
                        bestPossibleCompletions = poss;
                        bestIx = polyIx;
                    }
                }

                foreach (var poss in bestPossibleCompletions)
                {
                    var newGrid = grid.ToArray();
                    foreach (var possCell in poss)
                        newGrid[possCell] = bestPolySize;
                    foreach (var result in recurseFillomino(newGrid, polysSoFar.Insert(polysSoFar.Length, poss), allPolys.Where((poly, ix) => ix != bestIx && poly.cells.All(cell => !poss.Contains(cell))).ToList()))
                        yield return result;
                }
            }

            IEnumerable<int[][]> solveFillomino(int?[] grid)
            {
                var allPolys = new List<(int[] cells, int desiredSize)>();
                for (var cell = 0; cell < grid.Length; cell++)
                {
                    if (grid[cell] == null || allPolys.Any(poly => poly.cells.Contains(cell)))
                        continue;
                    var polyomino = findPolyomino(grid, cell);
                    allPolys.Add((polyomino, grid[cell].Value));
                }
                return recurseFillomino(grid, [], allPolys);
            }

            var possibleSwaps = Ut.NewArray(
                // There two 4-length ships that could be rotated or swapped
                (orig: new[] { 3, 4, 5, 6, 42, 43, 44, 45 }, rearrangements: Ut.NewArray<int[]>(
                    [3, 4, 5, 6, 42, 43, 44, 45],
                    [3, 4, 5, 6, 45, 44, 43, 42],
                    [6, 5, 4, 3, 42, 43, 44, 45],
                    [6, 5, 4, 3, 45, 44, 43, 42],
                    [42, 43, 44, 45, 3, 4, 5, 6],
                    [45, 44, 43, 42, 3, 4, 5, 6],
                    [42, 43, 44, 45, 6, 5, 4, 3],
                    [45, 44, 43, 42, 6, 5, 4, 3]
                )),
                // There are three 3-length ships
                (orig: new[] { 18, 19, 20, 30, 31, 32, 34, 41, 48 }, rearrangements: Ut.NewArray(
                    new[] { 18, 19, 20, 30, 31, 32, 34, 41, 48 },
                    new[] { 20, 19, 18, 30, 31, 32, 34, 41, 48 },
                    new[] { 18, 19, 20, 32, 31, 30, 34, 41, 48 },
                    new[] { 20, 19, 18, 32, 31, 30, 34, 41, 48 },
                    new[] { 18, 19, 20, 30, 31, 32, 48, 41, 34 },
                    new[] { 20, 19, 18, 30, 31, 32, 48, 41, 34 },
                    new[] { 18, 19, 20, 32, 31, 30, 48, 41, 34 },
                    new[] { 20, 19, 18, 32, 31, 30, 48, 41, 34 },
                    new[] { 18, 19, 20, 34, 41, 48, 30, 31, 32 },
                    new[] { 20, 19, 18, 34, 41, 48, 30, 31, 32 },
                    new[] { 18, 19, 20, 48, 41, 34, 30, 31, 32 },
                    new[] { 20, 19, 18, 48, 41, 34, 30, 31, 32 },
                    new[] { 18, 19, 20, 34, 41, 48, 32, 31, 30 },
                    new[] { 20, 19, 18, 34, 41, 48, 32, 31, 30 },
                    new[] { 18, 19, 20, 48, 41, 34, 32, 31, 30 },
                    new[] { 20, 19, 18, 48, 41, 34, 32, 31, 30 },
                    new[] { 30, 31, 32, 18, 19, 20, 34, 41, 48 },
                    new[] { 32, 31, 30, 18, 19, 20, 34, 41, 48 },
                    new[] { 30, 31, 32, 20, 19, 18, 34, 41, 48 },
                    new[] { 32, 31, 30, 20, 19, 18, 34, 41, 48 },
                    new[] { 30, 31, 32, 18, 19, 20, 48, 41, 34 },
                    new[] { 32, 31, 30, 18, 19, 20, 48, 41, 34 },
                    new[] { 30, 31, 32, 20, 19, 18, 48, 41, 34 },
                    new[] { 32, 31, 30, 20, 19, 18, 48, 41, 34 },
                    new[] { 30, 31, 32, 34, 41, 48, 18, 19, 20 },
                    new[] { 32, 31, 30, 34, 41, 48, 18, 19, 20 },
                    new[] { 30, 31, 32, 48, 41, 34, 18, 19, 20 },
                    new[] { 32, 31, 30, 48, 41, 34, 18, 19, 20 },
                    new[] { 30, 31, 32, 34, 41, 48, 20, 19, 18 },
                    new[] { 32, 31, 30, 34, 41, 48, 20, 19, 18 },
                    new[] { 30, 31, 32, 48, 41, 34, 20, 19, 18 },
                    new[] { 32, 31, 30, 48, 41, 34, 20, 19, 18 },
                    new[] { 34, 41, 48, 18, 19, 20, 30, 31, 32 },
                    new[] { 48, 41, 34, 18, 19, 20, 30, 31, 32 },
                    new[] { 34, 41, 48, 20, 19, 18, 30, 31, 32 },
                    new[] { 48, 41, 34, 20, 19, 18, 30, 31, 32 },
                    new[] { 34, 41, 48, 18, 19, 20, 32, 31, 30 },
                    new[] { 48, 41, 34, 18, 19, 20, 32, 31, 30 },
                    new[] { 34, 41, 48, 20, 19, 18, 32, 31, 30 },
                    new[] { 48, 41, 34, 20, 19, 18, 32, 31, 30 },
                    new[] { 34, 41, 48, 30, 31, 32, 18, 19, 20 },
                    new[] { 48, 41, 34, 30, 31, 32, 18, 19, 20 },
                    new[] { 34, 41, 48, 32, 31, 30, 18, 19, 20 },
                    new[] { 48, 41, 34, 32, 31, 30, 18, 19, 20 },
                    new[] { 34, 41, 48, 30, 31, 32, 20, 19, 18 },
                    new[] { 48, 41, 34, 30, 31, 32, 20, 19, 18 },
                    new[] { 34, 41, 48, 32, 31, 30, 20, 19, 18 },
                    new[] { 48, 41, 34, 32, 31, 30, 20, 19, 18 }
                ))
            );

            //var counter = 0;
            //while (true)
            //{
            //    var seed = 247 + 100 * counter;
            //    Console.WriteLine($"Trying seed: {seed}");
            //    var rnd = new Random(seed);
            //    var cells = Enumerable.Range(0, w * h).ToList().Shuffle(rnd);
            //    var required = Ut.ReduceRequiredSet(cells, skipConsistencyTest: true, test: set =>
            //    {
            //        Console.WriteLine(Enumerable.Range(0, w * h).Select(i => set.SetToTest.Contains(i) ? "█" : "░").JoinString());
            //        var thisGrid = new int?[w * h];
            //        foreach (var cell in set.SetToTest)
            //            thisGrid[cell] = grid[cell];

            //        var applicableGrids = new List<int?[]>();
            //        var (sh3Orig, sh3Rearrangements) = possibleSwaps[0];
            //        var (sh4Orig, sh4Rearrangements) = possibleSwaps[1];
            //        foreach (var rearr3 in sh3Rearrangements)
            //            foreach (var rearr4 in sh4Rearrangements)
            //            {
            //                var newGrid = thisGrid.ToArray();
            //                for (var i = 0; i < rearr3.Length; i++)
            //                    newGrid[sh3Orig[i]] = thisGrid[rearr3[i]];
            //                for (var i = 0; i < rearr4.Length; i++)
            //                    newGrid[sh4Orig[i]] = thisGrid[rearr4[i]];
            //                if (applicableGrids.Any(gr => gr.SequenceEqual(newGrid)))
            //                    continue;
            //                applicableGrids.Add(newGrid);
            //            }

            //        var possibleSolutions = new List<int[][]>();
            //        applicableGrids.ParallelForEach(Environment.ProcessorCount, grid =>
            //        {
            //            foreach (var solution in solveFillomino(grid).Take(2))
            //                lock (possibleSolutions)
            //                {
            //                    possibleSolutions.Add(solution);
            //                    if (possibleSolutions.Count > 1)
            //                        break;
            //                }
            //        });
            //        if (possibleSolutions.Count == 0)
            //            Debugger.Break();
            //        return possibleSolutions.Count == 1;
            //    }).ToArray();

            //    Console.WriteLine(required.JoinString(", "));
            //    File.AppendAllLines(@"D:\temp\temp.txt", new[] { required.JoinString(", ") });
            //    counter++;
            //}

            var testGrid = @"		2				
		3		1	4	3
			4	2		3
			2		6	6
	6		6	2		
1			1	2	6	
	6				5	5".Replace("\r", "").Replace("\n", "\t").Split('\t').Select(c => c == "" ? (int?) null : int.Parse(c)).ToArray();

            var applicableGrids = new List<int?[]>();
            var (sh3Orig, sh3Rearrangements) = possibleSwaps[0];
            var (sh4Orig, sh4Rearrangements) = possibleSwaps[1];
            foreach (var rearr3 in sh3Rearrangements)
                foreach (var rearr4 in sh4Rearrangements)
                {
                    var newGrid = testGrid.ToArray();
                    for (var i = 0; i < rearr3.Length; i++)
                        newGrid[sh3Orig[i]] = testGrid[rearr3[i]];
                    for (var i = 0; i < rearr4.Length; i++)
                        newGrid[sh4Orig[i]] = testGrid[rearr4[i]];
                    if (applicableGrids.Any(gr => gr.SequenceEqual(newGrid)))
                        continue;
                    applicableGrids.Add(newGrid);
                }

            var possibleSolutions = new List<int[][]>();
            applicableGrids.ParallelForEach(Environment.ProcessorCount, grid =>
            {
                foreach (var solution in solveFillomino(grid).Take(2))
                    lock (possibleSolutions)
                    {
                        possibleSolutions.Add(solution);
                        if (possibleSolutions.Count > 1)
                            break;
                    }
            });

            foreach (var solution in possibleSolutions)
            {
                if (solution.First(s => s.Contains(6 * 7 + 1)).Length != 6)
                    continue;
                ConsoleUtil.WriteLine(Enumerable.Range(0, w * h).Select(cell => { var poly = solution.FirstOrDefault(p => p.Contains(cell)); return poly == null ? "??" : "██".Color((ConsoleColor) (poly.Length)); }).Split(w).Select(row => row.JoinColoredString()).JoinColoredString("\n"));
                Console.WriteLine();
            }
        }
    }
}