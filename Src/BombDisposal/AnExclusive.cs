using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff.BombDisposal
{
    static class AnExclusive
    {
        const int _size = 6;

        private static ConsoleColoredString Stringify(bool[] puzzle, int? colorIndex = null) => Stringify(puzzle.Select(b => (bool?) b).ToArray(), colorIndex);
        private static ConsoleColoredString Stringify(bool?[] puzzle, int? colorIndex = null, char yes = '█', char no = '░', char unknown = '▓') =>
            puzzle
                .Split(_size)
                .Select((r, row) => r
                    .Select((b, col) => (b == true ? yes : b == false ? no : unknown)
                        .Color(col + _size * row == colorIndex ? ConsoleColor.Yellow : ConsoleColor.DarkGreen, col + _size * row == colorIndex ? ConsoleColor.DarkBlue : ConsoleColor.Black))
                    .JoinColoredString())
                .JoinColoredString("\n");
        private static ConsoleColoredString Stringify(bool[] puzzle, bool[] given) =>
            puzzle
                .Split(_size)
                .Select((r, row) => r
                    .Select((b, col) => !given[col + _size * row] ? ".".Color(null, ConsoleColor.DarkGray) :
                            b ? "1".Color(ConsoleColor.Yellow, ConsoleColor.DarkGreen) : "0".Color(ConsoleColor.Magenta, ConsoleColor.DarkRed))
                    .JoinColoredString())
                .JoinColoredString("\n");

        private static int _maxIx;
        public static void Generate()
        {
            //*
            var solutionStr = @"
1	1		1	1	1
1			1		1
1			1		1
1	1		1	1	1
		1			
0	0	0	0	0	0
";
            /*/
            var solutionStr = @"
1			1	1	
		1		1	1
1		1			1
1		1			1
1					
0	0	0	0	0	0
";
            /**/

            const bool useThree = true;

            var solution = solutionStr.Replace("\r", "").Trim('\n').Replace("\n", "\t").Split("\t").Select(s => s == "1" ? true : s == "0" ? null : (bool?) false).ToArray();

            if (solution.Length != _size * _size)
                Debugger.Break();

            _maxIx = 0;
            var all = new Dictionary<string, (bool[], bool[], bool[])>();
            foreach (var (first, second, third) in generateDoubleOrTriplePuzzle(new bool[_size * _size], new bool[_size * _size], useThree ? new bool[_size * _size] : null, solution, 0))
            {
                var str = third == null
                    ? Stringify(first.Zip(second, (b1, b2) => b1 ^ b2).ToArray()).ToString()
                    : Stringify(first.Zip(second, (b1, b2) => b1 ^ b2).Zip(third, (b1, b2) => b1 ^ b2).ToArray()).ToString();
                if (!all.ContainsKey(str))
                    all[str] = (first, second, third);
            }

            Console.WriteLine(all.Count);
            Console.WriteLine();

            foreach (var kvp in all)
            {
                Console.WriteLine(kvp.Key);
                Console.WriteLine();

                var (first, second, third) = kvp.Value;

                var givens1 = Stringify(first, generateGivens(first)).Split(new[] { "\n" });
                var givens2 = Stringify(second, generateGivens(second)).Split(new[] { "\n" });
                var givens3 = Stringify(third, generateGivens(third)).Split(new[] { "\n" });

                ConsoleUtil.WriteLine(givens1.Zip(givens2, (a, b) => a + "    " + b).Zip(givens3, (a, b) => a + "    " + b).JoinColoredString("\n"));

                Console.WriteLine(new string('─', 82));
                Console.WriteLine();
                Console.ReadLine();
            }
            if (_maxIx < _size * _size)
            {
                Console.WriteLine($"max index = {_maxIx}");
                ConsoleUtil.WriteLine(Stringify(solution, _maxIx));
            }
        }

        private static bool[] generateGivens(bool[] puzzle)
        {
            var given = new bool?[_size * _size];
            var puzzleIxs = Ut.ReduceRequiredSet(Enumerable.Range(0, puzzle.Length).ToArray().Shuffle(), skipConsistencyTest: true, test: test =>
            {
                for (int ix = 0; ix < _size * _size; ix++)
                    given[ix] = null;
                foreach (var ix in test.SetToTest)
                    given[ix] = puzzle[ix];
                //Console.WriteLine(given.Select(b => b != null ? "█" : " ").JoinString());
                return findSolutions(given.ToArray(), 0).Take(2).Count() == 1;
            });

            var ret = new bool[_size * _size];
            for (int ix = 0; ix < _size * _size; ix++)
                ret[ix] = false;
            foreach (var ix in puzzleIxs)
                ret[ix] = true;
            return ret;
        }

        private static IEnumerable<(bool[], bool[], bool[])> generateDoubleOrTriplePuzzle(bool[] one, bool[] two, bool[] three, bool?[] xor, int ix)
        {
            _maxIx = Math.Max(_maxIx, ix);
            var x = ix % _size;
            var y = ix / _size;

            if (ix == _size * _size)
            {
                yield return (one.ToArray(), two.ToArray(), three?.ToArray());
                yield break;
            }

            void checkValid(List<bool> list, bool[] current)
            {
                // Check that we don’t get more than two of the same digit in a straight row/column
                if (x >= 2 && current[ix - 2] == current[ix - 1])
                    list.Remove(current[ix - 1]);
                if (y >= 2 && current[ix - _size] == current[ix - 2 * _size])
                    list.Remove(current[ix - _size]);

                // Check if the current row or column already contains enough 0’s or 1’s
                var zeros = Enumerable.Range(0, x).Count(c => !current[c + _size * y]);
                if (zeros >= _size / 2)
                    list.Remove(false);
                else if (x - zeros >= _size / 2)
                    list.Remove(true);
                zeros = Enumerable.Range(0, y).Count(r => !current[x + _size * r]);
                if (zeros == _size / 2)
                    list.Remove(false);
                if (y - zeros == _size / 2)
                    list.Remove(true);

                // Make sure that the row we just filled isn’t identical to an earlier row. We can check this one column early because the last digit is determined by the rest
                if (x == _size - 2)
                    for (int r = 0; r < y; r++)
                        if (Enumerable.Range(0, _size - 2).All(c => current[c + _size * r] == current[c + _size * y]))
                            list.Remove(current[_size - 2 + _size * r]);

                // Make sure that the column we just filled isn’t identical to an earlier column. We can check this one row early because the last digit is determined by the rest
                if (y == _size - 2)
                    for (int c = 0; c < x; c++)
                        if (Enumerable.Range(0, _size - 2).All(r => current[c + _size * r] == current[x + _size * r]))
                            list.Remove(current[c + _size * (_size - 2)]);
            }

            var validOne = new List<bool> { false, true };
            checkValid(validOne, one);
            //if (validOne.Count > 1 && Rnd.Next(0, 2) == 0)
            //    for (int i = 0; i < validOne.Count; i++)
            //        validOne[i] = !validOne[i];

            var validTwo = new List<bool> { false, true };
            checkValid(validTwo, two);
            //if (validTwo.Count > 1 && Rnd.Next(0, 2) == 0)
            //    for (int i = 0; i < validTwo.Count; i++)
            //        validTwo[i] = validTwo[i];

            var validThree = new List<bool> { false, true };
            if (three != null)
            {
                checkValid(validThree, three);
                //if (validThree.Count > 1 && Rnd.Next(0, 2) == 0)
                //    for (int i = 0; i < validThree.Count; i++)
                //        validThree[i] = validThree[i];
            }

            for (int i = 0; i < validOne.Count; i++)
            {
                one[ix] = validOne[i];
                for (int j = 0; j < validTwo.Count; j++)
                {
                    two[ix] = validTwo[j];

                    if (three == null)
                    {
                        if (xor[ix] == null || (one[ix] ^ two[ix]) == xor[ix])
                            foreach (var tup in generateDoubleOrTriplePuzzle(one, two, null, xor, ix + 1))
                                yield return tup;
                    }
                    else
                    {
                        for (int k = 0; k < validThree.Count; k++)
                        {
                            three[ix] = validThree[k];
                            if (xor[ix] == null || (one[ix] ^ two[ix] ^ three[ix]) == xor[ix])
                                foreach (var tup in generateDoubleOrTriplePuzzle(one, two, three, xor, ix + 1))
                                    yield return tup;
                        }
                    }
                }
            }
        }

        private static IEnumerable<bool[]> findSolutions(bool?[] current, int ix)
        {
            obvious:;

            while (ix < _size * _size && current[ix] != null)
                ix++;

            // Search for some “obvious” things:
            var any = false;
            for (int i = 0; i < _size * _size; i++)
            {
                var x = i % _size;
                var y = i / _size;

                // 1) two in a row (e.g. 11 ⇒ 0110)
                if (x > 0 && current[i] != null && current[i - 1] == current[i])
                {
                    if (x < _size - 1)
                    {
                        if (current[i + 1] == null)
                            any = true;
                        else if (current[i + 1] == current[i])
                            return Enumerable.Empty<bool[]>();
                        current[i + 1] = !current[i];
                    }
                    if (x > 1)
                    {
                        if (current[i - 2] == null)
                            any = true;
                        else if (current[i - 2] == current[i])
                            return Enumerable.Empty<bool[]>();
                        current[i - 2] = !current[i];
                    }
                }
                if (y > 0 && current[i] != null && current[i - _size] == current[i])
                {
                    if (y < _size - 1)
                    {
                        if (current[i + _size] == null)
                            any = true;
                        else if (current[i + _size] == current[i])
                            return Enumerable.Empty<bool[]>();
                        current[i + _size] = !current[i];
                    }
                    if (y > 1)
                    {
                        if (current[i - 2 * _size] == null)
                            any = true;
                        else if (current[i - 2 * _size] == current[i])
                            return Enumerable.Empty<bool[]>();
                        current[i - 2 * _size] = !current[i];
                    }
                }

                // 2) two sandwiched (e.g. 1.1 ⇒ 101)
                if (x > 1 && current[i] != null && current[i - 2] == current[i])
                {
                    if (current[i - 1] == null)
                        any = true;
                    else if (current[i - 1] == current[i])
                        return Enumerable.Empty<bool[]>();
                    current[i - 1] = !current[i];
                }
                if (y > 1 && current[i] != null && current[i - 2 * _size] == current[i])
                {
                    if (current[i - _size] == null)
                        any = true;
                    else if (current[i - _size] == current[i])
                        return Enumerable.Empty<bool[]>();
                    current[i - _size] = !current[i];
                }
            }
            if (any)
                goto obvious;

            // 3) equal number of 0s and 1s
            for (int x = 0; x < _size; x++)
            {
                var ones = 0;
                var zeroes = 0;
                for (int y = 0; y < _size; y++)
                    if (current[x + y * _size] == true)
                        ones++;
                    else if (current[x + y * _size] == false)
                        zeroes++;
                if (ones == _size / 2 && zeroes < _size / 2)
                {
                    for (int y = 0; y < _size; y++)
                        if (current[x + y * _size] == null)
                            current[x + y * _size] = false;
                    goto obvious;
                }
                else if (zeroes == _size / 2 && ones < _size / 2)
                {
                    for (int y = 0; y < _size; y++)
                        if (current[x + y * _size] == null)
                            current[x + y * _size] = true;
                    goto obvious;
                }
                else if (ones > _size / 2 || zeroes > _size / 2)
                    return Enumerable.Empty<bool[]>();
            }
            for (int y = 0; y < _size; y++)
            {
                var ones = 0;
                var zeroes = 0;
                for (int x = 0; x < _size; x++)
                    if (current[x + y * _size] == true)
                        ones++;
                    else if (current[x + y * _size] == false)
                        zeroes++;
                if (ones == _size / 2 && zeroes < _size / 2)
                {
                    for (int x = 0; x < _size; x++)
                        if (current[x + y * _size] == null)
                            current[x + y * _size] = false;
                    goto obvious;
                }
                else if (zeroes == _size / 2 && ones < _size / 2)
                {
                    for (int x = 0; x < _size; x++)
                        if (current[x + y * _size] == null)
                            current[x + y * _size] = true;
                    goto obvious;
                }
                else if (ones > _size / 2 || zeroes > _size / 2)
                    return Enumerable.Empty<bool[]>();
            }

            // 4) two columns that are the same
            var numbers = new int?[_size];
            for (int x = 0; x < _size; x++)
            {
                var n = 0;
                for (int y = 0; y < _size - 1; y++)
                {
                    if (current[x + y * _size] == null)
                        goto hasNull;
                    n = (n << 1) | (current[x + y * _size].Value ? 1 : 0);
                }
                for (int prevX = 0; prevX < x; prevX++)
                    if (numbers[prevX] == n)
                        return Enumerable.Empty<bool[]>();
                numbers[x] = n;
                hasNull:;
            }
            // 5) two rows that are the same
            numbers = new int?[_size];
            for (int y = 0; y < _size; y++)
            {
                var n = 0;
                for (int x = 0; x < _size; x++)
                {
                    if (current[x + y * _size] == null)
                        goto hasNull;
                    n = (n << 1) | (current[x + y * _size].Value ? 1 : 0);
                }
                for (int prevY = 0; prevY < y; prevY++)
                    if (numbers[prevY] == n)
                        return Enumerable.Empty<bool[]>();
                numbers[y] = n;
                hasNull:;
            }

            return ix == _size * _size
                ? new[] { current.Select(b => b.Value).ToArray() }
                : findSolutionsIter(current, ix);
        }

        private static IEnumerable<bool[]> findSolutionsIter(bool?[] current, int ix)
        {
            current[ix] = false;
            foreach (var result in findSolutions(current.ToArray(), ix + 1))
                yield return result;

            current[ix] = true;
            foreach (var result in findSolutions(current.ToArray(), ix + 1))
                yield return result;
        }

        private static bool[] Solve(string puzzle)
        {
            var actualPuzzle = puzzle.Replace("\r", "").Replace("\n", "").Select(ch => ch == '1' ? true : ch == '0' ? false : (bool?) null).ToArray();
            var solutions = findSolutions(actualPuzzle, 0).Take(2).ToArray();
            if (solutions.Length != 1)
                Debugger.Break();
            return solutions[0];
        }

        public static void Test()
        {
            var solution1 = Solve(@"
...0.....1..
.0.....1....
...0..11....
.1.........1
.....11...1.
1........0.0
....11.1....
..........0.
....00......
1..........0
.1.1.1.1..1.
1..1.....1..
");

            var solution2 = Solve(@"
...0.0.0..1.
..1.........
......0....1
.1..........
...0..1.0...
..0....0.11.
...1........
..11....10..
.........0..
0..0........
.1.......0..
..0...00.0..
");

            ConsoleUtil.WriteLine(Stringify(solution1));
            Console.WriteLine();
            ConsoleUtil.WriteLine(Stringify(solution2));
            Console.WriteLine();
            ConsoleUtil.WriteLine(Stringify(solution1.Zip(solution2, (b1, b2) => b1 ^ b2).ToArray()));
        }
    }
}