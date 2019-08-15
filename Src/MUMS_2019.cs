using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff
{
    static class MUMS_2019
    {
        public static void Noteworthy()
        {
            var fractions = @"7/16 1/16 7/32 1/32 3/32 5/32
5/8 3/8 1/16 3/16 7/4
3/4 5/16 5/16 1/8 1/1 1/2
7/8 1/8 1/16 9/64 3/64 5/4";
            foreach (Match m in Regex.Matches(fractions, @"(\d+)/(\d+)"))
            {
                var val = int.Parse(m.Groups[1].Value) * 64 / int.Parse(m.Groups[2].Value);
                Console.WriteLine($"{(char) ('A' + val - 1)} = {val}");
            }
        }

        public static void AStickySituation()
        {
            var knownWords = "ACID,COST,COUPE,DELAY,FADED,ORCS,PADDLE,PIPE,RICE,RODS,SANK,STACK,STICK,STONE,WINNER,XRAY".Split(',');
            var lensOfUnknownWords = new[] { 4, 5, 5, 5, 5 };

            var sticks = Ut.NewArray(
                new[] { 1, 3, 10, 7, 11 },
                new[] { 2, 3, 4, 5 },
                new[] { 5, 6, 7, 8, 9 },
                new[] { 6, 15, 16, 17, 14 },
                new[] { 9, 12, 13, 14 },
                new[] { 18, 19, 20, 21, 22, 23 },
                new[] { 19, 24, 28, 32 },
                new[] { 21, 25, 28, 31, 34 },
                new[] { 26, 29, 32, 35, 40 },
                new[] { 27, 30, 33, 38 },
                new[] { 36, 37, 38, 39, 40 },
                new[] { 41, 44, 47, 51, 54 },
                new[] { 42, 43, 44, 45 },
                new[] { 46, 47, 48, 49 },
                new[] { 45, 48, 52, 55 },
                new[] { 50, 51, 52, 53 },
                new[] { 56, 61, 62, 63, 64, 65 },
                new[] { 56, 57, 58, 59, 60 },
                new[] { 57, 70, 68, 71 },
                new[] { 66, 67, 63, 68, 69 },
                new[] { 67, 64, 72, 73, 0 });

            var wordLengths = knownWords.Select(kw => kw.Length).Concat(lensOfUnknownWords).Order().ToArray();
            var stickLengths = sticks.Select(stick => stick.Length).Order().ToArray();

            IEnumerable<char[]> recurse(char[] sofar, int[] wordsUsed, int[] unusedUnknownLens, int stickIx)
            {
                if (stickIx == sticks.Length)
                {
                    yield return sofar;
                    yield break;
                }

                for (int i = 0; i < knownWords.Length; i++)
                {
                    if (knownWords[i].Length == sticks[stickIx].Length && !wordsUsed.Contains(i) && Enumerable.Range(0, sticks[stickIx].Length).All(j =>
                    {
                        var ch = sofar[sticks[stickIx][j]];
                        return ch == '\0' || ch == '?' || ch == knownWords[i][j];
                    }))
                    {
                        var sofarCopy = (char[]) sofar.Clone();
                        for (int j = 0; j < sticks[stickIx].Length; j++)
                            sofarCopy[sticks[stickIx][j]] = knownWords[i][j];
                        var wordsUsedCopy = new int[wordsUsed.Length + 1];
                        Array.Copy(wordsUsed, wordsUsedCopy, wordsUsed.Length);
                        wordsUsedCopy[wordsUsed.Length] = i;
                        foreach (var solution in recurse(sofarCopy, wordsUsedCopy, unusedUnknownLens, stickIx + 1))
                            yield return solution;
                    }
                    if (knownWords[i].Length == sticks[stickIx].Length && !wordsUsed.Contains(i) && Enumerable.Range(0, sticks[stickIx].Length).All(j =>
                    {
                        var ch = sofar[sticks[stickIx][j]];
                        return ch == '\0' || ch == '?' || ch == knownWords[i][knownWords[i].Length - 1 - j];
                    }))
                    {
                        var sofarCopy = (char[]) sofar.Clone();
                        for (int j = 0; j < sticks[stickIx].Length; j++)
                            sofarCopy[sticks[stickIx][j]] = knownWords[i][knownWords[i].Length - 1 - j];
                        var wordsUsedCopy = new int[wordsUsed.Length + 1];
                        Array.Copy(wordsUsed, wordsUsedCopy, wordsUsed.Length);
                        wordsUsedCopy[wordsUsed.Length] = i;
                        foreach (var solution in recurse(sofarCopy, wordsUsedCopy, unusedUnknownLens, stickIx + 1))
                            yield return solution;
                    }
                }

                var p = unusedUnknownLens.IndexOf(sticks[stickIx].Length);
                if (p != -1)
                {
                    var sofarCopy = (char[]) sofar.Clone();
                    for (int j = 0; j < sticks[stickIx].Length; j++)
                        if (sofarCopy[sticks[stickIx][j]] == '\0')
                            sofarCopy[sticks[stickIx][j]] = '?';
                    foreach (var solution in recurse(sofarCopy, wordsUsed, unusedUnknownLens.Remove(p, 1), stickIx + 1))
                        yield return solution;
                }
            }

            var patterns = Ut.NewArray(sticks.Length, _ => new HashSet<string>());
            foreach (var solution in recurse(new char[74], new int[0], lensOfUnknownWords, 0))
            {
                ConsoleUtil.WriteLine(solution.JoinString().Color(ConsoleColor.White));
                for (int stickIx = 0; stickIx < sticks.Length; stickIx++)
                {
                    if (sticks[stickIx].Any(ix => solution[ix] == '?'))
                    {
                        patterns[stickIx].Add(sticks[stickIx].Select(ix => solution[ix]).JoinString());
                        File.WriteAllText(@"D:\temp\temp.txt", patterns.Where(p => p.Count > 0).Select(p => p.JoinString(", ")).JoinString("\n"));
                    }
                }
            }
        }
    }
}