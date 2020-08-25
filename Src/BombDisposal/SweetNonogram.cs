using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using PuzzleSolvers;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff.BombDisposal
{
    static class SweetNonogram
    {
        public static void Generate()
        {
            const int width = 25;
            const int height = 15;
            var nonogram = @"		/				/	/	/		/	/	/		/	/			/	/	/		/	/	/
	/	/	/			/		/		/		/		/		/		/				/		/
/		/		/		/		/		/	/	/		/		/		/	/	/		/	/	/
		/				/		/		/	/			/		/		/				/	/	
/		/		/		/	/	/		/		/		/	/			/	/	/		/		/
/		/		/																				
/		/		/	/	/	/	/	/	/			/	/	/	/	/	/		/	/	/	/	/
/				/	/		/		/	/		/	/		/		/	/		/	/		/	/
/					/	/	/	/	/	/		/	/	/	/	/	/	/		/	/	/	/	/
/																								
		/			/		/			/		/	/			/	/	/		/	/		/	/
	/				/		/	/		/		/		/		/					/		/	
/	/	/	/		/		/		/	/		/		/		/	/	/				/		
	/				/		/		/	/		/		/		/					/		/	
		/			/		/			/		/	/			/	/	/		/	/		/	/".Replace("\r", "").Split('\n').Select(row => row.Split('\t')).ToArray();
            if (nonogram.Length != height || nonogram.Any(row => row.Length != width))
                Debugger.Break();

            static int[] makeClues(IEnumerable<string> strs) => strs.GroupConsecutiveBy(str => str == "/").Where(gr => gr.Key).Select(gr => gr.Count).ToArray();
            var rowClues = nonogram.Select(row => makeClues(row)).ToArray();
            var columnClues = Enumerable.Range(0, width).Select(col => makeClues(Enumerable.Range(0, height).Select(row => nonogram[row][col]))).ToArray();

            var problems = new List<string>();
            for (var i = 1; i <= 7; i++)
            {
                if (!columnClues.Any(cc => cc.Contains(i)))
                    problems.Add($"No column clue has {i}.");
                if (!rowClues.Any(rc => rc.Contains(i)))
                    problems.Add($"No row clue has {i}.");
            }
            for (var ix = 0; ix < height; ix++)
            {
                if (columnClues[ix].Any(c => c > 7))
                    problems.Add($"Column {ix} has a chunk > 7.");
                if (rowClues[ix].Any(c => c > 7))
                    problems.Add($"Row {ix} has a chunk > 7.");
            }
            if (problems.Count > 0)
            {
                Console.WriteLine(problems.JoinString("\n"));
                Debugger.Break();
                return;
            }

            var rowCluesRandom = Enumerable.Range(0, 7).Select(i => (char) ('A' + i)).ToArray().Shuffle(new Random(47)).JoinString();
            var rowCluesText = rowClues.Select(rc => rc.Select(clue => rowCluesRandom[clue - 1]).JoinString(" ")).JoinString("\n");
            Clipboard.SetText(rowCluesText);
            Clipboard.SetText(rowClues.Select(rc => $"={width}-{rc.Length - 1}-{rc.Select(clue => rowCluesRandom[clue - 1]).GroupBy(str => str).OrderBy(gr => gr.Key).Select(gr => $"{gr.Count()}*$AC${gr.Key - 'A' + 11}").JoinString("-")}").JoinString("\n"));
            //var rowCluesNumbers = rowClues.Select(rc => rc.JoinString(" ")).JoinString("\n");
            //Clipboard.SetText(rowCluesNumbers);

            var colCluesRandom = Enumerable.Range(0, 7).Select(i => (char) ('A' + i)).ToArray().Shuffle(new Random(147)).JoinString();
            var colCluesText = columnClues.Select(cc => $@"""{cc.Select(clue => colCluesRandom[clue - 1]).JoinString("\n")}""").JoinString("\t");
            Clipboard.SetText(colCluesText);
            Clipboard.SetText(columnClues.Select(cc => $"={height}-{cc.Length - 1}-{cc.Select(clue => colCluesRandom[clue - 1]).GroupBy(str => str).OrderBy(gr => gr.Key).Select(gr => $"{gr.Count()}*$AC${gr.Key - 'A' + 2}").JoinString("-")}").JoinString("\t"));
            //var colCluesNumbers = columnClues.Select(cc => $@"""{cc.JoinString("\n")}""").JoinString("\t");
            //Clipboard.SetText(colCluesNumbers);

            var locker = new object();
            var rowPermutations = Enumerable.Range(1, 7).Permutations().Select(p => p.ToArray()).Where(p => p[0] == 1 && p[1] == 2).ToArray();
            var colPermutations = Enumerable.Range(1, 7).Permutations().Select(p => p.ToArray()).Where(p => p[0] == 1).ToArray();

            (from rp in rowPermutations from cp in colPermutations select (rp, cp)).ParallelForEach(Environment.ProcessorCount, permutations =>
            {
                var puzzle = new Puzzle(width * height, 0, 1);
                //int[] tr(int[] input, bool trp) => (trp ^ testRowPermutations) ? input : input.Select(clue => permutation[clue - 1]).ToArray();
                for (var row = 0; row < rowClues.Length; row++)
                    puzzle.AddConstraint(new CombinationsConstraint(Enumerable.Range(0, width).Select(col => row * width + col), GetNonogramCombinations(width, rowClues[row].Select(clue => permutations.rp[clue - 1]).ToArray())));
                for (var col = 0; col < columnClues.Length; col++)
                    puzzle.AddConstraint(new CombinationsConstraint(Enumerable.Range(0, height).Select(row => row * width + col), GetNonogramCombinations(width, columnClues[col].Select(clue => permutations.cp[clue - 1]).ToArray())));

                var count = 0;
                int?[] commonalities = null;
                foreach (var solution in puzzle.Solve())
                {
                    //lock (locker)
                    //{
                    //    Console.WriteLine(solution.Split(width).Select(row => row.Select(n => new string("·█"[n], 2)).JoinString()).JoinString("\n"));
                    //    Console.WriteLine();
                    //}
                    count++;
                    if (commonalities == null)
                        commonalities = solution.SelectNullable().ToArray();
                    else
                        for (var i = 0; i < solution.Length; i++)
                            if (commonalities[i] != solution[i])
                                commonalities[i] = null;
                }
                lock (locker)
                {
                    var permStr = $"{permutations.cp.JoinString()}/{permutations.rp.JoinString()}";
                    if (count == 0 && permStr == "1234567/1234567")
                    {
                        ConsoleUtil.WriteLine($"Permutation {permStr} is impossible.".Color(ConsoleColor.Red));
                        Debugger.Break();
                    }
                    else if (count == 0)
                        ConsoleUtil.WriteLine($"Permutation {permStr} is impossible.".Color(ConsoleColor.Green));
                    else if (count == 1 && permStr == "1234567/1234567")
                        ConsoleUtil.WriteLine($"Permutation {permStr} works.".Color(ConsoleColor.Green));
                    else if (count == 1)
                    {
                        ConsoleUtil.WriteLine($"Permutation {permStr} works.".Color(ConsoleColor.Magenta));
                        Debugger.Break();
                    }
                    else
                    {
                        ConsoleUtil.WriteLine($"Permutation {permStr} works and is ambiguous:".Color(ConsoleColor.Yellow, ConsoleColor.DarkRed));
                        Console.WriteLine($"{count} solutions found.");
                        Console.WriteLine("Commonalities:");
                        ConsoleUtil.WriteLine(commonalities.Split(width).Select(row => row.Select(n => n == null ? "••".Color(ConsoleColor.Magenta, ConsoleColor.DarkRed) : new string("·█"[n.Value], 2).Color(ConsoleColor.White, ConsoleColor.DarkBlue)).JoinColoredString()).JoinColoredString("\n"));
                        Console.WriteLine();
                        Debugger.Break();
                    }
                }
            });
        }

        private static readonly Dictionary<(int size, int[] lengths), int[][]> _cache = new Dictionary<(int size, int[] lengths), int[][]>(new CustomEqualityComparer<(int size, int[] lengths)>(
            comparison: (a, b) => a.size == b.size && a.lengths.SequenceEqual(b.lengths),
            getHashCode: tup => Ut.ArrayHash(tup.size, Ut.ArrayHash(tup.lengths))
        ));
        private static int[][] GetNonogramCombinations(int size, int[] lengths)
        {
            lock (_cache)
                if (_cache.TryGetValue((size, lengths), out var result))
                    return result;

            var combinations = GenerateNonogramCombinations(new int[size], 0, lengths, 0).ToArray();
            lock (_cache)
                return _cache[(size, lengths)] = combinations;
        }

        private static IEnumerable<int[]> GenerateNonogramCombinations(int[] sofar, int sofarIx, int[] lengths, int lensIx)
        {
            if (lensIx == lengths.Length)
            {
                yield return sofar.ToArray();
                yield break;
            }

            var availableSpace = sofar.Length - sofarIx;
            var minLengthOfRest = lengths.Skip(lensIx).Sum() + lengths.Length - lensIx - 1;
            var leeway = availableSpace - minLengthOfRest;
            for (var offset = 0; offset <= leeway; offset++)
            {
                for (var i = 0; i < lengths[lensIx]; i++)
                    sofar[sofarIx + offset + i] = 1;
                foreach (var solution in GenerateNonogramCombinations(sofar, sofarIx + offset + lengths[lensIx] + 1, lengths, lensIx + 1))
                    yield return solution;
                for (var i = 0; i < lengths[lensIx]; i++)
                    sofar[sofarIx + offset + i] = 0;
            }
        }
    }
}