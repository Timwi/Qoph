using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PuzzleSolvers;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace Qoph
{
    static class PowerOfSudoku
    {
        public static void FindIndexingSudokus()
        {
            var words = @"minecraft,stoneage,gettinganupgrade,acquirehardware,suitup,hotstuff,isntitironpick,nottodaythankyou,icebucketchallenge,diamonds,weneedtogodeeper,covermewithdiamonds,enchanter,eyespy,theend,nether,thosewerethedays,hiddeninthedepths,aterriblefortress,whoiscuttingonions,ohshiny,thisboathaslegs,warpigs,countrylodetakemehome,spookyscaryskeleton,intofire,notquiteninelives,feelslikehome,witheringheights,localbrewery,bringhomethebeacon,theend,freetheend,remotegetaway,thecityattheendofthegame,adventure,voluntaryexile,isitabird,monsterhunter,whatadeal,stickysituation,olbetsy,surgeprotector,cavescliffs,sweetdreams,isitaballoon,athrowawayjoke,takeaim,startrader,whosthepillagernow,soundofmusic,lightasarabbit,isitaplane,veryveryfrightening,husbandry,beeourguest,theparrotsandthebats,whateverfloatsyourgoat,bestfriendsforever,glowandbehold,fishybusiness,totalbeelocation,aseedyplace,waxon,tacticalfishing,waxoff,thecutestpredator,thehealingpoweroffriendship".Split(',')
                // Remove advancements that look identical
                .Except(@"icebucketchallenge,nether,thosewerethedays,aterriblefortress,whoiscuttingonions,spookyscaryskeleton,notquiteninelives,olbetsy,whosthepillagernow".Split(',')).ToArray();
            var cluephrase = @"theansweriseraser";
            var clueCells = "..t.h.e............a.n.s.w............e.r.i............s.e.r.a............s.e.r..".SelectIndexWhere(ch => ch != '.').ToArray();

            var tooConfusing = @"".Split(',');

            if (clueCells.Length != cluephrase.Length)
                Debugger.Break();

            var doubleSudoku = new Puzzle(81 * 2, 1, words.Length);

            // SUDOKU CONSTRAINTS:
            for (var pos = 0; pos < 9; pos++)
            {
                // Rows
                doubleSudoku.AddConstraint(new UniquenessConstraint(Enumerable.Range(0, 9).Select(i => 9 * pos + i)));
                doubleSudoku.AddConstraint(new UniquenessConstraint(Enumerable.Range(0, 9).Select(i => 81 + 9 * pos + i)));
                // Columns
                doubleSudoku.AddConstraint(new UniquenessConstraint(Enumerable.Range(0, 9).Select(i => 9 * i + pos)));
                doubleSudoku.AddConstraint(new UniquenessConstraint(Enumerable.Range(0, 9).Select(i => 81 + 9 * i + pos)));
                // 3×3 regions
                doubleSudoku.AddConstraint(new UniquenessConstraint(Enumerable.Range(0, 9).Select(i => i % 3 + 3 * (pos % 3) + 9 * (i / 3 + 3 * (pos / 3)))));
                doubleSudoku.AddConstraint(new UniquenessConstraint(Enumerable.Range(0, 9).Select(i => 81 + i % 3 + 3 * (pos % 3) + 9 * (i / 3 + 3 * (pos / 3)))));
            }

            // Top sudoku is 1–9
            for (var pos = 0; pos < 81; pos++)
                doubleSudoku.AddConstraint(new OneCellLambdaConstraint(pos, v => v <= 9));

            // Top sudoku: a number placed in a clue cell means the corresponding bottom cell must have a word with the correct letter in the correct place
            // Bottom sudoku: a word placed in a clue cell means the corresponding top cell must have a suitable index
            doubleSudoku.AddConstraint(new LambdaConstraint(affectedCells: clueCells.SelectMany(i => new[] { i, 81 + i }), lambda: state =>
            {
                if (state.LastPlacedCell == null)
                    return null;
                var ix = state.LastPlacedCell;
                if (ix.Value < 81)
                {
                    var otherCell = ix.Value + 81;
                    var index = clueCells.IndexOf(ix.Value);
                    var val = state.LastPlacedValue;
                    state.MarkImpossible(otherCell, v => words[v - 1].Length < val || words[v - 1][val - 1] != cluephrase[index]);
                }
                else
                {
                    var otherCell = ix.Value - 81;
                    var index = clueCells.IndexOf(otherCell);
                    var word = words[state.LastPlacedValue - 1];
                    state.MarkImpossible(otherCell, v => word.Length < v || word[v - 1] != cluephrase[index]);
                }
                return null;
            }));

            // Bottom sudoku: clue cells can only contain words that actually include the required letter
            doubleSudoku.AddConstraints(clueCells.Select((cel, ix) => new OneCellLambdaConstraint(cel + 81, v => words[v - 1].Contains(cluephrase[ix]))));

            // Bottom sudoku: as soon as 9 different words have been placed, the remaining cells must be constrained to those 9 words only
            doubleSudoku.AddConstraint(new LambdaConstraint(affectedCells: Enumerable.Range(81, 81), lambda: state =>
            {
                if (state.LastPlacedCell == null)
                    return null;
                var valuesUsed = Enumerable.Range(0, 81).Select(ix => state[81 + ix]).WhereNotNull().ToHashSet();
                if (valuesUsed.Count > 9)
                    Debugger.Break();
                if (valuesUsed.Count < 9)
                    return null;
                for (var i = 81; i < 2 * 81; i++)
                    state.MarkImpossible(i, v => !valuesUsed.Contains(v));
                return ConstraintResult.Remove;
            }));

            var lockObject = new object();
            Enumerable.Range(4, 1).ParallelForEach(Environment.ProcessorCount, pr =>
            {
                var rnd = new Random(pr);
                foreach (var solution in doubleSudoku.Solve(new SolverInstructions { Randomizer = rnd/*, ShowContinuousProgress = 40, ShowContinuousProgressShortened = true */}))
                {
                    lock (lockObject)
                    {
                        ConsoleUtil.WriteLine(solution.JoinString(", "));
                        ConsoleUtil.WriteLine(solution.Take(81)
                            .Select((value, cell) => value.ToString().Color(clueCells.Contains(cell) ? ConsoleColor.White : ConsoleColor.DarkGray, clueCells.Contains(cell) ? ConsoleColor.DarkGreen : ConsoleColor.Black))
                            .Split(9)
                            .Select(row => row.JoinColoredString(" "))
                            .JoinColoredString("\n"));
                        Console.WriteLine();
                        var wordIxs = solution.Skip(81).Distinct().ToArray();
                        ConsoleUtil.WriteLine(solution.Skip(81)
                            .Select((word, cell) => (wordIxs.IndexOf(word) + 1).ToString().Color(clueCells.Contains(cell) ? ConsoleColor.White : ConsoleColor.DarkGray, clueCells.Contains(cell) ? ConsoleColor.DarkGreen : ConsoleColor.Black))
                            .Split(9)
                            .Select(row => row.JoinColoredString(" "))
                            .JoinColoredString("\n"));
                        Console.WriteLine(wordIxs.Select((wi, ix) => $"{ix + 1} = {words[wi - 1]}").JoinString(", "));
                        Console.WriteLine();
                        ConsoleUtil.WriteLine(solution.Skip(81)
                            .Select((word, cell) => clueCells.Contains(cell) ? words[word - 1][solution[cell] - 1].ToString().Color(ConsoleColor.White, ConsoleColor.DarkGreen) : "?".Color(ConsoleColor.DarkGray))
                            .Split(9)
                            .Select(row => row.JoinColoredString(" "))
                            .JoinColoredString("\n"));
                        Console.WriteLine();

                        var indexSudokuSolution = solution.Take(81).JoinString();
                        var achievementsSolution = solution.Subarray(81, 81).Select(i => wordIxs.IndexOf(i) + 1).JoinString();

                        int[] indexGivens = null;
                        int[] achGivens = null;

                        /*
                        var numAttempts = 0;
                        tryOnceMore:
                        numAttempts++;
                        if (numAttempts > 3)
                            return;

                        try
                        {
                            indexGivens = Ut.ReduceRequiredSet(Enumerable.Range(0, 81).Except(clueCells).ToArray().Shuffle(rnd), test: set =>
                            {
                                var sudoku = new Sudoku();
                                sudoku.AddGivens(set.SetToTest.Select(ix => (ix, indexSudokuSolution[ix] - '0')).ToArray());
                                return !sudoku.Solve().Skip(1).Any();
                            }).ToArray();
                        }
                        catch (Exception e)
                        {
                            if (e.Message != "The function does not return true for the original set.")
                                throw;
                        }

                        if (indexGivens == null)
                            return;

                        try
                        {
                            achGivens = Ut.ReduceRequiredSet(Enumerable.Range(0, 81).Except(clueCells).Except(indexGivens).ToArray().Shuffle(rnd), test: set =>
                            {
                                var sudoku = new Sudoku();
                                sudoku.AddGivens(set.SetToTest.Select(ix => (ix, achievementsSolution[ix] - '0')).ToArray());
                                return !sudoku.Solve().Skip(1).Any();
                            }).ToArray();
                        }
                        catch (Exception e)
                        {
                            if (e.Message != "The function does not return true for the original set.")
                                throw;
                        }

                        if (achGivens == null)
                            goto tryOnceMore;
                        /*/
                        indexGivens = new[] { 50, 64, 77, 70, 49, 52, 69, 73, 20, 54, 34, 41, 79, 15, 68, 10, 3, 72, 27, 44, 7, 29, 26, 9 };
                        achGivens = new[] { 28, 80, 60, 33, 5, 1, 13, 63, 53, 71, 43, 24, 0, 36, 35, 45, 18, 75, 8, 58, 67, 66, 39, 65, 31 };
                        /**/

                        Console.WriteLine(indexGivens.JoinString(", "));
                        Console.WriteLine(achGivens.JoinString(", "));

                        ConsoleUtil.WriteLine(new Sudoku()
                            .AddGivens(indexGivens.Select(g => (g, indexSudokuSolution[g] - '0')).ToArray(), ConsoleColor.White, ConsoleColor.DarkBlue)
                            .SolutionToConsole(indexSudokuSolution.Select(ch => ch - '0').ToArray()));
                        Console.WriteLine();
                        ConsoleUtil.WriteLine(new Sudoku()
                            .AddGivens(achGivens.Select(g => (g, indexSudokuSolution[g] - '0')).ToArray(), ConsoleColor.White, ConsoleColor.DarkBlue)
                            .SolutionToConsole(achievementsSolution.Select(ch => ch - '0').ToArray()));

                        File.WriteAllText(@"D:\temp\temp.txt", $"index: {solution.Subarray(0, 81).JoinString()}\ngivens: {indexGivens.JoinString(", ")}\nnames: {solution.Subarray(81, 81).Select(i => wordIxs.IndexOf(i) + 1).JoinString()}\ngivens: {achGivens.JoinString(", ")}\nwords:\n    {wordIxs.Select((w, ix) => $"{ix + 1} = {words[w - 1]}").JoinString("\n    ")}");

                        General.ReplaceInFile(@"D:\c\Qoph\EnigmorionFiles\Puzzles\objectionable-ranking.html", "<!--&&-->", "<!--&&&-->",
                            Enumerable.Range(0, 9).Select(row => $@"{"\t<tr>\n"}{
                                Enumerable.Range(0, 9).Select(col =>
                                {
                                    var classes = new List<string>();
                                    if (row % 3 == 2) classes.Add("bottom");
                                    if (col % 3 == 2) classes.Add("right");
                                    if (clueCells.Contains(col + 9 * row)) classes.Add("circle");
                                    if (achGivens.Contains(col + 9 * row)) classes.Add($"icon icon-{achievementsSolution[col + 9 * row]}");
                                    return $"\t\t<td{(classes.Count > 0 ? $" class='{classes.JoinString(" ")}'" : "")}>{(indexGivens.Contains(col + 9 * row) ? indexSudokuSolution[col + 9 * row].ToString() : "")}</td>\n";
                                }).JoinString()
                            }{"\t</tr>\n"}").JoinString());
                        Console.WriteLine();
                        Console.WriteLine($"Seed: {pr}");
                        Debugger.Break();
                    }
                }
            });
        }

        public static void GenerateIcons()
        {
            General.ReplaceInFile(@"D:\c\Qoph\EnigmorionFiles\Puzzles\objectionable-ranking.html", "/*start-power-icons*/", "/*end-power-icons*/",
                Enumerable.Range(1, 9).Select(digit => $@"		table.power td.icon-{digit} {{
			background-image: url('data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes($@"D:\c\Qoph\DataFiles\Objectionable Ranking\Power of Advancement\icon-{digit}.png"))}');
		}}").JoinString("\n") + "\n");
        }
    }
}