using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff
{
    static class BombDisposal
    {
        private static (string first, string second)[] getEpisodes()
        {
            return File.ReadAllLines(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Big Bang Meta\Big Bang Theory episode titles.txt")
                .Select(e => (str: e, match: Regex.Match(e, @"^The (.*) (\w+)$")))
                .Select(e => e.match.Success ? e : throw new InvalidOperationException(e.str))
                .Select(e => (first: e.match.Groups[1].Value, second: e.match.Groups[2].Value))
                .Where(e => (e.first + e.second).All(ch => ch == ' ' || (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z')))
                .ToArray();
        }

        private static IEnumerable<string> getEpisodeWords()
        {
            return getEpisodes().SelectMany(tup => new[] { tup.first, tup.second }).Select(w => w.ToUpperInvariant().Where(ch => ch >= 'A' && ch <= 'Z').JoinString()).Distinct();
        }

        sealed class Candidate
        {
            public string CandidateName;
            public string PuzzleSolution;
        }

        public static void CheckEpisodeWords()
        {
            var epi = generateSolutions();
            foreach (var row in epi)
            {
                //Console.WriteLine(row.Letter1);
                //Clipboard.SetText(row.LeftCandidates
                //    .OrderBy(lc => lc.PuzzleSolution[0] == lc.CandidateName[0] ? "ZZ" + lc.PuzzleSolution : lc.PuzzleSolution)
                //    .Select(lc => $"{(lc.PuzzleSolution[0] == lc.CandidateName[0] ? "~" : "")}{lc.PuzzleSolution}")
                //    .JoinString("\n"));
                //Console.ReadLine();

                Console.WriteLine(row.Letter2);
                Clipboard.SetText(row.RightCandidates
                    .OrderBy(lc => lc.PuzzleSolution[0] == lc.CandidateName[0] ? "ZZ" + lc.PuzzleSolution : lc.PuzzleSolution)
                    .Select(lc => $"{(lc.PuzzleSolution[0] == lc.CandidateName[0] ? "~" : "")}{lc.PuzzleSolution}")
                    .JoinString("\n"));
                Console.ReadLine();
            }
        }

        sealed class Row
        {
            public char Letter1;
            public char Letter2;
            public Candidate[] LeftCandidates;
            public Candidate[] RightCandidates;
        }

        private static Row[] generateSolutions()
        {
            var episodes = getEpisodes();

            var results = new List<Row>();
            foreach (var pair in "PA,NI,CA,VE,RT,ED".Split(','))
            {
                var leftCandidates = episodes.Where(e => e.first.StartsWith(pair[0]) && !e.second.StartsWith(pair[1]) && episodes.Count(e2 => e2.second.EqualsNoCase(e.second)) == 1).ToArray();
                if (leftCandidates.Length == 0)
                    Debugger.Break();
                var rightCandidates = episodes.Where(e => e.second.StartsWith(pair[1]) && !e.first.StartsWith(pair[0]) && episodes.Count(e2 => e2.first.EqualsNoCase(e.first)) == 1).ToArray();
                if (rightCandidates.Length == 0)
                    Debugger.Break();

                results.Add(new Row
                {
                    Letter1 = pair[0],
                    Letter2 = pair[1],
                    LeftCandidates = leftCandidates.Select(lc => new Candidate { CandidateName = lc.first, PuzzleSolution = lc.second }).ToArray(),
                    RightCandidates = rightCandidates.Select(rc => new Candidate { CandidateName = rc.second, PuzzleSolution = rc.first }).ToArray()
                });
            }

            return results.ToArray();
        }

        public static void BoardStateExperiment()
        {
            var countries = @"BELGIUM|2,BULGARIA-3,ESTONIA-2,FRANCE|2,GERMANY-2,HUNGARY-3,IRELAND|2,ITALY|1,LITHUANIA-3,LUXEMBOURG-3,NETHERLANDS-3,RUSSIA-2,SIERRALEONE-2".Split(',');
            var solutions = generateSolutions();
            var candidateSolutionWords = solutions.SelectMany(row => row.LeftCandidates.Concat(row.RightCandidates).Select(c => c.PuzzleSolution.Replace(" ", "").ToUpperInvariant())).Distinct().ToArray();

            foreach (var sol in solutions)
            {
                foreach (var lc in sol.LeftCandidates)
                    if (lc.PuzzleSolution == "Nomenclature")
                        Console.WriteLine($"{lc.CandidateName} {lc.PuzzleSolution} (L, {sol.Letter1}{sol.Letter2})");
                foreach (var rc in sol.RightCandidates)
                    if (rc.CandidateName == "Nomenclature")
                        Console.WriteLine($"{rc.PuzzleSolution} {rc.CandidateName} (R, {sol.Letter1}{sol.Letter2})");
            }

            foreach (var cand in new[] { "NOMENCLATURE" })//candidateSolutionWords.OrderByDescending(cs => cs.Length))
            {
                if (cand.Length > countries.Length)
                    continue;

                IEnumerable<(string[] countries, int[] ixs)> recurse(int[] sofarIxs, int[] ixs, int candIx)
                {
                    if (candIx == cand.Length)
                    {
                        yield return (sofarIxs.Select(ix => countries[ix]).ToArray(), ixs);
                        yield break;
                    }

                    for (int i = 0; i < countries.Length; i++)
                    {
                        if (sofarIxs.Contains(i))
                            continue;
                        var p = countries[i].IndexOf(cand[candIx]);
                        if (p == -1)
                            continue;
                        foreach (var solution in recurse(sofarIxs.Append(i).ToArray(), ixs.Append(p).ToArray(), candIx + 1))
                            yield return solution;
                    }
                }

                void outputResult((string[] countries, int[] ixs) result, int indexSum)
                {
                    ConsoleUtil.WriteLine($@"{result.countries
                                                        .Select((c, cIx) => $"{c} ({result.ixs[cIx] + 1})".ColorSubstring(result.ixs[cIx], 1, ConsoleColor.White, ConsoleColor.DarkBlue))
                                                        .JoinColoredString(",")} = {indexSum.ToString().Color(ConsoleColor.Green)}", null);
                }

                var smallestIndexSum = -1;
                var smallestCityLenMax = -1;
                var cityLengths = countries.Select(c => Regex.Match(c, @"(\d)$")).Select(m => int.Parse(m.Groups[1].Value)).ToArray();
                foreach (var thingie in new[] { "index sum"/*, "city length sum" */})
                {
                    ConsoleUtil.WriteLine(thingie.Color(ConsoleColor.White));
                    foreach (var result in recurse(new int[0], new int[0], 0))
                    {
                        var indexSum = result.ixs.Sum() + result.ixs.Length;
                        var cityLenSum = result.countries.Sum(c => cityLengths[countries.IndexOf(c)]);

                        if (cityLenSum == 27 && thingie == "index sum")
                        {
                            //if (smallestIndexSum == -1 || indexSum < smallestIndexSum)
                            if (indexSum == 33)
                            {
                                outputResult(result, indexSum);
                                smallestIndexSum = indexSum;
                            }
                        }
                        else
                        {
                            if (cityLenSum <= 27 && (smallestCityLenMax == -1 || cityLenSum <= smallestCityLenMax))
                            {
                                outputResult(result, indexSum);
                                smallestCityLenMax = cityLenSum;
                            }
                        }
                    }
                    Console.WriteLine();
                }
                //Console.WriteLine($"{cand} ({cand.Length}) (max ix = {firstResult.maxIndex})");
            }
        }

        public static void BoardStateFill()
        {
            /*
            
            // index sum = 28
            var countries = @"HUNGARY,ROMANIA,GERMANY,BELGIUM,NETHERLANDS,FRANCE,LUXEMBOURG,ITALY,LITHUANIA,BULGARIA,RUSSIA,ESTONIA".Split(',');
            var indexes = @"324215133211".Select(ch => ch - '0').ToArray();
            var horiz = @"TFTFTFTFTTTT".Select(ch => ch == 'T').ToArray();
            var cityLength = @"332232313322".Select(ch => ch - '0').ToArray();

            /*/

            //// index sum = 33
            //var stuff = @"NETHERLANDS-3 (1),ESTONIA-2 (4),GERMANY-2 (4),SIERRALEONE-2 (3),HUNGARY-3 (3),FRANCE|2 (5),LUXEMBOURG-3 (1),ITALY|1 (3),LITHUANIA-3 (3),RUSSIA-2 (2),IRELAND|2 (2),BELGIUM|2 (2)"
            //    .Split(',')
            //    .Select(str => Regex.Match(str, @"^(\w+)([\-\|])(\d) \((\d)\)$"))
            //    .ToArray();
            //var countries = stuff.Select(m => m.Groups[1].Value).ToArray();
            //var horiz = stuff.Select(m => m.Groups[2].Value == "-").ToArray();
            //var cityLength = stuff.Select(m => int.Parse(m.Groups[3].Value)).ToArray();
            //var indexes = stuff.Select(m => int.Parse(m.Groups[4].Value)).ToArray();

            var stuff = @"HUNGARY-3 (3),ESTONIA-2 (4),GERMANY-2 (4),BELGIUM|2 (2),NETHERLANDS-3 (1),FRANCE|2 (5),LITHUANIA-3 (1),IRELAND|2 (5),ITALY|1 (2),LUXEMBOURG-3 (2),RUSSIA-2 (1),SIERRALEONE-2 (3)"
                .Split(',')
                .Select(str => Regex.Match(str, @"^(\w+)([\-\|])(\d) \((\d)\)$"))
                .ToArray();
            var countries = stuff.Select(m => m.Groups[1].Value).ToArray();
            var horiz = stuff.Select(m => m.Groups[2].Value == "-").ToArray();
            var cityLength = stuff.Select(m => int.Parse(m.Groups[3].Value)).ToArray();
            var indexes = stuff.Select(m => int.Parse(m.Groups[4].Value)).ToArray();

            /**/
            var gridW = 8;
            var gridH = 9;
            var indexOffset = 1;
            Console.WriteLine(cityLength.Sum() + 3 * countries.Length);

            IEnumerable<string[]> recurse(string[] boardSoFar, int cIx, bool log)
            {
                if (cIx == countries.Length)
                {
                    yield return boardSoFar;
                    yield break;
                }

                for (var i = 0; i < gridW * gridH; i++)
                {
                    if (log)
                        Console.WriteLine(i);

                    var x = i % gridW;
                    var y = i / gridW;
                    for (var orientation = -1; orientation <= 1; orientation += 2)
                    {
                        var extent = orientation * (indexes[cIx] + indexOffset + cityLength[cIx] - 1);
                        if (horiz[cIx] && y < gridH - 2 && (x + extent) >= 0 && (x + extent) < gridW && boardSoFar[i] == null && boardSoFar[i + gridW] == null && boardSoFar[i + 2 * gridW] == null &&
                            Enumerable.Range(0, cityLength[cIx]).All(c => boardSoFar[x + orientation * (indexes[cIx] + indexOffset + c) + gridW * (y + 1)] == null))
                        {
                            var boardCopy = (string[]) boardSoFar.Clone();
                            boardCopy[i] = countries[cIx];
                            boardCopy[i + gridW] = countries[cIx];
                            boardCopy[i + 2 * gridW] = countries[cIx];
                            for (int c = 0; c < cityLength[cIx]; c++)
                                boardCopy[x + orientation * (indexes[cIx] + indexOffset + c) + gridW * (y + 1)] = countries[cIx];
                            foreach (var solution in recurse(boardCopy, cIx + 1, false))
                                yield return solution;
                        }

                        if (!horiz[cIx] && x < gridW - 2 && (y + extent) >= 0 && (y + extent) < gridH && boardSoFar[i] == null && boardSoFar[i + 1] == null && boardSoFar[i + 2] == null &&
                            Enumerable.Range(0, cityLength[cIx]).All(c => boardSoFar[x + 1 + gridW * (y + orientation * (indexes[cIx] + indexOffset + c))] == null))
                        {
                            var boardCopy = (string[]) boardSoFar.Clone();
                            boardCopy[i] = countries[cIx];
                            boardCopy[i + 1] = countries[cIx];
                            boardCopy[i + 2] = countries[cIx];
                            for (int c = 0; c < cityLength[cIx]; c++)
                                boardCopy[x + 1 + gridW * (y + orientation * (indexes[cIx] + indexOffset + c))] = countries[cIx];
                            foreach (var solution in recurse(boardCopy, cIx + 1, false))
                                yield return solution;
                        }
                    }
                }
            }

            var fewestAdjacentEmptyCells = -1;
            foreach (var solution in recurse(new string[gridW * gridH], 0, true))
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
                                return country == null ? "  " : country.Substring(0, 2).Color(ConsoleColor.White, (ConsoleColor) (countries.IndexOf(country) + 1));
                            })
                            .JoinColoredString(""));
                    }
                    Console.WriteLine();
                    fewestAdjacentEmptyCells = numAdjacentEmptyCells;
                }
            }
        }

        public static void FortySeven_FindMatches_OBSOLETE()
        {
            var roninsRaw = File.ReadAllLines(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\47\Ronin.txt");
            var ronins = roninsRaw.Where(r => roninsRaw.Count(r2 => r2.Equals(r)) == 1).ToArray();
            var prefs = File.ReadAllLines(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\47\Prefectures.txt");

            var count = 0;
            var hashset = new HashSet<char>();
            var matches = new Dictionary<char, List<(string ronin, string pref)>>();
            foreach (var ronin in ronins)
            {
                var ms = prefs
                    .Where(p => p.Length == ronin.Length)
                    .Select(p => (name: p, zip: ronin.Zip(p, (a, b) => (eq: a == b, ch: a)).Where(tup => tup.eq).ToArray()))
                    .Where(inf => inf.zip.Length == 1)
                    .Select(inf => (inf.name, inf.zip[0].ch))
                    .ToArray();
                if (ms.Length > 0)
                {
                    Console.WriteLine($"{ronin} = {ms.Select(tup => tup.ch).Distinct().Order().JoinString()} ({ms.Select(tup => tup.name).JoinString(", ")})");
                    hashset.AddRange(ms.Select(tup => tup.ch));
                    count++;

                    foreach (var (name, ch) in ms)
                        matches.AddSafe(ch, (ronin, pref: name));
                }
            }
            Console.WriteLine($"{count}, letters = {hashset.Order().JoinString()}");
            Console.WriteLine();

            foreach (var word in getEpisodeWords())
                if (word.All(ch => hashset.Contains(ch)))
                    Console.WriteLine(word);
            Console.WriteLine();

            foreach (var letter in "EROSION")
                Console.WriteLine($"{letter} = {matches[letter].Select(m => $"{m.pref} + {m.ronin}").JoinString(" / ")}");
        }

        public static void OneCanHope()
        {
            int LevenshteinDistance(string a, string b)
            {
                int lengthA = a.Length;
                int lengthB = b.Length;
                var distances = new int[lengthA + 1, lengthB + 1];
                for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
                for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

                for (int i = 1; i <= lengthA; i++)
                    for (int j = 1; j <= lengthB; j++)
                        distances[i, j] = Math.Min(Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1), distances[i - 1, j - 1] + (b[j - 1] == a[i - 1] ? 0 : 1));
                return distances[lengthA, lengthB];
            }

            var input = File.ReadAllText(@"D:\temp\temp.txt");
            foreach (var (esp, eng, leven) in Regex.Matches(input, @"<p><b>([^<]+)</b>\s*</p>\s*<dl><dd>([^<]+)</dd></dl>", RegexOptions.Singleline)
                .Cast<Match>()
                .Select(m => (esp: m.Groups[1].Value, eng: Regex.Replace(m.Groups[2].Value, "^(the|a|to) ", "")))
                .Select(m => (m.esp, m.eng, leven: (double) LevenshteinDistance(m.esp, m.eng) / m.esp.Length))
                .OrderBy(inf => inf.leven)
                )
                Console.WriteLine($"{esp} = {eng} = {leven}");
        }
    }
}