using System;
using RT.Util;
using RT.Util.ExtensionMethods;
using System.Linq;
using PuzzleSolvers;
using System.Diagnostics;
using System.IO;
using RT.Util.Consoles;

namespace PuzzleStuff.BombDisposal
{
    static class PowerOfTwo
    {
        public static void FindIndexingSudokus()
        {
            var words = @"acquireha,adventuri,ahoy,alternati,archer,artificia,atlantis,bakebread,beammeup,beeourgue,benchmaki,bodyguard,bullseye,buylowsel,camouflag,castaway,cheatingd,chestfulo,covermein,cowtipper,delicious,diamondst,diamonds,disenchan,dispensew,doabarrel,dryspell,echolocat,enchanter,feelingil,freediver,freetheen,freightst,fruitonth,gettingan,gettingwo,greatview,haveashea,hottopic,hottouris,iamamarin,ivegotaba,inception,intofire,ironbelly,ironman,itsasign,killthebe,leaderoft,letitgo,librarian,lionhunte,localbrew,maproom,mastertra,megold,moartools,monsterhu,moskstrau,onarail,onepickle,ooohshiny,organizat,overkill,overpower,passingth,plethorao,porkchop,potplante,rabbitsea,rainbowco,renewable,repopulat,returntos,saddleup,sailthe,sleepwith,smeltever,sniperdue,soigottha,soundthea,stayinfro,stickysit,superfuel,supersoni,takinginv,tasteofyo,thebeacon,thebeginn,thebeginn,thedeepen,theendaga,theend,thehaggle,thelie,tiedyeout,timeforst,timetofar,timetomin,timetostr,topofthew,totalbeel,trampolin,treasureh,weneedtog,werebeing,whenpigsf,wherehave,youneedam,zombiedoc,zoologist".Split(',');
            //var cluephrase = @"americanwordforwhatthebritishcallarubber";   // exactly 40 letters
            //var cluephrase = @"answeriseraser";
            //var clueCells = new[] { 10, 12, 14, 16, 29, 31, 33, 46, 48, 50, 52, 65, 67, 69 };
            var cluephrase = @"theansweriseraser";
            var clueCells = "..t.h.e............a.n.s.w............e.r.i............s.e.r.a............s.e.r..".SelectIndexWhere(ch => ch != '.').ToArray();
            //var cluephrase = @"filmarnieplayskrugerin";
            //var clueCells = ".#.#.#.#..........#.#.#.#.#..........#.#.#.#..........#.#.#.#.#..........#.#.#.#.".SelectIndexWhere(ch => ch == '#').ToArray();

            if (clueCells.Length != cluephrase.Length)
                Debugger.Break();

            //var randomWords = new[] { "organizat", "sniperdue", "gettingwo", "stayinfro", "cheatingd", "haveashea", "monsterhu", "topofthew", "passingth" };
            //var randomIndexSudoku = new Sudoku().Solve(new SolverInstructions { Randomizer = new Random(8472) }).First();
            //var randomWordsSudoku = new Sudoku().Solve(new SolverInstructions { Randomizer = new Random(8473) }).First();
            //var randomCluephrase = Enumerable.Range(0, 40).Select(i => randomWords[randomWordsSudoku[2 * i + 1] - 1][randomIndexSudoku[2 * i + 1] - 1]).JoinString();
            //Console.WriteLine(randomWords.JoinString("\n"));
            //Console.WriteLine();
            //Console.WriteLine(new Sudoku().SolutionToConsole(randomIndexSudoku));
            //Console.WriteLine();
            //Console.WriteLine(new Sudoku().SolutionToConsole(randomWordsSudoku));
            //Console.WriteLine();
            //Console.WriteLine(randomCluephrase);
            //Debugger.Break();

            //words = new[] { "organizat", "sniperdue", "gettingwo", "stayinfro", "cheatingd", "haveashea", "monsterhu", "topofthew", "passingth" };
            //cluephrase = @"ntneopefitntuhtiiuigiettpihigerygswuapui";


            var doubleSudoku = new Puzzle(81 * 2, 1, words.Length);
            //var permutations = Enumerable.Range(1, 9).Permutations().Select(p => p.ToArray()).ToArray();
            //File.WriteAllLines(@"D:\temp\permutations.txt", permutations.Select(p => p.JoinString(",")));
            //var permutations = File.ReadLines(@"D:\temp\permutations.txt").Select(line => line.Split(',').Select(int.Parse).ToArray()).ToArray();

            // SUDOKU CONSTRAINTS:
            for (var pos = 0; pos < 9; pos++)
            {
                // Rows
                //doubleSudoku.AddConstraint(new CombinationsConstraint(Enumerable.Range(0, 9).Select(i => 9 * pos + i), permutations));
                doubleSudoku.AddConstraint(new UniquenessConstraint(Enumerable.Range(0, 9).Select(i => 9 * pos + i)));
                doubleSudoku.AddConstraint(new UniquenessConstraint(Enumerable.Range(0, 9).Select(i => 81 + 9 * pos + i)));
                // Columns
                //doubleSudoku.AddConstraint(new CombinationsConstraint(Enumerable.Range(0, 9).Select(i => 9 * i + pos), permutations));
                doubleSudoku.AddConstraint(new UniquenessConstraint(Enumerable.Range(0, 9).Select(i => 9 * i + pos)));
                doubleSudoku.AddConstraint(new UniquenessConstraint(Enumerable.Range(0, 9).Select(i => 81 + 9 * i + pos)));
                // 3×3 regions
                //doubleSudoku.AddConstraint(new CombinationsConstraint(Enumerable.Range(0, 9).Select(i => i % 3 + 3 * (pos % 3) + 9 * (i / 3 + 3 * (pos / 3))), permutations));
                doubleSudoku.AddConstraint(new UniquenessConstraint(Enumerable.Range(0, 9).Select(i => i % 3 + 3 * (pos % 3) + 9 * (i / 3 + 3 * (pos / 3)))));
                doubleSudoku.AddConstraint(new UniquenessConstraint(Enumerable.Range(0, 9).Select(i => 81 + i % 3 + 3 * (pos % 3) + 9 * (i / 3 + 3 * (pos / 3)))));
            }

            // Top sudoku is 1–9
            for (var pos = 0; pos < 81; pos++)
                doubleSudoku.AddConstraint(new OneCellLambdaConstraint(pos, v => v <= 9));

            // Top sudoku: a number placed in a clue cell means the corresponding bottom cell must have a word with the correct letter in the correct place
            // Bottom sudoku: a word placed in a clue cell means the corresponding top cell must have a suitable index
            doubleSudoku.AddConstraint(new LambdaConstraint(affectedCells: clueCells.SelectMany(i => new[] { i, 81 + i }), lambda: (takens, grid, ix, minV, maxV) =>
            {
                if (ix == null)
                    return null;
                if (ix.Value < 81)
                {
                    var otherCell = ix.Value + 81;
                    var index = clueCells.IndexOf(ix.Value);
                    var val = grid[ix.Value].Value;
                    for (var v = 0; v < takens[otherCell].Length; v++)
                        if (words[v].Length < val + 1 || words[v][val] != cluephrase[index])
                            takens[otherCell][v] = true;
                }
                else
                {
                    var otherCell = ix.Value - 81;
                    var index = clueCells.IndexOf(otherCell);
                    var word = words[grid[ix.Value].Value];
                    for (var v = 0; v < takens[otherCell].Length; v++)
                        if (word.Length <= v || word[v] != cluephrase[index])
                            takens[otherCell][v] = true;
                }
                return null;
            }));

            // Bottom sudoku: clue cells can only contain words that actually include the required letter
            doubleSudoku.AddConstraints(clueCells.Select((cel, ix) => new OneCellLambdaConstraint(cel + 81, v => words[v - 1].Contains(cluephrase[ix]))));

            // Bottom sudoku: as soon as 9 different words have been placed, the remaining cells must be constrained to those 9 words only
            doubleSudoku.AddConstraint(new LambdaConstraint(affectedCells: Enumerable.Range(81, 81), lambda: (takens, grid, ix, minV, maxV) =>
            {
                if (ix == null)
                    return null;
                var valuesUsed = grid.Skip(81).WhereNotNull().ToHashSet();
                if (valuesUsed.Count > 9)
                    Debugger.Break();
                if (valuesUsed.Count != 9)
                    return null;
                for (var i = 81; i < 2 * 81; i++)
                    for (var v = 0; v < takens[i].Length; v++)
                        if (!valuesUsed.Contains(v))
                            takens[i][v] = true;
                return Enumerable.Empty<Constraint>();
            }));

            var lockObject = new object();
            Enumerable.Range(0, 1).ParallelForEach(pr =>
            {
                foreach (var solution in doubleSudoku.Solve(new SolverInstructions { Randomizer = new Random((1 + pr) * 100 + 47), ShowContinuousProgress = 40, ShowContinuousProgressShortened = true }))
                {
                    //Console.WriteLine(solution.JoinString(","));
                    lock (lockObject)
                    {
                        ConsoleUtil.WriteLine(solution.JoinString(", "));
                        //ConsoleUtil.WriteLine(doubleSudoku.SolutionToConsole(solution.Subarray(0, 81)));
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
                        File.AppendAllLines(@"D:\temp\temp.txt", new[] { solution.JoinString(",") });
                        Debugger.Break();
                    }
                }
            });
        }

        public static void GenerateGivens()
        {
            var indexSudokuSolution = @"415792836986135724372846159729654318863917542154328967637481295241579683598263471";
            var achievementsSolution = @"123456789549387126687129543236548917918273654754691238865914372371862495492735861";
            var clueCells = "..t.h.e............a.n.s.w............e.r.i............s.e.r.a............s.e.r..".SelectIndexWhere(ch => ch != '.').ToArray();

            var indexGivens = Ut.ReduceRequiredSet(Enumerable.Range(0, 81).Except(clueCells).ToArray().Shuffle(), test: set =>
            {
                var sudoku = new Sudoku();
                sudoku.AddGivens(set.SetToTest.Select(ix => (ix, indexSudokuSolution[ix] - '0')).ToArray());
                return !sudoku.Solve().Skip(1).Any();
            });
            Console.WriteLine(indexGivens.JoinString(", "));

            var achGivens = Ut.ReduceRequiredSet(Enumerable.Range(0, 81).Except(clueCells).Except(indexGivens).ToArray().Shuffle(), test: set =>
            {
                var sudoku = new Sudoku();
                sudoku.AddGivens(set.SetToTest.Select(ix => (ix, achievementsSolution[ix] - '0')).ToArray());
                return !sudoku.Solve().Skip(1).Any();
            });

            ConsoleUtil.WriteLine(new Sudoku()
                .AddGivens(indexGivens.Select(g => (g, indexSudokuSolution[g] - '0')).ToArray(), ConsoleColor.White, ConsoleColor.DarkBlue)
                .SolutionToConsole(indexSudokuSolution.Select(ch => ch - '0').ToArray()));
            Console.WriteLine();
            ConsoleUtil.WriteLine(new Sudoku()
                .AddGivens(achGivens.Select(g => (g, indexSudokuSolution[g] - '0')).ToArray(), ConsoleColor.White, ConsoleColor.DarkBlue)
                .SolutionToConsole(achievementsSolution.Select(ch => ch - '0').ToArray()));
        }
    }
}