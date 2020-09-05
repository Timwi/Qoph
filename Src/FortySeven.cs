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
    static class FortySeven
    {
        public static void Do()
        {
            const string cluephrase = "SATOSHINAKAMOTOCURRENCY";
            const int n = 8;

            var prefs = File.ReadAllLines(@"D:\c\Qoph\DataFiles\47\Prefectures.txt");
            var words = File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt");
            //var words = File.ReadAllLines(@"D:\Daten\Wordlists\peter_broda_wordlist_unscored.txt").Except(File.ReadLines(@"D:\Daten\Wordlists\English 60000.txt")).ToArray();

            var mod47Inverses = new int[47];
            var tt = new TextTable { ColumnSpacing = 1 };
            for (var row = 0; row < 47; row++)
                for (var col = 0; col < 47; col++)
                    if ((row * col) % 47 == 1)
                        mod47Inverses[row] = col;

            var wordRnd = new Random(47);
            var wordsStartingWith = words.Where(w => w.Length == 8 && w.All(ch => ch >= 'A' && ch <= 'Z')).GroupBy(w => w[0]).ToDictionary(gr => gr.Key, gr => gr.Distinct().Order().ToArray());

            var allTuples = (
                from aWord in new[] { "AMPHIBIA" }//wordsStartingWith['A'].ToArray().Shuffle(wordRnd).Take(100)
                from bWord in new[] { "BETATEST" }   //wordsStartingWith['B']
                from cWord in new[] { "CAREBEAR" }   //wordsStartingWith['C']
                from dWord in new[] { "DIAGNOSE" }
                from eWord in new[] { "EARMOLDS" }   //wordsStartingWith['E']
                from fWord in new[] { "FRONTIER" }  //wordsStartingWith['F'].ToArray().Shuffle(wordRnd).Take(100)
                from gWord in new[] { "GANYMEDE" }  //wordsStartingWith['G']
                from hWord in new[] { "HIROLLER" }  //wordsStartingWith['H'].ToArray().Shuffle(wordRnd).Take(100)
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
            int[][] bestOutputs = null;
            int[][] bestInputs = null;

            foreach (var feedersIx in Enumerable.Range(0, allTuples.Length))
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
                    continue;
                }

                lock (wordsStartingWith)
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

                Enumerable.Range(0, n).ParallelForEach(smallRowCandidate =>
                {
                    IEnumerable<(int[][] inputs, int[][] outputs)> recurse(int[][] sofarInput, int[][] sofarOutput, int rowUnderTest)
                    {
                        if (rowUnderTest == n)
                        {
                            yield return (sofarInput.ToArray(), sofarOutput.ToArray());
                            yield break;
                        }

                        var cluephraseSubstring =
                            rowUnderTest == smallRowCandidate ? cluephrase.Substring(chsPerFullRow * rowUnderTest, chsPerSmallRow) :
                            rowUnderTest < smallRowCandidate ? cluephrase.Substring(chsPerFullRow * rowUnderTest, chsPerFullRow) :
                            cluephrase.Substring(chsPerFullRow * (rowUnderTest - 1) + chsPerSmallRow, chsPerFullRow);

                        foreach (var (input, output) in testRow(rowUnderTest, cluephraseSubstring))
                        {
                            sofarInput[rowUnderTest] = input;
                            sofarOutput[rowUnderTest] = output;
                            foreach (var result in recurse(sofarInput, sofarOutput, rowUnderTest + 1))
                                yield return result;
                        }
                    }

                    foreach (var (inputs, outputs) in recurse(new int[n][], new int[n][], 0))
                    {
                        var numbersUsed = outputs.SelectMany(ar => ar).Distinct().Count();
                        lock (lockObject)
                            if (numbersUsed > bestScore)
                            {
                                var ccOutput = new TextTable { ColumnSpacing = 2, RowSpacing = 1 };
                                for (var row = 0; row < n; row++)
                                    for (var x = 0; x < n; x++)
                                        ccOutput.SetCell(x, row, "{0/Green}\n{1/Cyan}\n{2/Magenta}\n{3/Yellow}".Color(null)
                                            .Fmt(outputs[row][x], prefs[(outputs[row][x] + 46) % 47], inputs[row][x], inputs[row][x] == 0 ? "" : prefs[(outputs[row][x] + 46) % 47][inputs[row][x] - 1].ToString()));

                                Console.Clear();
                                ccOutput.WriteToConsole();
                                Console.WriteLine();
                                for (var row = 0; row < n; row++)
                                    Console.WriteLine(outputs[row].JoinString(" "));
                                Console.WriteLine();
                                Console.WriteLine($"Numbers used: {numbersUsed}");
                                bestScore = numbersUsed;
                                bestOutputs = outputs;
                                bestInputs = inputs;
                            }
                    }
                });
            }

            Clipboard.SetText(bestOutputs.Select(row => row.JoinString("\t")).JoinString("\n"));
        }

        public static void Test()
        {
            // Vector multiplication
            int[] vecmult(int[] m, int[] v, int size) => Ut.NewArray(size, i => (Enumerable.Range(0, size).Select(x => ((m[x + size * i] * v[x]) % 47 + 47) % 47).Sum() % 47 + 47) % 47);
            var prefs = File.ReadAllLines(@"D:\c\Qoph\DataFiles\47\Prefectures.txt");

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
