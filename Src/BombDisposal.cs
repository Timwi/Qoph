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
using RT.Util;
using RT.Util.Consoles;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace PuzzleStuff
{
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

        public static void GenerateSolutionCandidates_V1()
        {
            // V1: it alternates between first and second part of the episode title

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

        public static void GenerateSolutionCandidates_V2()
        {
            // V2: use only the first part of the episode titles

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

        public static void FortySeven()
        {
            // Puzzle idea involving the 47 prefectures and Hill Cipher (but not the ronin)

            var prefs = File.ReadAllLines(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\47\Prefectures.txt");
            const string cluephrase2 = "SATOSHINAKAMOTOCURRENCY";
            var cluephrase = "INDEX,BY BLUE,CHANNEL,MOD TEN,WHERE,IT IS,NOT ZERO".Split(',');

            var words = File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt");
            var wordRnd = new Random(47);

            //for (int i = 0; i < 7; i++)
            //    Console.WriteLine(words.Where(w => w.Length == 8 && w[0] - 'A' == i && w.All(ch => ch >= 'A' && ch <= 'Z')).ToList().Shuffle(wordRnd).Take(10).JoinString(", "));

            //var feeders = "ANYWHERE,BEDSHEET,COIFFURE,DEXTROSE,EXPLICIT,FILENAME,GRIDLOCK".Split(',');
            var feeders = Enumerable.Range(0, 7).Select(i => words.Where(w => w.Length == 8 && w[0] - 'A' == i && w.All(ch => ch >= 'A' && ch <= 'Z')).PickRandom(wordRnd)).ToArray();
            feeders[1] = "BETATEST";
            feeders[2] = "CHARMING";
            if (cluephrase.Length != feeders.Length)
                Debugger.Break();

            const int n = 8;
            var equations = new List<string>();
            int conv(string str, int ix) => ix < 0 || ix >= str.Length || str[ix] == ' ' ? 0 : str[ix] - 'A' + 1;
            for (int wordIx = 0; wordIx < feeders.Length; wordIx++)
                for (int clPhrIx = 0; clPhrIx < n; clPhrIx++)
                    equations.Add($"{Enumerable.Range(0, n).Select(fdrIx => $"{conv(feeders[wordIx], fdrIx)}*m_{clPhrIx + 1}_{fdrIx + 1}").JoinString(" + ")} = {conv(cluephrase[wordIx], clPhrIx)}");
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
            }
            if (anyInvalid)
                Debugger.Break();
            var inputParsed = input.Select(inp => Enumerable.Range(1, 9).Select(grIx => inp.Match.Groups[grIx].Success ? int.Parse(inp.Match.Groups[grIx].Value) : (int?) null).ToArray()).ToArray();
            var maxNn = inputParsed.Max(inp => inp[4] ?? inp[6].Value);

            var results = new int[n * n];
            var nns = new int[maxNn];

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

        public static void FourFocus_Find()
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
#SPADES,HEARTS,CLUBS,DIAMONDS
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

            ConsoleUtil.WriteParagraphs(set.Select(s => s.fragment).GroupBy(s => s[0]).OrderBy(gr => gr.Key).Select(gr => new ConsoleColoredString($"{(gr.Key + ":").Color(ConsoleColor.White)} {gr.Order().JoinString(", ").Color(ConsoleColor.DarkGreen)}")).JoinColoredString("\n"));
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
            var gridRaw = FourFocus_getGrid();
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

        private static (string valStr, int val, bool grp1)?[] FourFocus_getGrid() => @" 6 6 N M M11 J11 J N
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

        public static void FourFocus_ConstructGrid()
        {
            const int w = 11;
            const int h = 11;

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

        public static void FourFocus_GeneratePuzzle()
        {
            var grid = FourFocus_getGrid();
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
            File.WriteAllText(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Four Focus\Four Focus.html", $@"<!DOCTYPE html>
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
                var filename = "png,jpg,jpeg,bmp".Split(',').Select(ext => $@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Four Focus\{name}.{ext}").First(f => File.Exists(f));
                counts.IncSafe(valStr);
                using (var bmp = new Bitmap(filename))
                using (var mem = new MemoryStream())
                {
                    GraphicsUtil.DrawBitmap(targetWidth, targetHeight, g =>
                    {
                        g.DrawImage(bmp, GraphicsUtil.FitIntoMaintainAspectRatio(bmp.Size, new Rectangle(0, 0, targetWidth, targetHeight)));
                    }).Save(mem, ImageFormat.Png);
                    //return $@"<td><img src='data:image/{(filename.EndsWith("png") ? "png" : "jpeg")};base64,{Convert.ToBase64String(File.ReadAllBytes(filename))}' class='image' /></td>";
                    return $@"<td><img src='data:image/png;base64,{Convert.ToBase64String(mem.ToArray())}' class='image' /></td>";
                }
            }).JoinString()}</tr>").JoinString()}
        </table>
    </body>
</html>
");
        }
    }
}
