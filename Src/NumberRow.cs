using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using PuzzleSolvers;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace PuzzleStuff
{
    static class NumberRow
    {
        sealed class LeftOfPositionConstraint : RelativeToPositionConstraint { public LeftOfPositionConstraint(int value, int position) : base(value, position, true) { } }
        sealed class RightOfPositionConstraint : RelativeToPositionConstraint { public RightOfPositionConstraint(int value, int position) : base(value, position, false) { } }

        abstract class RelativeToPositionConstraint : Constraint
        {
            public int Value { get; private set; }
            public int Position { get; private set; }
            public bool IsLeft { get; private set; }
            public RelativeToPositionConstraint(int value, int position, bool isLeft) : base(null) { Value = value; Position = position; IsLeft = isLeft; }
            public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
            {
                // If the required value is already present, we’re good
                if (ix != null && (IsLeft ? (ix.Value < Position) : (ix.Value > Position)) && grid[ix.Value].Value + minValue == Value)
                    return Enumerable.Empty<Constraint>();

                // If there is only one unfilled cell left of the position, it needs to have this value
                var unmarkedCells = Enumerable.Range(IsLeft ? 0 : Position + 1, IsLeft ? Position : grid.Length - Position - 1).Where(i => grid[i] == null).ToArray();
                if (unmarkedCells.Length == 1)
                {
                    for (var v = 0; v < takens[unmarkedCells[0]].Length; v++)
                        if (v + minValue != Value)
                            takens[unmarkedCells[0]][v] = true;
                }
                return null;
            }
        }

        sealed class LeftOfNumberConstraint : Constraint
        {
            public int Value1 { get; private set; }
            public int Value2 { get; private set; }
            public LeftOfNumberConstraint(int v1, int v2) : base(null) { Value1 = v1; Value2 = v2; }
            public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
            {
                if (ix == null)
                    return null;
                if (grid[ix.Value].Value == Value1)
                    for (var i = 0; i < ix.Value; i++)
                        takens[i][Value2 - minValue] = true;
                if (grid[ix.Value].Value == Value2)
                    for (var i = ix.Value + 1; i < grid.Length; i++)
                        takens[i][Value1 - minValue] = true;
                return null;
            }
        }

        enum MinMaxMode { Min, Max }
        sealed class NotMinMaxConstraint : Constraint
        {
            public MinMaxMode Mode { get; private set; }
            public NotMinMaxConstraint(int cell, MinMaxMode mode) : base(new[] { cell }) { Mode = mode; }
            public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
            {
                if (ix == null)
                    return null;

                // NOTE: ‘min’ and ‘max’ are the min and max values in ‘grid’, NOT the actual min and max values
                int remainingCell = -1, min = int.MaxValue, max = int.MinValue;
                for (var i = 0; i < grid.Length; i++)
                {
                    if (grid[i] == null)
                    {
                        if (remainingCell == -1)
                            remainingCell = i;
                        else
                            return null;
                    }
                    else
                    {
                        if (grid[i].Value < min)
                            min = grid[i].Value;
                        if (grid[i].Value > max)
                            max = grid[i].Value;
                    }
                }

                if (remainingCell == AffectedCells[0])
                {
                    for (var v = 0; v < takens[remainingCell].Length; v++)
                        if ((Mode == MinMaxMode.Min) ? (v <= min) : (v >= max))
                            takens[remainingCell][v] = true;
                }
                else if (grid[AffectedCells[0]].Value == ((Mode == MinMaxMode.Min) ? min : max))
                {
                    for (var v = 0; v < takens[remainingCell].Length; v++)
                        if ((Mode == MinMaxMode.Min) ? (v >= min) : (v <= max))
                            takens[remainingCell][v] = true;
                }

                return Enumerable.Empty<Constraint>();
            }
        }

        sealed class MinMaxConstraint : Constraint
        {
            public MinMaxMode Mode { get; private set; }
            public MinMaxConstraint(int cell, MinMaxMode mode) : base(new[] { cell }) { Mode = mode; }
            public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
            {
                if (ix == null)
                    return null;

                if (ix.Value == AffectedCells[0])
                {
                    // Cell affected by the constraint is decided: everything else can’t be smaller/greater
                    for (var i = 0; i < grid.Length; i++)
                        if (grid[i] == null)
                            for (var v = 0; v < takens[i].Length; v++)
                                if ((Mode == MinMaxMode.Min) ? (v < grid[AffectedCells[0]].Value) : (v > grid[AffectedCells[0]].Value))
                                    takens[i][v] = true;
                    return Enumerable.Empty<Constraint>();
                }
                else
                {
                    // Another cell was filled in: the affected cell can’t be greater/smaller
                    var value = grid[ix.Value].Value;
                    for (var v = 0; v < takens[AffectedCells[0]].Length; v++)
                        if ((Mode == MinMaxMode.Min) ? (v > value) : (v < value))
                            takens[AffectedCells[0]][v] = true;
                    return null;
                }
            }
        }

        struct ValueCounter
        {
            public int Min;
            public int Max;
            public int Total;
            public void Count(int value)
            {
                if (value < Min) Min = value;
                if (value > Max) Max = value;
                Total += value;
            }

            public void AddToTable(TextTable tt, int row, string label, int num)
            {
                tt.SetCell(0, row, label);
                tt.SetCell(1, row, Min.ToString());
                tt.SetCell(3, row, $"{Total / (double) num:0.#}");
                tt.SetCell(2, row, Max.ToString());
            }
        }

        struct DoubleValueCounter
        {
            public double Min;
            public double Max;
            public double Total;
            public void Count(double value)
            {
                if (value < Min) Min = value;
                if (value > Max) Max = value;
                Total += value;
            }

            public void AddToTable(TextTable tt, int row, string label, int num)
            {
                tt.SetCell(0, row, label);
                tt.SetCell(1, row, $"{Min:0.#}");
                tt.SetCell(3, row, $"{Total / num:0.#}");
                tt.SetCell(2, row, $"{Max:0.#}");
            }
        }

        public static void Do()
        {
            const int min = 1;  // inclusive
            const int max = 27; // exclusive
            const int totalSeeds = 500;

            var words = File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt").Where(w => w.Length == 7 && w.All(ch => ch >= 'A' && ch <= 'Z')).ToArray();
            var dic = new Dictionary<string, int>();
            var timeCount = new DoubleValueCounter();
            var constraintsCount = new ValueCounter();
            var attemptsCount = new ValueCounter();

            var privilegedGroups = new[] { "# < ▲ < #", "#¬|", "< #", "> #", "¬#", "¬largest", "¬prime", "¬smallest", "¬square", "¬|concat", "< ▲", "even", "odd", "prime" };

            Enumerable.Range(1, totalSeeds).ParallelForEach(seed =>
            {
                var startTime = DateTime.UtcNow;
                var rnd = new Random(seed);
                var solution = words.PickRandom(rnd).Select(ch => ch - 'A' + 1).ToArray();
                var n = solution.Length;
                var numAttempts = 0;
                tryAgain:
                numAttempts++;
                var allConstraints = new List<(Constraint constraint, string group, string name)>();

                Puzzle makePuzzle(IEnumerable<Constraint> cs) => new Puzzle(n, min, max - 1, cs);

                static Constraint differenceConstraint(int cell1, int cell2, int diff) => new TwoCellLambdaConstraint(cell1, cell2, (a, b) => Math.Abs(a - b) == diff);
                static Constraint quotientConstraint(int cell1, int cell2, int quotient) => new TwoCellLambdaConstraint(cell1, cell2, (a, b) => a * quotient == b || b * quotient == a);
                static Constraint moduloDiffConstraint(int cell1, int cell2, int modulo) => new TwoCellLambdaConstraint(cell1, cell2, (a, b) => a % modulo == b % modulo);
                static Constraint moduloConstraint(int cell1, int cell2, int modulo) => new TwoCellLambdaConstraint(cell1, cell2, (a, b) => b != 0 && a % b == modulo);

                // Relations between two numbers (symmetric)
                for (var i = 0; i < n; i++)
                    for (var j = 0; j < n; j++)
                        if (j != i)
                        {
                            if (j > i)
                            {
                                for (var m = 2; m < max / 2; m++)
                                    if (solution[i] % m == solution[j] % m)
                                        allConstraints.Add((moduloDiffConstraint(i, j, m), "same %", $"{(char) (i + 'A')} is a multiple of {m} away from {(char) (j + 'A')}."));

                                allConstraints.Add((new SumConstraint(solution[i] + solution[j], new[] { i, j }), "+ #", $"The sum of {(char) (i + 'A')} and {(char) (j + 'A')} is {solution[i] + solution[j]}."));
                                allConstraints.Add((new ProductConstraint(solution[i] * solution[j], new[] { i, j }), "× #", $"The product of {(char) (i + 'A')} and {(char) (j + 'A')} is {solution[i] * solution[j]}."));
                                if (Math.Abs(solution[i] - solution[j]) < 6)
                                    allConstraints.Add((differenceConstraint(i, j, Math.Abs(solution[i] - solution[j])), "− #", $"The absolute difference of {(char) (i + 'A')} and {(char) (j + 'A')} is {Math.Abs(solution[i] - solution[j])}."));
                                if (solution[j] != 0 && solution[i] % solution[j] == 0 && solution[i] / solution[j] < 4)
                                    allConstraints.Add((quotientConstraint(i, j, solution[i] / solution[j]), "÷ #", $"Of {(char) (i + 'A')} and {(char) (j + 'A')}, one is {solution[i] / solution[j]} times the other."));
                                if (solution[i] != 0 && solution[j] % solution[i] == 0 && solution[j] / solution[i] < 4)
                                    allConstraints.Add((quotientConstraint(i, j, solution[j] / solution[i]), "÷ #", $"Of {(char) (i + 'A')} and {(char) (j + 'A')}, one is {solution[j] / solution[i]} times the other."));
                            }
                            if (solution[j] != 0)
                                allConstraints.Add((moduloConstraint(i, j, solution[i] % solution[j]), "% #", $"{(char) (i + 'A')} modulo {(char) (j + 'A')} is {solution[i] % solution[j]}."));
                        }

                // Relations between two numbers (asymmetric)
                for (var i = 0; i < n; i++)
                    for (var j = 0; j < n; j++)
                    {
                        if (i == j)
                            continue;

                        if (solution[i] < solution[j])
                            allConstraints.Add((new TwoCellLambdaConstraint(i, j, (a, b) => a < b), "< ▲", $"{(char) (i + 'A')} is less than {(char) (j + 'A')}."));

                        var concat = int.Parse($"{solution[i]}{solution[j]}");
                        foreach (var m in new[] { 3, 4, 6, 7, 8, 9, 11 })   // beware lambdas
                            if (concat % m == 0)
                                allConstraints.Add((new TwoCellLambdaConstraint(i, j, (a, b) => int.Parse($"{a}{b}") % m == 0), "|concat", $"The concatenation of {(char) (i + 'A')}{(char) (j + 'A')} is divisible by {m}."));
                            else
                                allConstraints.Add((new TwoCellLambdaConstraint(i, j, (a, b) => int.Parse($"{a}{b}") % m != 0), "¬|concat", $"The concatenation of {(char) (i + 'A')}{(char) (j + 'A')} is not divisible by {m}."));
                    }

                // Relations between three numbers
                for (var i = 0; i < n; i++)
                    for (var j = 0; j < n; j++)
                        if (j != i)
                            for (var k = 0; k < n; k++)
                                if (k != i && k != j)
                                {
                                    if (j > i && solution[i] + solution[j] == solution[k])
                                        allConstraints.Add((new ThreeCellLambdaConstraint(i, j, k, (a, b, c) => a + b == c), "sum ▲", $"{(char) (i + 'A')} + {(char) (j + 'A')} = {(char) (k + 'A')}"));
                                    if (j > i && solution[i] * solution[j] == solution[k])
                                        allConstraints.Add((new ThreeCellLambdaConstraint(i, j, k, (a, b, c) => a * b == c), "product ▲", $"{(char) (i + 'A')} × {(char) (j + 'A')} = {(char) (k + 'A')}"));
                                    if (solution[j] != 0 && solution[i] % solution[j] == solution[k])
                                        allConstraints.Add((new ThreeCellLambdaConstraint(i, j, k, (a, b, c) => a % b == c), "mod ▲", $"{(char) (i + 'A')} modulo {(char) (j + 'A')} = {(char) (k + 'A')}"));
                                }

                var minVal = solution.Min();
                var maxVal = solution.Max();

                // Value relation constraints
                for (var i = 0; i < n; i++)
                {
                    foreach (var v in Enumerable.Range(min, max - min)) // don’t use ‘for’ loop because the variable is captured by lambdas
                        if (solution[i] < v - 1)
                            allConstraints.Add((new OneCellLambdaConstraint(i, a => a < v), "< #", $"{(char) (i + 'A')} is less than {v}."));
                        else if (solution[i] > v + 1)
                            allConstraints.Add((new OneCellLambdaConstraint(i, a => a > v), "> #", $"{(char) (i + 'A')} is greater than {v}."));
                    allConstraints.Add(solution[i] == minVal
                        ? ((Constraint) new MinMaxConstraint(i, MinMaxMode.Min), "smallest", $"{(char) (i + 'A')} has the smallest value.")
                        : (new NotMinMaxConstraint(i, MinMaxMode.Min), "¬smallest", $"{(char) (i + 'A')} does not have the smallest value."));
                    allConstraints.Add(solution[i] == maxVal
                        ? ((Constraint) new MinMaxConstraint(i, MinMaxMode.Max), "largest", $"{(char) (i + 'A')} has the largest value.")
                        : (new NotMinMaxConstraint(i, MinMaxMode.Max), "¬largest", $"{(char) (i + 'A')} does not have the largest value."));
                }

                // Position constraints
                for (var i = 0; i < n; i++)
                    for (var j = 0; j < n; j++)
                        if (i < j - 1)
                            allConstraints.Add((new LeftOfPositionConstraint(solution[i], j), "# ← ▲", $"There is a {solution[i]} further left than {(char) (j + 'A')}."));
                        else if (i > j + 1)
                            allConstraints.Add((new RightOfPositionConstraint(solution[i], j), "▲ → #", $"There is a {solution[i]} further right than {(char) (j + 'A')}."));

                // Numerical properties of a single value
                var primes = new[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199 };
                var squares = Enumerable.Range(0, 100).Select(i => i * i).ToArray();
                for (var i = 0; i < n; i++)
                {
                    allConstraints.Add(primes.Contains(solution[i])
                        ? (new OneCellLambdaConstraint(i, a => primes.Contains(a)), "prime", $"{(char) (i + 'A')} is a prime number.")
                        : (new OneCellLambdaConstraint(i, a => !primes.Contains(a)), "¬prime", $"{(char) (i + 'A')} is not a prime number."));
                    allConstraints.Add(squares.Contains(solution[i])
                        ? (new OneCellLambdaConstraint(i, a => squares.Contains(a)), "square", $"{(char) (i + 'A')} is a square number.")
                        : (new OneCellLambdaConstraint(i, a => !squares.Contains(a)), "¬square", $"{(char) (i + 'A')} is not a square number."));
                    allConstraints.Add(solution[i] % 2 == 0
                        ? (new OneCellLambdaConstraint(i, a => a % 2 == 0), "even", $"{(char) (i + 'A')} is an even number.")
                        : (new OneCellLambdaConstraint(i, a => a % 2 != 0), "odd", $"{(char) (i + 'A')} is an odd number."));
                    foreach (var m in Enumerable.Range(3, 5))   // don’t use ‘for’ loop because the value is captured by lambdas
                        allConstraints.Add(solution[i] % m == 0
                            ? (new OneCellLambdaConstraint(i, a => a % m == 0), "#|", $"{(char) (i + 'A')} is divisible by {m}.")
                            : (new OneCellLambdaConstraint(i, a => a % m != 0), "#¬|", $"{(char) (i + 'A')} is not divisible by {m}."));
                }

                // Presence and absence of values
                foreach (var v in Enumerable.Range(min, max - min)) // don’t use ‘for’ loop because the value is captured by lambdas
                    if (!solution.Contains(v))
                        allConstraints.Add((new LambdaConstraint((taken, grid, ix, mv, mxv) =>
                        {
                            if (ix == null)
                                foreach (var arr in taken)
                                    arr[v - mv] = true;
                            return null;
                        }), "¬#", $"There is no {v}."));

                static Constraint betweenConstraint(int low, int high) => new LambdaConstraint((taken, grid, ix, mv, mxv) =>
                {
                    if (ix == null)
                        return null;
                    int remainingCell = -1, numRemaining = 0;
                    for (var i = 0; i < grid.Length; i++)
                        if (grid[i] == null)
                        {
                            numRemaining++;
                            remainingCell = i;
                        }
                        else if (grid[i].Value + mv >= low && grid[i].Value + mv <= high)
                            return Enumerable.Empty<Constraint>();
                    if (numRemaining != 1)
                        return null;
                    for (var v = 0; v < taken[remainingCell].Length; v++)
                        if (v + mv < low || v + mv > high)
                            taken[remainingCell][v] = true;
                    return null;
                });

                for (var low = min; low <= max; low++)
                    for (var high = low + 1; high <= max; high++)
                    {
                        if (solution.Any(v => v >= low && v <= high))
                            allConstraints.Add((betweenConstraint(low, high), "# < ▲ < #", $"There is a value between {low} and {high}."));
                        if (solution.Any(v => v < low && v > high))
                            allConstraints.Add((betweenConstraint(low, high), "▲ < #–# < ▲", $"There is a value outside of {low} to {high}."));
                    }

                // Group the constraints
                var constraintGroups = allConstraints.GroupBy(c => c.group).Select(gr => gr.ToList()).ToList();

                // Choose one constraint from each group (50% chance for deprivileged groups)
                var constraints = new List<(Constraint constraint, string group, string name)>();
                foreach (var gr in constraintGroups)
                {
                    var ix = rnd.Next(0, gr.Count);
                    constraints.Add(gr[ix]);
                    gr.RemoveAt(ix);
                }
                var constraintDic = constraintGroups.Where(gr => gr.Count > 0 && privilegedGroups.Contains(gr[0].group)).ToDictionary(gr => gr[0].group);
                //Console.WriteLine($"Got {constraints.Count} constraints (one from each group).");

                // Add more constraints if this is not unique
                var addedCount = 0;
                do
                {
                    foreach (var kvp in constraintDic)
                    {
                        if (kvp.Value.Count == 0)
                            continue;
                        addedCount++;
                        var ix = rnd.Next(0, kvp.Value.Count);
                        constraints.Add(kvp.Value[ix]);
                        kvp.Value.RemoveAt(ix);
                    }
                }
                while (makePuzzle(constraints.Select(c => c.constraint)).Solve().Skip(1).Any());
                //Console.WriteLine($"Added {addedCount} extra constraints, now have {constraints.Count}.");
                Console.WriteLine($"Seed: {seed,10} - extra: {addedCount}");

                // Reduce the set of constraints again
                var req = Ut.ReduceRequiredSet(
                    constraints.Select((c, ix) => (c.constraint, c.group, c.name, ix)).ToArray().Shuffle(rnd),
                    set => !makePuzzle(set.SetToTest.Select(c => c.constraint)).Solve().Skip(1).Any()).ToArray();
                //Console.WriteLine($"Left with {req.Length} constraints after reduce.");

                if (req.Length > 16)
                {
                    //Console.WriteLine("Trying again...");
                    goto tryAgain;
                }

                lock (dic)
                {
                    foreach (var group in req.Select(c => c.group).Distinct())
                        dic.IncSafe(group);

                    timeCount.Count((DateTime.UtcNow - startTime).TotalSeconds);
                    constraintsCount.Count(req.Length);
                    attemptsCount.Count(numAttempts);

                    //Console.WriteLine($"There are {n} positions (labeled A–{(char) ('A' + n - 1)}) containing digits {min}–{max - 1}.");
                    //foreach (var (constraint, group, name, ix) in req.OrderBy(t => t.name))
                    //    Console.WriteLine($"{name}");
                    //Console.WriteLine();
                    //Console.ReadLine();
                    //foreach (var sol in makePuzzle(req.Select(c => c.constraint)).Solve())
                    //    Console.WriteLine(sol.JoinString(", "));
                }
            });

            Console.WriteLine();
            //var groupTotal = dic.Sum(p => p.Value);
            foreach (var kvp in dic.OrderBy(p => p.Value))
                ConsoleUtil.WriteLine($"{kvp.Key,10} = {kvp.Value * 100 / (double) totalSeeds,4:0.0}% {new string('▒', kvp.Value * 100 / totalSeeds)}".Color(privilegedGroups.Contains(kvp.Key) ? ConsoleColor.Yellow : ConsoleColor.Gray));
            Console.WriteLine();

            var tt = new TextTable { ColumnSpacing = 2 };
            tt.SetCell(1, 0, "Min");
            tt.SetCell(3, 0, "Avg");
            tt.SetCell(2, 0, "Max");
            timeCount.AddToTable(tt, 1, "Time (sec)", totalSeeds);
            attemptsCount.AddToTable(tt, 2, "Attempts", totalSeeds);
            constraintsCount.AddToTable(tt, 3, "Constraints", totalSeeds);
            tt.WriteToConsole();
        }
    }
}