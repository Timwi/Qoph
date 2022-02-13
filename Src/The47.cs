using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace Qoph
{
    static class The47
    {
        public static void Do()
        {
            const string cluephrase = "SATOSHINAKAMOTOCURRENCY";
            const int n = 8;

            var prefs = File.ReadAllLines(@"D:\c\Qoph\DataFiles\The 47\Prefectures.txt");
            //var words = File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt");
            var words = File.ReadAllLines(@"D:\Daten\Wordlists\peter_broda_wordlist_unscored.txt")
                //.Except(File.ReadLines(@"D:\Daten\Wordlists\English 60000.txt"))
                .ToArray();

            var mod47Inverses = new int[47];
            for (var row = 0; row < 47; row++)
                for (var col = 0; col < 47; col++)
                    if ((row * col) % 47 == 1)
                        mod47Inverses[row] = col;

            var wordRnd = new Random(47);
            var wordsStartingWith = words.Where(w => w.Length == 8 && w.All(ch => ch >= 'A' && ch <= 'Z')).GroupBy(w => w[0]).ToDictionary(gr => gr.Key, gr => gr.Distinct().Order().ToArray());

            var allTuples = (
                from aWord in new[] { "ANTERIOR" }  //wordsStartingWith['A']
                from bWord in new[] { "BUSINESS" }   //wordsStartingWith['B']
                from cWord in new[] { "CAREBEAR" }
                from dWord in new[] { "DIAGNOSE" }
                from eWord in new[] { "EMPHATIC" }  //wordsStartingWith['E'].Where(w => w.Distinct().Count() == 8 && !w.Contains('N') && !w.Contains('V') && !w.Contains('W'))
                from fWord in new[] { "FRONTIER" }
                from gWord in new[] { "GANYMEDE" }
                from hWord in new[] { "HIROLLER" }
                select new[] { aWord, bWord, cWord, dWord, eWord, fWord, gWord, hWord }).ToArray();

            Console.WriteLine(allTuples.Length);
            allTuples.Shuffle(wordRnd);

            int[] matrixInverse(int[] matrix, int size)
            {
                var w = 2 * size;   // width of a row in the augmented matrix
                var augmented = Ut.NewArray(w * size, ix => ix % w < size ? matrix[ix % w + ix / w * size] : ix % w - size == ix / w ? 1 : 0);

                // Since we’re going kind of diagonal, ‘rc’ may refer to a row or a column, but we’re really processing one column at a time.
                // Each iteration of this loop turns column ‘rc’ into the corresponding column of an identity matrix.
                for (var rc = 0; rc < size; rc++)
                {
                    // Turn matrix[rc, rc] into 1 by multiplying the row by its inverse.
                    // If this coefficient is currently 0, find a later row that we can swap this one with.
                    // If there is no other such row, the matrix is not invertible.
                    if (augmented[rc + w * rc] == 0)
                    {
                        var otherRowIx = Enumerable.Range(rc + 1, size - 1 - rc).FirstOrDefault(r => augmented[rc + w * r] != 0, -1);
                        if (otherRowIx == -1)
                            throw new InvalidOperationException(@"This matrix is not invertible.");
                        var otherRow = augmented.Subarray(otherRowIx * w, w);
                        Array.Copy(augmented, rc * w, augmented, otherRowIx * w, w);
                        Array.Copy(otherRow, 0, augmented, rc * w, w);
                    }

                    var inv = mod47Inverses[augmented[rc + w * rc]];
                    for (var i = 0; i < w; i++) // could start at ‘rc’ for efficiency, but this provides a check that the algorithm is correct
                        augmented[i + w * rc] = (augmented[i + w * rc] * inv) % 47;

                    for (var row = 0; row < size; row++)
                    {
                        if (row == rc)
                            continue;

                        // Need to turn this index into 0 by subtracting a multiple of row ‘rc’ (where the relevant index is now 1)
                        var mult = augmented[rc + w * row];
                        for (var i = 0; i < w; i++)
                            augmented[i + w * row] = ((augmented[i + w * row] - mult * augmented[i + w * rc]) % 47 + 47) % 47;
                    }
                }

                return Ut.NewArray(size * size, ix => augmented[size + (ix % size) + w * (ix / size)]);
            }

            var lockObject = new object();
            var bestScore = 0;
            (int[] input, int[] output)[] bestResults = null;

            foreach (var feedersIx in Enumerable.Range(0, allTuples.Length))
            //Enumerable.Range(0, allTuples.Length).ParallelForEach(Environment.ProcessorCount, feedersIx =>
            {
                var feeders = allTuples[feedersIx];
                var feederMatrix = Ut.NewArray(64, i => feeders[i / 8][i % 8] - 'A' + 1);
                int[] inv;
                try
                {
                    inv = matrixInverse(feederMatrix, 8);
                }
                catch (InvalidOperationException)
                {
                    goto busted;
                }

                lock (lockObject)
                    ConsoleUtil.Write($"Trying: {feeders.JoinString(", ")} ({feedersIx})   \r".Color(ConsoleColor.Yellow));

                var chsPerFullRow = (cluephrase.Length + 7) / 8;
                var chsPerSmallRow = cluephrase.Length - chsPerFullRow * 7;

                IEnumerable<(int[] input, int[] output)> testRow(int rowUnderTest, string cluephraseSubstring)
                {
                    foreach (var subseqL in Enumerable.Range(0, n).Subsequences(minLength: cluephraseSubstring.Length, maxLength: cluephraseSubstring.Length).ToArray().Shuffle())
                    {
                        const int maxIndex = 9;
                        var subseq = subseqL.ToArray();
                        var pow = 1;
                        var prefPosses = new List<(int i, char ch)>();
                        for (var i = 0; i < cluephraseSubstring.Length; i++)
                        {
                            prefPosses.Add((subseq[i], cluephraseSubstring[i]));
                            pow *= maxIndex;
                        }

                        foreach (var orderRaw in Enumerable.Range(0, pow).ToArray().Shuffle())
                        {
                            var order = orderRaw;
                            var input = new (int value, char ch)[n];
                            foreach (var (i, ch) in prefPosses)
                            {
                                input[i] = (value: order % maxIndex + 1, ch);
                                order /= maxIndex;
                            }

                            var output = Ut.NewArray(n, x => Enumerable.Range(0, n).Select(j => inv[8 * j + x] * input[j].value).Sum() % 47);
                            if (Enumerable.Range(0, n).All(x => output[x] != 0 && (input[x].value == 0 || (input[x].value <= prefs[(output[x] + 46) % 47].Length && prefs[(output[x] + 46) % 47][input[x].value - 1] == input[x].ch))))
                                yield return (input.Select(tup => tup.value).ToArray(), output);
                        }
                    }
                }

                //for (var smallRowCandidate = 0; smallRowCandidate < n; smallRowCandidate++)
                Enumerable.Range(0, n).ParallelForEach(Environment.ProcessorCount, smallRowCandidate =>
                {
                    var rowResults = Enumerable.Range(0, n).Select(rowUnderTest => testRow(rowUnderTest,
                            rowUnderTest == smallRowCandidate ? cluephrase.Substring(chsPerFullRow * rowUnderTest, chsPerSmallRow) :
                            rowUnderTest < smallRowCandidate ? cluephrase.Substring(chsPerFullRow * rowUnderTest, chsPerFullRow) :
                            cluephrase.Substring(chsPerFullRow * (rowUnderTest - 1) + chsPerSmallRow, chsPerFullRow)).ToArray()).ToArray();
                    if (rowResults.Any(ar => ar.Length == 0))
                        return;

                    // QUICK FIND
                    //lock (lockObject)
                    //{
                    //    ConsoleUtil.WriteLine($"Found: {feeders.JoinString(", ")} ({feedersIx})   \r".Color(ConsoleColor.Green));
                    //    goto busted;
                    //}

                    var rowOrder = Enumerable.Range(0, n).OrderBy(row => rowResults[row].Length).ToArray();

                    IEnumerable<(int[] input, int[] output)[]> recurse((int[] input, int[] output)[] sofar, int rowOrderIx)
                    {
                        if (rowOrderIx == n)
                        {
                            yield return sofar.ToArray();
                            yield break;
                        }

                        var numbersStillLeft = Enumerable.Range(0, 47).Except(rowOrder.Take(rowOrderIx).SelectMany(row => sofar[row].output)).ToArray();
                        foreach (var rowTuple in rowResults[rowOrder[rowOrderIx]].OrderByDescending(row => row.output.Intersect(numbersStillLeft).Count()))
                        {
                            sofar[rowOrder[rowOrderIx]] = rowTuple;
                            foreach (var result in recurse(sofar, rowOrderIx + 1))
                                yield return result;
                        }
                    }

                    var arrangement = recurse(new (int[] input, int[] output)[n], 0).First();
                    var numbersUsed = arrangement.SelectMany(ar => ar.output).Distinct().Count();
                    lock (lockObject)
                    {
                        if (numbersUsed > bestScore)
                        {
                            var ccOutput = new TextTable { ColumnSpacing = 2, RowSpacing = 1 };
                            for (var row = 0; row < n; row++)
                                for (var x = 0; x < n; x++)
                                    ccOutput.SetCell(x, row, "{0/Green}\n{1/Cyan}\n{2/Magenta}\n{3/Yellow}".Color(null)
                                        .Fmt(arrangement[row].output[x], prefs[(arrangement[row].output[x] + 46) % 47], arrangement[row].input[x], arrangement[row].input[x] == 0 ? "" : prefs[(arrangement[row].output[x] + 46) % 47][arrangement[row].input[x] - 1].ToString()));

                            Console.Clear();
                            ccOutput.WriteToConsole();
                            Console.WriteLine();
                            for (var row = 0; row < n; row++)
                                Console.WriteLine(arrangement[row].output.JoinString(" "));
                            Console.WriteLine();
                            Console.WriteLine($"Numbers used: {numbersUsed}");
                            bestScore = numbersUsed;
                            bestResults = arrangement;
                        }
                    }
                });

                busted:;
            }

            Clipboard.SetText(bestResults.Select(row => row.output.JoinString("\t")).JoinString("\n"));
        }

        public static void FindHillCipher()
        {
            // This code requires a list of A/E combinations from The47.Do

            //var list = File.ReadAllLines(@"D:\temp\temp.txt");
            //foreach (var line in list)
            //{
            //    var str = line + "BUSINESS CAREBEAR DIAGNOSE FRONTIER GANYMEDE HIROLLER";

            //    var chs = "HILLCIPHER";
            //    for (var i = 0; i < chs.Length; i++)
            //    {
            //        var p = str.IndexOf(chs[i]);
            //        if (p == -1)
            //            goto busted;
            //        str = str.Substring(0, p) + str.Substring(p + 1);
            //    }

            //    if (SomethingsFishy.CanFit(line.Substring(11, 8)))
            //        Console.WriteLine(line);

            //    busted:;
            //}
        }

        public static void GenerateHtml()
        {
            var grid = new[] { 5, 36, 13, 19, 14, 42, 37, 15, 10, 29, 6, 20, 43, 44, 27, 25, 12, 7, 46, 17, 31, 20, 45, 15, 34, 30, 20, 24, 21, 7, 43, 42, 28, 11, 22, 1, 22, 33, 36, 9, 27, 37, 2, 28, 23, 12, 29, 18, 22, 41, 45, 40, 34, 8, 3, 26, 32, 9, 43, 39, 1, 6, 16, 22 };
            Console.WriteLine(grid.Distinct().Order().JoinString(", "));
            Console.WriteLine(grid.Distinct().Count());
            var prefs = File.ReadAllLines(@"D:\c\Qoph\DataFiles\The 47\Prefectures.txt");
            General.ReplaceInFile(@"D:\c\Qoph\EnigmorionFiles\Puzzles\the-47.html", "<!--%%-->", "<!--%%%-->",
                Enumerable.Range(0, 8).Select((row, rowIx) => $@"<tr>{(rowIx==0?"<td rowspan='8' class='left bracket'></td>":"")}{Enumerable.Range(0, 8).Select(col =>
                    $"<td><img class='flag' src='data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes($@"D:\c\Qoph\DataFiles\The 47\Flags\{prefs[(grid[col + 8 * row] + 46) % 47]} prefecture.png"))}'/></td>").JoinString()}{(rowIx == 0 ? "<td rowspan='8' class='right bracket'></td>" : "")}</tr>").JoinString("\n"));
        }
    }
}
