using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PuzzleSolvers;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff
{
    public class BinairoSudoku
    {
        public static void Generate()
        {
            var sudoku = makeBinairoSudoku(@"...81..0..9.....87..........2...1.4...7....6..........0............1....3.......5..9......8...7....5".Select(ch => ch == '.' ? null : (int?) ch - '0').ToArray());
            var count = 0;
            var rnd = new Random(8472);
            const string path = @"D:\temp\BinairoSudokusGenerated.txt";
            File.WriteAllText(path, "");

            foreach (var sudokuSolution in sudoku.Solve(randomizer: rnd))
            {
                count++;
                Console.WriteLine($"Solution #{count}:");
                var solStr = sudoku.SudokuSolutionToConsoleString(sudokuSolution, 10);
                ConsoleUtil.WriteLine(solStr);
                Console.WriteLine();
                File.AppendAllText(path, $"Solution #{count}:" + Environment.NewLine + solStr.ToString() + Environment.NewLine);
            }
            Console.WriteLine();
            Console.WriteLine($"{count} solutions found in total.");
        }

        private static Puzzle makeBinairoSudoku(int?[] givens = null)
        {
            var constraints = new List<Constraint>();
            constraints.AddRange(Constraint.LatinSquare(10, 10));
            constraints.AddRange(new[] { "A-D1,A-C2-3", "E-G1-2,D2,D-F3", "H-J1-3,G3", "A-B4-7,C5-6", "C4,C7,D-E4-7", "F-G4-7,H4,H7", "H5-6,I-J4-7", "A-D8,A-C9-10", "E-G8-9,D9,D-F10", "H-J8-10,G10" }
                .Select((region, ix) => new UniquenessConstraint(Constraint.TranslateCoordinates(region, gridWidth: 10), (ConsoleColor) (1 + ix % 6))));
            constraints.Add(new BinairoOddEvenConstraint(10));
            if (givens != null)
                for (var i = 0; i < givens.Length; i++)
                    if (givens[i] != null)
                        constraints.Add(new GivenConstraint(i, givens[i].Value));
            return new Puzzle(100, 0, 9, constraints);
        }

        public static void GenerateGivens()
        {
            var binairoSudoku = new[] { 7, 6, 5, 8, 1, 2, 9, 0, 3, 4, 9, 1, 2, 0, 3, 4, 8, 7, 5, 6, 0, 3, 4, 5, 6, 7, 1, 8, 2, 9, 2, 8, 1, 3, 4, 9, 5, 6, 7, 0, 3, 0, 7, 6, 9, 8, 2, 5, 4, 1, 1, 5, 6, 7, 0, 3, 4, 2, 9, 8, 4, 9, 8, 2, 5, 0, 7, 1, 6, 3, 6, 4, 3, 1, 2, 5, 0, 9, 8, 7, 5, 7, 0, 9, 8, 6, 3, 4, 1, 2, 8, 2, 9, 4, 7, 1, 6, 3, 0, 5 };
            var rnd = new Random(8472);
            var items = Enumerable.Range(0, binairoSudoku.Length).ToList().Shuffle(rnd);

            int?[] getGrid(IEnumerable<int> cells)
            {
                var grid = new int?[binairoSudoku.Length];
                foreach (var cell in cells)
                    grid[cell] = binairoSudoku[cell];
                return grid;
            }

            var givens = Ut.ReduceRequiredSet(items, skipConsistencyTest: true, test: state =>
            {
                var grid = getGrid(state.SetToTest);
                ConsoleUtil.WriteLine(grid.Split(2).Select(ch => " ▌▐█"[(ch.First() == null ? 0 : 1) + (ch.Last() == null ? 0 : 2)]).JoinString().Color(ConsoleColor.White, ConsoleColor.DarkBlue));
                return !makeBinairoSudoku(grid).Solve().Skip(1).Any();
            });

            Console.WriteLine();
            var solStr = makeBinairoSudoku().SudokuSolutionToConsoleString(getGrid(givens), 10);
            ConsoleUtil.WriteLine(solStr);
        }
    }
}