using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff.Esperanto
{
    static class BinaryPuzzle
    {
        const int _size = 6;

        private static ConsoleColoredString Stringify(bool[] puzzle, int? colorIndex = null) => Stringify(puzzle.Select(b => (bool?) b).ToArray(), colorIndex);
        private static ConsoleColoredString Stringify(bool?[] puzzle, int? colorIndex = null, char yes = '█', char no = '░', char unknown = '▒') =>
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

        private static bool[] generateGivens(bool[] puzzle)
        {
            var given = new bool?[_size * _size];
            var puzzleIxs = Ut.ReduceRequiredSet(Enumerable.Range(0, puzzle.Length).ToArray().Shuffle(), skipConsistencyTest: true, test: test =>
            {
                for (int ix = 0; ix < _size * _size; ix++)
                    given[ix] = null;
                foreach (var ix in test.SetToTest)
                    given[ix] = puzzle[ix];
                Console.WriteLine(given.Select(b => b != null ? "█" : " ").JoinString());
                return findSolutions(given.ToArray(), 0).Take(2).Count() == 1;
            });

            var ret = new bool[_size * _size];
            for (int ix = 0; ix < _size * _size; ix++)
                ret[ix] = false;
            foreach (var ix in puzzleIxs)
                ret[ix] = true;
            return ret;
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

        public static void Do()
        {
            var given = @"
##
.#
..#.##
  .#.#
  ..#.
".Replace("\r", "").Substring(1).Split('\n').SelectMany(row => Enumerable.Range(0, _size).Select(i => i >= row.Length ? null : row[i] == '#' ? true : row[i] == '.' ? false : (bool?) null).ToArray()).ToArray();
            foreach (var solution in findSolutions(given, 0))
            {
                ConsoleUtil.WriteLine(Stringify(given));
                Console.WriteLine();
                var givens = generateGivens(solution);
                Console.WriteLine();
                ConsoleUtil.WriteLine(Stringify(solution, givens));
            }
        }
    }
}