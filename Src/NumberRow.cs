using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff
{
    static class NumberRow
    {
        abstract class RelativeToPositionConstraint : Constraint
        {
            public int Value { get; private set; }
            public int Position { get; private set; }
            public bool IsLeft { get; private set; }
            public RelativeToPositionConstraint(int value, int position, bool isLeft) : base(null) { Value = value; Position = position; IsLeft = isLeft; }
            public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
            {
                if (ix == null)
                    for (var i = IsLeft ? Position : 0; i < (IsLeft ? takens.Length : Position); i++)
                        takens[i][Value - minValue] = true;
                return null;
            }
        }
        sealed class LeftOfPositionConstraint : RelativeToPositionConstraint { public LeftOfPositionConstraint(int value, int position) : base(value, position, true) { } }
        sealed class RightOfPositionConstraint : RelativeToPositionConstraint { public RightOfPositionConstraint(int value, int position) : base(value, position, false) { } }

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

        sealed class DifferenceConstraint : Constraint
        {
            public int Difference { get; private set; }
            public int Cell1 { get; private set; }
            public int Cell2 { get; private set; }
            public DifferenceConstraint(int diff, int cell1, int cell2) : base(new[] { cell1, cell2 }) { Difference = diff; Cell1 = cell1; Cell2 = cell2; }
            public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
            {
                if (ix == Cell1)
                    for (var v = 0; v < takens[Cell2].Length; v++)
                        if ((v + minValue) != grid[Cell1].Value + Difference && (v + minValue) != grid[Cell1].Value - Difference)
                            takens[Cell2][v] = true;
                if (ix == Cell2)
                    for (var v = 0; v < takens[Cell1].Length; v++)
                        if ((v + minValue) != grid[Cell2].Value + Difference && (v + minValue) != grid[Cell2].Value - Difference)
                            takens[Cell1][v] = true;
                return null;
            }
        }

        sealed class QuotientConstraint : Constraint
        {
            public int Quotient { get; private set; }
            public int Cell1 { get; private set; }
            public int Cell2 { get; private set; }
            public QuotientConstraint(int quotient, int cell1, int cell2) : base(new[] { cell1, cell2 }) { Quotient = quotient; Cell1 = cell1; Cell2 = cell2; }
            public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
            {
                if (ix == Cell1)
                    for (var v = 0; v < takens[Cell2].Length; v++)
                        if (((v + minValue) * Quotient != grid[Cell1].Value) && (v + minValue) != grid[Cell1].Value * Quotient)
                            takens[Cell2][v] = true;
                if (ix == Cell2)
                    for (var v = 0; v < takens[Cell1].Length; v++)
                        if (((v + minValue) * Quotient != grid[Cell2].Value) && (v + minValue) != grid[Cell2].Value * Quotient)
                            takens[Cell1][v] = true;
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

                int remainingCell = -1, min = int.MaxValue, max = int.MinValue, minIx = -1, maxIx = -1;
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
                        {
                            min = grid[i].Value;
                            minIx = i;
                        }
                        if (grid[i].Value > max)
                        {
                            max = grid[i].Value;
                            maxIx = i;
                        }
                    }
                }

                if (remainingCell == AffectedCells[0])
                {
                    for (var v = 0; v < takens[remainingCell].Length; v++)
                        if ((Mode == MinMaxMode.Min) ? (v + minValue <= min) : (v + minValue >= max))
                            takens[remainingCell][v] = true;
                }
                else if (((Mode == MinMaxMode.Min) ? minIx : maxIx) == AffectedCells[0])
                {
                    for (var v = 0; v < takens[remainingCell].Length; v++)
                        if ((Mode == MinMaxMode.Min) ? (v + minValue >= min) : (v + minValue <= max))
                            takens[remainingCell][v] = true;
                }

                return null;
            }
        }

        public static void Do()
        {
            const int min = 0;  // inclusive
            const int max = 10; // exclusive
            const int n = 5;

            var rnd = new Random(20);
            var solution = Enumerable.Range(min, max - min).ToArray().Shuffle(rnd).Take(n).ToArray();
            var constraints = new List<(Constraint constraint, string name)>();

            // Relations between two numbers
            for (var i = 0; i < n; i++)
                for (var j = i + 1; j < n; j++)
                {
                    if (solution[i] < solution[j])
                        constraints.Add((new TwoCellLambdaConstraint(i, j, (a, b) => a < b), $"{(char) (i + 'A')} is less than {(char) (j + 'A')}."));
                    else if (solution[i] > solution[j])
                        constraints.Add((new TwoCellLambdaConstraint(i, j, (a, b) => a > b), $"{(char) (i + 'A')} is greater than {(char) (j + 'A')}."));

                    foreach (var m in Enumerable.Range(2, max / 2 - 2)) // don’t use ‘for’ loop because the variable is captured by lambdas
                        if (solution[i] % m == solution[j] % m)
                            constraints.Add((new TwoCellLambdaConstraint(i, j, (a, b) => (a % m) == (b % m)), $"{(char) (i + 'A')} is a multiple of {m} away from {(char) (j + 'A')}."));

                    //constraints.Add((new SumConstraint(solution[i] + solution[j], new[] { i, j }), $"The sum of {(char) (i + 'A')} and {(char) (j + 'A')} is {solution[i] + solution[j]}."));
                    //constraints.Add((new ProductConstraint(solution[i] * solution[j], new[] { i, j }), $"The product of {(char) (i + 'A')} and {(char) (j + 'A')} is {solution[i] * solution[j]}."));
                    if (Math.Abs(solution[i] - solution[j]) < 6)
                        constraints.Add((new DifferenceConstraint(Math.Abs(solution[i] - solution[j]), i, j), $"The absolute difference of {(char) (i + 'A')} and {(char) (j + 'A')} is {Math.Abs(solution[i] - solution[j])}."));
                    if (solution[j] != 0 && solution[i] % solution[j] == 0 && solution[i] / solution[j] < 4)
                        constraints.Add((new QuotientConstraint(solution[i] / solution[j], i, j), $"Of {(char) (i + 'A')} and {(char) (j + 'A')}, one is {solution[i] / solution[j]} times the other."));
                    if (solution[i] != 0 && solution[j] % solution[i] == 0 && solution[j] / solution[i] < 4)
                        constraints.Add((new QuotientConstraint(solution[j] / solution[i], i, j), $"Of {(char) (i + 'A')} and {(char) (j + 'A')}, one is {solution[j] / solution[i]} times the other."));
                }

            for (var m1 = min; m1 < max; m1++)
                for (var m2 = min; m2 < max; m2++)
                {
                    var p1 = solution.IndexOf(m1);
                    var p2 = solution.IndexOf(m2);
                    if (p1 == -1 || p2 == -1 || p1 < p2)
                        constraints.Add((new LeftOfNumberConstraint(m1, m2), $"If there is a {m1} and a {m2}, the {m1} is further left."));
                }

            // Relations between three numbers
            for (var i = 0; i < n; i++)
                for (var j = i + 1; j < n; j++)
                    for (var k = 0; k < n; k++)
                        if (k != i && k != j)
                        {
                            if (solution[i] + solution[j] == solution[k])
                                constraints.Add((new ThreeCellLambdaConstraint(i, j, k, (a, b, c) => a + b == c), $"{(char) (i + 'A')} + {(char) (j + 'A')} = {(char) (k + 'A')}"));
                            if (solution[i] * solution[j] == solution[k])
                                constraints.Add((new ThreeCellLambdaConstraint(i, j, k, (a, b, c) => a * b == c), $"{(char) (i + 'A')} × {(char) (j + 'A')} = {(char) (k + 'A')}"));
                        }

            var minPos = solution.MinIndex(i => i);
            var maxPos = solution.MaxIndex(i => i);

            // Value relation constraints
            for (var i = 0; i < n; i++)
            {
                foreach (var v in Enumerable.Range(min, max - min)) // don’t use ‘for’ loop because the variable is captured by lambdas
                    if (solution[i] < v - 1)
                        constraints.Add((new OneCellLambdaConstraint(i, a => a < v), $"{(char) (i + 'A')} is less than {v}."));
                    else if (solution[i] > v + 1)
                        constraints.Add((new OneCellLambdaConstraint(i, a => a > v), $"{(char) (i + 'A')} is greater than {v}."));
                if (i != minPos)
                    constraints.Add((new NotMinMaxConstraint(i, MinMaxMode.Min), $"{(char) (i + 'A')} does not have the smallest number."));
                if (i != maxPos)
                    constraints.Add((new NotMinMaxConstraint(i, MinMaxMode.Max), $"{(char) (i + 'A')} does not have the largest number."));
            }

            // Position constraints
            for (var i = 0; i < n; i++)
                for (var j = 0; j < n; j++)
                    if (i < j - 1)
                        constraints.Add((new LeftOfPositionConstraint(solution[i], j), $"There is a {solution[i]} further left than {(char) (j + 'A')}."));
                    else if (i > j + 1)
                        constraints.Add((new RightOfPositionConstraint(solution[i], j), $"There is a {solution[i]} further right than {(char) (j + 'A')}."));

            // Miscellaneous
            var primes = new[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199 };
            var squares = Enumerable.Range(0, 100).Select(i => i * i).ToArray();
            for (var i = 0; i < n; i++)
            {
                if (primes.Contains(solution[i]))
                    constraints.Add((new OneCellLambdaConstraint(i, a => primes.Contains(a)), $"{(char) (i + 'A')} is a prime number."));
                if (squares.Contains(solution[i]))
                    constraints.Add((new OneCellLambdaConstraint(i, a => squares.Contains(a)), $"{(char) (i + 'A')} is a square number."));
                if (solution[i] % 2 == 0)
                    constraints.Add((new OneCellLambdaConstraint(i, a => a % 2 == 0), $"{(char) (i + 'A')} is an even number."));
                else
                    constraints.Add((new OneCellLambdaConstraint(i, a => a % 2 != 0), $"{(char) (i + 'A')} is an odd number."));
                if (solution[i] % 3 == 0)
                    constraints.Add((new OneCellLambdaConstraint(i, a => a % 3 == 0), $"{(char) (i + 'A')} is divisible by 3."));
                else
                    constraints.Add((new OneCellLambdaConstraint(i, a => a % 3 != 0), $"{(char) (i + 'A')} is not divisible by 3."));

                if (i < n - 1)
                {
                    var concat = int.Parse(solution[i].ToString() + solution[i + 1].ToString());
                    foreach (var m in new[] { 3, 4, 6, 7 })
                        if (concat % m == 0)
                            constraints.Add((new TwoCellLambdaConstraint(i, i + 1, (a, b) => int.Parse(a.ToString() + b.ToString()) % m == 0), $"The concatenation of {(char) (i + 'A')}{(char) (i + 1 + 'A')} is divisible by {m}."));
                }
            }

            foreach (var v in Enumerable.Range(min, max - min)) // don’t use ‘for’ loop because the value is captured by lambdas
                if (!solution.Contains(v))
                    constraints.Add((new LambdaConstraint((taken, grid, ix, mv, mxv) =>
                    {
                        if (ix == null)
                            foreach (var arr in taken)
                                arr[v - mv] = true;
                        return null;
                    }), $"There is no {v}."));

            static Puzzle makePuzzle(IEnumerable<Constraint> cs) => new Puzzle(n, min, max - 1, cs.Concat(new UniquenessConstraint(Enumerable.Range(0, n))));

            var req = Ut.ReduceRequiredSet(constraints.Select((c, ix) => (c.constraint, c.name, ix)).ToArray().Shuffle(rnd), test: set =>
            {
                //var arr = new bool[constraints.Count];
                //foreach (var (_, _, ix) in set.SetToTest)
                //    arr[ix] = true;
                //Console.WriteLine(arr.Select(b => b ? "█" : "░").JoinString());
                return !makePuzzle(set.SetToTest.Select(c => c.constraint)).Solve().Skip(1).Any();
            }).ToArray();

            Console.WriteLine($"There are {n} positions (labeled A–{(char) ('A' + n - 1)}) containing digits {min}–{max - 1}.");
            Console.WriteLine("The digits are all different.");
            foreach (var (_, name, _) in req.OrderBy(t => t.name))
                Console.WriteLine(name);
            Console.WriteLine();
            Console.ReadLine();
            foreach (var sol in makePuzzle(req.Select(c => c.constraint)).Solve())
                Console.WriteLine(sol.JoinString(", "));
        }
    }
}