using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using PuzzleSolvers;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff.BombDisposal
{
    static class MultiSudokuExperiment
    {
        public static void Do()
        {
            var grid = @"O	F	S	E	A	B	I	R	D
I	N	R	E	A	L	L	I	F
E	A	S	C	E	N	D	S	O
F	Q	U	I	Z	O	N	S	P
E	A	R	P	U	N	S	E	L
F	O	R	E	G	O	N	A	I
N	T	L	O	U	D	W	C	I
L	O	C	I	K	R	G	E	T
B	S	U	R	E	L	O	E	E".Replace("\r", "").Split('\n').Select(row => row.Split('\t').Select(s => s[0]).ToArray()).ToArray();
            var clues = @"ANSFLOWER,ANSPENCIL,ANSISFIRE,FROZENDIE,BEEQUALTO,FROZENORB,ANSNEEDLE,ANSSPONGE,BFDIRANKS".Split(',');

            for (var puzIx = 0; puzIx < 9; puzIx++)
            {
                var clue = clues[puzIx];
                IEnumerable<int[]> recurse(int[] sofar, int rowIx)
                {
                    if (rowIx == 9)
                    {
                        yield return sofar;
                        yield break;
                    }
                    for (var col = 0; col < 9; col++)
                    {
                        if (grid[rowIx][col] != clue[rowIx] || sofar.Take(rowIx).Contains(col))
                            continue;
                        sofar[rowIx] = col;
                        foreach (var sol in recurse(sofar, rowIx + 1))
                            yield return sol;
                    }
                }

                foreach (var arrangement in recurse(new int[9], 0))
                {
                    var puzzle = new Sudoku();
                    //puzzle.AddConstraint(new AntiKnightConstraint(9, 9));
                    puzzle.AddGivens(arrangement.Select((col, row) => (9 * row + col, puzIx + 1)).ToArray());
                    ConsoleUtil.WriteLine($"{clue} = {arrangement.Select(i => i + 1).JoinString()}".Color(puzzle.Solve().Any() ? ConsoleColor.Green : ConsoleColor.Red));
                }
            }
        }

        public static void FindLetterArrangement()
        {
            var clues = @"FROZENDIE,FROZENORB,ANSPENCIL,ANSISFIRE,ANSNEEDLE,ANSSPONGE,ANSFLOWER,BEEQUALTO,BFDIRANKS".Split(',');

            IEnumerable<(char[] board, int[][] cellsForEachClue)> recurse(char[] sofar, int[][] cellsForClues, int clueIx, int ltrIx)
            {
                if (ltrIx == 9)
                {
                    clueIx++;
                    ltrIx = 0;
                }

                if (clueIx == 9)
                {
                    yield return (sofar.ToArray(), cellsForClues.Select(c => c.ToArray()).ToArray());
                    yield break;
                }

                static bool clash(int i, int j) => i % 9 == j % 9 || i / 9 == j / 9 || (i % 9) / 3 + 3 * (i / 27) == (j % 9) / 3 + 3 * (j / 27);

                var alreadyInRow = false;
                for (var col = 0; col < 9; col++)
                {
                    var newCell = ltrIx * 9 + col;
                    if (sofar[newCell] != clues[clueIx][ltrIx])
                        continue;
                    alreadyInRow = true;
                    if (cellsForClues[clueIx].Take(ltrIx).Any(c => clash(c, newCell)))
                        continue;
                    cellsForClues[clueIx][ltrIx] = newCell;
                    foreach (var s in recurse(sofar, cellsForClues, clueIx, ltrIx + 1))
                        yield return s;
                }

                if (alreadyInRow)
                    yield break;

                for (var col = 0; col < 9; col++)
                {
                    var newCell = ltrIx * 9 + col;
                    if (sofar[newCell] != default)
                        continue;
                    if (cellsForClues[clueIx].Take(ltrIx).Any(c => clash(c, newCell)))
                        continue;
                    sofar[newCell] = clues[clueIx][ltrIx];
                    cellsForClues[clueIx][ltrIx] = newCell;
                    foreach (var s in recurse(sofar, cellsForClues, clueIx, ltrIx + 1))
                        yield return s;
                    sofar[newCell] = default;
                }
            }

            var count = 0;
            var boards = new HashSet<string>();
            foreach (var (board, cellsForEachClue) in recurse(new char[81], Ut.NewArray(9, _ => new int[9]), 0, 0))
            {
                //Console.WriteLine(board.Select(c => c == default ? '?' : c).Split(9).Select(row => row.JoinString(" ")).JoinString("\n"));
                var boardStr = board.Select(c => c == default ? '?' : c).JoinString();
                boards.Add(boardStr);
                //for (var clueIx = 0; clueIx < 9; clueIx++)
                //{
                //    ConsoleUtil.WriteLine(clues[clueIx].Color(ConsoleColor.White, ConsoleColor.DarkGreen));
                //    ConsoleUtil.WriteLine(board.Select(c => c == default ? '?' : c).Split(9).Select((row, rowIx) => row.Select((c, colIx) => (cellsForEachClue[clueIx][rowIx] == 9 * rowIx + colIx).Apply(h => c.ToString().Color(h ? ConsoleColor.White : ConsoleColor.DarkGray, h ? ConsoleColor.DarkBlue : ConsoleColor.Black))).JoinColoredString(" ")).JoinColoredString("\n"));
                //    Console.WriteLine();
                //}
                count++;
                //Debugger.Break();
            }
            File.WriteAllLines(@"D:\temp\sudokuboards.txt", boards);
            Console.WriteLine($"Found {count} solutions.");
        }
    }
}