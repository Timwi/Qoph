using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff.BombDisposal
{
    static class SomethingsFishy
    {// 0 = up; going clockwise
        private static readonly (int left, int right)[] _semaphoreOrientations = new[] { (5, 4), (6, 4), (7, 4), (0, 4), (4, 1), (4, 2), (4, 3), (6, 5), (5, 7), (0, 2), (5, 0), (5, 1), (5, 2), (5, 3), (6, 7), (6, 0), (6, 1), (6, 2), (6, 3), (7, 0), (7, 1), (0, 3), (1, 2), (1, 3), (7, 2), (3, 2) };

        public static void Do()
        {
            const string solution = @"KUHLIIDAE";
            const int w = 6;
            const int h = 6;
            var directions = new (int dx, int dy)[] { (0, -1), (1, -1), (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1) }.Select(d => (dx: 2 * d.dx, dy: 2 * d.dy)).ToArray();

            IEnumerable<int[]> recurse(int[] sofar, int solutionIx)
            {
                if (solutionIx == solution.Length)
                {
                    yield return sofar;
                    yield break;
                }

                var (left, right) = _semaphoreOrientations[solution[solutionIx] - 'A'];
                var (ldx, ldy) = directions[left];
                var (rdx, rdy) = directions[right];

                for (int x = 0; x < w; x++)
                    if (x + ldx >= 0 && x + ldx < w && x + rdx >= 0 && x + rdx < w)
                        for (int y = 0; y < h; y++)
                            if (y + ldy >= 0 && y + ldy < h && y + rdy >= 0 && y + rdy < h)
                            {
                                if (sofar[x + w * y] != 0 || sofar[x + ldx + w * (y + ldy)] != 0 || sofar[x + rdx + w * (y + rdy)] != 0)
                                    continue;
                                var sofarCopy = (int[]) sofar.Clone();
                                sofarCopy[x + w * y] = solutionIx + 1;
                                sofarCopy[x + ldx + w * (y + ldy)] = solutionIx + 1;
                                sofarCopy[x + rdx + w * (y + rdy)] = solutionIx + 1;
                                foreach (var result in recurse(sofarCopy, solutionIx + 1))
                                    yield return result;
                            }
            }

            var minNumAdj = int.MaxValue;
            foreach (var result in recurse(new int[w * h], 0))
            {
                var numAdjacentEmptyCells = Enumerable.Range(0, w * h).Count(i => (result[i] == 0 && (i % w < w - 1) && result[i + 1] == 0) || (result[i] == 0 && (i / w < h - 1) && result[i + w] == 0));
                if (numAdjacentEmptyCells <= minNumAdj)
                {
                    minNumAdj = numAdjacentEmptyCells;
                    ConsoleUtil.WriteLine($"{numAdjacentEmptyCells} adjacent empty cells:".Color(ConsoleColor.White));
                    ConsoleUtil.WriteLine(result.Split(w).Select(row => row.Select(v => (v == 0 ? "" : v.ToString()).PadLeft(2).Color(ConsoleColor.White, (ConsoleColor) v)).JoinColoredString(" ")).JoinColoredString("\n"));
                    Console.WriteLine();
                }
            }
            Console.WriteLine($"Red herrings needed: {w * h - 3 * solution.Length}");
        }
    }
}
