using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CsQuery;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;
using RT.Util.Text;

namespace PuzzleStuff
{
    using static Md;

    static class BombDisposal
    {
        private static (string first, string second)[] getEpisodes()
        {
            return File.ReadAllLines(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Big Bang Theory episode titles.txt")
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

        public static void GenerateSolutionCandidates()
        {
            // Alternates between first and second part of the episode title

            var epi = generateSolutions();
            foreach (var row in epi)
            {
                Console.WriteLine(row.Letter1);
                Clipboard.SetText(row.LeftCandidates
                    .OrderBy(lc => lc.PuzzleSolution[0] == lc.CandidateName[0] ? "ZZ" + lc.PuzzleSolution : lc.PuzzleSolution)
                    .Select(lc => $"{(lc.PuzzleSolution[0] == lc.CandidateName[0] ? "~" : "")}{lc.PuzzleSolution}")
                    .JoinString("\n"));
                Console.ReadLine();

                Console.WriteLine(row.Letter2);
                Clipboard.SetText(row.RightCandidates
                    .OrderBy(lc => lc.PuzzleSolution[0] == lc.CandidateName[0] ? "ZZ" + lc.PuzzleSolution : lc.PuzzleSolution)
                    .Select(lc => $"{(lc.PuzzleSolution[0] == lc.CandidateName[0] ? "~" : "")}{lc.PuzzleSolution}")
                    .JoinString("\n"));
                Console.ReadLine();
            }
        }

        public static void GenerateSolutionCandidates_Unused()
        {
            // Unused alternative: use only the first part of the episode titles

            var epi = getEpisodes();
            const string solution = "PANICAVERTED";
            for (int i = 0; i < solution.Length; i++)
            {
                var ltr = solution.Substring(i, 1);
                ConsoleUtil.WriteLine(ltr.Color(ConsoleColor.Yellow));
                ConsoleUtil.WriteLine(epi
                    .Where(e => e.second.StartsWith(ltr) && epi.Count(e2 => e2.first.Equals(e.first)) == 1)
                    .Select(e => e.first.Color(ConsoleColor.Green) + " " + e.second.Color(ConsoleColor.DarkGreen))
                    .JoinColoredString("\n"));
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

        public static void BoardStateGenerate()
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

        public static void FortySeven_FindMatches_OBSOLETE()
        {
            // Old idea where the 47 ronin are represented by initials and prefectures by flags.
            // You have to find the ronin where the middle name is the same length as the prefecture
            // and then extract the one and only letter they have in common in the same place.

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

            // Show all possible solution words that could be formed. (At time of writing, only EROSION works.)
            foreach (var word in getEpisodeWords())
                if (word.All(ch => hashset.Contains(ch)))
                    Console.WriteLine(word);
            Console.WriteLine();

            foreach (var letter in "EROSION")
                Console.WriteLine($"{letter} = {matches[letter].Select(m => $"{m.pref} + {m.ronin}").JoinString(" / ")}");
        }

        public static void FortySeven_WordMatches()
        {
            var phrase = @"SATOSHINAKAMOTOCURRENCY";
            Console.WriteLine(phrase.Length);
            var words = @"BETATEST,APRICOTS,FIGHTING,CHARMING,GANYMEDE".Split(',');
            var tt = new TextTable { ColumnSpacing = 1 };
            for (var i = 0; i < phrase.Length; i++)
                tt.SetCell(i + 1, 0, phrase[i].ToString().Color(ConsoleColor.White));
            for (var row = 0; row < words.Length; row++)
            {
                tt.SetCell(0, row + 1, words[row].Color(ConsoleColor.Yellow));
                for (var i = 0; i < phrase.Length; i++)
                    if (words[row].Contains(phrase[i]))
                        tt.SetCell(i + 1, row + 1, phrase[i].ToString().Color(ConsoleColor.Green));
            }
            for (var i = 0; i < phrase.Length; i++)
                if (words.All(w => !w.Contains(phrase[i])))
                    tt.SetCell(i + 1, words.Length + 1, phrase[i].ToString().Color(ConsoleColor.Magenta));
            tt.WriteToConsole();
        }

        public static void FortySeven_ConstructMatrix_OBSOLETE()
        {
            // Puzzle idea involving the 47 prefectures and Hill Cipher (but not the ronin)

            var prefs = File.ReadAllLines(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\47\Prefectures.txt");
            const string cluephrase2 = "SATOSHINAKAMOTOCURRENCY";
            var cluephrase = "TAKE RED,CHANNEL,MODEIGHT,AND BLUE,MOD NINE,AND THEN,INDEX".Split(',');
            //var cluephrase = "RED,CHANNEL,MODSEVEN,BLUE,CHANNEL,MOD TEN,INDEX".Split(',');

            var words = File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt");
            var wordRnd = new Random(8472);

            //for (int i = 0; i < 7; i++)
            //    Console.WriteLine(words.Where(w => w.Length == 8 && w[0] - 'A' == i && w.All(ch => ch >= 'A' && ch <= 'Z')).ToList().Shuffle(wordRnd).Take(10).JoinString(", "));

            //var feeders = "ANYWHERE,BEDSHEET,COIFFURE,DEXTROSE,EXPLICIT,FILENAME,GRIDLOCK".Split(',');
            var feeders = Ut.NewArray(
                "APRICOTS",
                "BETATEST",
                "CHARMING",
                words.Where(w => w.Length == 8 && w[0] == 'D' && w.All(ch => ch >= 'A' && ch <= 'Z') && w.Contains('K')).PickRandom(wordRnd),
                words.Where(w => w.Length == 8 && w[0] == 'E' && w.All(ch => ch >= 'A' && ch <= 'Z') && w.Contains('U')).PickRandom(wordRnd),
                "FIGHTING",
                "GANYMEDE");

            if (cluephrase.Length != feeders.Length)
                Debugger.Break();

            const int n = 8;
            var equations = new List<string>();
            int conv(string str, int ix) => ix < 0 || ix >= str.Length || str[ix] == ' ' ? 0 : str[ix] - 'A' + 1;
            for (int wordIx = 0; wordIx < feeders.Length; wordIx++)
                for (int clPhrIx = 0; clPhrIx < n; clPhrIx++)
                    equations.Add($"{Enumerable.Range(0, n).Select(fdrIx => $"{conv(cluephrase[wordIx], fdrIx)}*m_{clPhrIx + 1}_{fdrIx + 1}").JoinString(" + ")} = {conv(feeders[wordIx], clPhrIx)}");
            File.WriteAllText(@"D:\temp\temp.txt", $@"msolve({{ {equations.JoinString(", ")} }}, 47);");

            // PASTE D:\temp\temp.txt INTO MAPLE, RUN, AND PUT RESULT BACK INTO THAT FILE
            Debugger.Break();

            var inputRaw = File.ReadAllText(@"D:\temp\temp.txt");
            //inputRaw = @"{m_5_8 = _NN4, m_7_8 = _NN6, m_6_8 = _NN5, m_5_2 = 41+32*_NN4, m_5_1 = 17+32*_NN4, m_2_6 = 6+35*_NN2, m_2_5 = 41+44*_NN2, m_2_3 = 16+7*_NN2, m_2_2 = 8+32*_NN2, m_2_4 = 29+7*_NN2, m_2_1 = 10+32*_NN2, m_1_6 = 21+35*_NN1, m_1_5 = 3+44*_NN1, m_5_4 = 42+7*_NN4, m_4_7 = _NN7, m_2_8 = _NN2, m_8_8 = _NN8, m_6_6 = 26+35*_NN5, m_6_3 = 33+7*_NN5, m_6_4 = 20+7*_NN5, m_6_2 = 19+32*_NN5, m_6_1 = 23+32*_NN5, m_6_5 = 38+44*_NN5, m_8_4 = 7*_NN8, m_8_1 = 32*_NN8, m_7_6 = 16+35*_NN6, m_7_5 = 17+44*_NN6, m_7_3 = 12+7*_NN6, m_7_4 = 43+7*_NN6, m_7_2 = 18+32*_NN6, m_7_1 = 43+32*_NN6, m_8_5 = 44*_NN8, m_8_3 = 7*_NN8, m_8_2 = 32*_NN8, m_3_5 = 31+44*_NN3, m_3_3 = 21+7*_NN3, m_3_4 = 28+7*_NN3, m_3_2 = 41+32*_NN3, m_3_1 = 35+32*_NN3, m_8_6 = 35*_NN8, m_1_3 = 12+7*_NN1, m_1_4 = 44+7*_NN1, m_1_1 = 13+32*_NN1, m_4_6 = 9+2*_NN7, m_4_5 = 20+24*_NN7, m_4_3 = 18+38*_NN7, m_4_4 = 17+38*_NN7, m_4_2 = 19+26*_NN7, m_4_1 = 38+26*_NN7, m_3_6 = 29+35*_NN3, m_1_2 = 31+32*_NN1, m_4_8 = 2+39*_NN7, m_1_7 = 9+41*_NN1, m_2_7 = 24+41*_NN2, m_5_7 = 8+41*_NN4, m_5_6 = 15+35*_NN4, m_5_5 = 18+44*_NN4, m_5_3 = 30+7*_NN4, m_7_7 = 39+41*_NN6, m_8_7 = 41*_NN8, m_3_7 = 33+41*_NN3, m_6_7 = 42+41*_NN5, m_3_8 = _NN3, m_1_8 = _NN1";
            var input = inputRaw.Trim('{', '}', ' ', '\t', '\r', '\n').Split(',').Select(eq => new { Match = Regex.Match(eq, @"^\s*m_(\d+)_(\d+) *= *(?:(?:(\d+) *\+ *)?(?:(\d+) *\*? *)?_NN(\d+)~?|(?:(\d+) *\*? *)?_NN(\d+)~?(?: *\+ *(\d+))?|(\d+))\s*$"), Line = eq }).ToArray();
            var anyInvalid = false;
            for (int i = 0; i < input.Length; i++)
            {
                if (!input[i].Match.Success)
                {
                    Console.WriteLine(input[i].Line);
                    anyInvalid = true;
                }
                if (input[i].Match.Groups[9].Success && input[i].Match.Groups[9].Value == "0")
                    Debugger.Break();
            }
            if (anyInvalid)
                Debugger.Break();
            var inputParsed = input.Select(inp => Enumerable.Range(1, 9).Select(grIx => inp.Match.Groups[grIx].Success ? int.Parse(inp.Match.Groups[grIx].Value) : (int?) null).ToArray()).ToArray();
            var maxNn = inputParsed.Max(inp => inp[4] ?? inp[6]);

            var results = new int[n * n];
            var nns = new int[maxNn.Value];

            var maxNums = 0;
            for (var seed = 0; seed < int.MaxValue; seed++)
            {
                var rnd = new Random(seed);
                for (int i = 0; i < maxNn; i++)
                    nns[i] = rnd.Next(0, 47);
                foreach (var result in inputParsed)
                    results[(result[0].Value - 1) * n + (result[1].Value - 1)] = (result[8] ?? ((result[2] ?? result[7] ?? 0) + nns[(result[4] ?? result[6].Value) - 1] * (result[3] ?? result[5] ?? 1))) % 47;

                if (results.Any(r => r == 0))
                    continue;

                var substringIx = 0;
                var resultsIx = 0;
                var matchIxs = new List<(int resultsIx, int prefIx)>();
                while (resultsIx < 64 && substringIx < cluephrase2.Length)
                {
                    var p = prefs[results[resultsIx] - 1].IndexOf(cluephrase2[substringIx]);
                    if (p != -1)
                    {
                        matchIxs.Add((resultsIx, p));
                        substringIx++;
                    }
                    resultsIx++;
                }
                if (substringIx < cluephrase2.Length)
                    continue;

                var num = results.Distinct().Count();
                if (num > maxNums)
                {
                    Console.Clear();
                    ConsoleUtil.WriteLine($"Feeders: {feeders.JoinString(", ")}".Color(ConsoleColor.White));
                    Console.WriteLine();
                    ConsoleUtil.WriteLine($"Seed: {seed}".Color(ConsoleColor.White));
                    ConsoleUtil.WriteLine($"{num} values: {results.Distinct().Order().JoinString(" ")}".Color(ConsoleColor.DarkGray));
                    ConsoleUtil.WriteLine(results.Select((r, ix) =>
                    {
                        var mIx = matchIxs.IndexOf(tup => tup.resultsIx == ix);
                        var cc = r.ToString().PadLeft(2).Color(mIx >= 0 ? ConsoleColor.Yellow : ConsoleColor.Green, mIx >= 0 ? ConsoleColor.DarkBlue : ConsoleColor.Black);
                        cc += $"[{(mIx == -1 ? 0 : matchIxs[mIx].prefIx + 1)}]";
                        return cc;
                    }).Split(n).Select(row => row.JoinColoredString(" ")).JoinColoredString("\n"));
                    ConsoleUtil.WriteLine(feeders.Select(f => Enumerable.Range(0, 8).Select(r => (char) (Enumerable.Range(0, 8).Select(c => results[c + 8 * r] * (f[c] - 'A' + 1)).Sum() % 47 - 1 + 'A')).JoinString().Replace('@', '_')).JoinString(", "));
                    Console.WriteLine();
                    maxNums = num;
                }
            }
        }

        public static void FortySeven_ConstructMatrix_V2()
        {
            const string cluephrase = "SATOSHINAKAMOTOCURRENCY";
            const int n = 8;

            var prefs = File.ReadAllLines(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\47\Prefectures.txt");
            //var words = File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt");
            var words = File.ReadAllLines(@"D:\Daten\Wordlists\peter_broda_wordlist_unscored.txt").Except(File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt")).ToArray();

            var wordRnd = new Random();
            var hWords = words.Where(w => w.Length == 8 && w[0] == 'H' && w.All(ch => ch >= 'A' && ch <= 'Z')).ToArray().Shuffle(wordRnd).ToArray();

            var mod47Inverses = new int[47];
            var tt = new TextTable { ColumnSpacing = 1 };
            for (var row = 0; row < 47; row++)
                for (var col = 0; col < 47; col++)
                    if ((row * col) % 47 == 1)
                        mod47Inverses[row] = col;

            // Matrix multiplication
            int[] mult(int[] m1, int[] m2, int size) => Ut.NewArray(size * size, i => Enumerable.Range(0, size).Select(x => ((m1[x + size * (i / size)] * m2[(i % size) + size * x]) % 47 + 47) % 47).Sum());
            int[] mult2(int size, params int[][] ms) => ms.Aggregate((prev, next) => mult(prev, next, size));
            int[] muls(int scalar, int[] m) => m.Select(i => i * scalar).ToArray();
            int[] add(int[] m1, int[] m2) => m1.Zip(m2, (a, b) => a + b).ToArray();
            int[] mod47(int[] m) => m.Select(i => (i % 47 + 47) % 47).ToArray();

            // Find the inverse matrix by repeatedly subdividing
            int[] inverse(int[] matrix, int size)
            {
                if (size == 2)
                {
                    var (a, b, c, d) = (matrix[0], matrix[1], matrix[2], matrix[3]);
                    var det = ((a * d - b * c) % 47 + 47) % 47;
                    if (det == 0)
                        throw new InvalidOperationException();
                    return new[] { d, -b, -c, a }.Select(i => ((i * mod47Inverses[det]) % 47 + 47) % 47).ToArray();
                }
                else
                {
                    var ns = size / 2;
                    var a = Ut.NewArray(ns * ns, i => matrix[i % ns + (i / ns) * size]);
                    var b = Ut.NewArray(ns * ns, i => matrix[i % ns + ns + (i / ns) * size]);
                    var c = Ut.NewArray(ns * ns, i => matrix[i % ns + ((i / ns) + ns) * size]);
                    var d = Ut.NewArray(ns * ns, i => matrix[i % ns + ns + ((i / ns) + ns) * size]);

                    var aI = inverse(a, ns);
                    var dI = inverse(d, ns);

                    var mD = mod47(inverse(add(d, muls(-1, mult2(ns, c, aI, b))), ns));
                    var mA = mod47(add(aI, mult2(ns, aI, b, mD, c, aI)));
                    var mB = mod47(muls(-1, mult2(ns, aI, b, mD)));
                    var mC = mod47(muls(-1, mult2(ns, mD, c, aI)));

                    var result = Ut.NewArray(size * size, i => ((((i % size) < ns ? (i / size) < ns ? mA : mC : (i / size) < ns ? mB : mD)[i % ns + ns * ((i / size) % ns)]) % 47 + 47) % 47);
                    return result;
                }
            }

            void outputMatrix(int[] m, int size)
            {
                var txt = new TextTable { ColumnSpacing = 1 };
                for (var x = 0; x < size; x++)
                    for (var y = 0; y < size; y++)
                        txt.SetCell(x, y, m[x + size * y].ToString(), alignment: HorizontalTextAlignment.Right);
                txt.WriteToConsole();
            }

            foreach (var hWord in hWords)
            {
                var feeders = Ut.NewArray(
                    "APRICOTS",
                    "BETATEST",
                    "CHARMING",
                    "DIALOGUE",
                    "EMIGRATE",
                    "FIGHTING",
                    "GANYMEDE",
                    hWord);

                ConsoleUtil.WriteLine($"Feeders: {feeders.JoinString(", ")}".Color(ConsoleColor.Yellow));
                var feederMatrix = Ut.NewArray(64, i => feeders[i / 8][i % 8] - 'A' + 1);
                int[] inv;
                try
                {
                    inv = inverse(feederMatrix, 8);
                }
                catch (InvalidOperationException)
                {
                    continue;
                }

                var chsPerRow = (cluephrase.Length + 7) / 8;
                var ccOutput = new List<ConsoleColoredString>();

                for (var rowUnderTest = 0; rowUnderTest < n; rowUnderTest++)
                {
                    ccOutput.Add($"Row {rowUnderTest + 1}:".Color(ConsoleColor.White));

                    var ssLen = rowUnderTest == n - 1 ? cluephrase.Length - (n - 1) * chsPerRow : chsPerRow;
                    var cluephraseStart = rowUnderTest * chsPerRow;
                    foreach (var subseq in Enumerable.Range(0, n).Subsequences(minLength: ssLen, maxLength: ssLen).Select(sseq => sseq.ToArray()).ToArray().Shuffle())
                    {
                        const int maxIndex = 9;
                        var pow = 1;
                        var prefPosses = new List<(int i, char ch)>();
                        for (var i = 0; i < subseq.Length; i++)
                        {
                            prefPosses.Add((subseq[i], cluephrase[chsPerRow * rowUnderTest + i]));
                            pow *= maxIndex;
                        }

                        var orders = Enumerable.Range(0, pow).ToArray().Shuffle();
                        foreach (var orderRaw in orders)
                        {
                            var order = orderRaw;
                            var input = new (int value, char ch)[n];
                            foreach (var (i, ch) in prefPosses)
                            {
                                input[i] = (value: order % maxIndex + 1, ch);
                                order /= maxIndex;
                            }

                            var output = Ut.NewArray(n, x => Enumerable.Range(0, n).Select(j => inv[j + 8 * x] * input[j].value).Sum() % 47);
                            if (Enumerable.Range(0, n).All(x => input[x].value == 0 || (input[x].value <= prefs[(output[x] + 46) % 47].Length && prefs[(output[x] + 46) % 47][input[x].value - 1] == input[x].ch)))
                            {
                                ccOutput.Add(new ConsoleColoredString($"Input:  {input.Select(tup => tup.value.ToString().PadLeft(2)).JoinString(" ").Color(ConsoleColor.Green)}"));
                                ccOutput.Add(new ConsoleColoredString($"Output: {output.Select(i => i.ToString().PadLeft(2)).JoinString(" ").Color(ConsoleColor.Cyan)}"));
                                ccOutput.Add(new ConsoleColoredString($"Expect: {input.Select(tup => tup.ch == default ? "/" : tup.ch.ToString()).JoinString(" ").Color(ConsoleColor.Magenta)}"));
                                ccOutput.Add("");
                                goto next;
                            }
                        }
                    }

                    goto busted;

                    next:;
                }

                outputMatrix(feederMatrix, 8);
                Console.WriteLine();
                outputMatrix(inverse(feederMatrix, 8), 8);
                Console.WriteLine();

                foreach (var cc in ccOutput)
                    ConsoleUtil.WriteLine(cc);

                // We have a row with no match :(
                busted:;
            }
        }

        public static void FortySeven_Test()
        {
            // Vector multiplication
            int[] vecmult(int[] m, int[] v, int size) => Ut.NewArray(size, i => (Enumerable.Range(0, size).Select(x => ((m[x + size * i] * v[x]) % 47 + 47) % 47).Sum() % 47 + 47) % 47);
            var prefs = File.ReadAllLines(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\47\Prefectures.txt");

            // Feeder answers matrix
            var matrix = new[] { 1, 16, 18, 9, 3, 15, 20, 19, 2, 5, 20, 1, 20, 5, 19, 20, 3, 8, 1, 18, 13, 9, 14, 7, 4, 9, 14, 15, 19, 1, 21, 18, 5, 20, 3, 5, 20, 5, 18, 1, 6, 9, 7, 8, 20, 9, 14, 7, 7, 1, 14, 25, 13, 5, 4, 5, 8, 15, 14, 5, 25, 4, 5, 23 };
            var vectors = Ut.NewArray(
                new[] { 23, 1, 32, 19, 3, 36, 12, 21 },
                new[] { 35, 0, 9, 15, 32, 40, 22, 39 },
                new[] { 46, 34, 0, 10, 16, 5, 30, 35 },
                new[] { 28, 38, 31, 27, 42, 33, 38, 10 },
                new[] { 31, 37, 11, 15, 13, 7, 36, 28 },
                new[] { 39, 12, 42, 25, 35, 7, 19, 29 },
                new[] { 46, 19, 42, 29, 24, 38, 42, 43 },
                new[] { 46, 34, 42, 43, 35, 33, 13, 7 });

            for (var vIx = 0; vIx < vectors.Length; vIx++)
            {
                var vector = vectors[vIx];
                var v = vecmult(matrix, vector, 8);
                Console.WriteLine($"{v.JoinString(", ")} = {v.Select((value, ix) => value == 0 ? null : prefs[(vector[ix] + 46) % 47][value - 1].Nullable()).JoinString()}");
            }
        }

        public static void OneCanHope()
        {
            // (potential puzzle idea with Esperanto words, some of which are similar to their English translation, some of which are false friends)

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
                .OrderBy(inf => inf.leven))
                Console.WriteLine($"{esp} = {eng} = {leven}");
        }

        public static void NoU_FindPairs()
        {
            var lines = @"WARSCHAUERSTRAßE,SCHLESISCHESTOR,GÖRLITZERBAHNHOF,KOTTBUSSERTOR,PRINZENSTRAßE,HALLESCHESTOR,MÖCKERNBRÜCKE,GLEISDREIECK,KURFÜRSTENSTRAßE,NOLLENDORFPLATZ,WITTENBERGPLATZ,KURFÜRSTENDAMM,UHLANDSTRAßE
PANKOW,VINETASTRAßE,SCHÖNHAUSERALLEE,EBERSWALDERSTRAßE,SENEFELDERPLATZ,ROSALUXEMBURGPLATZ,ALEXANDERPLATZ,KLOSTERSTRAßE,MÄRKISCHESMUSEUM,SPITTELMARKT,HAUSVOGTEIPLATZ,STADTMITTE,MOHRENSTRAßE,POTSDAMERPLATZ,MENDELSSOHNBARTHOLDYPARK,GLEISDREIECK,BÜLOWSTRAßE,NOLLENDORFPLATZ,WITTENBERGPLATZ,ZOOLOGISCHERGARTEN,ERNSTREUTERPLATZ,DEUTSCHEOPER,BISMARCKSTRAßE,SOPHIECHARLOTTEPLATZ,KAISERDAMM,THEODORHEUSSPLATZ,NEUWESTEND,OLYMPIASTADION,RUHLEBEN
WARSCHAUERSTRAßE,SCHLESISCHESTOR,GÖRLITZERBAHNHOF,KOTTBUSSERTOR,PRINZENSTRAßE,HALLESCHESTOR,MÖCKERNBRÜCKE,GLEISDREIECK,KURFÜRSTENSTRAßE,NOLLENDORFPLATZ,WITTENBERGPLATZ,AUGSBURGERSTRAßE,SPICHERNSTRAßE,HOHENZOLLERNPLATZ,FEHRBELLINERPLATZ,HEIDELBERGERPLATZ,RÜDESHEIMERPLATZ,BREITENBACHPLATZ,PODBIELSKIALLEE,DAHLEMDORF,FREIEUNIVERSITÄT,OSKARHELENEHEIM,ONKELTOMSHÜTTE,KRUMMELANKE
NOLLENDORFPLATZ,VIKTORIALUISEPLATZ,BAYERISCHERPLATZ,RATHAUSSCHÖNEBERG,INNSBRUCKERPLATZ
BERLINHAUPTBAHNHOF,BUNDESTAG,BRANDENBURGERTOR,UNTERDENLINDEN,FRANZÖSISCHESTRAßE,MUSEUMSINSEL,BERLINERRATHAUS,ALEXANDERPLATZ,SCHILLINGSTRAßE,STRAUSBERGERPLATZ,WEBERWIESE,FRANKFURTERTOR,SAMARITERSTRAßE,FRANKFURTERALLEE,MAGDALENENSTRAßE,LICHTENBERG,FRIEDRICHSFELDE,TIERPARK,BIESDORFSÜD,ELSTERWERDAERPLATZ,WUHLETAL,KAULSDORFNORD,KIENBERG,COTTBUSSERPLATZ,HELLERSDORF,LOUISLEWINSTRAßE,HÖNOW
ALTTEGEL,BORSIGWERKE,HOLZHAUSERSTRAßE,OTISSTRAßE,SCHARNWEBERSTRAßE,KURTSCHUMACHERPLATZ,AFRIKANISCHESTRAßE,REHBERGE,SEESTRAßE,LEOPOLDPLATZ,WEDDING,REINICKENDORFERSTRAßE,SCHWARTZKOPFFSTRAßE,NATURKUNDEMUSEUM,ORANIENBURGERTOR,FRIEDRICHSTRAßE,UNTERDENLINDEN,FRANZÖSISCHESTRAßE,STADTMITTE,KOCHSTRAßE,HALLESCHESTOR,LANDWEHRCANAL,MEHRINGDAMM,PLATZDERLUFTBRÜCKE,PARADESTRAßE,TEMPELHOF,ALTTEMPELHOF,KAISERINAUGUSTASTRAßE,ULLSTEINSTRAßE,WESTPHALWEG,ALTMARIENDORF
RATHAUSSPANDAU,ALTSTADTSPANDAU,ZITADELLE,HASELHORST,PAULSTERNSTRAßE,ROHRDAMM,SIEMENSDAMM,HALEMWEG,JAKOBKAISERPLATZ,JUNGFERNHEIDE,MIERENDORFFPLATZ,RICHARDWAGNERPLATZ,BISMARCKSTRAßE,WILMERSDORFERSTRAßE,ADENAUERPLATZ,KONSTANZERSTRAßE,FEHRBELLINERPLATZ,BLISSESTRAßE,BERLINERSTRAßE,BAYERISCHERPLATZ,EISENACHERSTRAßE,KLEISTPARK,YORCKSTRAßE,MÖCKERNBRÜCKE,MEHRINGDAMM,GNEISENAUSTRAßE,SÜDSTERN,HERMANNPLATZ,RATHAUSNEUKÖLLN,KARLMARXSTRAßE,NEUKÖLLN,GRENZALLEE,BLASCHKOALLEE,PARCHIMERALLEE,BRITZSÜD,JOHANNISTHALERCHAUSSEE,LIPSCHITZALLEE,WUTZKYALLEE,ZWICKAUERDAMM,RUDOW
WITTENAU,RATHAUSREINICKENDORF,KARLBONHOEFFERNERVENKLINIK,LINDAUERALLEE,PARACELSUSBAD,RESIDENZSTRAßE,FRANZNEUMANNPLATZ,OSLOERSTRAßE,PANKSTRAßE,GESUNDBRUNNEN,VOLTASTRAßE,BERNAUERSTRAßE,ROSENTHALERPLATZ,WEINMEISTERSTRAßE,ALEXANDERPLATZ,JANNOWITZBRÜCKE,HEINRICHHEINESTRAßE,MORITZPLATZ,KOTTBUSSERTOR,SCHÖNLEINSTRAßE,HERMANNPLATZ,BODDINSTRAßE,LEINESTRAßE,HERMANNSTRAßE
OSLOERSTRAßE,NAUENERPLATZ,LEOPOLDPLATZ,AMRUMERSTRAßE,WESTHAFEN,BIRKENSTRAßE,TURMSTRAßE,HANSAPLATZ,ZOOLOGISCHERGARTEN,KURFÜRSTENDAMM,SPICHERNSTRAßE,GÜNTZELSTRAßE,BERLINERSTRAßE,BUNDESPLATZ,FRIEDRICHWILHELMPLATZ,WALTHERSCHREIBERPLATZ,SCHLOßSTRAßE,RATHAUSSTEGLITZ"
                .Replace("\r", "").Split('\n')
                .Zip(@"WARSCHAUERSTRASSE,SCHLESISCHESTOR,GOERLITZERBAHNHOF,KOTTBUSSERTOR,PRINZENSTRASSE,HALLESCHESTOR,MOECKERNBRUECKE,GLEISDREIECK,KURFUERSTENSTRASSE,NOLLENDORFPLATZ,WITTENBERGPLATZ,KURFUERSTENDAMM,UHLANDSTRASSE
PANKOW,VINETASTRASSE,SCHOENHAUSERALLEE,EBERSWALDERSTRASSE,SENEFELDERPLATZ,ROSALUXEMBURGPLATZ,ALEXANDERPLATZ,KLOSTERSTRASSE,MAERKISCHESMUSEUM,SPITTELMARKT,HAUSVOGTEIPLATZ,STADTMITTE,MOHRENSTRASSE,POTSDAMERPLATZ,MENDELSSOHNBARTHOLDYPARK,GLEISDREIECK,BUELOWSTRASSE,NOLLENDORFPLATZ,WITTENBERGPLATZ,ZOOLOGISCHERGARTEN,ERNSTREUTERPLATZ,DEUTSCHEOPER,BISMARCKSTRASSE,SOPHIECHARLOTTEPLATZ,KAISERDAMM,THEODORHEUSSPLATZ,NEUWESTEND,OLYMPIASTADION,RUHLEBEN
WARSCHAUERSTRASSE,SCHLESISCHESTOR,GOERLITZERBAHNHOF,KOTTBUSSERTOR,PRINZENSTRASSE,HALLESCHESTOR,MOECKERNBRUECKE,GLEISDREIECK,KURFUERSTENSTRASSE,NOLLENDORFPLATZ,WITTENBERGPLATZ,AUGSBURGERSTRASSE,SPICHERNSTRASSE,HOHENZOLLERNPLATZ,FEHRBELLINERPLATZ,HEIDELBERGERPLATZ,RUEDESHEIMERPLATZ,BREITENBACHPLATZ,PODBIELSKIALLEE,DAHLEMDORF,FREIEUNIVERSITAET,OSKARHELENEHEIM,ONKELTOMSHUETTE,KRUMMELANKE
NOLLENDORFPLATZ,VIKTORIALUISEPLATZ,BAYERISCHERPLATZ,RATHAUSSCHOENEBERG,INNSBRUCKERPLATZ
BERLINHAUPTBAHNHOF,BUNDESTAG,BRANDENBURGERTOR,UNTERDENLINDEN,FRANZOESISCHESTRASSE,MUSEUMSINSEL,BERLINERRATHAUS,ALEXANDERPLATZ,SCHILLINGSTRASSE,STRAUSBERGERPLATZ,WEBERWIESE,FRANKFURTERTOR,SAMARITERSTRASSE,FRANKFURTERALLEE,MAGDALENENSTRASSE,LICHTENBERG,FRIEDRICHSFELDE,TIERPARK,BIESDORFSUED,ELSTERWERDAERPLATZ,WUHLETAL,KAULSDORFNORD,KIENBERG,COTTBUSSERPLATZ,HELLERSDORF,LOUISLEWINSTRASSE,HOENOW
ALTTEGEL,BORSIGWERKE,HOLZHAUSERSTRASSE,OTISSTRASSE,SCHARNWEBERSTRASSE,KURTSCHUMACHERPLATZ,AFRIKANISCHESTRASSE,REHBERGE,SEESTRASSE,LEOPOLDPLATZ,WEDDING,REINICKENDORFERSTRASSE,SCHWARTZKOPFFSTRASSE,NATURKUNDEMUSEUM,ORANIENBURGERTOR,FRIEDRICHSTRASSE,UNTERDENLINDEN,FRANZOESISCHESTRASSE,STADTMITTE,KOCHSTRASSE,HALLESCHESTOR,LANDWEHRCANAL,MEHRINGDAMM,PLATZDERLUFTBRUECKE,PARADESTRASSE,TEMPELHOF,ALTTEMPELHOF,KAISERINAUGUSTASTRASSE,ULLSTEINSTRASSE,WESTPHALWEG,ALTMARIENDORF
RATHAUSSPANDAU,ALTSTADTSPANDAU,ZITADELLE,HASELHORST,PAULSTERNSTRASSE,ROHRDAMM,SIEMENSDAMM,HALEMWEG,JAKOBKAISERPLATZ,JUNGFERNHEIDE,MIERENDORFFPLATZ,RICHARDWAGNERPLATZ,BISMARCKSTRASSE,WILMERSDORFERSTRASSE,ADENAUERPLATZ,KONSTANZERSTRASSE,FEHRBELLINERPLATZ,BLISSESTRASSE,BERLINERSTRASSE,BAYERISCHERPLATZ,EISENACHERSTRASSE,KLEISTPARK,YORCKSTRASSE,MOECKERNBRUECKE,MEHRINGDAMM,GNEISENAUSTRASSE,SUEDSTERN,HERMANNPLATZ,RATHAUSNEUKOELLN,KARLMARXSTRASSE,NEUKOELLN,GRENZALLEE,BLASCHKOALLEE,PARCHIMERALLEE,BRITZSUED,JOHANNISTHALERCHAUSSEE,LIPSCHITZALLEE,WUTZKYALLEE,ZWICKAUERDAMM,RUDOW
WITTENAU,RATHAUSREINICKENDORF,KARLBONHOEFFERNERVENKLINIK,LINDAUERALLEE,PARACELSUSBAD,RESIDENZSTRASSE,FRANZNEUMANNPLATZ,OSLOERSTRASSE,PANKSTRASSE,GESUNDBRUNNEN,VOLTASTRASSE,BERNAUERSTRASSE,ROSENTHALERPLATZ,WEINMEISTERSTRASSE,ALEXANDERPLATZ,JANNOWITZBRUECKE,HEINRICHHEINESTRASSE,MORITZPLATZ,KOTTBUSSERTOR,SCHOENLEINSTRASSE,HERMANNPLATZ,BODDINSTRASSE,LEINESTRASSE,HERMANNSTRASSE
OSLOERSTRASSE,NAUENERPLATZ,LEOPOLDPLATZ,AMRUMERSTRASSE,WESTHAFEN,BIRKENSTRASSE,TURMSTRASSE,HANSAPLATZ,ZOOLOGISCHERGARTEN,KURFUERSTENDAMM,SPICHERNSTRASSE,GUENTZELSTRASSE,BERLINERSTRASSE,BUNDESPLATZ,FRIEDRICHWILHELMPLATZ,WALTHERSCHREIBERPLATZ,SCHLOSSSTRASSE,RATHAUSSTEGLITZ"
                    .Replace("\r", "").Split('\n'),
                    (line1, line2) => line1.Split(',').Zip(line2.Split(','), (stop1, stop2) => (de: stop1, en: stop2)).ToArray()).ToArray();

            var solution = @"TOPOLOGY";

            // Collect all the pairs of stations that surround a station
            var allPairs = new List<(string stop1, string stop2, string actualStop)>();
            for (int line = 0; line < lines.Length; line++)
                for (int stopIx = 1; stopIx < lines[line].Length - 1; stopIx++)
                    allPairs.Add((lines[line][stopIx - 1].en, lines[line][stopIx + 1].en, lines[line][stopIx].de));
            allPairs.Sort(CustomComparer<(string stop1, string stop2, string actualStop)>.By(p => p.stop1.Length + p.stop2.Length));

            // Find out which stations can be used for each solution letter
            var usablePairs = new List<int>[solution.Length];
            for (int solIx = 0; solIx < solution.Length; solIx++)
            {
                usablePairs[solIx] = new List<int>();
                for (int pairIx = 0; pairIx < allPairs.Count; pairIx++)
                {
                    const int maxStopLength = 15;
                    var (stop1, stop2, actualStop) = allPairs[pairIx];
                    var p = actualStop.IndexOf(solution[solIx]);
                    var inv = actualStop.IndexOf(ch => ch < 'A' || ch > 'Z');
                    if (p >= 0 && p < lines.Length && (inv < 0 || p < inv))
                        if (stop1.Length < maxStopLength && stop2.Length < maxStopLength)
                            usablePairs[solIx].Add(pairIx);
                }
            }

            // Find a set of matching pairs
            IEnumerable<(string stop1, string stop2, string actualStop, int stopIx, int solIx)[]> recurse((string stop1, string stop2, string actualStop, int stopIx, int solIx)[] sofar, int[] lengthsUnaccountedFor, bool[] solutionLettersUsed, int[] pairsUsed)
            {
                if (lengthsUnaccountedFor.Length == 0 && solutionLettersUsed.All(b => b))
                {
                    yield return sofar.ToArray();
                    yield break;
                }

                if (lengthsUnaccountedFor.Length > 0)
                {
                    var len = lengthsUnaccountedFor[0];
                    for (int i = 0; i < solution.Length; i++)
                        if (!solutionLettersUsed[i])
                            for (var j = 0; j < usablePairs[i].Count; j++)
                            {
                                if (pairsUsed.Contains(usablePairs[i][j]))
                                    continue;
                                var (stop1, stop2, actualStop) = allPairs[usablePairs[i][j]];
                                var len1 = stop1.Length;
                                var len2 = stop2.Length;
                                if ((len1 != len && len2 != len) || len1 == len2)
                                    continue;
                                if (sofar.Any(sf => sf.stop1 == stop1 || sf.stop2 == stop1 || sf.stop1 == stop2 || sf.stop2 == stop2))
                                    continue;

                                for (int asIx = 0; asIx < actualStop.Length && asIx < lines.Length; asIx++)
                                    if (actualStop[asIx] == solution[i] && !sofar.Any(sf => sf.stopIx == asIx))
                                    {
                                        var otherLen = len1 == len ? len2 : len1;
                                        var u = lengthsUnaccountedFor.IndexOf(otherLen);
                                        var newSoFar = sofar.Insert(sofar.Length, (stop1, stop2, actualStop, asIx, i));
                                        var newLengthsUnaccountedFor = u == -1
                                            ? lengthsUnaccountedFor.Insert(lengthsUnaccountedFor.Length, otherLen).Remove(0, 1)
                                            : lengthsUnaccountedFor.Remove(u, 1).Remove(0, 1);
                                        solutionLettersUsed[i] = true;
                                        foreach (var result in recurse(newSoFar, newLengthsUnaccountedFor, solutionLettersUsed, pairsUsed.Insert(0, usablePairs[i][j])))
                                            yield return result;
                                        solutionLettersUsed[i] = false;
                                    }
                            }
                }
                else
                {
                    var i = solutionLettersUsed.IndexOf(b => !b);
                    for (var j = 0; j < usablePairs[i].Count; j++)
                    {
                        if (pairsUsed.Contains(usablePairs[i][j]))
                            continue;
                        var (stop1, stop2, actualStop) = allPairs[usablePairs[i][j]];
                        if (sofar.Any(sf => sf.stop1 == stop1 || sf.stop2 == stop1 || sf.stop1 == stop2 || sf.stop2 == stop2))
                            continue;

                        for (int asIx = 0; asIx < actualStop.Length && asIx < lines.Length; asIx++)
                            if (actualStop[asIx] == solution[i] && !sofar.Any(sf => sf.stopIx == asIx))
                            {
                                var newLengthsUnaccountedFor = stop1.Length == stop2.Length
                                ? lengthsUnaccountedFor
                                : new[] { stop1.Length, stop2.Length };
                                solutionLettersUsed[i] = true;
                                foreach (var result in recurse(sofar.Insert(sofar.Length, (stop1, stop2, actualStop, asIx, i)), newLengthsUnaccountedFor, solutionLettersUsed, pairsUsed.Insert(0, usablePairs[i][j])))
                                    yield return result;
                                solutionLettersUsed[i] = false;
                            }
                    }
                }
            }

            var minTotalLength = int.MaxValue;
            foreach (var result in recurse(new (string stop1, string stop2, string actualStop, int stopIx, int solIx)[0], new int[0], new bool[solution.Length], new int[0]))
            {
                var totalLength = result.Sum(r => r.stop1.Length + r.stop2.Length);
                if (totalLength < minTotalLength && result.All(tup => tup.stopIx != 2))
                {
                    minTotalLength = totalLength;
                    ConsoleUtil.WriteLine($"{totalLength / 2}".Color(ConsoleColor.White));

                    var tt = new TextTable { ColumnSpacing = 2 };
                    var order = Enumerable.Range(0, result.Length).OrderBy(i => result[i].solIx).ToArray();
                    for (int i = 0; i < result.Length; i++)
                    {
                        var (stop1, stop2, actualStop, stopIx, solIx) = result[order[i]];
                        tt.SetCell(0, i, stop1.Length.ToString(), alignment: HorizontalTextAlignment.Right);
                        tt.SetCell(1, i, stop1.Color((ConsoleColor) (stop1.Length % 15 + 1)), alignment: HorizontalTextAlignment.Left);
                        tt.SetCell(2, i, stop2.Length.ToString(), alignment: HorizontalTextAlignment.Right);
                        tt.SetCell(3, i, stop2.Color((ConsoleColor) (stop2.Length % 15 + 1)), alignment: HorizontalTextAlignment.Left);
                        tt.SetCell(4, i, (stopIx + 1).ToString().Color(stopIx == 8 ? ConsoleColor.White : ConsoleColor.DarkGray), alignment: HorizontalTextAlignment.Right);
                        tt.SetCell(5, i, actualStop.Insert(stopIx + 1, "]").Insert(stopIx, "[").ColorSubstring(stopIx, 3, ConsoleColor.White, ConsoleColor.DarkBlue), alignment: HorizontalTextAlignment.Left);
                    }
                    tt.WriteToConsole();
                    Console.WriteLine();
                }
            }
        }

        public static void NoU_GenerateWords()
        {
            // Generates sets of words where the first letter spells out one station, and another station is shuffled with an A-Z indexer column

            var word1 = "SUEDSTERN";
            var word2 = "ZITADELLE";
            var inputLen = word1.Length;
            if (word2.Length != inputLen)
                Debugger.Break();

            BigInteger b(int n) => ~(BigInteger.MinusOne << n);

            //*
            var allWords = File.ReadAllLines(@"D:\Daten\Wordlists\peter_broda_wordlist.txt")
                .Select(row => row.Split(';'))
                .Select(row => (word: row[0].Where(ch => ch >= 'A' && ch <= 'Z').JoinString(), score: int.Parse(row[1])))
                .Where(w => w.score >= 75)
                .Concat(File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt").Select((w, wIx) => (word: w.Where(ch => ch >= 'A' && ch <= 'Z').JoinString(), score: 60024 - wIx)))
                .ToArray();
            /*/
            var allWords = File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt").Select(w => (word: w.Where(ch => ch >= 'A' && ch <= 'Z').JoinString(), score: 100)).ToArray();
            /**/

            const int maxCol = 5;
            var pairs = Enumerable.Range(1, maxCol)
                .SelectMany(az => Enumerable.Range(1, maxCol).Where(w2 => w2 != az).Select(w2 => (azIx: az, w2Ix: w2)))
                .ToArray().Shuffle();

            foreach (var (azIx, w2Ix) in new[] { (3, 4) })
            {
                IEnumerable<BigInteger> recurse(BigInteger available, BigInteger used, int ix)
                {
                    if (ix == inputLen)
                    {
                        yield return used;
                        yield break;
                    }

                    var row = (available >> (inputLen * ix)) & ~(BigInteger.MinusOne << inputLen);
                    if (row.IsZero)
                        yield break;
                    var availableWithoutRow = available & ~((~(BigInteger.MinusOne << inputLen)) << (inputLen * ix));
                    if (ix < inputLen - 1 && availableWithoutRow.IsZero)
                        yield break;
                    var colMask = ~((~(BigInteger.MinusOne << (inputLen * inputLen))) / (~(BigInteger.MinusOne << inputLen)));
                    for (int i = 0; i < inputLen; i++)
                    {
                        if (!row.IsEven)
                            foreach (var solution in recurse(availableWithoutRow & (colMask << i), used | (BigInteger.One << (inputLen * ix + i)), ix + 1))
                                yield return solution;
                        row >>= 1;
                    }
                }

                var translations = Enumerable.Range(0, inputLen).ToArray().Shuffle();
                var translationsRev = new int[inputLen];
                for (int i = 0; i < inputLen; i++)
                    translationsRev[translations[i]] = i;

                var combinationsPossible = BigInteger.Zero;
                for (int w = 0; w < allWords.Length; w++)
                {
                    var (word, score) = allWords[w];
                    if (word.Length <= azIx || word.Length <= w2Ix || word[azIx] - 'A' >= inputLen || word2[word[azIx] - 'A'] != word[w2Ix])
                        continue;
                    for (int w1x = 0; w1x < word1.Length; w1x++)
                        if (word1[w1x] == word[0])
                            combinationsPossible |= BigInteger.One << (w1x + inputLen * translations[word[azIx] - 'A']);
                }

                var bestScore = 0;
                if (Enumerable.Range(0, inputLen).All(row => !(combinationsPossible & (b(inputLen) << (inputLen * row))).IsZero) &&
                    Enumerable.Range(0, inputLen).All(col => !(combinationsPossible & ((b(inputLen * inputLen) / b(inputLen)) << col)).IsZero))
                {
                    foreach (var solution in recurse(combinationsPossible, BigInteger.Zero, 0))
                    {
                        (string word, int score) getWord(int w1x, int w2x) => allWords.Where(aw => aw.word.Length > azIx && aw.word.Length > w2Ix && aw.word[0] == word1[w1x] && aw.word[azIx] - 'A' == w2x && aw.word[w2Ix] == word2[w2x]).MaxElement(w => w.score);

                        var words = Enumerable.Range(0, inputLen * inputLen)
                            .Where(i => !(solution >> i).IsEven)
                            .OrderBy(i => i % inputLen)
                            .Select(i => getWord(i % inputLen, translationsRev[i / inputLen]).Apply(tup => (ix: i, tup.word, tup.score)))
                            .ToArray();
                        var score = words.Sum(w => w.score);
                        if (score > bestScore)
                        {
                            bestScore = score;
                            ConsoleUtil.WriteLine($"azIx={azIx}, w2Ix={w2Ix}, score={score.ToString().Color(ConsoleColor.Yellow)}", null);
                            ConsoleUtil.WriteLine(words.Select(w => new ConsoleColoredString($@"{w.score.ToString().PadLeft(5)} {w.word
                                    .ColorSubstring(0, 1, ConsoleColor.White, ConsoleColor.DarkBlue)
                                    .ColorSubstring(azIx, 1, ConsoleColor.White, ConsoleColor.DarkGreen)
                                    .ColorSubstring(w2Ix, 1, ConsoleColor.White, ConsoleColor.DarkRed)}"))
                                .JoinColoredString("\n"));
                            Console.WriteLine();
                            //goto next;
                        }
                    }
                }
            }
        }

        public static void NoU_GenerateWords_3D()
        {
            // Generates sets of words where the first letter spells out one station, and two other stations are shuffled, each with an A-Z indexer column
            // Turns out this doesn’t find any matches for anything

            //*
            var word1 = "WUHLETAL";
            var word2 = "KIENBERG";
            var word3 = "RUHLEBEN";
            /*/
            // simple test case that suggests that the algorithm does work
            var word1 = "SGD";
            var word2 = "MUE";
            var word3 = "RSY";
            /**/
            var inputLen = word1.Length;

            //var targetLen = 7;
            for (var targetLen = 5; targetLen < 50; targetLen++)
            {
                ConsoleUtil.WriteLine($"TARGET LENGTH: {targetLen}".Color(ConsoleColor.White));

                if (word2.Length != inputLen || word2.Length != inputLen)
                    Debugger.Break();
                var allWords = File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt")
                    .Concat(File.ReadAllLines(@"D:\Daten\Wordlists\peter_broda_wordlist.txt").Select(row => row.Split(';')).Where(row => int.Parse(row[1]) >= 50).Select(row => row[0]))
                    .Select(w => w.Where(ch => ch >= 'A' && ch <= 'Z').JoinString())
                    .Where(w => w.Length == targetLen)
                    .ToArray();

                var quadruplets = (
                    from az2Ix in Enumerable.Range(1, targetLen - 1)
                    from az3Ix in Enumerable.Range(1, targetLen - 1)
                    where az3Ix != az2Ix
                    from w2Ix in Enumerable.Range(1, targetLen - 1)
                    where w2Ix != az2Ix && w2Ix != az3Ix
                    from w3Ix in Enumerable.Range(1, targetLen - 1)
                    where w3Ix != az2Ix && w3Ix != az3Ix && w3Ix != w2Ix
                    select (az2Ix, az3Ix, w2Ix, w3Ix)
                )
                    .ToList().Shuffle();

                BigInteger b(int n) => ~(BigInteger.MinusOne << n);

                foreach (var (az2Ix, az3Ix, w2Ix, w3Ix) in quadruplets)
                {
                    string getWord(int w1x, int w2x, int w3x) => allWords.FirstOrDefault(aw => aw[0] == word1[w1x] && aw[az2Ix] - 'A' == w2x && aw[w2Ix] == word2[w2x] && aw[az3Ix] - 'A' == w3x && aw[w3Ix] == word3[w3x]);

                    IEnumerable<BigInteger> recurse(BigInteger available, BigInteger used, int ix)
                    {
                        if (ix == inputLen)
                        {
                            yield return used;
                            yield break;
                        }

                        var wall = (available >> (inputLen * inputLen * ix)) & b(inputLen * inputLen);
                        if (wall.IsZero)
                            yield break;
                        var availableWithoutWall = available & ~(b(inputLen) << (inputLen * inputLen * ix));
                        if (ix < inputLen * inputLen - 1 && availableWithoutWall.IsZero)
                            yield break;
                        var alleyMask = b(inputLen * inputLen * inputLen) / b(inputLen);
                        var shelfMask = b(inputLen * inputLen * inputLen) / b(inputLen * inputLen) * b(inputLen);
                        for (int i = 0; i < inputLen * inputLen; i++)
                        {
                            if (ix == 0)
                                Console.Write($"azs={az2Ix}/{az3Ix}, wixs={w2Ix}/{w3Ix}, i={i}            \r");
                            if (!wall.IsEven)
                                foreach (var solution in recurse(availableWithoutWall & ~(alleyMask << (i % inputLen)) & ~(shelfMask << (inputLen * (i / inputLen))), used | (BigInteger.One << (inputLen * inputLen * ix + i)), ix + 1))
                                    yield return solution;
                            wall >>= 1;
                        }
                    }

                    var combinationsPossible = BigInteger.Zero;
                    for (int w = 0; w < allWords.Length; w++)
                    {
                        var aw = allWords[w];
                        if (aw[az2Ix] - 'A' >= inputLen || word2[aw[az2Ix] - 'A'] != aw[w2Ix] || aw[az3Ix] - 'A' >= inputLen || word3[aw[az3Ix] - 'A'] != aw[w3Ix])
                            continue;
                        for (int w1x = 0; w1x < word1.Length; w1x++)
                            if (word1[w1x] == aw[0])
                                combinationsPossible |= BigInteger.One << (w1x + inputLen * (aw[az2Ix] - 'A' + inputLen * (aw[az3Ix] - 'A')));
                    }

                    if (
                        Enumerable.Range(0, inputLen).All(wall => !(combinationsPossible & (b(inputLen * inputLen) << (wall * inputLen * inputLen))).IsZero) &&
                        Enumerable.Range(0, inputLen).All(shelf => !(combinationsPossible & ((b(inputLen * inputLen * inputLen) / b(inputLen * inputLen) * b(inputLen)) << (shelf * inputLen))).IsZero) &&
                        Enumerable.Range(0, inputLen).All(alley => !(combinationsPossible & ((b(inputLen * inputLen * inputLen) / b(inputLen)) << alley)).IsZero))
                    {
                        foreach (var solution in recurse(combinationsPossible, BigInteger.Zero, 0))
                        {
                            Console.WriteLine($"azs={az2Ix}/{az3Ix}, wixs={w2Ix}/{w3Ix}            ");
                            ConsoleUtil.WriteLine(Enumerable.Range(0, inputLen * inputLen * inputLen)
                                .Where(i => !(solution >> i).IsEven)
                                .OrderBy(i => i % inputLen)
                                .Select(ix => getWord(ix % inputLen, (ix / inputLen) % inputLen, ix / inputLen / inputLen)
                                    .ColorSubstring(0, 1, ConsoleColor.White, ConsoleColor.DarkBlue)
                                    .ColorSubstring(az2Ix, 1, ConsoleColor.White, ConsoleColor.DarkGreen)
                                    .ColorSubstring(w2Ix, 1, ConsoleColor.White, ConsoleColor.DarkCyan)
                                    .ColorSubstring(az3Ix, 1, ConsoleColor.White, ConsoleColor.DarkRed)
                                    .ColorSubstring(w3Ix, 1, ConsoleColor.White, ConsoleColor.DarkMagenta)
                                )
                                .JoinColoredString("\n"));
                            Console.WriteLine();
                            break;
                        }
                    }
                }
                Console.WriteLine($"                             ");
            }
        }

        // 0 = up; going clockwise
        private static readonly (int left, int right)[] _semaphoreOrientations = new[] { (5, 4), (6, 4), (7, 4), (0, 4), (4, 1), (4, 2), (4, 3), (6, 5), (5, 7), (0, 2), (5, 0), (5, 1), (5, 2), (5, 3), (6, 7), (6, 0), (6, 1), (6, 2), (6, 3), (7, 0), (7, 1), (0, 3), (1, 2), (1, 3), (7, 2), (3, 2) };

        public static void SomethingsFishy()
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

        public static void RectangleMangle_Find()
        {
            var allData = @"
NORTH/!BOREAL,SOUTH/!AUSTRAL,WEST/!OCCIDENTAL,EAST/!ORIENTAL
WINTER/!HIBERNAL,SPRING/!VERNAL,SUMMER/!ESTIVAL,AUTUMN/!AUTUMNAL
DONATELLO,MICHELANGELO,RAPHAEL,LEONARDO
#ARAMIS,PORTHOS,ATHOS,DARTAGNAN
CONQUEST,WAR,FAMINE,DEATH
ADDITION/ADD/PLUS/SUM/ADDEND/ADDEND,SUBTRACTION/SUBTRACT/MINUS/DIFFERENCE/MINUEND/SUBTRAHEND,MULTIPLICATION/MULTIPLY/TIMES/PRODUCT/FACTOR/FACTOR,DIVISION/DIVIDE/OVER/QUOTIENT/DIVIDEND/DIVISOR
FIRE,WATER,EARTH,AIR
MATTHEW,MARK,LUKE,JOHN
JOHNLENNON/LENNON,PAULMCCARTNEY/MCCARTNEY,GEORGEHARRISON/HARRISON,RINGOSTARR/STARR
LEFTVENTRICLE,RIGHTVENTRICLE,LEFTAURICLE,RIGHTAURICLE
TINKYWINKY,DIPSY,LAALAA,PO
ADENINE,THYMINE,GUANINE,CYTOSINE
SPADES,HEARTS,CLUBS,DIAMONDS
MERCURY,VENUS,EARTH,MARS
JUPITER,SATURN,URANUS,NEPTUNE
#CALLISTO,EUROPA,GANYMEDE,IO
#BLACKBILE/CHOLERIC,YELLOWBILE/MELANCHOLIC,PHLEGM/PHLEGMATIC,BLOOD/SANGUINE
#RAIDERSOFTHELOSTARK,TEMPLEOFDOOM,LASTCRUSADE,KINGDOMOFTHECRYSTALSKULL
ENGLAND,SCOTLAND,WALES,NORTHERNIRELAND
JUSTICE,PRUDENCE,FORTITUDE,TEMPERANCE
JERRY,GEORGE,ELAINE,KRAMER
#RED,GREEN,BLUE,YELLOW
CYAN,MAGENTA,YELLOW,KEY
#DUKKHA,SAMUDAYA,NIRODHA,MAGGA
#SARAH,REBEKAH,LEAH,RACHEL
#RIGVEDA,SAMAVEDA,YAJURVEDA,ATHARVAVEDA
A,B,AB,O
SOLID,LIQUID,GAS,PLASMA
GRAVITY,ELECTROMAGNETISM,WEAK,STRONG
INTAKE,COMPRESSION,POWER,EXHAUST
FIRSTBASE,SECONDBASE,THIRDBASE,HOMEPLATE
COLORADO,UTAH,NEWMEXICO,ARIZONA
MRFANTASTIC,THEINVISIBLEWOMAN,THEHUMANTORCH,THETHING
GRYFFINDOR,HUFFLEPUFF,RAVENCLAW,SLYTHERIN
HARRYPOTTER,FLEURDELACOUR,CEDRICDIGGORY,VIKTORKRUM
#WESTEROS,ESSOS,SOTHORYOS,ULTHOS
PETERVENKMAN,RAYSTANTZ,EGONSPENGLER,WINSTONZEDDEMORE
STANMARSH/STAN,KYLEBROFLOVSKI/KYLE,KENNYMCCORMICK/KENNY,ERICCARTMAN/ERIC
INKY,BLINKY,PINKY,CLYDE
MURDOCK,BABARACUS,HANNIBAL,FACEMAN
PLUM,ORCHID,CHRYSANTHEMUM,BAMBOO"
                .Trim().Replace("\r", "").Split('\n')
                .Where(row => row.Length > 0 && !row.StartsWith("#"))
                .Select(row => row.TrimStart('#').Split(',', '/').Where(str => !str.StartsWith("!")).Select(str => str.TrimStart('!')).ToArray())
                .ToArray();

            var set = new List<(string fragment, string item1, int dataIx1, string item2, int dataIx2)>();
            for (int i = 0; i < allData.Length; i++)
                for (int j = i + 1; j < allData.Length; j++)
                    for (int ii = 0; ii < allData[i].Length; ii++)
                        for (int jj = 0; jj < allData[j].Length; jj++)
                            if (allData[i][ii].Length == allData[j][jj].Length)
                            {
                                var commonalities = Enumerable.Range(0, allData[i][ii].Length).Where(ix => allData[i][ii][ix] == allData[j][jj][ix]).Select(ix => allData[i][ii][ix]).JoinString();
                                if (commonalities.Length > 0)
                                    set.Add((commonalities, allData[i][ii], i, allData[j][jj], j));
                            }

            ConsoleUtil.WriteParagraphs(set.Select(s => s.fragment).GroupBy(s => s[0]).OrderBy(gr => gr.Key).Select(gr => new ConsoleColoredString($"{(gr.Key + ":").Color(ConsoleColor.White)} {gr.Distinct().Order().JoinString(", ").Color(ConsoleColor.DarkGreen)}")).JoinColoredString("\n"));
            Console.WriteLine();
            var phrase = "EUROPAIOCALLISTO";

            IEnumerable<int[]> recurse(int[] sofar, string phraseLeft, int[] available)
            {
                if (phraseLeft == "")
                {
                    yield return sofar;
                    yield break;
                }

                for (int len = phraseLeft.Length; len > 0; len--)
                {
                    var substr = phraseLeft.Substring(0, len);
                    for (int p = 0; p < available.Length; p++)
                        if (set[available[p]].fragment == substr)
                            foreach (var solution in recurse(
                                sofar.Insert(sofar.Length, available[p]),
                                phraseLeft.Substring(len),
                                available
                                    .Where(av => set[av].dataIx1 != set[available[p]].dataIx1 && set[av].dataIx1 != set[available[p]].dataIx2 && set[av].dataIx2 != set[available[p]].dataIx1 && set[av].dataIx2 != set[available[p]].dataIx2)
                                    .ToArray()))
                                yield return solution;
                }
            }

            // GRID FOR A SOLUTION WITH 14 ELEMENTS ONLY!
            var gridRaw = RectangleMangle_getGrid();
            var grid = Enumerable.Range(0, 14).Select(ix =>
            {
                var numPositions = Enumerable.Range(0, gridRaw.Length).Where(grIx => gridRaw[grIx] != null && gridRaw[grIx].Value.val == ix && gridRaw[grIx].Value.grp1 == false).ToArray();
                var ltrPositions = Enumerable.Range(0, gridRaw.Length).Where(grIx => gridRaw[grIx] != null && gridRaw[grIx].Value.val == ix && gridRaw[grIx].Value.grp1 == true).ToArray();
                var x = numPositions.Aggregate(0, (prev, next) => prev ^ (next % 10));
                var x2 = ltrPositions.Aggregate(0, (prev, next) => prev ^ (next % 10));
                var y = numPositions.Aggregate(0, (prev, next) => prev ^ (next / 10));
                var y2 = ltrPositions.Aggregate(0, (prev, next) => prev ^ (next / 10));
                if (numPositions.Length != 3 || ltrPositions.Length != 3 || x != x2 || y != y2)
                    Debugger.Break();
                return (ix, x, y);
            })
                .OrderBy(loc => loc.y).ThenBy(loc => loc.x)
                .ToArray();

            foreach (var solution in recurse(new int[0], phrase, Enumerable.Range(0, set.Count).ToArray()))
            {
                if (solution.Length != 14)
                {
                    // ABOVE GRID WORKS ONLY FOR A SOLUTION WITH 14 ELEMENTS
                    continue;
                }

                ConsoleUtil.WriteLine($"{solution.Select(ix => set[ix].fragment).JoinString(", ").Color(ConsoleColor.White)} ({solution.Length.ToString().Color(ConsoleColor.Yellow)})", null);
                var sb = new StringBuilder();
                for (var solIx = 0; solIx < solution.Length; solIx++)
                {
                    var setIx = solution[solIx];
                    var (fragment, item1, dataIx1, item2, dataIx2) = set[setIx];
                    ConsoleUtil.WriteLine($"    {fragment.Color(ConsoleColor.Yellow)} = {item1.Color(ConsoleColor.Green)} + {item2.Color(ConsoleColor.Cyan)}", null);
                    var (ix, x, y) = grid[solIx];
                    string remainders(int dataIx, string exception)
                    {
                        var batchLen = allData[dataIx].Length / 4;
                        var ixInBatch = allData[dataIx].IndexOf(exception) % batchLen;
                        return Enumerable.Range(0, 4).Select(i => allData[dataIx][ixInBatch + i * batchLen]).Where(i => i != exception).JoinString("\t");
                    }
                    sb.AppendLine($"{(char) ('A' + ix)}\t{remainders(dataIx1, item1)}\t{item1}\t{ix + 1}\t{remainders(dataIx2, item2)}\t{item2}\t{fragment}");
                }
                Clipboard.SetText(sb.ToString());
                Console.ReadLine();
                Console.WriteLine();
            }
        }

        private static (string valStr, int val, bool grp1)?[] RectangleMangle_getGrid() => @" 6 6 N M M11 J11 J N
13 512 M██ H██12 H E
10 F 3 F 3████11 J K
 6██ 2 F░░ 7 E 2 7 E
 814██ 1 1██ B B H N
 8 412 A 4 8 L██░░ A
 9 5████ 3 C 5 G G I
101414██ 1 710 G██ A
 9██ 2 9 4 K B██ D K
13 D C I13 C L L D I".Replace("\r", "").Split('\n').SelectMany(row => row.Split(2).Select(str =>
                          int.TryParse(str, out int value) ? (valStr: str.Trim(), val: value - 1, grp1: false).Nullable() :
                          str[1] >= 'A' && str[1] <= 'Z' ? (valStr: str.Trim(), val: str[1] - 'A', grp1: true).Nullable() : null).ToArray()).ToArray();

        public static void RectangleMangle_ConstructGrid()
        {
            const int w = 10;
            const int h = 10;

            IEnumerable<(int[] board, int num)> recurse(int[] sofar, int ix)
            {
                var spaces = Enumerable.Range(0, w * h).Where(i => (i % w) > 0 && (i % w) < w - 1 && (i / w) > 0 && (i / w) < h - 1 && sofar[i] == 0).ToArray().Shuffle();
                var any = false;
                foreach (var space in spaces)
                {
                    var x = space % w;
                    var y = space / w;
                    var candidates = Enumerable.Range(0, w)
                        .Where(x2 => x2 != x && sofar[x2 + w * y] == 0)
                        .SelectMany(x2 => Enumerable.Range(0, h).Where(y2 => y2 != y && sofar[x + w * y2] == 0 && sofar[x2 + w * y2] == 0).Select(y2 => (x2, y2)))
                        .ToArray();
                    var candidatePairs = candidates.UniquePairs()
                        .Select(tup => (x1: tup.Item1.x2, y1: tup.Item1.y2, tup.Item2.x2, tup.Item2.y2))
                        .Where(tup => tup.x1 != tup.x2 && tup.y1 != tup.y2)
                        .ToArray().Shuffle();
                    foreach (var (x1, y1, x2, y2) in candidatePairs)
                    {
                        sofar[space] = -1;
                        sofar[x1 + w * y1] = 2 * ix + 1;
                        sofar[x + w * y1] = 2 * ix + 1;
                        sofar[x1 + w * y] = 2 * ix + 1;
                        sofar[x2 + w * y] = 2 * ix + 2;
                        sofar[x + w * y2] = 2 * ix + 2;
                        sofar[x2 + w * y2] = 2 * ix + 2;

                        foreach (var result in recurse(sofar, ix + 1))
                        {
                            yield return result;
                            any = true;
                        }

                        sofar[space] = 0;
                        sofar[x1 + w * y1] = 0;
                        sofar[x + w * y1] = 0;
                        sofar[x1 + w * y] = 0;
                        sofar[x2 + w * y] = 0;
                        sofar[x + w * y2] = 0;
                        sofar[x2 + w * y2] = 0;
                    }
                }
                if (!any)
                {
                    yield return (sofar.ToArray(), ix);
                    yield break;
                }
            }

            var best = 0;
            foreach (var (solution, num) in recurse(new int[w * h], 0))
            {
                if (num > best)
                {
                    best = num;
                    ConsoleUtil.WriteLine($"{num}".Color(ConsoleColor.White));
                    ConsoleUtil.WriteLine(solution.Split(w)
                        .Select(row => row
                            .Select(i => (i == -1 ? "██" : i == 0 ? "░░" : i % 2 == 0 ? ((char) ('A' + (i - 1) / 2)).ToString().PadLeft(2) : ((i + 1) / 2).ToString().PadLeft(2))
                                .Color(i == -1 ? ConsoleColor.DarkGray : ConsoleColor.White, i < 1 ? ConsoleColor.Black : (ConsoleColor) (((i + 1) / 2 - 1) % 15 + 1)))
                            .JoinColoredString())
                        .JoinColoredString("\n"));
                    Console.WriteLine();
                }
            }
        }

        public static void RectangleMangle_ConstructGridWithSquares()
        {
            const int w = 10;
            const int h = 10;

            IEnumerable<(int[] board, int num)> recurse(int[] sofar, int ix)
            {
                var spaces = Enumerable.Range(0, w * h).Where(i => (i % w) > 0 && (i % w) < w - 1 && (i / w) > 0 && (i / w) < h - 1 && sofar[i] == 0).ToArray().Shuffle();
                var any = false;
                foreach (var space in spaces)
                {
                    var x = space % w;
                    var y = space / w;
                    var candidates = Enumerable.Range(0, w)
                        .Where(x2 => x2 != x && y + (x2 - x) >= 0 && y + (x2 - x) < h && sofar[x2 + w * y] == 0 && sofar[x + w * (y + x2 - x)] == 0 && sofar[x2 + w * (y + x2 - x)] == 0)
                        .Select(x2 => (x2, y2: y + x2 - x))
                        .ToArray();
                    var candidatePairs = candidates.UniquePairs()
                        .Select(tup => (x1: tup.Item1.x2, y1: tup.Item1.y2, tup.Item2.x2, tup.Item2.y2))
                        .Where(tup => tup.x1 != tup.x2 && tup.y1 != tup.y2)
                        .ToArray().Shuffle();
                    foreach (var (x1, y1, x2, y2) in candidatePairs)
                    {
                        sofar[space] = -1;
                        sofar[x1 + w * y1] = 2 * ix + 1;
                        sofar[x + w * y1] = 2 * ix + 1;
                        sofar[x1 + w * y] = 2 * ix + 1;
                        sofar[x2 + w * y] = 2 * ix + 2;
                        sofar[x + w * y2] = 2 * ix + 2;
                        sofar[x2 + w * y2] = 2 * ix + 2;

                        foreach (var result in recurse(sofar, ix + 1))
                        {
                            yield return result;
                            any = true;
                        }

                        sofar[space] = 0;
                        sofar[x1 + w * y1] = 0;
                        sofar[x + w * y1] = 0;
                        sofar[x1 + w * y] = 0;
                        sofar[x2 + w * y] = 0;
                        sofar[x + w * y2] = 0;
                        sofar[x2 + w * y2] = 0;
                    }
                }
                if (!any)
                {
                    yield return (sofar.ToArray(), ix);
                    yield break;
                }
            }

            var best = 0;
            foreach (var (solution, num) in recurse(new int[w * h], 0))
            {
                if (num > best)
                {
                    best = num;
                    ConsoleUtil.WriteLine($"{num}".Color(ConsoleColor.White));
                    ConsoleUtil.WriteLine(solution.Split(w)
                        .Select(row => row
                            .Select(i => (i == -1 ? "██" : i == 0 ? "░░" : i % 2 == 0 ? ((char) ('A' + (i - 1) / 2)).ToString().PadLeft(2) : ((i + 1) / 2).ToString().PadLeft(2))
                                .Color(i == -1 ? ConsoleColor.DarkGray : ConsoleColor.White, i < 1 ? ConsoleColor.Black : (ConsoleColor) (((i + 1) / 2 - 1) % 15 + 1)))
                            .JoinColoredString())
                        .JoinColoredString("\n"));
                    Console.WriteLine();
                }
            }
        }

        public static void RectangleMangle_GeneratePuzzle()
        {
            var grid = RectangleMangle_getGrid();
            var values = @"M	VENUS	EARTHPLANET	MARS
E	SOUTH	WEST	EAST
K	CYAN	MAGENTA	KEY
J	INTAKE	COMPRESSION	EXHAUST
F	FIRE	WATER	AIR
N	JOHNLENNON	PAULMCCARTNEY	GEORGEHARRISON
H	DEATH	WAR	FAMINE
L	PETERVENKMAN	EGONSPENGLER	WINSTONZEDDEMORE
C	MICHELANGELO	RAPHAEL	LEONARDO
I	TINKYWINKY	DIPSY	PO
A	SUBTRACTION	MULTIPLICATION	DIVISION
G	WINTER	SPRING	AUTUMN
D	MARK	LUKE	JOHN
B	COLORADO	UTAH	NEWMEXICO
13	JUPITER	SATURN	URANUS
5	GEORGE	ELAINE	KRAMER
11	PLUM	ORCHID	CHRYSANTHEMUM
10	INKY	BLINKY	CLYDE
6	ENGLAND	SCOTLAND	NORTHERNIRELAND
14	HARRYPOTTER	FLEURDELACOUR	CEDRICDIGGORY
8	ADENINE	THYMINE	GUANINE
12	KYLEBROFLOVSKI	KENNYMCCORMICK	ERICCARTMAN
3	GRYFFINDOR	HUFFLEPUFF	SLYTHERIN
9	SOLID	GAS	PLASMA
1	MRFANTASTIC	THEINVISIBLEWOMAN	THEHUMANTORCH
7	GRAVITY	ELECTROMAGNETISM	WEAK
4	PRUDENCE	FORTITUDE	TEMPERANCE
2	BABARACUS	HANNIBAL	FACEMAN".Replace("\r", "").Split('\n').Select(row => row.Split('\t')).Select(arr => (valStr: arr[0], names: arr.Skip(1).ToArray())).ToArray();
            var counts = new Dictionary<string, int>();
            const int targetWidth = 105;
            const int targetHeight = 105;
            File.WriteAllText(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Rectangle Mangle\Rectangle Mangle.html", $@"<!DOCTYPE html>
<html>
    <head>
        <style>
            .image {{
                width: {targetWidth}px;
            }}
        </style>
    </head>
    <body>
        <table class='puzzle'>
            {Enumerable.Range(0, 10).Select(row => $@"<tr>{Enumerable.Range(0, 10).Select(col =>
            {
                if (grid[col + 10 * row] == null)
                    return "<td></td>";
                var (valStr, names) = values.First(vd => vd.valStr == grid[col + 10 * row].Value.valStr);
                var count = counts.Get(valStr, 0);
                var name = names[count];
                var filename = "png,jpg,jpeg,bmp".Split(',').Select(ext => $@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Rectangle Mangle\{name}.{ext}").First(f => File.Exists(f));
                counts.IncSafe(valStr);
                using (var bmp = new Bitmap(filename))
                using (var mem = new MemoryStream())
                {
                    GraphicsUtil.DrawBitmap(targetWidth, targetHeight, g =>
                    {
                        g.DrawImage(bmp, GraphicsUtil.FitIntoMaintainAspectRatio(bmp.Size, new Rectangle(0, 0, targetWidth, targetHeight)));
                    }).Save(mem, ImageFormat.Png);
                    return $@"<td><img src='data:image/png;base64,{Convert.ToBase64String(mem.ToArray())}' class='image' /></td>";
                }
            }).JoinString()}</tr>").JoinString()}
        </table>
    </body>
</html>
");
        }

        public static void PolyhedralPuzzle_DownloadFiles()
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
                    var targetPath = $@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Polyhedral Puzzle\Txt\{filename}";
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

            public double XOffset;
            public double YOffset;
            public double Rotation;

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

        public static Polyhedron PolyhedralPuzzle_Parse(string path)
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

        public static void PolyhedralPuzzle_Test()
        {
            const int extraFaces = 1;

            var polyhedra = PolyhedralPuzzle_GetPolyhedra();

            foreach (var p in polyhedra)
                if (p.Faces.Length == 24)
                    Console.WriteLine(p.Name);
            Console.WriteLine();

            polyhedra = polyhedra.Where(p => p.Faces.Length <= 25 && !"{}".Any(ch => p.Name.Contains(ch))).ToList();

            var loopLen = 6;
            IEnumerable<int[]> recurse(int[] sofar)
            {
                if (sofar.Length == loopLen)
                {
                    yield return sofar;
                    yield break;
                }

                for (var i = 0; i < polyhedra.Count; i++)
                {
                    if (sofar.Contains(i))
                        continue;
                    var polyhedron = polyhedra[i];
                    var nameLen1 = polyhedron.Name.ToUpperInvariant().Count(ch => ch >= 'A' && ch <= 'Z');
                    var nameLen2 = Regex.Replace(polyhedron.Name, @"\s*\(.*\)\s*$", "").ToUpperInvariant().Count(ch => ch >= 'A' && ch <= 'Z');
                    var joinsUp = true;
                    if (sofar.Length == loopLen - 1)
                    {
                        var oNameLen1 = polyhedra[sofar[0]].Name.ToUpperInvariant().Count(ch => ch >= 'A' && ch <= 'Z');
                        var oNameLen2 = Regex.Replace(polyhedra[sofar[0]].Name, @"\s*\(.*\)\s*$", "").ToUpperInvariant().Count(ch => ch >= 'A' && ch <= 'Z');
                        joinsUp = polyhedra[i].Faces.Length - extraFaces == oNameLen1 || polyhedra[i].Faces.Length - extraFaces == oNameLen2;
                    }
                    if (joinsUp && (sofar.Length == 0 || polyhedra[sofar[sofar.Length - 1]].Faces.Length - extraFaces == nameLen1 || polyhedra[sofar[sofar.Length - 1]].Faces.Length - extraFaces == nameLen2))
                        foreach (var solution in recurse(sofar.Insert(sofar.Length, i)))
                            yield return solution;
                }
            }

            var count = 0;
            var best = int.MaxValue;
            var bestCount = 0;
            foreach (var solution in recurse(new int[0]))
            {
                count++;

                var totalFaces = solution.Sum(ix => polyhedra[ix].Faces.Length);
                if (totalFaces < best)
                {
                    best = totalFaces;
                    var tt = new TextTable { ColumnSpacing = 2 };

                    tt.SetCell(4, 0, "V".Color(ConsoleColor.Green));
                    tt.SetCell(5, 0, "F".Color(ConsoleColor.Cyan));

                    var row = 1;
                    for (var i = 0; i < solution.Length; i++)
                    {
                        var polyhedron = polyhedra[solution[i]];
                        var nameLen1 = polyhedron.Name.ToUpperInvariant().Count(ch => ch >= 'A' && ch <= 'Z');
                        var nameLen2 = Regex.Replace(polyhedron.Name, @"\s*\(.*\)\s*$", "").ToUpperInvariant().Count(ch => ch >= 'A' && ch <= 'Z');
                        tt.SetCell(0, row, polyhedron.Filename.Color(ConsoleColor.DarkYellow));
                        tt.SetCell(1, row, polyhedron.Name.Color(ConsoleColor.Yellow));
                        tt.SetCell(2, row, nameLen1.ToString().Color(ConsoleColor.Magenta));
                        tt.SetCell(3, row, nameLen2.ToString().Color(ConsoleColor.Magenta));
                        tt.SetCell(4, row, polyhedron.Vertices.Length.ToString().Color(ConsoleColor.Green));
                        tt.SetCell(5, row, polyhedron.Faces.Length.ToString().Color(ConsoleColor.Cyan));
                        row++;
                    }
                    tt.SetCell(5, row, totalFaces.ToString().Color(ConsoleColor.White));
                    tt.WriteToConsole();
                    Console.WriteLine();
                    bestCount = 0;
                }
                else if (totalFaces == best)
                    bestCount++;
            }
            Console.WriteLine(count);
            Console.WriteLine(bestCount);
        }

        private static List<Polyhedron> PolyhedralPuzzle_GetPolyhedra()
        {
            var polyhedra = new List<Polyhedron>();
            var files = new DirectoryInfo(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Polyhedral Puzzle\Txt").EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly).ToArray();
            files.ParallelForEach(4, file =>
            {
                var polyhedron = PolyhedralPuzzle_Parse(file.FullName);
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

        public static void PolyhedralPuzzle_GenerateNets()
        {
            var polyhedra = PolyhedralPuzzle_GetPolyhedra();
            File.WriteAllText(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Polyhedral Puzzle\Polyhedral Puzzle.html",
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
    <body>{polyhedra.Where(p => p.Faces.Length < 300).OrderBy(p => p.Faces.Length).ThenBy(p => p.Name).Select(p =>
                {
                    try { return $"<div class='poly-wrap'>{PolyhedralPuzzle_GenerateNet(p).svg}<div class='filename'>{p.Filename}</div><div class='name'><a href='http://dmccooey.com/polyhedra/{Path.GetFileNameWithoutExtension(p.Filename)}.html'>{p.Name}</a> ({p.Faces.Length} faces)</div></div>"; }
                    catch (Exception e) { return $"<div>Error - {p.Filename} - <a href='http://dmccooey.com/polyhedra/{Path.GetFileNameWithoutExtension(p.Filename)}.html'>{p.Name}</a> - {e.Message} - {e.GetType().FullName}</div>"; }
                }).JoinString()}</body></html>");
        }

        private static (string svg, PointD[][] polygons) PolyhedralPuzzle_GenerateNet(Polyhedron polyhedron)
        {
            // Numbers closer than this are considered equal
            const double closeness = .00001;
            bool sufficientlyClose(Pt p1, Pt p2) => Math.Abs(p1.X - p2.X) < closeness && Math.Abs(p1.Y - p2.Y) < closeness && Math.Abs(p1.Z - p2.Z) < closeness;

            // Take a full copy
            var faces = polyhedron.Faces.Select(face => face.Select(vIx => polyhedron.Vertices[vIx]).ToArray()).ToArray();

            var svg = new StringBuilder();
            var svgOutlines = new StringBuilder();

            // Restricted variable scope
            {
                var vx = faces[0][0];
                // Put first vertex at origin and apply rotation
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
            var seen = new HashSet<int>();

            q.Enqueue((0, faces));
            while (q.Count > 0)
            {
                var (fromFaceIx, rotatedPolyhedron) = q.Dequeue();
                polygons[fromFaceIx] = rotatedPolyhedron[fromFaceIx].Select(pt => p(pt.X, pt.Y)).ToArray();

                svgOutlines.Append($@"<path id='outline-{fromFaceIx}' d='M{polygons[fromFaceIx].Select(p => $"{p.X},{p.Y}").JoinString(" ")}z' fill='transparent' />");

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
                        var newPolyhedron = Ut.NewArray(
                            rotatedPolyhedron.Select(face => face.Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], asin)).ToArray()).ToArray(),
                            rotatedPolyhedron.Select(face => face.Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], -asin)).ToArray()).ToArray(),
                            rotatedPolyhedron.Select(face => face.Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], 180 + asin)).ToArray()).ToArray(),
                            rotatedPolyhedron.Select(face => face.Select(p => p.Rotate(toFace[(toEdgeIx + 1) % toFace.Length], toFace[toEdgeIx], 180 - asin)).ToArray()).ToArray())
                            .Where(sld => sld.All(fc => fc.All(p => p.Z > -closeness)))
                            .MinElement(sld => sld[toFaceIx].Sum(p => p.Z * p.Z));
                        q.Enqueue((toFaceIx, newPolyhedron));
                    }
                    else
                    {
                        var p1 = polygons[fromFaceIx][fromEdgeIx];
                        var p2 = polygons[fromFaceIx][(fromEdgeIx + 1) % polygons[fromFaceIx].Length];
                        IEnumerable<string> classes = new[] { $"face-{fromFaceIx}", $"face-{toFaceIx}", $"edge-{fromFaceIx}-{fromEdgeIx}", $"edge-{toFaceIx}-{toEdgeIx}" };
                        svg.Append($@"<path id='edge-{fromFaceIx}-{fromEdgeIx}' d='M {p1.X},{p1.Y} L {p2.X},{p2.Y}' stroke-width='.025' stroke='black' />");

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

                            classes = classes.Concat("decor");
                            //switch (adj & Adjacency.ConnectionMask)
                            //{
                            //    case Adjacency.Portaled:
                            //        var ch = polyhedron.GetPortalLetter(fromFaceIx, fromEdgeIx);
                            //        sendText($"portal-letter-{fromFaceIx}-{fromEdgeIx}", classes, .5, p1c.X, p1c.Y, ch.ToString(), "#000", edgeData);
                            //        sendText($"portal-letter-{toFaceIx}-{toEdgeIx}", classes, .5, p2c.X, p2c.Y, ch.ToString(), "#000", edgeData);
                            //        sendPath($"portal-marker-{fromFaceIx}-{fromEdgeIx}", classes, edgeData, $"M {(p11.X + p1m.X) / 2},{(p11.Y + p1m.Y) / 2} {(p1c.X + p1m.X) / 2},{(p1c.Y + p1m.Y) / 2} {(p12.X + p1m.X) / 2},{(p12.Y + p1m.Y) / 2} z", fill: "#888");
                            //        sendPath($"portal-marker-{toFaceIx}-{toEdgeIx}", classes, edgeData, $"M {(p21.X + p2m.X) / 2},{(p21.Y + p2m.Y) / 2} {(p2c.X + p2m.X) / 2},{(p2c.Y + p2m.Y) / 2} {(p22.X + p2m.X) / 2},{(p22.Y + p2m.Y) / 2} z", fill: "#888");
                            //        break;

                            //    case Adjacency.Curved:
                            svg.Append($@"<path stroke-width='.025' stroke='cornflowerblue' fill='none' id='curve-{fromFaceIx}-{fromEdgeIx}' d='{(
                                (p2m - p1m).Distance() < .5 ? $"M {p1m.X},{p1m.Y} L {p2m.X},{p2m.Y}" :
                                l1 >= 0 && l1 <= 1 && l2 >= 0 && l2 <= 1 ? $"M {p1m.X},{p1m.Y} C {intersect.X},{intersect.Y} {intersect.X},{intersect.Y} {p2m.X},{p2m.Y}" :
                                $"M {p1m.X},{p1m.Y} C {p1c.X},{p1c.Y} {p2c.X},{p2c.Y} {p2m.X},{p2m.Y}")}' />");
                            //        break;
                            //}
                        }
                    }
                }
            }

            var xMin = polygons.Min(pg => pg?.Min(p => p.X)).Value;
            var yMin = polygons.Min(pg => pg?.Min(p => p.Y)).Value;
            var xMax = polygons.Max(pg => pg?.Max(p => p.X)).Value;
            var yMax = polygons.Max(pg => pg?.Max(p => p.Y)).Value;
            return (svg: $@"<svg class='polyhedron' xmlns='http://www.w3.org/2000/svg' viewBox='{xMin - .5} {yMin - .5} {xMax - xMin + 1} {yMax - yMin + 1}'>{svg}{svgOutlines}</svg>", polygons);
        }

        public static void PolyhedralPuzzle_GenerateCrossword()
        {
            var polyhedron = PolyhedralPuzzle_Parse(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Polyhedral Puzzle\Txt\SelfDualIcosioctahedron4.txt");
            var (svg, polygons) = PolyhedralPuzzle_GenerateNet(polyhedron);
            var polyMidPoints = polygons.Select(poly => new PointD(poly.Average(p => p.X), poly.Average(p => p.Y))).ToArray();
            var xMin = polygons.Min(pg => pg.Min(p => p.X));
            var yMin = polygons.Min(pg => pg.Min(p => p.Y));
            var xMax = polygons.Max(pg => pg.Max(p => p.X));
            var yMax = polygons.Max(pg => pg.Max(p => p.Y));

            var minGridSize = 1;
            var maxGridSize = 1000;
            (double x, double y)[] coords = null;
            while (Math.Abs(maxGridSize - minGridSize) > 1)
            {
                var gridSize = (maxGridSize + minGridSize) / 2;
                coords = polyMidPoints.Select(p => (x: Math.Floor((p.X - xMin) / (xMax - xMin) * gridSize), y: Math.Floor((p.Y - yMin) / (yMax - yMin) * gridSize))).ToArray();
                if (coords.UniquePairs().Any(tup => tup.Item1 == tup.Item2))
                    minGridSize = gridSize;
                else
                    maxGridSize = gridSize;
            }


            //// CROSSWORD STUFF
            //var adjacents = polyhedron.Faces.Select((f, faceIx) => f.Select((v, ix) => polyhedron.Faces.IndexOf(f2 => Enumerable.Range(0, f2.Length).Any(ix2 => f2[ix2] == f[(ix + 1) % f.Length] && f2[(ix2 + 1) % f2.Length] == f[ix]))).ToArray()).ToArray();
            //var lightFragments = adjacents.SelectMany((f, fIx) => f.SelectMany((adj, vIx) =>
            //{
            //    var list = new List<(int prior, int face, int next)>();
            //    for (var i = f.Length / 2; i <= (f.Length + 1) / 2; i++)
            //        list.Add((adj, fIx, f[(vIx + i) % f.Length]));
            //    return list;
            //})).ToList();
            ////Console.WriteLine(ClassifyJson.Serialize(lightFragments).ToStringIndented());
            //var walls = new List<(int face1, int face2)>();
            //var lights = new List<int[]>();
            //var rnd = new Random(47);

            //while (lightFragments.Count > 0)
            //{
            //    var startIx = rnd.Next(0, lightFragments.Count);
            //    var chain = new List<(int prior, int face, int next)> { lightFragments[startIx] };
            //    var (prior, face, next) = lightFragments[startIx];
            //    var startFace = face;
            //    lightFragments.RemoveAt(startIx);

            //    walls.Add((prior, face));

            //    while (true)
            //    {
            //        var nextFragmentCandidates = lightFragments.Where(nf => nf.prior == face && nf.face == next).ToArray();
            //        if (nextFragmentCandidates.Length == 0)
            //            break;
            //        var nextFragment = nextFragmentCandidates.PickRandom(rnd);
            //        chain.Add(nextFragment);
            //        (prior, face, next) = nextFragment;
            //        if (next == startFace || walls.Any(w => (w.face1 == face && w.face2 == next) || (w.face2 == face && w.face1 == next)))
            //            break;
            //    }

            //    lightFragments.RemoveRange(chain);

            //    more:
            //    var targetLength = Enumerable.Range(3, chain.Count).Where(l => (l >= 3 && l <= chain.Count - 3) || l == chain.Count).PickRandom(rnd);
            //    var light = chain.Take(targetLength).Select(lf => lf.face).ToArray();
            //    var lightReverse = light.ToArray().ReverseInplace();
            //    if (!lights.Any(l => l.IndexOfSubarray(light) != -1 || l.IndexOfSubarray(lightReverse) != -1))
            //        lights.Add(light);

            //    if (targetLength < chain.Count)
            //    {
            //        walls.Add((chain[targetLength].prior, chain[targetLength].face));
            //        chain.RemoveRange(0, targetLength);
            //        goto more;
            //    }
            //}

            var cellW = (xMax - xMin) / maxGridSize;
            var cellH = (yMax - yMin) / maxGridSize;

            var facePins = coords.Select((c, ix) => $"<circle cx='{c.x * cellW + xMin}' cy='{c.y * cellH + yMin}' r='{Math.Min(cellW, cellH) * .2}' /><line stroke-width='.03' stroke='red' x1='{c.x * cellW + xMin}' y1='{c.y * cellH + yMin}' x2='{polyMidPoints[ix].X}' y2='{polyMidPoints[ix].Y}' />").JoinString();
            var faceLabels = polygons.Select((f, faceIx) => $"<text font-size='.2' text-anchor='middle' fill='#080' x='{f.Average(p => p.X)}' y='{f.Average(p => p.Y)}'>{faceIx}</text>").JoinString();
            File.WriteAllText(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Polyhedral Puzzle\Polyhedral Puzzle Crossword.svg",
                svg.Replace("</svg>", $"{facePins}{faceLabels}</svg>"));

            Console.WriteLine("Spaces:");
            Console.WriteLine(coords.Select((c, ix) => $"{ix} = {c}").JoinString("\n"));
            Console.WriteLine();
            Console.WriteLine("Lights:");
            var path = @"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Polyhedral Puzzle\Polyhedral Puzzle Crossword (lights).txt";
            File.WriteAllText(path, @"19 9 17 15 4 13 14 2 18,6 19 20 0 12,13 11 23 21,2 26 7 6 27 9,11 25 1 3 27 9,2 26 8,16 10 20 18 7,10 24 0,11 25 4 5 24 10,22 23 1 15,3 21 22 8,14 12 5 16 17"
                .Split(',')
                .Select(light => light.Split(' ').Select(i => int.Parse(i)).Select(f => coords[f]).Select(c => $"{c.x} {c.y}").JoinString("\r\n"))
                .Concat(coords.Select(c => $"{c.x} {c.y}").JoinString("\r\n"))
                .JoinString("\r\n\r\n"));
            Console.WriteLine($"Written to {path}");
            Console.WriteLine();

            //var allWords = Ut.NewArray(
            //    @"D:\Daten\Wordlists\English 60000.txt",
            //    @"D:\Daten\Wordlists\English words.txt",
            //    @"D:\Daten\Wordlists\peter_broda_wordlist_unscored.txt",
            //    @"D:\Daten\Wordlists\sowpods.txt"
            //)
            //    .SelectMany(file => File.ReadAllLines(file))
            //    .Where(word => Regex.IsMatch(word, @"^[-./a-zA-Z]+$"))
            //    .Select(word => word.ToUpperInvariant().Where(ch => ch >= 'A' && ch <= 'Z').JoinString())
            //    .Distinct()
            //    .ToArray();

            //Console.WriteLine(allWords.Where(w => Regex.IsMatch(w, @"^..Z..X$")).JoinString("\n"));
            //Console.WriteLine(allWords.Where(w => Regex.IsMatch(w, @"^X..Z..$")).JoinString("\n"));
        }

        enum LtGtClue { Equal, LessThan, GreaterThan };

        public static void PolyhedralPuzzle_GenerateLessThanGreaterThanPuzzle()
        {
            var polyhedron = PolyhedralPuzzle_Parse(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Polyhedral Puzzle\Txt\SnubSquareAntiprism.txt");
            var n = polyhedron.Faces.Length;
            var solution = Enumerable.Range(0, n).ToArray().Shuffle();
            Console.WriteLine($"Solution: {solution.JoinString(", ")}");
            var minValue = solution.Min();
            var maxValue = solution.Max();
            var adjacents = Enumerable.Range(0, n).Select(fIx => polyhedron.FindAdjacent(fIx).ToArray()).ToArray();

            // Clues that would be given in the puzzle
            var ltGtClues = Enumerable.Range(0, n).SelectMany(f1Ix => adjacents[f1Ix].Select(f2Ix => (f1Ix, f2Ix, op: solution[f1Ix] > solution[f2Ix] ? -1 : solution[f1Ix] < solution[f2Ix] ? 1 : 0))).ToArray();
            var givens = solution.Select((sol, i) => (fIx: i, value: sol)).ToArray().Shuffle().Take(n - 3).ToArray();

            //var originalClues = Ut.NewArray(n * n, ix =>
            //{
            //    var f1Ix = ix % n;
            //    var f2Ix = ix / n;
            //    return !adjacents[f1Ix].Contains(f2Ix) ? (int?) null : solution[f1Ix] > solution[f2Ix] ? -1 : solution[f1Ix] < solution[f2Ix] ? 1 : 0;
            //});


            // SOLVER STARTS HERE

            // Compute a sort of transitive closure of the given clues
            var transitiveClues = new int?[n * n];
            foreach (var (f1Ix, f2Ix, op) in ltGtClues)
            {
                transitiveClues[f1Ix + n * f2Ix] = op;
                transitiveClues[f2Ix + n * f1Ix] = -op;
            }
            while (true)
            {
                var anyChanges = false;

                for (var i = 0; i < n; i++)
                    for (var j = 0; j < n; j++)
                        if (j != i && transitiveClues[i + j * n] is int ij)
                            for (var k = 0; k < n; k++)
                                if (k != i && k != j && transitiveClues[j + k * n] is int jk)
                                    if ((ij >= 0 && jk >= 0 && (transitiveClues[i + k * n] == null || transitiveClues[i + k * n].Value < ij + jk)) ||
                                        (ij <= 0 && jk <= 0 && (transitiveClues[i + k * n] == null || transitiveClues[i + k * n].Value > ij + jk)))
                                    {
                                        transitiveClues[i + k * n] = ij + jk;
                                        anyChanges = true;
                                    }

                if (!anyChanges)
                    break;
            }

            IEnumerable<int[]> solve(int?[] clues, int[] sofar, int[] minValues, int[] maxValues, bool[] used, bool[] filled)
            {
                var allFilled = true;
                int faceIx = 0;
                int bestConstraint = int.MaxValue;
                for (var i = 0; i < n; i++)
                    if (!filled[i])
                    {
                        allFilled = false;
                        if (maxValues[i] - minValues[i] < bestConstraint)
                        {
                            bestConstraint = maxValues[i] - minValues[i];
                            faceIx = i;
                        }
                    }

                if (allFilled)
                {
                    // Check if the original constraints still hold
                    foreach (var (f1Ix, f2Ix, op) in ltGtClues)
                        if ((op == 0 && sofar[f1Ix] != sofar[f2Ix]) ||
                            (op == -1 && !(sofar[f1Ix] > sofar[f2Ix])) ||
                            (op == 1 && !(sofar[f1Ix] < sofar[f2Ix])))
                            Debugger.Break();

                    yield return sofar.ToArray();
                    yield break;
                }

                for (var v = minValues[faceIx]; v <= maxValues[faceIx]; v++)
                {
                    // Use this only if values aren’t supposed to be all unique
                    if (used[v])
                        goto busted;

                    var mins = minValues;
                    var minsCopied = false;
                    var maxs = maxValues;
                    var maxsCopied = false;
                    for (var otherFaceIx = 0; otherFaceIx < n; otherFaceIx++)
                    {
                        if (otherFaceIx != faceIx && clues[faceIx + n * otherFaceIx] is int clue)
                        {
                            if (clue >= 0 && mins[otherFaceIx] < v + clue)
                            {
                                if (!minsCopied)
                                {
                                    mins = (int[]) minValues.Clone();
                                    minsCopied = true;
                                }
                                mins[otherFaceIx] = v + clue;
                            }
                            if (clue <= 0 && maxs[otherFaceIx] > v + clue)
                            {
                                if (!maxsCopied)
                                {
                                    maxs = (int[]) maxValues.Clone();
                                    maxsCopied = true;
                                }
                                maxs[otherFaceIx] = v + clue;
                            }
                            if (mins[otherFaceIx] > maxs[otherFaceIx])
                                goto busted;
                        }
                    }

                    sofar[faceIx] = v;
                    used[v] = true;
                    filled[faceIx] = true;
                    foreach (var sol in solve(clues, sofar, mins, maxs, used, filled))
                        yield return sol;
                    used[v] = false;
                    filled[faceIx] = false;

                    busted:;
                }
            }

            var newMinValues = Ut.NewArray(n, i => minValue + Enumerable.Range(0, n).Where(i2 => transitiveClues[i2 + n * i] is int clue && clue > 0).MaxOrDefault(i2 => transitiveClues[i2 + n * i].Value, 0));
            var newMaxValues = Ut.NewArray(n, i => maxValue + Enumerable.Range(0, n).Where(i2 => transitiveClues[i2 + n * i] is int clue && clue < 0).MinOrDefault(i2 => transitiveClues[i2 + n * i].Value, 0));
            foreach (var (fIx, value) in givens)
            {
                newMinValues[fIx] = value;
                newMaxValues[fIx] = value;
            }
            ConsoleUtil.WriteLine($"{"GIVENS:  ".Color(ConsoleColor.Cyan)} — {Enumerable.Range(0, n).Select(i => (givens.Where(tup => tup.fIx == i).FirstOrNull().NullOr(tup => tup.value.ToString()) ?? "").PadLeft(3)).JoinString(", ").Color(ConsoleColor.DarkCyan)}", null);
            ConsoleUtil.WriteLine($"{"MINS:    ".Color(ConsoleColor.Magenta)} — {newMinValues.Select(i => i.ToString().PadLeft(3)).JoinString(", ").Color(ConsoleColor.DarkMagenta)}", null);
            ConsoleUtil.WriteLine($"{"MAXS:    ".Color(ConsoleColor.Green)} — {newMaxValues.Select(i => i.ToString().PadLeft(3)).JoinString(", ").Color(ConsoleColor.DarkGreen)}", null);
            foreach (var sol in solve(transitiveClues, new int[n],
                minValues: newMinValues,
                maxValues: newMaxValues,
                new bool[n], new bool[n]))
                ConsoleUtil.WriteLine($"{"SOLUTION:".Color(ConsoleColor.White)} — {sol.Select(i => i.ToString().PadLeft(3)).JoinString(", ").Color(ConsoleColor.Yellow)}", null);
        }

        enum EdgeClueType
        {
            Sum,
            Difference,
            Product,
            Quotient,
            Rel
        }

        public static void PolyhedralPuzzle_GenerateEdgeClues()
        {
            //var polyhedra = PolyhedralPuzzle_GetPolyhedra();
            //foreach (var p in polyhedra)
            //    if (p.Faces.Length == 28)
            //        Console.WriteLine($"{p.Name} = {p.Filename}");
            //return;

            var polyhedron = PolyhedralPuzzle_Parse(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Polyhedral Puzzle\Txt\SelfDualIcosioctahedron2.txt");
            Console.WriteLine(polyhedron.Faces.Length);
            var n = polyhedron.Faces.Length;

            var rnd = new Random(3);
            var solution = Enumerable.Range(1, 26).Concat(new[] { 1, 4 }).ToArray().Shuffle(rnd);
            if (solution.Length != n)
                Debugger.Break();

            IEnumerable<(int face1, int face2, EdgeClueType type, int value)> generateClues(int face1, int face2)
            {
                var list = new List<(int face1, int face2, EdgeClueType type, int value)>();
                list.Add((face1, face2, EdgeClueType.Sum, solution[face1] + solution[face2]));
                list.Add((face1, face2, EdgeClueType.Difference, Math.Abs(solution[face1] - solution[face2])));
                var product = solution[face1] * solution[face2];
                if (new[] { 2, 3, 5, 7 }.Count(f => product % f == 0) >= 3)
                    list.Add((face1, face2, EdgeClueType.Product, product));
                //list.Add((face1, face2, KenKenClueType.Rel, Math.Sign(solution[face2] - solution[face1])));
                if (solution[face1] != 0 && solution[face2] % solution[face1] == 0 && solution[face2] / solution[face1] <= 5)
                    list.Add((face1, face2, EdgeClueType.Quotient, solution[face2] / solution[face1]));
                else if (solution[face2] != 0 && solution[face1] % solution[face2] == 0 && solution[face1] / solution[face2] <= 5)
                    list.Add((face1, face2, EdgeClueType.Quotient, solution[face1] / solution[face2]));
                yield return list.PickRandom(rnd);
                //return list;
            }

            var minValue = solution.Min();
            var maxValue = solution.Max();
            var maxLength = Math.Max(minValue.ToString().Length, maxValue.ToString().Length);
            ConsoleUtil.WriteLine($"SOLTN: {solution.Select(i => i.ToString().PadLeft(maxLength)).JoinString(", ").Color(ConsoleColor.Yellow)}", null);

            IEnumerable<int[]> recurse((int face1, int face2, EdgeClueType type, int value)[] clues, int[] sofar, bool[] used, int[][] possibleValues)
            {
                var bestFace = 0;
                var bestConstraint = int.MaxValue;
                var allUsed = true;
                for (var i = 0; i < n; i++)
                {
                    if (!used[i])
                    {
                        allUsed = false;
                        if (possibleValues[i].Length < bestConstraint)
                        {
                            bestConstraint = possibleValues[i].Length;
                            bestFace = i;
                        }
                    }
                }
                if (allUsed)
                {
                    yield return sofar;
                    yield break;
                }

                used[bestFace] = true;
                foreach (var value in possibleValues[bestFace])
                {
                    //for (var i = 0; i < n; i++)
                    //    if (i != bestFace && used[i] && sofar[i] == value)
                    //        goto busted;

                    sofar[bestFace] = value;
                    var newPossibleValues = (int[][]) possibleValues.Clone();
                    foreach (var clue in clues)
                    {
                        var (cFace1, cFace2, cType, cValue) = clue;
                        if (cFace1 == bestFace)
                        {
                        }
                        else if (cFace2 == bestFace)
                        {
                            var t = cFace1;
                            cFace1 = cFace2;
                            cFace2 = t;
                            if (cType == EdgeClueType.Rel)
                                cValue = -cValue;
                        }
                        else
                            continue;

                        switch (cType)
                        {
                            case EdgeClueType.Sum: newPossibleValues[cFace2] = newPossibleValues[cFace2].Where(i => i + value == cValue).ToArray(); break;
                            case EdgeClueType.Difference: newPossibleValues[cFace2] = newPossibleValues[cFace2].Where(i => Math.Abs(i - value) == cValue).ToArray(); break;
                            case EdgeClueType.Product: newPossibleValues[cFace2] = newPossibleValues[cFace2].Where(i => i * value == cValue).ToArray(); break;
                            case EdgeClueType.Quotient: newPossibleValues[cFace2] = newPossibleValues[cFace2].Where(i => i == cValue * value || value == cValue * i).ToArray(); break;
                            case EdgeClueType.Rel: newPossibleValues[cFace2] = newPossibleValues[cFace2].Where(i => Math.Sign(i - value) == cValue).ToArray(); break;
                        }
                        if (newPossibleValues[cFace2].Length == 0)
                            goto busted;
                    }

                    foreach (var sol in recurse(clues, sofar, used, newPossibleValues))
                        yield return sol;

                    busted:;
                }
                used[bestFace] = false;
            }

            var allClues = polyhedron.Faces
                .SelectMany((face, faceIx) => polyhedron.FindAdjacent(faceIx).Select(adjFaceIx => (face1: faceIx, face2: adjFaceIx)))
                .Where(tup => tup.face1 < tup.face2)
                .SelectMany(edge => generateClues(edge.face1, edge.face2))
                .ToArray()
                .Shuffle(rnd);
            Console.WriteLine("ALL CLUES:");
            Console.WriteLine(allClues.JoinString("\n"));
            Console.WriteLine();
            var requiredClues = Ut.ReduceRequiredSet(allClues, set =>
            {
                var setToTest = set.SetToTest.ToArray();
                return !recurse(setToTest, new int[n], new bool[n], Ut.NewArray(n, _ => Enumerable.Range(minValue, maxValue - minValue + 1).ToArray())).Skip(1).Any();
            }).ToArray();
            Console.WriteLine($"TRIMMED CLUES ({requiredClues.Length}):");
            Console.WriteLine(requiredClues.JoinString("\n"));
            Console.WriteLine();
            Console.WriteLine("DUPLICATES:");
            Console.WriteLine($"{requiredClues.UniquePairs().Where(pair => (pair.Item1.face1, pair.Item1.face2) == (pair.Item2.face1, pair.Item2.face2) || (pair.Item1.face1, pair.Item1.face2) == (pair.Item2.face2, pair.Item2.face1)).JoinString("\n")}");
        }

        enum VertexClueType
        {
            Sum,
            Product,
            NumberOfEvens,
        }

        public static void PolyhedralPuzzle_GenerateVertexClues()
        {
            //var polyhedra = PolyhedralPuzzle_GetPolyhedra();
            //foreach (var p in polyhedra)
            //    if (p.Faces.Length == 28)
            //        Console.WriteLine($"{p.Name} = {p.Filename}");
            //return;

            var polyhedron = PolyhedralPuzzle_Parse(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Polyhedral Puzzle\Txt\SelfDualIcosioctahedron2.txt");
            Console.WriteLine(polyhedron.Faces.Length);
            var n = polyhedron.Faces.Length;

            var rnd = new Random(4);
            var solution = Enumerable.Range(1, 26).Concat(new[] { 2, 9 }).ToArray().Shuffle(rnd);
            if (solution.Length != n)
                Debugger.Break();

            IEnumerable<(int vx, VertexClueType type, int value)> generateClues(int vx)
            {
                var list = new List<(int vx, VertexClueType type, int value)>();
                list.Add((vx, VertexClueType.Sum, Enumerable.Range(0, n).Where(fx => polyhedron.Faces[fx].Contains(vx)).Sum(fx => solution[fx])));
                //list.Add((vx, VertexClueType.Product, Enumerable.Range(0, n).Where(fx => polyhedron.Faces[fx].Contains(vx)).Aggregate(1, (prev, fx) => prev * solution[fx])));
                //list.Add((vx, VertexClueType.NumberOfEvens, Enumerable.Range(0, n).Where(fx => polyhedron.Faces[fx].Contains(vx)).Count(fx => solution[fx] % 2 == 0)));
                yield return list.PickRandom(rnd);
                //return list;
            }

            var minValue = solution.Min();
            var maxValue = solution.Max();
            var maxLength = Math.Max(minValue.ToString().Length, maxValue.ToString().Length);
            ConsoleUtil.WriteLine($"SOLTN: {solution.Select(i => i.ToString().PadLeft(maxLength)).JoinString(", ").Color(ConsoleColor.Yellow)}", null);

            IEnumerable<int[]> recurse((int vx, VertexClueType type, int value)[] clues, int[] sofar, bool[] used, int[][] possibleValues)
            {
                var bestFace = -1;
                var bestFaceScore = -1;
                for (var i = 0; i < n; i++)
                {
                    if (used[i])
                        continue;
                    var numClues = clues.Count(c => polyhedron.Faces[i].Contains(c.vx));
                    var score = (maxValue - minValue + 1) - possibleValues[i].Length + 2 * numClues;
                    if (score > bestFaceScore)
                    {
                        bestFaceScore = score;
                        bestFace = i;
                    }
                }
                if (bestFace == -1)
                {
                    yield return sofar;
                    yield break;
                }

                if (possibleValues[bestFace].Length == 0)
                    yield break;
                used[bestFace] = true;
                foreach (var value in possibleValues[bestFace])
                {
                    //for (var i = 0; i < n; i++)
                    //    if (i != bestFace && used[i] && sofar[i] == value)
                    //        goto busted;

                    sofar[bestFace] = value;
                    var newPossibleValues = (int[][]) possibleValues.Clone();
                    newPossibleValues[bestFace] = new[] { value };
                    foreach (var (cVx, cType, cValue) in clues)
                    {
                        if (!polyhedron.Faces[bestFace].Contains(cVx))
                            continue;
                        var faces = Enumerable.Range(0, n).Where(fx => polyhedron.Faces[fx].Contains(cVx)).ToArray();
                        var unusedFaces = faces.Where(f => !used[f]).ToArray();

                        switch (cType)
                        {
                            case VertexClueType.Sum:
                                if (unusedFaces.Length == 0 && faces.Sum(fx => sofar[fx]) != cValue)
                                    goto busted;
                                else if (unusedFaces.Length == 1)
                                {
                                    var required = cValue - faces.Sum(fx => used[fx] ? sofar[fx] : 0);
                                    if (!newPossibleValues[unusedFaces[0]].Contains(required))
                                        goto busted;
                                    newPossibleValues[unusedFaces[0]] = new[] { required };
                                }
                                else if (unusedFaces.Length == 2)
                                {
                                    var required = cValue - faces.Sum(fx => used[fx] ? sofar[fx] : 0);
                                    newPossibleValues[unusedFaces[0]] = newPossibleValues[unusedFaces[0]].Where(v => newPossibleValues[unusedFaces[1]].Any(v2 => v + v2 == required)).ToArray();
                                    newPossibleValues[unusedFaces[1]] = newPossibleValues[unusedFaces[1]].Where(v => newPossibleValues[unusedFaces[0]].Any(v2 => v + v2 == required)).ToArray();
                                    if (newPossibleValues[unusedFaces[0]].Length == 0 || newPossibleValues[unusedFaces[1]].Length == 0)
                                        goto busted;
                                }
                                // Check the smallest and largest possible sums
                                else
                                {
                                    foreach (var unusedFace in unusedFaces)
                                    {
                                        var min = cValue - faces.Sum(fx => used[fx] ? sofar[fx] : fx != unusedFace ? newPossibleValues[fx].Max() : 0);
                                        var max = cValue - faces.Sum(fx => used[fx] ? sofar[fx] : fx != unusedFace ? newPossibleValues[fx].Min() : 0);
                                        newPossibleValues[unusedFace] = newPossibleValues[unusedFace].Where(v => v >= min && v <= max).ToArray();
                                        if (newPossibleValues[unusedFace].Length == 0)
                                            goto busted;
                                    }
                                }

                                break;

                            case VertexClueType.Product:
                                var productSoFar = faces.Aggregate(1, (prev, fx) => used[fx] ? sofar[fx] * prev : prev);
                                if (cValue % productSoFar != 0)
                                    goto busted;
                                if (unusedFaces.Length == 1)
                                {
                                    var required = cValue / productSoFar;
                                    if (!newPossibleValues[unusedFaces[0]].Contains(required))
                                        goto busted;
                                    newPossibleValues[unusedFaces[0]] = new[] { required };
                                }
                                // Check the smallest and largest possible products
                                else if (faces.Aggregate(1, (prev, fx) => prev * possibleValues[fx].Min()) > cValue || faces.Aggregate(1, (prev, fx) => prev * possibleValues[fx].Max()) < cValue)
                                    goto busted;

                                break;

                            case VertexClueType.NumberOfEvens:
                                var usedEvenCount = faces.Count(fx => used[fx] && sofar[fx] % 2 == 0);
                                if (usedEvenCount > cValue)
                                    goto busted;
                                else if (usedEvenCount == cValue)
                                    foreach (var uf in unusedFaces)
                                    {
                                        newPossibleValues[uf] = newPossibleValues[uf].Where(v => v % 2 != 0).ToArray();
                                        if (newPossibleValues[uf].Length == 0)
                                            goto busted;
                                    }
                                else if (usedEvenCount == cValue - unusedFaces.Length)
                                    foreach (var uf in unusedFaces)
                                    {
                                        newPossibleValues[uf] = newPossibleValues[uf].Where(v => v % 2 == 0).ToArray();
                                        if (newPossibleValues[uf].Length == 0)
                                            goto busted;
                                    }
                                else if (usedEvenCount < cValue - unusedFaces.Length)
                                    goto busted;
                                break;
                        }
                    }

                    foreach (var sol in recurse(clues, sofar, used, newPossibleValues))
                        yield return sol;

                    busted:;
                }
                used[bestFace] = false;
            }

            var allClues = Enumerable.Range(0, polyhedron.Vertices.Length)
                .SelectMany(vx => generateClues(vx))
                .ToArray()
                .Shuffle(rnd);
            //Console.WriteLine("ALL CLUES:");
            //Console.WriteLine(allClues.JoinString("\n"));
            //Console.WriteLine();
            var initialPossibleValues = Enumerable.Range(0, n).Select(fx =>
            {
                var values = Enumerable.Range(minValue, maxValue - minValue + 1);
                //foreach (var (vx, type, value) in allClues)
                //{
                //    if (!polyhedron.Faces[fx].Contains(vx))
                //        continue;
                //    if (type == VertexClueType.Sum)
                //        values = values.Where(v => v <= value);
                //    if (type == VertexClueType.NumberOfEvens && value == 0)
                //        values = values.Where(v => v % 2 != 0);
                //    else if (type == VertexClueType.NumberOfEvens && value == polyhedron.Faces.Count(f => f.Contains(vx)))
                //        values = values.Where(v => v % 2 == 0);
                //}
                return values.ToArray();
            }).ToArray();

            var count = 0;
            foreach (var sol in recurse(allClues, new int[n], new bool[n], initialPossibleValues))
            {
                ConsoleUtil.WriteLine($"FOUND: {sol.Select(i => i.ToString().PadLeft(maxLength)).JoinString(", ").Color(ConsoleColor.Yellow)}", null);
                count++;
                if (count > 1)
                    break;
            }
            if (count != 1)
                // Clues are ambiguous or impossible
                Debugger.Break();

            var requiredClues = Ut.ReduceRequiredSet(allClues, skipConsistencyTest: true, test: set =>
            {
                var arr = set.SetToTest.ToArray();
                Console.WriteLine(allClues.Select(c => arr.Contains(c) ? "█" : "░").JoinString());
                return !recurse(arr, new int[n], new bool[n], initialPossibleValues).Skip(1).Any();
            }).ToArray();
            Console.WriteLine($"TRIMMED CLUES ({requiredClues.Length}):");
            Console.WriteLine(requiredClues.JoinString("\n"));
            Console.WriteLine();
            Console.WriteLine("DUPLICATES:");
            Console.WriteLine($"{requiredClues.UniquePairs().Where(pair => pair.Item1.vx == pair.Item2.vx).JoinString("\n")}");
            Console.WriteLine();
            foreach (var sol in recurse(requiredClues, new int[n], new bool[n], initialPossibleValues))
                ConsoleUtil.WriteLine($"FOUND: {sol.Select(i => i.ToString().PadLeft(maxLength)).JoinString(", ").Color(ConsoleColor.Yellow)}", null);
        }

        public static void Circles_Generate()
        {
            var distances = @"ABCDEFGHI";   // for sorting
            var len = distances.Length;
            var radii = @"SAAHTSEIE";
            var angles = @"PRNOACFCN";
            var angleMult = 4;
            var tap = @"EMRSTOFIT";     // = SPEARMANRHOSTATSCOEFFICIENT (= CORRELATION)

            var rnd = new Random(1);

            if (tap.Length != len || angles.Length != len || distances.Length != len || radii.Length != len)
                Debugger.Break();

            var cumulativeAngles = Enumerable.Range(0, angles.Length).Select(aIx => angles.Take(aIx).Select(a => a - 'A' + 1).Sum() * angleMult).ToArray();
            var points = new List<(PointD pt, int th)>();
            var centers = new List<PointD>();

            var offsets = new int?[]
            {
                120,
                -100,
                30,
                50,
                10,
                0,
                0,
                40,
                -90
            };
            if (offsets.Length != len)
                Debugger.Break();
            var highlight = offsets.IndexOf(value => value == null);

            for (int ix = 0; ix < len; ix++)
            {
                var (_, row, col) = TapCode.TapFromCh[tap[ix]];
                var dotAngles = Enumerable.Range(0, row).Select(r => 20 * r).ToList();
                dotAngles.AddRange(Enumerable.Range(0, col).Select(c => 20 * row + 60 + 20 * c));
                var center = (distances[ix] - 'A' + 1).Apply(d => (cumulativeAngles[ix] * Math.PI / 180).Apply(θ => new PointD(d * Math.Cos(θ), d * Math.Sin(θ))));
                centers.Add(center);
                var radius = radii[ix] - 'A' + 1;
                var offset = offsets[ix] ?? 0;
                foreach (var angle in dotAngles)
                    points.Add((new PointD(radius * Math.Cos((angle + offset) * Math.PI / 180), radius * Math.Sin((angle + offset) * Math.PI / 180)) + center, angle + offset));
            }

            for (int i = 0; i < points.Count; i++)
                points[i] = (new PointD(points[i].pt.X, -points[i].pt.Y), points[i].th);

            var minX = points.Min(p => p.pt.X);
            var maxX = points.Max(p => p.pt.X);
            var minY = points.Min(p => p.pt.Y);
            var maxY = points.Max(p => p.pt.Y);
            for (var step = 0; step < 3; step++)
            {
                File.WriteAllText($@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Circles\Circles{(step > 0 ? step.ToString() : "")}.html", $@"<!DOCTYPE html>
<html>
    <head>
        <title>Circles</title>
        <style>
            html, body {{ margin: 0; padding: 0; }}
        </style>
    </head>
    <body>
        <h1 style='text-align: center; border-bottom: 1px solid #ccc'>Circles</h1>
        <p style='text-align: center; font-style: italic'>Center to origin, widdershins.</p>
        <svg style='width: 99vw' viewBox='{minX - 2} {minY - 2} {maxX - minX + 4} {maxY - minY + 4}'>
            {points.Select(p => $"<rect x='{p.pt.X - .1}' y='{p.pt.Y - .1}' width='.2' height='.2' transform='rotate({-p.th} {p.pt.X} {p.pt.Y})' />").JoinString()}
            <g fill='none' stroke='black'>
                {(step > 0 ? Enumerable.Range(0, len).Select(ix => $"<circle cx='{centers[ix].X}' cy='{-centers[ix].Y}' r='{radii[ix] - 'A' + 1}' stroke='{(ix == highlight ? "#F00" : "#248")}' stroke-width='{(ix == highlight ? ".1" : ".02")}' />").JoinString() : "")}
                {(step > 1 ? Enumerable.Range(0, len).Select(ix => $"<line x1='{centers[ix].X}' y1='{-centers[ix].Y}' x2='0' y2='0' stroke='#822' stroke-width='.02' />").JoinString() : "")}
                <line x1='0' y1='{minY - 1}' x2='0' y2='{maxY + 1}' stroke-width='.05' />
                <line x1='{minX - 1}' y1='0' x2='{maxX + 1}' y2='0' stroke-width='.05' />
            </g>
            <path d='M .4 {minY - .5} h -.8 l .4 -1 z' />
            <path d='M {maxX + .5} .4 v -.8 l 1 .4 z' />
        </svg>
    </body>
</html>
");
            }
        }
    }
}
