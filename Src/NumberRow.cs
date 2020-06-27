using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        sealed class NotPresentConstraint : Constraint
        {
            public int Value { get; private set; }
            public NotPresentConstraint(int value) : base(null) { Value = value; }
            public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
            {
                if (ix == null)
                    foreach (var arr in takens)
                        arr[Value - minValue] = true;
                return null;
            }
        }

        sealed class BetweenConstraint : Constraint
        {
            public int Low { get; private set; }
            public int High { get; private set; }
            public bool Reversed { get; private set; }
            public BetweenConstraint(int low, int high, bool reversed) : base(null) { Low = low; High = high; Reversed = reversed; }
            public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
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
                    else if (Reversed ? (grid[i].Value + minValue < Low || grid[i].Value + minValue > High) : (grid[i].Value + minValue > Low && grid[i].Value + minValue < High))
                        return Enumerable.Empty<Constraint>();
                if (numRemaining != 1)
                    return null;
                for (var v = 0; v < takens[remainingCell].Length; v++)
                    if (Reversed ? (v + minValue >= Low && v + minValue <= High) : (v + minValue <= Low || v + minValue >= High))
                        takens[remainingCell][v] = true;
                return null;
            }
        }

        sealed class HasXorConstraint : Constraint
        {
            public int Value1 { get; private set; }
            public int Value2 { get; private set; }
            public HasXorConstraint(int v1, int v2) : base(null) { Value1 = v1; Value2 = v2; }
            public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
            {
                if (ix == null)
                    return null;
                if (grid[ix.Value].Value + minValue == Value1)
                {
                    for (var i = 0; i < takens.Length; i++)
                        takens[i][Value2 - minValue] = true;
                    return Enumerable.Empty<Constraint>();
                }
                if (grid[ix.Value].Value + minValue == Value2)
                {
                    for (var i = 0; i < takens.Length; i++)
                        takens[i][Value1 - minValue] = true;
                    return Enumerable.Empty<Constraint>();
                }
                int remainingCell = -1;
                for (var i = 0; i < grid.Length; i++)
                    if (grid[i] == null)
                    {
                        if (remainingCell == -1)
                            remainingCell = i;
                        else
                            return null;
                    }
                for (var v = 0; v < takens[remainingCell].Length; v++)
                    if (v + minValue != Value1 && v + minValue != Value2)
                        takens[remainingCell][v] = true;
                return Enumerable.Empty<Constraint>();
            }
        }

        sealed class HasXnorConstraint : Constraint
        {
            public int Value1 { get; private set; }
            public int Value2 { get; private set; }
            public HasXnorConstraint(int value1, int value2) : base(null) { Value1 = value1; Value2 = value2; }
            public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
            {
                if (ix == null)
                    return null;

                bool found1 = false, found2 = false, possible1 = false, possible2 = false;
                int remainingCellCount = 0, remainingCell = -1;
                for (var i = 0; i < grid.Length; i++)
                {
                    if (grid[i] == null)
                    {
                        if (!takens[i][Value1 - minValue])
                            possible1 = true;
                        if (!takens[i][Value2 - minValue])
                            possible2 = true;
                        remainingCellCount++;
                        remainingCell = i;
                    }
                    else
                    {
                        if (grid[i].Value + minValue == Value1)
                            possible1 = found1 = true;
                        if (grid[i].Value + minValue == Value2)
                            possible2 = found2 = true;
                        if (found1 && found2)
                            return Enumerable.Empty<Constraint>();
                    }
                }
                if (remainingCellCount == 1)
                {
                    for (var v = 0; v < takens[remainingCell].Length; v++)
                        if ((found1 && v + minValue != Value2) || (found2 && v + minValue != Value1) || (!found1 && !found2 && (v + minValue == Value1 || v + minValue == Value2)))
                            takens[remainingCell][v] = true;
                    return Enumerable.Empty<Constraint>();
                }
                if (!possible1 && !possible2)
                    return Enumerable.Empty<Constraint>();
                if (!possible1 || !possible2)
                    for (var i = 0; i < grid.Length; i++)
                        for (var v = 0; v < takens[i].Length; v++)
                            if ((!possible1 && v + minValue == Value2) || (!possible2 && v + minValue == Value1))
                                takens[i][v] = true;
                return null;
            }
        }

        sealed class HasSumConstraint : Constraint
        {
            public int Sum { get; private set; }
            public HasSumConstraint(int sum) : base(null) { Sum = sum; }
            public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
            {
                if (ix == null)
                    return null;
                var val = grid[ix.Value].Value;
                int numRemaining = 0, remainingCell = -1;
                for (var i = 0; i < grid.Length; i++)
                {
                    if (i != ix.Value && grid[i] != null && grid[i].Value + minValue + val + minValue == Sum)
                        return Enumerable.Empty<Constraint>();
                    if (grid[i] == null)
                    {
                        numRemaining++;
                        remainingCell = i;
                    }
                }
                if (numRemaining == 1)
                    for (var v = 0; v < takens[remainingCell].Length; v++)
                        if (!Enumerable.Range(0, grid.Length).Any(i => i != remainingCell && grid[i].Value + minValue + v + minValue == Sum))
                            takens[remainingCell][v] = true;
                return null;
            }
        }

        sealed class HasSumOf2Constraint : Constraint
        {
            public int Cell1 { get; private set; }
            public int Cell2 { get; private set; }
            public HasSumOf2Constraint(int i, int j) : base(null) { Cell1 = i; Cell2 = j; }
            public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
            {
                if (ix == null)
                    return null;
                int? expectingSum = null;
                if (grid[Cell1] != null && grid[Cell2] != null)
                    expectingSum = grid[Cell1].Value + minValue + grid[Cell2].Value + minValue;

                int remainingCell = -1;
                for (var i = 0; i < grid.Length; i++)
                {
                    if (expectingSum != null && grid[i] + minValue == expectingSum.Value)
                        return Enumerable.Empty<Constraint>();

                    if (grid[i] == null)
                    {
                        if (remainingCell != -1)
                            return null;
                        remainingCell = i;
                    }
                }

                if (remainingCell == -1)
                    return null;
                else if (remainingCell == Cell1 || remainingCell == Cell2)
                {
                    var otherCell = remainingCell == Cell1 ? Cell2 : Cell1;
                    var otherValue = grid[otherCell].Value + minValue;
                    for (var v = 0; v < takens[remainingCell].Length; v++)
                        if (!Enumerable.Range(0, grid.Length).Any(i => i != remainingCell && grid[i].Value + minValue == v + minValue + otherValue))
                            takens[remainingCell][v] = true;
                }
                else
                {
                    for (var v = 0; v < takens[remainingCell].Length; v++)
                        if (v + minValue != expectingSum)
                            takens[remainingCell][v] = true;
                }
                return null;
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
                tt.SetCell(2, row, Max.ToString());
                tt.SetCell(3, row, $"{Total / (double) num:0.#}");
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
                tt.SetCell(2, row, $"{Max:0.#}");
                tt.SetCell(3, row, $"{Total / num:0.#}");
            }
        }

        public static void Do()
        {
            const int min = 1;  // inclusive
            const int max = 26; // inclusive
            const int totalSeeds = 1000;

            var words = File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt").Where(w => w.Length == 6 && w.All(ch => ch >= 'A' && ch <= 'Z')).ToArray();
            var dic = new Dictionary<string, int>();
            var timeCount = new DoubleValueCounter();
            var constraintsCount = new ValueCounter();
            var attemptsCount = new ValueCounter();
            var twoSymbolConstraintsCount = new ValueCounter();

            var privilegedGroups = new[] { "# < ▲ < #", "# < ¬▲ < #", "#¬|", "< #", "> #", "¬#", "¬largest", "¬prime", "¬smallest", "¬square", "¬|concat", "< ▲", "prime" };

            Enumerable.Range(2 * totalSeeds, totalSeeds).ParallelForEach(Environment.ProcessorCount, seed =>
            {
                var startTime = DateTime.UtcNow;
                var rnd = new Random(seed);
                var solutionWord = words.PickRandom(rnd);
                //Console.WriteLine(solutionWord);
                var solution = solutionWord.Select(ch => ch - 'A' + 1).ToArray();
                var n = solution.Length;
                var numAttempts = 0;
                tryAgain:
                numAttempts++;

                var startTimeThisAttempt = DateTime.UtcNow;
                var allConstraints = new List<(Constraint constraint, string group, string name)>();

                Puzzle makePuzzle(IEnumerable<Constraint> cs) => new Puzzle(n, min, max, cs);

                static Constraint differenceConstraint(int cell1, int cell2, int diff) => new TwoCellLambdaConstraint(cell1, cell2, (a, b) => Math.Abs(a - b) == diff);
                static Constraint quotientConstraint(int cell1, int cell2, int quotient) => new TwoCellLambdaConstraint(cell1, cell2, (a, b) => a * quotient == b || b * quotient == a);
                static Constraint moduloDiffConstraint(int cell1, int cell2, int modulo) => new TwoCellLambdaConstraint(cell1, cell2, (a, b) => a % modulo == b % modulo);
                static Constraint moduloConstraint(int cell1, int cell2, int modulo) => new TwoCellLambdaConstraint(cell1, cell2, (a, b) => b != 0 && a % b == modulo);

                // Relations between two numbers (symmetric)
                for (var i = 0; i < n; i++)
                    for (var j = i + 1; j < n; j++)
                    {
                        for (var m = 2; m < max / 2; m++)
                            if (solution[i] % m == solution[j] % m)
                                allConstraints.Add((moduloDiffConstraint(i, j, m), "same %", $"{(char) (i + 'A')} is a multiple of {m} away from {(char) (j + 'A')}."));

                        allConstraints.Add((new SumConstraint(solution[i] + solution[j], new[] { i, j }), "+ #", $"The sum of {(char) (i + 'A')} and {(char) (j + 'A')} is {solution[i] + solution[j]}."));
                        allConstraints.Add((new ProductConstraint(solution[i] * solution[j], new[] { i, j }), "× #", $"The product of {(char) (i + 'A')} and {(char) (j + 'A')} is {solution[i] * solution[j]}."));
                        allConstraints.Add((differenceConstraint(i, j, Math.Abs(solution[i] - solution[j])), "− #", $"The absolute difference of {(char) (i + 'A')} and {(char) (j + 'A')} is {Math.Abs(solution[i] - solution[j])}."));
                        if (solution[j] != 0 && solution[i] % solution[j] == 0 && solution[i] / solution[j] < 4)
                            allConstraints.Add((quotientConstraint(i, j, solution[i] / solution[j]), "÷ #", $"Of {(char) (i + 'A')} and {(char) (j + 'A')}, one is {solution[i] / solution[j]} times the other."));
                        if (solution[i] != 0 && solution[j] % solution[i] == 0 && solution[j] / solution[i] < 4)
                            allConstraints.Add((quotientConstraint(i, j, solution[j] / solution[i]), "÷ #", $"Of {(char) (i + 'A')} and {(char) (j + 'A')}, one is {solution[j] / solution[i]} times the other."));

                        var minIj = Math.Min(solution[i], solution[j]);
                        var maxIj = Math.Max(solution[i], solution[j]);
                        if (maxIj - minIj > 1)
                            foreach (var k in Enumerable.Range(minIj + 1, maxIj - minIj - 1))
                                allConstraints.Add((new TwoCellLambdaConstraint(i, j, (a, b) => (a < k && k < b) || (b < k && k < a)), "▲ < # < ▲", $"{k} is between {(char) (i + 'A')} and {(char) (j + 'A')}."));

                        allConstraints.Add((new HasSumConstraint(solution[i] + solution[j]), "∃∑", $"There are two values that add up to {solution[i] + solution[j]}."));
                        //if (solution.Contains(solution[i] + solution[j]))
                        //    allConstraints.Add((new HasSumOf2Constraint(i, j), "∃∑▲", $"There is a value equal to {(char) (i + 'A')} + {(char) (j + 'A')}."));
                    }

                // Relations between two numbers (asymmetric)
                var concatenationModulos = new[] { 3, 4, 6, 7, 8, 9, 11 };
                for (var i = 0; i < n; i++)
                    for (var j = 0; j < n; j++)
                        if (i != j)
                        {
                            if (solution[i] < solution[j])
                                allConstraints.Add((new TwoCellLambdaConstraint(i, j, (a, b) => a < b), "< ▲", $"{(char) (i + 'A')} is less than {(char) (j + 'A')}."));

                            var concat = int.Parse($"{solution[i]}{solution[j]}");
                            foreach (var m in concatenationModulos)   // beware lambdas
                                if (concat % m == 0)
                                    allConstraints.Add((new TwoCellLambdaConstraint(i, j, (a, b) => int.Parse($"{a}{b}") % m == 0), "|concat", $"The concatenation of {(char) (i + 'A')}{(char) (j + 'A')} is divisible by {m}."));
                                else
                                    allConstraints.Add((new TwoCellLambdaConstraint(i, j, (a, b) => int.Parse($"{a}{b}") % m != 0), "¬|concat", $"The concatenation of {(char) (i + 'A')}{(char) (j + 'A')} is not divisible by {m}."));
                            if (solution[j] != 0)
                                allConstraints.Add((moduloConstraint(i, j, solution[i] % solution[j]), "% #", $"{(char) (i + 'A')} modulo {(char) (j + 'A')} is {solution[i] % solution[j]}."));
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
                    foreach (var v in Enumerable.Range(min, max - min + 1)) // don’t use ‘for’ loop because the variable is captured by lambdas
                        if (solution[i] < v)
                            allConstraints.Add((new OneCellLambdaConstraint(i, a => a < v), "< #", $"{(char) (i + 'A')} is less than {v}."));
                        else if (solution[i] > v)
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
                        if (i < j)
                            allConstraints.Add((new LeftOfPositionConstraint(solution[i], j), "# ← ▲", $"There is a {solution[i]} further left than {(char) (j + 'A')}."));
                        else if (i > j)
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
                    foreach (var m in Enumerable.Range(2, 6))   // don’t use ‘for’ loop because the value is captured by lambdas
                        allConstraints.Add(solution[i] % m == 0
                             ? (new OneCellLambdaConstraint(i, a => a % m == 0), "#|", $"{(char) (i + 'A')} is divisible by {m}.")
                             : (new OneCellLambdaConstraint(i, a => a % m != 0), "#¬|", $"{(char) (i + 'A')} is not divisible by {m}."));
                }

                // Presence and absence of values
                for (var v = min; v <= max; v++)
                    if (!solution.Contains(v))
                        allConstraints.Add((new NotPresentConstraint(v), "¬#", $"There is no {v}."));

                for (var low = min; low <= max; low++)
                    for (var high = low + 1; high <= max; high++)
                    {
                        if (solution.Any(v => v > low && v < high))
                            allConstraints.Add((new BetweenConstraint(low, high, reversed: false), "# < ▲ < #", $"There is a value between {low} and {high}."));
                        if (solution.Any(v => v < low || v > high))
                            allConstraints.Add((new BetweenConstraint(low, high, reversed: true), "# < ¬▲ < #", $"There is a value outside of {low} to {high}."));
                    }

                for (var v1 = min; v1 <= max; v1++)
                    for (var v2 = v1 + 1; v2 <= max; v2++)
                    {
                        if ((solution.Contains(v1) && !solution.Contains(v2)) || (solution.Contains(v2) && !solution.Contains(v1)))
                            allConstraints.Add((new HasXorConstraint(v1, v2), "#/¬#", $"There is a {v1} or a {v2}, but not both."));
                        if ((solution.Contains(v1) && solution.Contains(v2)) || (!solution.Contains(v1) && !solution.Contains(v2)))
                            allConstraints.Add((new HasXnorConstraint(v1, v2), "##/¬#¬#", $"There is a {v1} and a {v2}, or neither."));
                    }

                // Group the constraints
                var constraintGroups = allConstraints.GroupBy(c => c.group).Select(gr => gr.ToList()).ToList();

                // Choose one constraint from each group
                var constraints = new List<(Constraint constraint, string group, string name)>();
                foreach (var gr in constraintGroups)
                {
                    var ix = rnd.Next(0, gr.Count);
                    constraints.Add(gr[ix]);
                    gr.RemoveAt(ix);
                }
                var constraintDic = constraintGroups.Where(gr => gr.Count > 0 && privilegedGroups.Contains(gr[0].group)).ToDictionary(gr => gr[0].group);

                // Add more constraints if this is not unique
                var addedCount = 0;
                int solutionCount;
                while ((solutionCount = makePuzzle(constraints.Select(c => c.constraint)).Solve().Take(2).Count()) > 1)
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
                if (solutionCount == 0) // No solution: pretty bad bug
                {
                    Console.WriteLine($"--- NO SOLUTION! Testing... ({solutionWord})");
                    // Reduce the set of constraints again
                    var cnstr = Ut.ReduceRequiredSet(
                        constraints.Select((c, ix) => (c.constraint, c.group, c.name, ix)).ToArray(),
                        set => !makePuzzle(set.SetToTest.Select(c => c.constraint)).Solve().Any()).ToArray();

                    for (var i = 0; i < cnstr.Length; i++)
                        Console.WriteLine($"{i}. {cnstr[i].name}");

                    Console.WriteLine();

                    // DEBUG OUTPUT IS GENERATED HERE. Change “ExamineConstraints” to contain the offending constraint. The above code has listed the constraints in the console.
                    var result = makePuzzle(cnstr.Select(c => c.constraint)).Solve(new SolverInstructions { ExamineConstraints = new[] { cnstr[2].constraint }, IntendedSolution = solution, UseLetters = true }).ToArray();
                    Console.WriteLine(result.Length);

                    System.Diagnostics.Debugger.Break();
                }
                Console.WriteLine($"Seed: {seed,10} - extra: {addedCount} ({solutionCount})");

                // Reduce the set of constraints again
                var req = Ut.ReduceRequiredSet(
                    constraints.Select((c, ix) => (c.constraint, c.group, c.name, ix)).ToArray().Shuffle(rnd),
                    set =>
                    {
                        //Console.WriteLine(Enumerable.Range(0, constraints.Count).Select(ix => set.SetToTest.Any(c => c.ix == ix) ? "█".Color(ConsoleColor.Green) : "░".Color(ConsoleColor.DarkBlue)).JoinColoredString());
                        return !makePuzzle(set.SetToTest.Select(c => c.constraint)).Solve().Skip(1).Any();
                    }).ToArray();
                var numTwoSymbolConstraints = req.Count(tup => new[] { "sum ▲", "product ▲", "mod ▲" }.Contains(tup.group));

                if (numTwoSymbolConstraints > 2 || req.Length - numTwoSymbolConstraints > 12)
                    goto tryAgain;

                lock (dic)
                {
                    foreach (var group in req.Select(c => c.group).Distinct())
                        dic.IncSafe(group);

                    timeCount.Count((DateTime.UtcNow - startTime).TotalSeconds);
                    constraintsCount.Count(req.Length);
                    attemptsCount.Count(numAttempts);
                    twoSymbolConstraintsCount.Count(numTwoSymbolConstraints);

                    //    Console.WriteLine($"There are {n} positions (labeled A–{(char) ('A' + n - 1)}) containing digits {min}–{max - 1}.");
                    //    foreach (var (constraint, group, name, ix) in req.OrderBy(t => t.name))
                    //        Console.WriteLine($"{name}");
                    //    Console.WriteLine();
                    //    Console.ReadLine();
                    //    foreach (var sol in makePuzzle(req.Select(c => c.constraint)).Solve())
                    //        Console.WriteLine(sol.JoinString(", "));
                }
            });

            Console.WriteLine();
            //var groupTotal = dic.Sum(p => p.Value);
            foreach (var kvp in dic.OrderBy(p => p.Value))
                ConsoleUtil.WriteLine($"{kvp.Key,10} = {kvp.Value * 100 / (double) totalSeeds,4:0.0}% {new string('▒', kvp.Value * 100 / totalSeeds)}".Color(privilegedGroups.Contains(kvp.Key) ? ConsoleColor.Yellow : ConsoleColor.Gray));
            Console.WriteLine();

            var tt = new TextTable { ColumnSpacing = 2 };
            tt.SetCell(1, 0, "Min");
            tt.SetCell(2, 0, "Max");
            tt.SetCell(3, 0, "Avg");
            timeCount.AddToTable(tt, 1, "Time (sec)", totalSeeds);
            attemptsCount.AddToTable(tt, 2, "Attempts", totalSeeds);
            constraintsCount.AddToTable(tt, 3, "Constraints", totalSeeds);
            twoSymbolConstraintsCount.AddToTable(tt, 4, "2-symbol c.", totalSeeds);
            tt.WriteToConsole();
        }
    }
}