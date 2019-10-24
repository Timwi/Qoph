using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace PuzzleStuff.BombDisposal
{
    static class FortySeven
    {
        public static void FindMatches_OBSOLETE()
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

            foreach (var letter in "EROSION")
                Console.WriteLine($"{letter} = {matches[letter].Select(m => $"{m.pref} + {m.ronin}").JoinString(" / ")}");
        }

        public static void ConstructMatrix_OBSOLETE()
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

        public static void ConstructMatrix_V2()
        {
            const string cluephrase = "SATOSHINAKAMOTOCURRENCY";
            const int n = 8;

            var prefs = File.ReadAllLines(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\47\Prefectures.txt");
            //var words = File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt");
            var words = File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt");
            //File.ReadAllLines(@"D:\Daten\Wordlists\peter_broda_wordlist_unscored.txt").Except().ToArray();

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

            var wordRnd = new Random();
            var wordsStartingWith = words.Where(w => w.Length == 8 && w.All(ch => ch >= 'A' && ch <= 'Z')).GroupBy(w => w[0]).ToDictionary(gr => gr.Key, gr => gr.Distinct().Order().ToArray());

            foreach (var hWord in new[] { "HIJACKED", "HECKLING", /* "HACIENDA", "HAGGLING", "HALFBACK", "HANDBALL", "HANDBELL", "HANDBILL", "HANDLING", "HANGNAIL", "HATCHING", "HEADACHE", "HEADBAND", "HEADLAND", "HEADLINE", "HEGELIAN", "HEIGHTEN", "HELLENIC", "HIGHBALL", "HIGHTAIL", "HITCHING"*/ })
            {
                wordsStartingWith['E'].ParallelForEach(4, eWord =>
                {
                    var feeders = Ut.NewArray(
                        "APRICOTS",
                        "BETATEST",
                        "CHARMING",
                        "DIALOGUE",
                        eWord,
                        "FIGHTING",
                        "GANYMEDE",
                        hWord);

                    var feederMatrix = Ut.NewArray(64, i => feeders[i / 8][i % 8] - 'A' + 1);
                    int[] inv;
                    try
                    {
                        inv = inverse(feederMatrix, 8);
                    }
                    catch (InvalidOperationException)
                    {
                        return;
                    }

                    lock (wordsStartingWith)
                        ConsoleUtil.Write($"Trying: {feeders.JoinString(", ")}   \r".Color(ConsoleColor.Yellow));

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

                    lock (wordsStartingWith)
                        ConsoleUtil.WriteLine($"Found: {feeders.JoinString(", ")}        ".Color(ConsoleColor.Green));
                    //outputMatrix(feederMatrix, 8);
                    //Console.WriteLine();
                    //outputMatrix(inverse(feederMatrix, 8), 8);
                    //Console.WriteLine();

                    //foreach (var cc in ccOutput)
                    //    ConsoleUtil.WriteLine(cc);

                    // We have a row with no match :(
                    busted:;
                });
            }
        }

        public static void Test()
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
    }
}
