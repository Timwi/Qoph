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
            var sudoku = makeBinairoSudoku();
            string path = PathUtil.AppPathCombine(@"BinairoSudokusGenerated.txt");
            File.WriteAllText(path, "");
            var seeds = new[] { 48, 49, 50, 51 };

            //Enumerable.Range(0, seeds.Length).ParallelForEach(seedIx =>
            var seedIx = 0;
            {
                foreach (var sudokuSolution in sudoku.Solve(new SolverInstructions { Randomizer = new Random(seeds[seedIx]), ShowContinuousProgress = seedIx == 0 ? 20 : (int?) null }))
                {
                    lock (path)
                    {
                        Console.WriteLine($"Solution:");
                        var solStr = sudoku.SudokuSolutionToConsoleString(sudokuSolution, 10);
                        ConsoleUtil.WriteLine(solStr);
                        Console.WriteLine();
                        File.AppendAllText(path, $"Solution:" + Environment.NewLine + solStr.ToString() + Environment.NewLine);
                    }
                }
            }//);
        }

        private static Puzzle makeBinairoSudoku()
        {
            var constraints = new List<Constraint>();
            constraints.AddRange(Constraint.LatinSquare(10, 10));
            constraints.AddRange(new[] { "A-C1-3,D1", "E-F1,D-G2-3", "H-J1-3,G1", "A-B4-7,C5-6", "D-E4-7,C4,C7", "F-G4-7,H4,H7", "I-J4-7,H5-6", "A-C8-10,D10", "D-G8-9,E-F10", "H-J8-10,G10" }
                .Select((region, ix) => new UniquenessConstraint(Constraint.TranslateCoordinates(region, gridWidth: 10), backgroundColor: (ConsoleColor) (1 + ix % 6))));
            constraints.Add(new BinairoOddEvenConstraintWithoutUniqueness(10));

            // Unique diagonals
            //constraints.Add(new UniquenessConstraint(Enumerable.Range(0, 10).Select(i => i * 11))); // Forward diagonal
            //constraints.Add(new UniquenessConstraint(Enumerable.Range(0, 10).Select(i => 9 + i * 9))); // Backward diagonal

            // Thermometers
            constraints.Add(new LessThanConstraint(Constraint.TranslateCoordinates(@"B3,C2,D2,E3,E4,E5,D6,C6,B5,B4", 10), color: ConsoleColor.Cyan));
            constraints.Add(new LessThanConstraint(Constraint.TranslateCoordinates(@"H2,H3,G4,G5,F6,F7,E8,E9", 10), color: ConsoleColor.Green));
            constraints.Add(new LessThanConstraint(Constraint.TranslateCoordinates(@"I5-9", 10), color: ConsoleColor.Magenta));

            // Clone region
            //constraints.Add(new CloneConstraint(Constraint.TranslateCoordinates(@"G-H1,F-I2-3,G-H4", 10), Constraint.TranslateCoordinates(@"C-D7,B-E8-9,C-D10", 10)));

            //if (givens != null)
            //    for (var i = 0; i < givens.Length; i++)
            //        if (givens[i] != null)
            //            constraints.Add(new GivenConstraint(i, givens[i].Value));
            return new Puzzle(100, 0, 9, constraints);
        }

        //public static void GenerateGivens()
        //{
        //    var binairoSudoku = new[] { 7, 6, 5, 8, 1, 2, 9, 0, 3, 4, 9, 1, 2, 0, 3, 4, 8, 7, 5, 6, 0, 3, 4, 5, 6, 7, 1, 8, 2, 9, 2, 8, 1, 3, 4, 9, 5, 6, 7, 0, 3, 0, 7, 6, 9, 8, 2, 5, 4, 1, 1, 5, 6, 7, 0, 3, 4, 2, 9, 8, 4, 9, 8, 2, 5, 0, 7, 1, 6, 3, 6, 4, 3, 1, 2, 5, 0, 9, 8, 7, 5, 7, 0, 9, 8, 6, 3, 4, 1, 2, 8, 2, 9, 4, 7, 1, 6, 3, 0, 5 };
        //    var rnd = new Random(8472);
        //    var items = Enumerable.Range(0, binairoSudoku.Length).ToList().Shuffle(rnd);

        //    int?[] getGrid(IEnumerable<int> cells)
        //    {
        //        var grid = new int?[binairoSudoku.Length];
        //        foreach (var cell in cells)
        //            grid[cell] = binairoSudoku[cell];
        //        return grid;
        //    }

        //    var givens = Ut.ReduceRequiredSet(items, skipConsistencyTest: true, test: state =>
        //    {
        //        var grid = getGrid(state.SetToTest);
        //        ConsoleUtil.WriteLine(grid.Split(2).Select(ch => " ▌▐█"[(ch.First() == null ? 0 : 1) + (ch.Last() == null ? 0 : 2)]).JoinString().Color(ConsoleColor.White, ConsoleColor.DarkBlue));
        //        return !makeBinairoSudoku(grid).Solve().Skip(1).Any();
        //    });

        //    Console.WriteLine();
        //    var solStr = makeBinairoSudoku().SudokuSolutionToConsoleString(getGrid(givens), 10);
        //    ConsoleUtil.WriteLine(solStr);
        //}
    }
}