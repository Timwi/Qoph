using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace Qoph
{
    static class TakeTheLight
    {
        public static ((int[] cells, int sum)[] lights, int width, int height) GetKakuro()
        {
            var kakuro = @"
4	9	6	/	9	7	5	/	3	8	1
1	5	2	/	5	1	2	/	1	5	2
3	6	1	2	/	2	3	1	8	9	6
/	/	8	5	1	/	4	8	9	/	/
5	8	9	6	2	3	1	4	/	8	4
3	9	5	/	4	7	6	/	9	7	2
2	6	/	2	5	9	7	6	8	3	1
/	/	6	4	7	/	9	3	7	/	/
5	8	2	1	6	4	/	1	2	8	3
2	5	1	/	8	2	7	/	3	5	1
8	9	5	/	3	1	2	/	1	9	4".Trim().Replace("\r", "").Split('\n').Select(row => row.Split('\t')).ToArray();

            var height = kakuro.Length;
            var width = kakuro[0].Length;
            if (kakuro.Any(row => row.Length != width))
                Debugger.Break();

            var lights =
                // Horizontal lights
                Enumerable.Range(0, height).SelectMany(row => Enumerable.Range(0, width).GroupConsecutiveBy(col => kakuro[row][col] != "/").Where(gr => gr.Key).Select(gr => gr.Select(col => row * width + col).ToArray()))
                // Vertical lights
                .Concat(Enumerable.Range(0, width).SelectMany(col => Enumerable.Range(0, height).GroupConsecutiveBy(row => kakuro[row][col] != "/").Where(gr => gr.Key).Select(gr => gr.Select(row => row * width + col).ToArray()))).ToArray();
            return (lights.Select(light => (cells: light, sum: light.Sum(cell => int.Parse(kakuro[cell / width][cell % width])))).ToArray(), width, height);
        }

        public static void SolveKakuro()
        {
            var (lights, width, _) = GetKakuro();
            SolveKakuro(lights, width);
        }

        private static void SolveKakuro((int[] cells, int sum)[] lights, int gridWidth, int? preferredStartingLight = null)
        {
            var numCells = lights.Max(light => light.cells.Max()) + 1;
            if (numCells % gridWidth != 0)
                numCells += (gridWidth - numCells % gridWidth);
            var gridHeight = numCells / gridWidth;
            var lightsRemaining = lights.Select(light => (light.cells, combinations: FindCombinations(light.cells.Length, light.sum))).ToList();
            var cells = new int[0];
            var combinations = new List<int[]> { new int[0] };
            while (lightsRemaining.Count > 0)
            {
                // Find the light with the fewest combinations, and among those, the one with the most crossings
                var bestLightIx = -1;
                if (cells.Length == 0 && preferredStartingLight != null)
                {
                    bestLightIx = preferredStartingLight.Value;
                    goto immediate;
                }
                var bestNumCombinations = -1;
                var bestNumCrossings = -1;
                for (var i = 0; i < lightsRemaining.Count; i++)
                {
                    var numCrossings = lightsRemaining[i].cells.Count(c => cells.Contains(c));
                    if (cells.Length > 0 && numCrossings == 0)
                        continue;
                    if (numCrossings >= lightsRemaining[i].cells.Length - 1)    // This is guaranteed to not increase the number of combinations
                    {
                        bestLightIx = i;
                        goto immediate;
                    }
                    if (bestLightIx == -1 || lightsRemaining[i].combinations.Length < bestNumCombinations || (lightsRemaining[i].combinations.Length == bestNumCombinations && numCrossings > bestNumCrossings))
                    {
                        bestLightIx = i;
                        bestNumCrossings = numCrossings;
                        bestNumCombinations = lightsRemaining[i].combinations.Length;
                    }
                }

                immediate:
                var (newLightCells, newLightCombinations) = lightsRemaining[bestLightIx];

                // Debug output
                ConsoleUtil.WriteLine(Enumerable.Range(0, gridHeight).Select(row =>
                    Enumerable.Range(0, gridWidth).Select(col =>
                    {
                        var cell = col + gridWidth * row;
                        var p = cells.IndexOf(cell);
                        ConsoleColoredString str;
                        if (p == -1)
                            str = lightsRemaining.Any(l => l.cells.Contains(col + gridWidth * row)) ? "▒▒" : "██";
                        else
                        {
                            var uniqueValues = combinations.Select(c => c[p]).Distinct().Take(3).ToArray();
                            str = uniqueValues.Length == 1 ? uniqueValues[0].ToString().PadLeft(2).Color(ConsoleColor.White, ConsoleColor.DarkBlue) :
                                uniqueValues.Length == 2 ? uniqueValues.JoinString().Color(ConsoleColor.Green, ConsoleColor.DarkGreen) :
                                "??".Color(ConsoleColor.DarkGray);
                        }
                        return newLightCells.Contains(cell) ? str.ColorBackground(ConsoleColor.DarkRed) : str;
                    }).JoinColoredString()
                ).JoinColoredString("\n"));
                Console.WriteLine($"Combinations: {combinations.Count} × {newLightCombinations.Length}");

                // Combine them!
                var newCells = cells.Union(newLightCells).ToArray();

                var newCombinations = (List<int[]>) Enumerable.Range(0, combinations.Count).ParallelSelectMany(Environment.ProcessorCount, ix =>
                {
                    var oldCombination = combinations[ix];
                    lock (newCells)
                        Console.Write($"{ix}/{combinations.Count}\r");
                    return newLightCombinations
                        .Where(newCombination => cells.Select((cell, ix) => newLightCells.IndexOf(cell).Apply(p => p == -1 || newCombination[p] == oldCombination[ix])).All(b => b))
                        .Select(newCombination => newCells.Select(cell => newLightCells.IndexOf(cell).Apply(p => p == -1 ? oldCombination[cells.IndexOf(cell)] : newCombination[p])).ToArray());
                });
                Console.Write("Culling...              \r");

                lightsRemaining.RemoveAt(bestLightIx);

                for (var lIx = 0; lIx < lightsRemaining.Count; lIx++)
                    if (lightsRemaining[lIx].cells.Intersect(newLightCells).Any())
                        lightsRemaining[lIx] = (lightsRemaining[lIx].cells, lightsRemaining[lIx].combinations.Where(comb => comb.Select((val, ix) =>
                        {
                            var combIx = cells.IndexOf(lightsRemaining[lIx].cells[ix]);
                            return combIx == -1 || combinations.Any(c => c[combIx] == val);
                        }).All(b => b)).ToArray());
                Console.WriteLine(new string(' ', 30));

                cells = newCells;
                combinations = newCombinations;
            }

            // OUTPUT SOLUTION
            ConsoleUtil.WriteLine($"SOLUTIONS: {combinations.Count}".Color(ConsoleColor.White, ConsoleColor.DarkGreen));
            ConsoleUtil.WriteLine(Enumerable.Range(0, gridHeight).Select(row =>
                Enumerable.Range(0, gridWidth).Select(col =>
                {
                    var cell = col + gridWidth * row;
                    var p = cells.IndexOf(cell);
                    ConsoleColoredString str;
                    if (p == -1)
                        str = lightsRemaining.Any(l => l.cells.Contains(col + gridWidth * row)) ? "▒▒" : "██";
                    else
                    {
                        var uniqueValues = combinations.Select(c => c[p]).Distinct().Take(3).ToArray();
                        str = uniqueValues.Length == 1 ? uniqueValues[0].ToString().PadLeft(2).Color(ConsoleColor.White, ConsoleColor.DarkBlue) :
                            uniqueValues.Length == 2 ? uniqueValues.JoinString().Color(ConsoleColor.Green, ConsoleColor.DarkGreen) :
                            "??".Color(ConsoleColor.DarkGray);
                    }
                    return str;
                }).JoinColoredString()
            ).JoinColoredString("\n"));
            Console.WriteLine();
        }

        private static int[][] FindCombinations(int length, int sum)
        {
            static IEnumerable<int[]> recurse(int[] sofar, int ix, int sum)
            {
                if (ix == sofar.Length)
                {
                    if (sofar.Sum() == sum)
                        yield return sofar.ToArray();
                    yield break;
                }

                for (var i = 1; i <= 9; i++)
                {
                    if (sofar.Take(ix).Contains(i))
                        continue;
                    sofar[ix] = i;
                    foreach (var solution in recurse(sofar, ix + 1, sum))
                        yield return solution;
                }
            }
            return recurse(new int[length], 0, sum).ToArray();
        }

        public static void KakuroInteractive()
        {
            while (true)
            {
                var line = Console.ReadLine().Split(' ');
                if (line[0] == "exit")
                    break;
                try
                {
                    var unique = false;
                    var regexes = new List<string>();
                    foreach (var extra in line.Skip(2))
                        if (extra == "u")
                            unique = true;
                        else
                            regexes.Add(extra);
                    var numCells = int.Parse(line[1]);
                    var used = Ut.NewArray(numCells, _ => new HashSet<int>());
                    foreach (var comb in FindCombinations(numCells, int.Parse(line[0])))
                        if (!unique || Enumerable.Range(0, comb.Length - 1).All(ix => comb[ix] < comb[ix + 1]))
                            if (regexes.All(regex => Regex.IsMatch(comb.JoinString(), regex)))
                            {
                                Console.WriteLine(comb.JoinString(", "));
                                for (var i = 0; i < comb.Length; i++)
                                    used[i].Add(comb[i]);
                            }
                    Console.WriteLine($"Summary: {used.Select(set => $"[{set.Order().JoinString()}]").JoinString(" ")}");
                    Console.WriteLine();
                }
                catch (Exception e)
                {
                    ConsoleUtil.WriteLine($"{e.Message.Color(ConsoleColor.Magenta)} {$"({e.GetType().FullName})".Color(ConsoleColor.DarkMagenta)}", null);
                }
            }
        }

        public static void GenerateSvg()
        {
            var (lights, width, height) = GetKakuro();
            var lightsWithHeads = lights.Select(light =>
            {
                var isVert = light.cells[1] % width == light.cells[0] % width;
                var x = isVert ? light.cells[0] % width : light.cells.Min() % width - 1;
                var y = isVert ? light.cells.Min() / width - 1 : light.cells[0] / width;
                return (light.cells, light.sum, isVert, head: (x, y));
            }).ToArray();

            var svg = new StringBuilder();
            svg.Append($"<rect x='-1' y='-1' width='{width + 2}' height='{height + 2}' stroke='none' fill='hsl(220, 10%, 50%)' />");
            foreach (var cell in lights.SelectMany(l => l.cells).Distinct())
                svg.Append($"<rect x='{cell % width}' y='{cell / width}' width='1' height='1' fill='white' />");
            var path = new StringBuilder();
            for (var y = 0; y < height + 3; y++)
                path.Append($"M -1 {y} {width + 1} {y}");
            for (var x = 0; x < width + 3; x++)
                path.Append($"M {x} -1 {x} {height + 1}");
            foreach (var (x, y) in lightsWithHeads.Select(light => light.head).Distinct())
                path.Append($"M{x} {y} l 1 1");
            svg.Append($"<path fill='none' stroke='hsl(220, 10%, 75%)' stroke-width='.025' d='{path}' />");
            svg.Append($"<rect x='-1.0125' y='-1.0125' width='{width + 2.025}' height='{height + 2.025}' stroke='black' stroke-width='.05' fill='none' />");
            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\Objectionable Ranking\Objectionable Ranking.html", "<!--%%-->", "<!--%%%-->", svg.ToString());

            svg = new StringBuilder();
            foreach (var (cells, sum, isVert, head) in lightsWithHeads)
                svg.Append($"<text x='{head.x + (isVert ? .3 : .7)}' y='{head.y + (isVert ? .85 : .4)}' fill='white'>{sum}</text>");
            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\Objectionable Ranking\Objectionable Ranking.html", "<!--##-->", "<!--###-->", svg.ToString());
        }
    }
}