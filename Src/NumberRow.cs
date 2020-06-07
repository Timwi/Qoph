using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using PuzzleSolvers;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

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

        public static void Do()
        {
            const int min = 1;  // inclusive
            const int max = 27; // exclusive

            var words = File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt").Where(w => w.Length == 7 && w.All(ch => ch >= 'A' && ch <= 'Z')).ToArray();
            var dic = new Dictionary<char, int>();
            TimeSpan minTime = TimeSpan.MaxValue, maxTime = TimeSpan.MinValue, totalTime = TimeSpan.Zero;

            for (var seed = 1; seed <= 100; seed++)
            {
                Console.WriteLine($"Seed: {seed}");
                var startTime = DateTime.UtcNow;
                var rnd = new Random(seed);
                //var solution = Enumerable.Range(min, max - min).ToArray().Shuffle(rnd).Take(n).ToArray();
                var solution = words.PickRandom(rnd).Select(ch => ch - 'A' + 1).ToArray();
                Console.WriteLine(solution.Select(ch => (char) ('A' + ch - 1)).JoinString());
                var n = solution.Length;
                tryAgain:
                var constraints = new List<(Constraint constraint, string name)>();

                Puzzle makePuzzle(IEnumerable<Constraint> cs) => new Puzzle(n, min, max - 1, cs);

                static Constraint differenceConstraint(int cell1, int cell2, int diff) => new TwoCellLambdaConstraint(cell1, cell2, (a, b) => Math.Abs(a - b) == diff);
                static Constraint quotientConstraint(int cell1, int cell2, int quotient) => new TwoCellLambdaConstraint(cell1, cell2, (a, b) => a * quotient == b || b * quotient == a);
                static Constraint moduloConstraint(int cell1, int cell2, int modulo) => new TwoCellLambdaConstraint(cell1, cell2, (a, b) => a % modulo == b % modulo);

                // Relations between two numbers (symmetric)
                for (var i = 0; i < n; i++)
                    for (var j = i + 1; j < n; j++)
                    {
                        for (var m = 2; m < max / 2; m++)
                            if (solution[i] % m == solution[j] % m)
                                constraints.Add((moduloConstraint(i, j, m), $"A: {(char) (i + 'A')} is a multiple of {m} away from {(char) (j + 'A')}."));

                        constraints.Add((new SumConstraint(solution[i] + solution[j], new[] { i, j }), $"B: The sum of {(char) (i + 'A')} and {(char) (j + 'A')} is {solution[i] + solution[j]}."));
                        constraints.Add((new ProductConstraint(solution[i] * solution[j], new[] { i, j }), $"C: The product of {(char) (i + 'A')} and {(char) (j + 'A')} is {solution[i] * solution[j]}."));
                        if (Math.Abs(solution[i] - solution[j]) < 6)
                            constraints.Add((differenceConstraint(i, j, Math.Abs(solution[i] - solution[j])), $"D: The absolute difference of {(char) (i + 'A')} and {(char) (j + 'A')} is {Math.Abs(solution[i] - solution[j])}."));
                        if (solution[j] != 0 && solution[i] % solution[j] == 0 && solution[i] / solution[j] < 4)
                            constraints.Add((quotientConstraint(i, j, solution[i] / solution[j]), $"E: Of {(char) (i + 'A')} and {(char) (j + 'A')}, one is {solution[i] / solution[j]} times the other."));
                        if (solution[i] != 0 && solution[j] % solution[i] == 0 && solution[j] / solution[i] < 4)
                            constraints.Add((quotientConstraint(i, j, solution[j] / solution[i]), $"E: Of {(char) (i + 'A')} and {(char) (j + 'A')}, one is {solution[j] / solution[i]} times the other."));
                    }

                // Relations between two numbers (asymmetric)
                for (var i = 0; i < n; i++)
                    for (var j = 0; j < n; j++)
                    {
                        if (i == j)
                            continue;

                        if (solution[i] < solution[j])
                            constraints.Add((new TwoCellLambdaConstraint(i, j, (a, b) => a < b), $"F: {(char) (i + 'A')} is less than {(char) (j + 'A')}."));

                        var concat = int.Parse($"{solution[i]}{solution[j]}");
                        foreach (var m in new[] { 3, 4, 6, 7, 8, 9, 11 })   // beware lambdas
                            if (concat % m == 0)
                                constraints.Add((new TwoCellLambdaConstraint(i, j, (a, b) => int.Parse($"{a}{b}") % m == 0), $"G: The concatenation of {(char) (i + 'A')}{(char) (j + 'A')} is divisible by {m}."));
                        //else
                        //    constraints.Add((new TwoCellLambdaConstraint(i, j, (a, b) => int.Parse($"{a}{b}") % m != 0), $"H: The concatenation of {(char) (i + 'A')}{(char) (j + 'A')} is not divisible by {m}."));
                    }

                // Relations between three numbers
                for (var i = 0; i < n; i++)
                    for (var j = i + 1; j < n; j++)
                        for (var k = 0; k < n; k++)
                            if (k != i && k != j)
                            {
                                if (solution[i] + solution[j] == solution[k])
                                    constraints.Add((new ThreeCellLambdaConstraint(i, j, k, (a, b, c) => a + b == c), $"I: {(char) (i + 'A')} + {(char) (j + 'A')} = {(char) (k + 'A')}"));
                                if (solution[i] * solution[j] == solution[k])
                                    constraints.Add((new ThreeCellLambdaConstraint(i, j, k, (a, b, c) => a * b == c), $"J: {(char) (i + 'A')} × {(char) (j + 'A')} = {(char) (k + 'A')}"));
                            }

                var minVal = solution.Min();
                var maxVal = solution.Max();

                // Value relation constraints
                for (var i = 0; i < n; i++)
                {
                    foreach (var v in Enumerable.Range(min, max - min)) // don’t use ‘for’ loop because the variable is captured by lambdas
                        if (solution[i] < v - 1)
                            constraints.Add((new OneCellLambdaConstraint(i, a => a < v), $"K: {(char) (i + 'A')} is less than {v}."));
                        else if (solution[i] > v + 1)
                            constraints.Add((new OneCellLambdaConstraint(i, a => a > v), $"K: {(char) (i + 'A')} is greater than {v}."));
                    constraints.Add(solution[i] == minVal
                        ? ((Constraint) new MinMaxConstraint(i, MinMaxMode.Min), $"L: {(char) (i + 'A')} has the smallest value.")
                        : (new NotMinMaxConstraint(i, MinMaxMode.Min), $"M: {(char) (i + 'A')} does not have the smallest value."));
                    constraints.Add(solution[i] == maxVal
                        ? ((Constraint) new MinMaxConstraint(i, MinMaxMode.Max), $"N: {(char) (i + 'A')} has the largest value.")
                        : (new NotMinMaxConstraint(i, MinMaxMode.Max), $"O: {(char) (i + 'A')} does not have the largest value."));
                }

                // Position constraints
                for (var i = 0; i < n; i++)
                    for (var j = 0; j < n; j++)
                        if (i < j - 1)
                            constraints.Add((new LeftOfPositionConstraint(solution[i], j), $"P: There is a {solution[i]} further left than {(char) (j + 'A')}."));
                        else if (i > j + 1)
                            constraints.Add((new RightOfPositionConstraint(solution[i], j), $"P: There is a {solution[i]} further right than {(char) (j + 'A')}."));

                // Numerical properties of a single value
                var primes = new[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199 };
                var squares = Enumerable.Range(0, 100).Select(i => i * i).ToArray();
                for (var i = 0; i < n; i++)
                {
                    constraints.Add(primes.Contains(solution[i])
                        ? (new OneCellLambdaConstraint(i, a => primes.Contains(a)), $"Q: {(char) (i + 'A')} is a prime number.")
                        : (new OneCellLambdaConstraint(i, a => !primes.Contains(a)), $"R: {(char) (i + 'A')} is not a prime number."));
                    constraints.Add(squares.Contains(solution[i])
                        ? (new OneCellLambdaConstraint(i, a => squares.Contains(a)), $"S: {(char) (i + 'A')} is a square number.")
                        : (new OneCellLambdaConstraint(i, a => !squares.Contains(a)), $"T: {(char) (i + 'A')} is not a square number."));
                    constraints.Add(solution[i] % 2 == 0
                        ? (new OneCellLambdaConstraint(i, a => a % 2 == 0), $"U: {(char) (i + 'A')} is an even number.")
                        : (new OneCellLambdaConstraint(i, a => a % 2 != 0), $"V: {(char) (i + 'A')} is an odd number."));
                    constraints.Add(solution[i] % 3 == 0
                        ? (new OneCellLambdaConstraint(i, a => a % 3 == 0), $"W: {(char) (i + 'A')} is divisible by 3.")
                        : (new OneCellLambdaConstraint(i, a => a % 3 != 0), $"X: {(char) (i + 'A')} is not divisible by 3."));
                }

                foreach (var v in Enumerable.Range(min, max - min)) // don’t use ‘for’ loop because the value is captured by lambdas
                    if (!solution.Contains(v))
                        constraints.Add((new LambdaConstraint((taken, grid, ix, mv, mxv) =>
                        {
                            if (ix == null)
                                foreach (var arr in taken)
                                    arr[v - mv] = true;
                            return null;
                        }), $"Y: There is no {v}."));

                //File.WriteAllLines(@"D:\temp\temp.txt", constraints.Select(c => c.name));

                //var already = new Dictionary<string, ((Constraint constraint, string name, int ix)[], bool)>();
                var req = Ut.ReduceRequiredSet(constraints.Select((c, ix) => (c.constraint, c.name, ix)).ToArray().Shuffle(rnd), test: set =>
                {
                    var result = !makePuzzle(set.SetToTest.Select(c => c.constraint)).Solve().Skip(1).Any();

                    //var arr = new bool[constraints.Count];
                    //foreach (var (_, _, ix) in set.SetToTest)
                    //    arr[ix] = true;
                    //var str = arr.Select(b => b ? "█" : "░").JoinString();
                    ////ConsoleUtil.WriteLine((str + (result ? " Y" : " N")).Color(result ? ConsoleColor.Green : ConsoleColor.Red));
                    //if (already.ContainsKey(str) && already[str].Item2 != result)
                    //{
                    //    Console.WriteLine("SET 1:");
                    //    foreach (var (constraint, name, ix) in already[str].Item1)
                    //        Console.WriteLine($"{ix}. {name}");
                    //    Console.WriteLine();
                    //    Console.WriteLine("SET 2:");
                    //    foreach (var (constraint, name, ix) in set.SetToTest)
                    //        Console.WriteLine($"{ix}. {name}");
                    //    System.Diagnostics.Debugger.Break();
                    //}
                    //already[str] = (set.SetToTest.ToArray(), result);
                    return result;
                }).ToArray();

                if (req.Length < 7)
                {
                    Console.WriteLine("Trying again");
                    goto tryAgain;
                }

                lock (dic)
                {
                    foreach (var (constraint, name, ix) in req)
                        dic.IncSafe(name[0]);

                    var took = DateTime.UtcNow - startTime;
                    if (took > maxTime)
                        maxTime = took;
                    if (took < minTime)
                        minTime = took;
                    totalTime += took;
                }

                //Console.WriteLine($"There are {n} positions (labeled A–{(char) ('A' + n - 1)}) containing digits {min}–{max - 1}.");
                //foreach (var (_, name, ix) in req.OrderBy(t => t.name))
                //    Console.WriteLine($"{ix}. {name}");
                //Console.WriteLine();
                //Console.ReadLine();
                //foreach (var sol in makePuzzle(req.Select(c => c.constraint)).Solve())
                //    Console.WriteLine(sol.JoinString(", "));
            }

            Console.WriteLine();
            foreach (var kvp in dic.OrderBy(p => p.Key))
                Console.WriteLine($"{kvp.Key} = {kvp.Value,3} {new string('▒', kvp.Value)}");
            Console.WriteLine();
            Console.WriteLine($"Min time: {minTime.TotalSeconds:0.#} sec");
            Console.WriteLine($"Max time: {maxTime.TotalSeconds:0.#} sec");
            Console.WriteLine($"Avg time: {totalTime.TotalSeconds / 100:0.#} sec");
        }
    }
}