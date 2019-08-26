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

        public static void CheckEpisodeWords()
        {
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
            feeders[1] = "BRACHIUM";
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

        public static void NoU()
        {
            // Potential puzzle idea involving the Berlin underground (U-Bahn)

            var lines = @"
WARSCHAUERSTRAßE,SCHLESISCHESTOR,GÖRLITZERBAHNHOF,KOTTBUSSERTOR,PRINZENSTRAßE,HALLESCHESTOR,MÖCKERNBRÜCKE,GLEISDREIECK,KURFÜRSTENSTRAßE,NOLLENDORFPLATZ,WITTENBERGPLATZ,KURFÜRSTENDAMM,UHLANDSTRAßE
PANKOW,VINETASTRAßE,SCHÖNHAUSERALLEE,EBERSWALDERSTRAßE,SENEFELDERPLATZ,ROSALUXEMBURGPLATZ,ALEXANDERPLATZ,KLOSTERSTRAßE,MÄRKISCHESMUSEUM,SPITTELMARKT,HAUSVOGTEIPLATZ,STADTMITTE,MOHRENSTRAßE,POTSDAMERPLATZ,MENDELSSOHNBARTHOLDYPARK,GLEISDREIECK,BÜLOWSTRAßE,NOLLENDORFPLATZ,WITTENBERGPLATZ,ZOOLOGISCHERGARTEN,ERNSTREUTERPLATZ,DEUTSCHEOPER,BISMARCKSTRAßE,SOPHIECHARLOTTEPLATZ,KAISERDAMM,THEODORHEUSSPLATZ,NEUWESTEND,OLYMPIASTADION,RUHLEBEN
WARSCHAUERSTRAßE,SCHLESISCHESTOR,GÖRLITZERBAHNHOF,KOTTBUSSERTOR,PRINZENSTRAßE,HALLESCHESTOR,MÖCKERNBRÜCKE,GLEISDREIECK,KURFÜRSTENSTRAßE,NOLLENDORFPLATZ,WITTENBERGPLATZ,AUGSBURGERSTRAßE,XXXXXXXXXXXXXXXXX,SPICHERNSTRAßE,HOHENZOLLERNPLATZ,FEHRBELLINERPLATZ,HEIDELBERGERPLATZ,RÜDESHEIMERPLATZ,BREITENBACHPLATZ,PODBIELSKIALLEE,DAHLEMDORF,FREIEUNIVERSITÄT,OSKARHELENEHEIM,ONKELTOMSHÜTTE,KRUMMELANKE
NOLLENDORFPLATZ,VIKTORIALUISEPLATZ,BAYERISCHERPLATZ,RATHAUSSCHÖNEBERG,INNSBRUCKERPLATZ
BERLINHAUPTBAHNHOF,BUNDESTAG,BRANDENBURGERTOR,UNTERDENLINDEN,FRANZÖSISCHESTRAßE,MUSEUMSINSEL,BERLINERRATHAUS,ALEXANDERPLATZ,SCHILLINGSTRAßE,STRAUSBERGERPLATZ,WEBERWIESE,FRANKFURTERTOR,SAMARITERSTRAßE,FRANKFURTERALLEE,MAGDALENENSTRAßE,LICHTENBERG,FRIEDRICHSFELDE,TIERPARK,BIESDORFSÜD,ELSTERWERDAERPLATZ,WUHLETAL,KAULSDORFNORD,KIENBERG,COTTBUSSERPLATZ,HELLERSDORF,LOUISLEWINSTRAßE,HÖNOW
ALTTEGEL,BORSIGWERKE,HOLZHAUSERSTRAßE,OTISSTRAßE,SCHARNWEBERSTRAßE,KURTSCHUMACHERPLATZ,AFRIKANISCHESTRAßE,REHBERGE,SEESTRAßE,LEOPOLDPLATZ,WEDDING,REINICKENDORFERSTRAßE,SCHWARTZKOPFFSTRAßE,NATURKUNDEMUSEUM,ORANIENBURGERTOR,FRIEDRICHSTRAßE,UNTERDENLINDEN,FRANZÖSISCHESTRAßE,STADTMITTE,KOCHSTRAßE,HALLESCHESTOR,LANDWEHRCANAL,MEHRINGDAMM,PLATZDERLUFTBRÜCKE,PARADESTRAßE,TEMPELHOF,ALTTEMPELHOF,KAISERINAUGUSTASTRAßE,ULLSTEINSTRAßE,WESTPHALWEG,ALTMARIENDORF
RATHAUSSPANDAU,ALTSTADTSPANDAU,ZITADELLE,HASELHORST,PAULSTERNSTRAßE,ROHRDAMM,SIEMENSDAMM,HALEMWEG,JAKOBKAISERPLATZ,JUNGFERNHEIDE,MIERENDORFFPLATZ,RICHARDWAGNERPLATZ,BISMARCKSTRAßE,WILMERSDORFERSTRAßE,ADENAUERPLATZ,KONSTANZERSTRAßE,FEHRBELLINERPLATZ,BLISSESTRAßE,BERLINERSTRAßE,BAYERISCHERPLATZ,EISENACHERSTRAßE,KLEISTPARK,YORCKSTRAßE,MÖCKERNBRÜCKE,MEHRINGDAMM,GNEISENAUSTRAßE,SÜDSTERN,HERMANNPLATZ,RATHAUSNEUKÖLLN,KARLMARXSTRAßE,NEUKÖLLN,GRENZALLEE,BLASCHKOALLEE,PARCHIMERALLEE,BRITZSÜD,JOHANNISTHALERCHAUSSEE,LIPSCHITZALLEE,WUTZKYALLEE,ZWICKAUERDAMM,RUDOW
WITTENAU,RATHAUSREINICKENDORF,KARLBONHOEFFERNERVENKLINIK,LINDAUERALLEE,PARACELSUSBAD,RESIDENZSTRAßE,FRANZNEUMANNPLATZ,OSLOERSTRAßE,PANKSTRAßE,GESUNDBRUNNEN,VOLTASTRAßE,BERNAUERSTRAßE,ROSENTHALERPLATZ,WEINMEISTERSTRAßE,ALEXANDERPLATZ,JANNOWITZBRÜCKE,HEINRICHHEINESTRAßE,MORITZPLATZ,KOTTBUSSERTOR,SCHÖNLEINSTRAßE,HERMANNPLATZ,BODDINSTRAßE,LEINESTRAßE,HERMANNSTRAßE
OSLOERSTRAßE,NAUENERPLATZ,LEOPOLDPLATZ,AMRUMERSTRAßE,WESTHAFEN,BIRKENSTRAßE,TURMSTRAßE,HANSAPLATZ,ZOOLOGISCHERGARTEN,KURFÜRSTENDAMM,SPICHERNSTRAßE,GÜNTZELSTRAßE,BERLINERSTRAßE,BUNDESPLATZ,FRIEDRICHWILHELMPLATZ,WALTHERSCHREIBERPLATZ,SCHLOßSTRAßE,RATHAUSSTEGLITZ
"
                .Replace("\r", "")
                .Split('\n')
                .Select(row => row.Split(','))
                .ToArray();

            var letterToStation = lines
                .SelectMany((line, lineIx) => line.Select(stop => (letter: stop.SubstringSafe(lineIx, 1), line: $"U{lineIx + 1}", stop)).Where(tup => tup.letter.Length > 0))
                .GroupBy(tup => tup.letter)
                .ToDictionary(gr => gr.Key[0], gr => gr.ToList());

            var intendedSolution = @"SCATTERING";

            for (int i = 0; i < intendedSolution.Length; i++)
            {
                var valid = letterToStation.ContainsKey(intendedSolution[i]);
                ConsoleUtil.WriteLine(intendedSolution[i].ToString().Color(valid ? ConsoleColor.Green : ConsoleColor.Red));
                if (valid)
                    foreach (var (letter, line, stop) in letterToStation[intendedSolution[i]])
                        ConsoleUtil.WriteLine($" — {line.Color(ConsoleColor.Yellow)} {stop.ColorSubstring(int.Parse(line.Substring(1)) - 1, 1, ConsoleColor.White, ConsoleColor.DarkBlue)}", null);
                Console.WriteLine();
            }
        }
    }
}