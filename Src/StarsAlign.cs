using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using PuzzleSolvers;
using RT.Geometry;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace Qoph
{
    public static class StarsAlign
    {
        // Constraint that prevents a word from being used twice
        private class DontUseAWordTwiceConstraint : Constraint
        {
            public DontUseAWordTwiceConstraint(int[][] wordCells) : base(null) { _wordCells = wordCells; }
            private readonly int[][] _wordCells;

            public override ConstraintResult Process(SolverState state)
            {
                for (var wordIx = 0; wordIx < 10 * 5; wordIx++)
                {
                    for (var ltr = 0; ltr < 7; ltr++)
                        if (state[_wordCells[wordIx][ltr]] == null)
                            goto next2;
                    for (var word2Ix = 0; word2Ix < 10 * 5; word2Ix++)
                    {
                        if (word2Ix == wordIx)
                            continue;
                        int? nullLetter = null;
                        for (var ltr = 0; ltr < 7; ltr++)
                        {
                            if (state[_wordCells[word2Ix][ltr]] == null)
                            {
                                if (nullLetter != null)
                                    goto next;
                                nullLetter = ltr;
                            }
                            else if (state[_wordCells[word2Ix][ltr]] != state[_wordCells[wordIx][ltr]])
                                goto next;
                        }
                        if (nullLetter != null)
                            state.MarkImpossible(_wordCells[word2Ix][nullLetter.Value], state[_wordCells[wordIx][nullLetter.Value]].Value);
                        next:;
                    }
                    next2:;
                }
                return null;
            }
        }

        public static void Generate()
        {
            var solutionWord = "VOCATIONAL";
            if (solutionWord.Length != 10)
                Debugger.Break();

            var allWords = new string[0]
                //.Union(File.ReadLines(@"D:\Daten\Wordlists\English 60000.txt"))
                .Union(File.ReadLines(@"D:\Daten\Wordlists\peter_broda_wordlist.txt").Select(line => Regex.Match(line, @"^([A-Z]{7});(\d+)$")).Where(m => m.Success && int.Parse(m.Groups[2].Value) >= 86).Select(m => m.Groups[1].Value))
                .Union(File.ReadLines(@"D:\Daten\Wordlists\english-frequency-raw.txt"))
                .Union(File.ReadLines(@"D:\Daten\Wordlists\VeryCommonWords.txt"))
                .Except("MEOWMIX,XMLTAGS,WWIIACE,CHOLERA,COLITIS,GRANADA,ALYSSIA,ANAEMIA,TELFORD,COLONIC,NORIEGA,GYPSIES,LAMBETH,SUNSOFT,EDOUARD,LEOPOLD,THOMSON,SETTLOR,LORETTA,SCOTVEC,DRAUGHT,SOUNESS,CORBETT,TAUNTON,LEONORA,SUICIDE".Split(','))
                .ToHashSet();
            var words = allWords.Where(w => w.Length == 7 && w.All(ch => ch >= 'A' & ch <= 'Z')).ToHashSet();
            var wordsAsInts = words.Select(w => w.Select(ch => ch - 'A' + 1).ToArray()).ToArray();
            var (wordCells, ringCells, joinerCells) = GetCells();

            var lockObject = new object();
            Enumerable.Range(120, 10000).ParallelForEach(Environment.ProcessorCount, (seed, processor) =>
            {
                lock (lockObject)
                {
                    Console.CursorTop = processor;
                    Console.CursorLeft = 0;
                    Console.Write($"Proc {processor}: seed {seed}    ");
                }

                var puzzle = new Puzzle(238, 1, 26);

                // Constraint that each line should form a word
                for (var wordIx = 0; wordIx < 10 * 5; wordIx++)
                    puzzle.AddConstraint(new CombinationsConstraint(wordCells[wordIx], wordsAsInts));

                puzzle.AddConstraint(new DontUseAWordTwiceConstraint(wordCells));

                //*
                // Constraint to ensure that the solution phrase is there
                puzzle.AddConstraint(new LambdaConstraint(state =>
                 {
                     for (var star = 0; star < 10; star++)
                     {
                         int? nullLtr = null;
                         for (var ltr = 0; ltr < 10; ltr++)
                             if (state[ringCells[star][ltr]] == null)
                             {
                                 if (nullLtr != null)
                                     goto next;
                                 nullLtr = ltr;
                             }
                             else if (state[ringCells[star][ltr]].Value + 'A' - 1 == solutionWord[star])
                                 goto next;
                         if (nullLtr != null)
                             state.MustBe(ringCells[star][nullLtr.Value], solutionWord[star] - 'A' + 1);
                         next:;
                     }
                     return null;
                 }));
                /*/
                //Constraint that puts the solution phrase at North everywhere
                for (var i = 0; i < 10; i++)
                    puzzle.AddConstraint(new GivenConstraint(wordCells[i * 5 + 2][3], solutionWord[i] - 'A' + 1));
                /**/

                // Constraint to not re-use a shared letter too many times
                puzzle.AddConstraint(new LambdaConstraint(state =>
                 {
                     foreach (var cell in joinerCells)
                         if (state[cell] != null)
                             foreach (var cell2 in joinerCells)
                                 if (state[cell2] == null)
                                     state.MarkImpossible(cell2, state[cell].Value);
                     return null;
                 }));

                foreach (var solution in puzzle.Solve(new SolverInstructions { Randomizer = new Random(seed) }).Take(1))
                {
                    Directory.CreateDirectory($@"F:\temp\Stars Align (VOCATIONAL)\Seed {seed}");
                    var outputs = new List<ConsoleColoredString>();
                    void output(ConsoleColoredString str)
                    {
                        //ConsoleUtil.WriteLine(str);
                        outputs.Add(str);
                        File.WriteAllLines($@"F:\temp\Stars Align (VOCATIONAL)\Seed {seed}\Output.txt", outputs.Select(c => c.ToString()));
                    }
                    output(solution.Select(ltr => (char) (ltr + 'A' - 1)).JoinString());
                    output(new ConsoleColoredString($"Joiner letters: {joinerCells.Select(ix => (char) (solution[ix] + 'A' - 1)).Order().JoinString().Color(ConsoleColor.White)}"));

                    var generatedWords = new HashSet<string>();
                    for (var star = 0; star < 10; star++)
                    {
                        var starWords = new List<string>();
                        for (var arm = 0; arm < 5; arm++)
                            starWords.Add(wordCells[star * 5 + arm].Select(cell => (char) (solution[cell] + 'A' - 1)).JoinString());
                        output(new ConsoleColoredString($"{$"Star {star + 1}:".Color(ConsoleColor.White)} {starWords.Select(w => w.Color(ConsoleColor.Yellow)).JoinColoredString(", ".Color(ConsoleColor.DarkGray))}"));
                        generatedWords.AddRange(starWords);
                    }
                    for (var ring = 0; ring < 10; ring++)
                        output(new ConsoleColoredString($"{$"Ring {ring + 1}:".Color(ConsoleColor.Cyan)} {ringCells[ring].Select(cell => (char) (solution[cell] + 'A' - 1)).JoinString().Color(ConsoleColor.Green)}"));
                    output($"Seed {seed}: {generatedWords.Count}".Color(generatedWords.Count == 10 * 5 ? ConsoleColor.Green : ConsoleColor.Red));
                    writeXml(seed, solution, "Solution");

                    var generatedWordsAsInts = generatedWords.Select(w => w.Select(ch => ch - 'A' + 1).ToArray()).ToArray();
                    Puzzle makeTestPuzzle(IEnumerable<int> givenIxs)
                    {
                        var puzzleTest = new Puzzle(238, 1, 26);
                        for (var wordIx = 0; wordIx < 10 * 5; wordIx++)
                            puzzleTest.AddConstraint(new CombinationsConstraint(wordCells[wordIx], generatedWordsAsInts));
                        puzzleTest.AddConstraint(new DontUseAWordTwiceConstraint(wordCells));
                        foreach (var ix in givenIxs)
                            puzzleTest.AddConstraint(new GivenConstraint(ix, solution[ix]));
                        return puzzleTest;
                    }

                    // Find set of givens to ensure that the placement is unique
                    var requiredGivens = !makeTestPuzzle(Enumerable.Empty<int>()).Solve().Skip(1).Any() ? new int[0] : Ut.ReduceRequiredSet(Enumerable.Range(0, 238).Except(ringCells.SelectMany(x => x)).ToArray().Shuffle(new Random(47)), skipConsistencyTest: true, test: state =>
                     {
                         output(Enumerable.Range(0, 238).Select(ix => state.SetToTest.Contains(ix) ? "█" : "░").JoinString());
                         return !makeTestPuzzle(state.SetToTest).Solve().Skip(1).Any();
                     });
                    var givens = Enumerable.Range(0, 238).Select(ix => requiredGivens.Contains(ix) ? solution[ix] : (int?) null).ToArray();
                    writeXml(seed, givens, "Puzzle");
                    File.WriteAllText($@"F:\temp\Stars Align (VOCATIONAL)\Seed {seed}\Givens={requiredGivens.Count()}.txt", "");
                    if (requiredGivens.Count() == 0)
                        Directory.Move($@"F:\temp\Stars Align (VOCATIONAL)\Seed {seed}", $@"F:\temp\Stars Align (VOCATIONAL)\0! Seed {seed}");
                }
            });
        }

        private static (int[][] wordCells, int[][] ringCells, int[] joinerCells) GetCells()
        {
            var cells = Ut.NewArray(238, _ => new List<(int star, int arm, int pos)>());
            var armPairs = new[] {
                new (int arm, int pos)[] { (0, 2), (2, 4) },
                new (int arm, int pos)[] { (0, 4), (3, 2) },
                new (int arm, int pos)[] { (1, 2), (3, 4) },
                new (int arm, int pos)[] { (1, 4), (4, 2) },
                new (int arm, int pos)[] { (2, 2), (4, 4) },
                new (int arm, int pos)[] { (0, 6), (1, 0) },
                new (int arm, int pos)[] { (1, 6), (2, 0) },
                new (int arm, int pos)[] { (2, 6), (3, 0) },
                new (int arm, int pos)[] { (3, 6), (4, 0) },
                new (int arm, int pos)[] { (4, 6), (0, 0) }
            };
            var ix = 0;
            var overlaps = new[] { 0, 1, 2, 4, 5, 7 };
            var above = new[] { -1, -1, -1, -1, 0, 1, 2, 4, 5, 7 };
            for (var star = 0; star < 10; star++)
                for (var arm = 0; arm < 5; arm++)
                    for (var pos = 0; pos < 7; pos++)
                    {
                        var p = cells.Take(ix).IndexOf(lst => lst.Any(tup => tup.star == star && armPairs.Any(pair => (pair[0] == (arm, pos) && pair[1] == (tup.arm, tup.pos)) || (pair[1] == (arm, pos) && pair[0] == (tup.arm, tup.pos)))));
                        if (p == -1 && overlaps.Contains(star - 1) && arm == 1 && pos == 6)
                            p = cells.Take(ix).IndexOf(lst => lst.Any(tup => tup.star == star - 1 && tup.arm == 2 && tup.pos == 6));
                        if (p == -1 && above[star] != -1 && arm == 0 && pos == 0)
                            p = cells.Take(ix).IndexOf(lst => lst.Any(tup => tup.star == above[star] && tup.arm == 0 && tup.pos == 6));
                        if (p == -1)
                            cells[ix++] = new List<(int star, int arm, int pos)> { (star, arm, pos) };
                        else
                            cells[p].Add((star, arm, pos));
                    }
            var wordCells = Enumerable.Range(0, 10 * 5).Select(i => Enumerable.Range(0, 7).Select(pos => cells.IndexOf(lst => lst.Any(tup => tup.star == i / 5 && tup.arm == i % 5 && tup.pos == pos))).ToArray()).ToArray();
            var ringCells = Enumerable.Range(0, 10).Select(star => cells.SelectIndexWhere(lst => lst.Any(tup => tup.star == star && tup.pos >= 2 && tup.pos <= 4)).ToArray()).ToArray();
            var joinerCells = cells.SelectIndexWhere(lst => lst.Count >= 4).ToArray();
            return (wordCells, ringCells, joinerCells);
        }

        private static void writeXml(int seed, int[] solution, string filename) => writeXml(seed, solution.Select(i => i.Nullable()).ToArray(), filename);
        private static void writeXml(int seed, int?[] solution, string filename)
        {
            var xml = XDocument.Parse(File.ReadAllText($@"F:\temp\Stars Align (VOCATIONAL)\Star.svg"));
            var textObjs = xml.Descendants().Where(d => d.Name.LocalName == "tspan").ToArray();
            for (var i = 0; i < 238; i++)
            {
                if (solution[i] == null)
                    textObjs[i].Remove();
                else
                    textObjs[i].Value = new string((char) (solution[i] + 'A' - 1), 1);
            }
            xml.Save($@"F:\temp\Stars Align (VOCATIONAL)\Seed {seed}\{filename}.svg", SaveOptions.DisableFormatting);
        }

        public static void FindSolutionsNoGivens(int seed)
        {
            var (wordCells, ringCells, joinerCells) = GetCells();
            var words = File.ReadLines(@"F:\Temp\Stars Align (VOCATIONAL)\Seed 3\Output.txt").Skip(2).Take(10).Select(line => Regex.Match(line, @"^Star \d+: (.*)$")).SelectMany(m => m.Groups[1].Value.Split(", ")).ToArray();
            //var words = @"EXHIBIT,TURMOIL,LEATHER,ROBERTS,STORAGE,SKINNER,REGULAR,REALISM,MANAGED,DOLLARS,STORMED,DIAGRAM,MALCOLM,MAMMALS,SURPLUS,DYNAMIC,CONFORM,MADONNA,AMMONIA,AVOIDED,SLAMMED,DILEMMA,ARREARS,SIMPLER,REMARKS,TRIUMPH,HOSTAGE,EYELIDS,SIMPSON,NEAREST,REPLICA,ARTISTS,SLAPPED,DRIFTED,DESPAIR,DEBATED,DRESSED,DURABLE,EXTREME,ENSURED,CHEESES,SCIENCE,EARNEST,TESTING,GENERIC,HOUSING,GALLONS,STATUES,SHIELDS,STOMACH,AQUATIC,CANCERS,SEIZURE,ESTONIA,AMERICA,DINNERS,SURVIVE,ENHANCE,EYEBROW,WEIGHED,GLAMOUR,READING,GUITARS,SLOGANS,SAILING,CLASSIC,CLIENTS,SETBACK,KISSING,GENETIC,REVISED,DROPPED,DERIVED,DISPOSE,EMPEROR".Split(',');

            // Make sure they all work
            var solutionTemp = new int?[238];
            for (var star = 0; star < 10; star++)
                for (var arm = 0; arm < 5; arm++)
                    for (var ltr = 0; ltr < 7; ltr++)
                    {
                        var ix = wordCells[5 * star + arm][ltr];
                        if (solutionTemp[ix] != null && solutionTemp[ix] != words[5 * star + arm][ltr] - 'A' + 1)
                            Debugger.Break();
                        solutionTemp[ix] = words[5 * star + arm][ltr] - 'A' + 1;
                    }
            var solution = solutionTemp.WhereNotNull().ToArray();
            if (solution.Length != 238)
                Debugger.Break();

            File.WriteAllText($@"F:\temp\Stars Align (VOCATIONAL)\Seed {seed}\Solution raw.txt", solution.Select(i => (char) (i + 'A' - 1)).JoinString());

            // Test uniqueness
            var wordsAsInts = words.Select(w => w.Select(ch => ch - 'A' + 1).ToArray()).ToArray();
            Puzzle makeTestPuzzle(IEnumerable<int> givenIxs)
            {
                var puzzleTest = new Puzzle(238, 1, 26);
                for (var wordIx = 0; wordIx < 10 * 5; wordIx++)
                    puzzleTest.AddConstraint(new CombinationsConstraint(wordCells[wordIx], wordsAsInts));
                puzzleTest.AddConstraint(new DontUseAWordTwiceConstraint(wordCells));
                foreach (var ix in givenIxs)
                    puzzleTest.AddConstraint(new GivenConstraint(ix, solution[ix]));
                return puzzleTest;
            }
            var puzzle = makeTestPuzzle(new int[] { });
            var countSolutions = 0;
            var solutions = new List<string>();
            foreach (var tentativeSolution in puzzle.Solve(new SolverInstructions { ShowContinuousProgress = 10 }))
            {
                countSolutions++;
                Console.WriteLine($"{countSolutions} solutions found.");
                solutions.Add(tentativeSolution.Select(i => (char) (i + 'A' - 1)).JoinString());
                File.WriteAllLines($@"F:\temp\Stars Align (VOCATIONAL)\Seed {seed}\Solutions no givens.txt", solutions);
                writeXml(3, tentativeSolution, $"Reconstructed solution {countSolutions}");
            }
            Console.WriteLine();
            Console.WriteLine($"{countSolutions} solutions in total.");
        }

        public static void ExamineSolutions(int seed)
        {
            var correctSolution = File.ReadAllText($@"F:\temp\Stars Align (VOCATIONAL)\Seed {seed}\Solution raw.txt");
            var solutions = File.ReadAllLines($@"F:\temp\Stars Align (VOCATIONAL)\Seed {seed}\Solutions no givens.txt");
            if (correctSolution.Length != 238)
                Debugger.Break();

            for (var i = 0; i < 238; i++)
            {
                var ch = correctSolution[i];
                var count = solutions.Count(s => s[i] == ch);
                if (count == 1)
                    Console.WriteLine($"Char {i} ({correctSolution[i]}) is unique");
            }
        }

        public static void FindIndividualStars()
        {
            var words = @"AMOUNTS,ARREARS,ARTICLE,CEILING,CENTRES,CHICAGO,DECLINE,DEFENCE,DESIRES,DEVIOUS,DISRUPT,DOMINIC,DONATED,DRAWERS,DROUGHT,DYNAMIC,EARNEST,ELEGANT,ESTONIA,EXISTED,GARDNER,GRANTED,HARBOUR,HOTTAKE,LENDING,LEONARD,LIAISON,MALCOLM,MATILDA,MELISSA,MINIMUM,NATIVES,NEUTRAL,OPTIMAL,REACTED,REGARDS,REMAINS,SKILLED,SPANISH,SPONSOR,SQUARED,STADIUM,STEWARD,STIRRED,SUSTAIN,SYMPTOM,THROUGH,TRAFFIC,TURMOIL,TWISTED".Split(',');
            IEnumerable<string[][]> recurse(string[][] sofar, string[] remainingWords)
            {
                if (sofar.Length == 10)
                {
                    yield return sofar;
                    yield break;
                }

                for (var a = sofar.Length == 0 ? 0 : remainingWords.IndexOf(w => w.CompareTo(sofar.Last()[0]) > 0); a < remainingWords.Length; a++)
                {
                    if (a == -1)
                        yield break;
                    for (var b = a + 1; b < remainingWords.Length; b++)
                        if (remainingWords[b][0] == remainingWords[a][6])
                            for (var c = a + 1; c < remainingWords.Length; c++)
                                if (c != b && remainingWords[c][0] == remainingWords[b][6] && remainingWords[c][4] == remainingWords[a][2])
                                    for (var d = a + 1; d < remainingWords.Length; d++)
                                        if (d != b && d != c && remainingWords[d][0] == remainingWords[c][6] && remainingWords[d][2] == remainingWords[a][4] && remainingWords[d][4] == remainingWords[b][2])
                                            for (var e = a + 1; e < remainingWords.Length; e++)
                                                if (e != b && e != c && e != d && remainingWords[e][0] == remainingWords[d][6] && remainingWords[e][2] == remainingWords[b][4] && remainingWords[e][4] == remainingWords[c][2] && remainingWords[e][6] == remainingWords[a][0])
                                                {
                                                    var newRemainingWords = Enumerable.Range(0, remainingWords.Length).Where(x => x != a && x != b && x != c && x != d && x != e).Select(ix => remainingWords[ix]).ToArray();
                                                    var newSofar = sofar.Insert(sofar.Length, new[] { remainingWords[a], remainingWords[b], remainingWords[c], remainingWords[d], remainingWords[e] });
                                                    foreach (var solution in recurse(newSofar, newRemainingWords))
                                                        yield return solution;
                                                }
                }
            }

            var c = 0;
            foreach (var solution in recurse(new string[0][], words))
            {
                Console.WriteLine(solution.Select((arr, starIx) => arr.Select(word => $"{word} {starIx + 1}").JoinString("\n")).JoinString("\n"));
                Console.WriteLine();
                c++;
            }
            Console.WriteLine(c);
        }

        public static void CreateSolverSVG(int seed)
        {
            double cos(double x) => Math.Cos(x * Math.PI / 180);
            double sin(double x) => Math.Sin(x * Math.PI / 180);
            var correctSolution = File.ReadAllText($@"F:\temp\Stars Align (VOCATIONAL)\Seed {seed}\Solution raw.txt");
            var (wordCells, ringCells, joinerCells) = GetCells();
            var rnd = new Random(8472);
            var pentaLetters = Enumerable.Range(0, 10)
                .Select(star => Enumerable.Range(0, 5).Select(arm => correctSolution[wordCells[star * 5 + arm][0]]).JoinString())
                .Select(penta => { var offset = rnd.Next(0, 5); return penta.Substring(offset) + penta.Substring(0, offset); })
                .ToArray()
                .Shuffle(rnd);
            Console.WriteLine(pentaLetters.SelectMany(c => c).GroupBy(c => c).OrderBy(g => g.Count()).Select(gr => $"{gr.Key} = {gr.Count()}").JoinString("\n"));
            var pentagrams = pentaLetters.Select((ltr, pentIx) =>
            {
                PointD pt(int ix) => new PointD(cos(144 * ix - 90), sin(144 * ix - 90)) * 10;
                var pts = Enumerable.Range(0, 5).Select(pt);
                var path = $"<path d='M{pts.Select(p => $"{p.X} {p.Y}").JoinString(" ")}z' fill='none' stroke='black' stroke-width='.2' />";
                var circles = pts.Select((c, ix) => $"<circle cx='{c.X}' cy='{c.Y}' fill='#88aaff' stroke='none' r='3' /><text x='{c.X}' y='{c.Y + 1.5}' text-anchor='middle' font-size='4.5' font-family='Work Sans'>{ltr[ix]}</text>");
                return $"<g transform='translate({(pentIx % 4) * 30}, {(pentIx / 4) * 25})'>{path}{circles.JoinString()}</g>";
            });
            File.WriteAllText($@"F:\temp\Stars Align (VOCATIONAL)\Seed {seed}\Solve test.svg",
                $@"<svg viewBox='0 0 100 100'>{pentagrams.JoinString()}</svg>");
        }
    }
}