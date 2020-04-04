using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace PuzzleStuff.BombDisposal
{
    static class FortySeven
    {
        public static void Do()
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

            var wordRnd = new Random();
            var wordsStartingWith = words.Where(w => w.Length == 8 && w.All(ch => ch >= 'A' && ch <= 'Z')).GroupBy(w => w[0]).ToDictionary(gr => gr.Key, gr => gr.Distinct().Order().ToArray());

            //var debug_words = Ut.NewArray("AMPHIBIA", "BETATEST", "CAREBEAR", "DRUMHEAD", "ELAURIAN", "FEELINGS", "GANYMEDE", "HIROLLER");
            //var debug_matrix = Ut.NewArray(64, i => debug_words[i / 8][i % 8] - 'A' + 1);
            //var debug_inverse = newInverse(debug_matrix, 8);
            //Console.WriteLine(debug_inverse.Split(8).Select(row => row.Select(val => $"{val,4}").JoinString("")).JoinString("\n"));

            var allTuples = (
                from aWord in new[] { "AMPHIBIA" }//wordsStartingWith['A'].ToArray().Shuffle().Take(100)
                from bWord in new[] { "BETATEST" }   //wordsStartingWith['B']
                from cWord in new[] { "CAREBEAR" }   //wordsStartingWith['C']
                from dWord in wordsStartingWith['D'].ToArray().Shuffle()
                from eWord in new[] { "ELEPHANT" }  //wordsStartingWith['E'].ToArray().Shuffle().Take(100)
                from fWord in new[] { "FRONTIER" }  //wordsStartingWith['F'].ToArray().Shuffle().Take(100)
                from gWord in new[] { "GANYMEDE" }  //wordsStartingWith['G']
                from hWord in new[] { "HIROLLER" }  //wordsStartingWith['H'].ToArray().Shuffle().Take(100)
                select new[] { aWord, bWord, cWord, dWord, eWord, fWord, gWord, hWord }).ToArray();

            Console.WriteLine(allTuples.Length);
            allTuples.Shuffle();

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

            Enumerable.Range(0, allTuples.Length).ParallelForEach(2, feedersIx =>
            //foreach (var feedersIx in Enumerable.Range(0, allTuples.Length))
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

                lock (wordsStartingWith)
                    ConsoleUtil.Write($"Trying: {feeders.JoinString(", ")} ({feedersIx})   \r".Color(ConsoleColor.Yellow));

                var chsPerRow = (cluephrase.Length + 7) / 8;
                var ccOutput = new TextTable { ColumnSpacing = 2, RowSpacing = 1 };
                var clipboardText = new StringBuilder();
                var outputNumbers = new int[n][];

                for (var rowUnderTest = 0; rowUnderTest < n; rowUnderTest++)
                {
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

                            var output = Ut.NewArray(n, x => Enumerable.Range(0, n).Select(j => inv[8 * j + x] * input[j].value).Sum() % 47);
                            if (Enumerable.Range(0, n).All(x => output[x] != 0 && (input[x].value == 0 || (input[x].value <= prefs[(output[x] + 46) % 47].Length && prefs[(output[x] + 46) % 47][input[x].value - 1] == input[x].ch))))
                            {
                                for (var x = 0; x < n; x++)
                                    ccOutput.SetCell(x, rowUnderTest, "{0/Green}\n{1/Cyan}\n{2/Magenta}\n{3/Yellow}".Color(null)
                                        .Fmt(output[x], prefs[(output[x] + 46) % 47], input[x].value, input[x].value == 0 ? "" : prefs[(output[x] + 46) % 47][input[x].value - 1].ToString()));
                                clipboardText.AppendLine($"{output.JoinString("\t")}\t\t{output.Select(pIx => prefs[pIx - 1]).JoinString("\t")}");
                                outputNumbers[rowUnderTest] = output;
                                goto next;
                            }
                        }
                    }

                    goto busted;

                    next:;
                }

                lock (wordsStartingWith)
                {
                    ConsoleUtil.WriteLine($"Found: {feeders.JoinString(", ")}        ".Color(ConsoleColor.Green));
                    //for (var i = 0; i < n; i++)
                    //    Console.WriteLine(outputNumbers[i].JoinString(" "));
                    //Clipboard.SetText(clipboardText.ToString());
                    //ccOutput.WriteToConsole();
                    //Console.WriteLine();
                    //Debugger.Break();
                }
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
