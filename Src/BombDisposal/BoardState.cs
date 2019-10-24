using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff.BombDisposal
{
    static class BoardState
    {
        public static void Generate()
        {
            var countries = @"BELGIUM|2,BULGARIA-3,ESTONIA-2,FRANCE|2,GERMANY-2,HUNGARY-3,IRELAND|2,ITALY|1,LITHUANIA-3,LUXEMBOURG-3,NETHERLANDS-3,RUSSIA-2,SIERRALEONE-2"
                .Split(',')
                .Select(str => Regex.Match(str, @"^(.*)([\|-])(\d+)$"))
                .Select(m => (name: m.Groups[1].Value, isVertical: m.Groups[2].Value == "|", cityLen: int.Parse(m.Groups[3].Value)))
                .ToArray();

            var intendedSolution = @"SUBMERGENCE";

            IEnumerable<((string name, bool isVertical, int cityLen, int index)[] countries, int score)> recurseFindCountries((int countryIx, int ixInCountryName)[] sofar, int solutionIx)
            {
                if (solutionIx == intendedSolution.Length)
                {
                    var cityLenSum = sofar.Sum(c => countries[c.countryIx].cityLen);
                    var indexSum = sofar.Sum(c => c.ixInCountryName);
                    var score = cityLenSum * cityLenSum + indexSum * indexSum;
                    yield return (sofar.Select(sf => (countries[sf.countryIx].name, countries[sf.countryIx].isVertical, countries[sf.countryIx].cityLen, sf.ixInCountryName)).ToArray(), score);
                    yield break;
                }

                for (int i = 0; i < countries.Length; i++)
                {
                    if (sofar.Any(sf => sf.countryIx == i))
                        continue;
                    var p = countries[i].name.IndexOf(intendedSolution[solutionIx]);
                    if (p == -1)
                        continue;
                    foreach (var solution in recurseFindCountries(sofar.Insert(sofar.Length, (i, p)), solutionIx + 1))
                        yield return solution;
                }
            }

            var allResults = recurseFindCountries(new (int, int)[0], 0).OrderBy(result => result.score).ToArray();
            var already = new HashSet<string>();

            foreach (var (solutionCountries, score) in allResults)
            {
                var gridW = 8;
                var gridH = 8;
                var indexOffset = 2;

                if (score < 1117 || solutionCountries.Any(c => c.index + c.cityLen + indexOffset > (c.isVertical ? gridH : gridW)))
                    continue;

                ConsoleUtil.Write($@"{solutionCountries
                    .Select((c, cIx) => $"{c.name} ({c.index + 1})".ColorSubstring(c.index, 1, ConsoleColor.White, ConsoleColor.DarkBlue))
                    .JoinColoredString(",")} = {score.ToString().Color(ConsoleColor.Green)}, sq={solutionCountries.Sum(c => 3 + c.cityLen)}", null);

                var hash = solutionCountries.Select(c => $"{(c.isVertical ? "|" : "-")}{c.index}-{c.cityLen}").Order().JoinString(",");
                if (!already.Add(hash))
                {
                    ConsoleUtil.WriteLine(" — identical to earlier; skipping".Color(ConsoleColor.Magenta));
                    continue;
                }
                Console.WriteLine();

                IEnumerable<string[]> recurseFillBoard(string[] boardSoFar, int cIx, bool log)
                {
                    if (cIx == solutionCountries.Length)
                    {
                        yield return boardSoFar;
                        yield break;
                    }

                    var (name, isVertical, cityLen, index) = solutionCountries[cIx];

                    for (var i = 0; i < gridW * gridH; i++)
                    {
                        //if (log)
                        //    Console.WriteLine(i);

                        var x = i % gridW;
                        var y = i / gridW;
                        for (var orientation = -1; orientation <= 1; orientation += 2)
                        {
                            var extent = orientation * (index + indexOffset + cityLen - 1);
                            if (!isVertical && y < gridH - 2 && (x + extent) >= 0 && (x + extent) < gridW && boardSoFar[i] == null && boardSoFar[i + gridW] == null && boardSoFar[i + 2 * gridW] == null &&
                                Enumerable.Range(0, cityLen).All(c => boardSoFar[x + orientation * (index + indexOffset + c) + gridW * (y + 1)] == null))
                            {
                                var boardCopy = (string[]) boardSoFar.Clone();
                                boardCopy[i] = name;
                                boardCopy[i + gridW] = name;
                                boardCopy[i + 2 * gridW] = name;
                                for (int c = 0; c < cityLen; c++)
                                    boardCopy[x + orientation * (index + indexOffset + c) + gridW * (y + 1)] = name;
                                foreach (var solution in recurseFillBoard(boardCopy, cIx + 1, false))
                                    yield return solution;
                            }

                            if (isVertical && x < gridW - 2 && (y + extent) >= 0 && (y + extent) < gridH && boardSoFar[i] == null && boardSoFar[i + 1] == null && boardSoFar[i + 2] == null &&
                                Enumerable.Range(0, cityLen).All(c => boardSoFar[x + 1 + gridW * (y + orientation * (index + indexOffset + c))] == null))
                            {
                                var boardCopy = (string[]) boardSoFar.Clone();
                                boardCopy[i] = name;
                                boardCopy[i + 1] = name;
                                boardCopy[i + 2] = name;
                                for (int c = 0; c < cityLen; c++)
                                    boardCopy[x + 1 + gridW * (y + orientation * (index + indexOffset + c))] = name;
                                foreach (var solution in recurseFillBoard(boardCopy, cIx + 1, false))
                                    yield return solution;
                            }
                        }
                    }
                }

                var fewestAdjacentEmptyCells = -1;
                foreach (var solution in recurseFillBoard(new string[gridW * gridH], 0, true))
                {
                    var numAdjacentEmptyCells = Enumerable.Range(0, gridW * gridH).Count(i => (solution[i] == null && (i % gridW < gridW - 1) && solution[i + 1] == null) || (solution[i] == null && (i / gridW < gridH - 1) && solution[i + gridW] == null));
                    if (fewestAdjacentEmptyCells == -1 || numAdjacentEmptyCells < fewestAdjacentEmptyCells)
                    {
                        Console.WriteLine($"Num adjacent empty cells: {numAdjacentEmptyCells}");
                        for (int row = 0; row < gridH; row++)
                        {
                            ConsoleUtil.WriteLine(Enumerable.Range(0, gridW)
                                .Select(col =>
                                {
                                    var country = solution[col + gridW * row];
                                    return country == null ? "  " : country.Substring(0, 2).Color(ConsoleColor.White, (ConsoleColor) (countries.IndexOf(c => c.name == country) + 1));
                                })
                                .JoinColoredString(""));
                        }
                        Console.WriteLine();
                        fewestAdjacentEmptyCells = numAdjacentEmptyCells;
                    }
                }
            }
        }
    }
}
