using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff
{
    static class PuzzleBoat
    {
        // “Bob the Dinosaur”
        public static void ThinMud()
        {
            var allowed = new[] { -5, -4, -3, -2, -1, 1, 2, 3, 4, 5 };
            foreach (var cmb in makeCombinations(3, -4, allowed))
                if (cmb[0] < 0 && cmb[2] < 0)
                    Console.WriteLine(cmb.JoinString(", "));
            Debugger.Break();

            var dataRaw = @"
                ##-15#-5#-7#9#15###-11#-7#2#0###-12#9#-15#
                #3.....#-11....#-7...
                #3.....15 9....#-4...
                #-6...#4.....6##-12...
                #-4...15 11......#0...
                #-8..10 9...12 -12...9 2...
                ##-3#3 9..#7..-15#7 12...-15#15#
                #5....9 10......5 -3..
                #7.....15 0....3 2...
                #3......9 -8.......
                #6...###9....8 11....
                #1...15#12 11.....12 -2...
                ####8#-11 12...11#-9#13 8..-15#8#-15#
                ##-14 9........11 6....
                #-3.....12 9........
                #2....#9.....#-11...
                #5....#13.....#-6...
                #-12...###15.....#-4...";
            const int w = 15;
            const int h = 18;

            var matches = Regex.Matches(dataRaw, @"\s*(#|\.|-?\d+)\s*");
            var clues = new (int? horizClue, int? vertClue)?[w * h];
            var arrIx = 0;
            var mIx = 0;
            while (mIx < matches.Count)
            {
                if (matches[mIx].Value.Trim() == ".")
                {
                    clues[arrIx++] = null;
                    mIx++;
                }
                else
                {
                    if (mIx == matches.Count - 1)
                        throw new InvalidOperationException();
                    static int? convert(string str) => str.Trim() == "#" ? (int?) null : int.TryParse(str.Trim(), out int value) ? value : throw new InvalidOperationException($"String not recognized: “{str}”.");
                    var vert = convert(matches[mIx].Value);
                    var horiz = convert(matches[mIx + 1].Value);
                    clues[arrIx++] = (horiz, vert);
                    mIx += 2;
                }
            }
            if (arrIx != w * h)
                Debugger.Break();

            var lights = new List<Light>();
            var fullBoard = Ut.NewArray(clues.Length, _ => new List<(Light light, int ix)>());
            for (var cIx = 0; cIx < clues.Length; cIx++)
            {
                if (clues[cIx] == null)
                    continue;
                if (clues[cIx].Value.horizClue is int hc)
                {
                    var len = Enumerable.Range(1, w - (cIx % w + 1)).TakeWhile(ix => clues[cIx + ix] == null).Count();
                    var light = new Light { Clue = hc, Combinations = makeCombinations(len, hc, allowed).ToList() };
                    lights.Add(light);
                    for (var i = 0; i < len; i++)
                        fullBoard[cIx + i + 1].Add((light, i));
                }
                if (clues[cIx].Value.vertClue is int vc)
                {
                    var len = Enumerable.Range(1, h - (cIx / w + 1)).TakeWhile(ix => clues[cIx + w * ix] == null).Count();
                    var light = new Light { Clue = vc, Combinations = makeCombinations(len, vc, allowed).ToList() };
                    lights.Add(light);
                    for (var i = 0; i < len; i++)
                        fullBoard[cIx + w * (i + 1)].Add((light, i));
                }
            }

            IEnumerable<int?[]> recurse(List<(Light light, int ix)>[] board, int?[] answers, int ix)
            {
                while (ix < w * h && clues[ix] != null)
                    ix++;

                if (ix == w * h)
                {
                    yield return answers;
                    yield break;
                }

                var origLightCombinations = board[ix].Select(tup => tup.light.Combinations).ToArray();
                foreach (var num in allowed)
                {
                    if (!board[ix].All(tup => tup.light.Combinations.Any(c => c[tup.ix] == num)))
                        continue;

                    // Attempt to put the number “num” into the square “bestSquare”
                    answers[ix] = num;

                    // Filter the combinations in each intersecting light
                    for (var i = 0; i < board[ix].Count; i++)
                        board[ix][i].light.Combinations = board[ix][i].light.Combinations.Where(c => c[board[ix][i].ix] == num).ToList();

                    foreach (var solution in recurse(board, answers, ix + 1))
                        yield return solution;

                    // Restore the combinations in the intersecting lights
                    for (var i = 0; i < board[ix].Count; i++)
                        board[ix][i].light.Combinations = origLightCombinations[i];
                }
                answers[ix] = null;
            }

            foreach (var solution in recurse(fullBoard, new int?[w * h], 0))
                Console.WriteLine(solution.JoinString(", "));
        }

        private static IEnumerable<int[]> makeCombinations(int len, int sum, int[] allowed)
        {
            if (len == 0)
            {
                if (sum == 0)
                    yield return new int[0];
                yield break;
            }

            for (var alIx = 0; alIx < allowed.Length; alIx++)
                foreach (var solution in makeCombinations(len - 1, sum - allowed[alIx], allowed.Remove(alIx, 1)))
                    yield return solution.Insert(0, allowed[alIx]);
        }

        sealed class Light
        {
            public int Clue;
            public List<int[]> Combinations;
        }
    }
}
