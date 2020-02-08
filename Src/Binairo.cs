using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;

namespace PuzzleStuff
{
    internal class Binairo
    {
        public static IEnumerable<bool[]> Generate(int size)
        {
            return generatePuzzle(size, new bool[size * size], new bool[size * size], 0);
        }

        private static IEnumerable<bool[]> generatePuzzle(int size, bool[] current, bool[] given, int ix)
        {
            var x = ix % size;
            var y = ix / size;

            if (ix == size * size)
            {
                yield return current;
                yield break;
            }

            var valid = new List<bool> { false, true };

            // Check that we don’t get more than two of the same digit in a straight row/column
            if (x >= 2 && current[ix - 2] == current[ix - 1])
                valid.Remove(current[ix - 1]);
            if (y >= 2 && current[ix - size] == current[ix - 2 * size])
                valid.Remove(current[ix - size]);

            // Check if the current row or column already contains enough 0’s or 1’s
            var zeros = Enumerable.Range(0, x).Count(c => !current[c + size * y]);
            if (zeros >= size / 2)
                valid.Remove(false);
            else if (x - zeros >= size / 2)
                valid.Remove(true);
            zeros = Enumerable.Range(0, y).Count(r => !current[x + size * r]);
            if (zeros == size / 2)
                valid.Remove(false);
            if (y - zeros == size / 2)
                valid.Remove(true);

            // Make sure that the row we just filled isn’t identical to an earlier row. We can check this one column early because the last digit is determined by the rest
            if (x == size - 2)
                for (int r = 0; r < y; r++)
                    if (Enumerable.Range(0, size - 2).All(c => current[c + size * r] == current[c + size * y]))
                        valid.Remove(current[size - 2 + size * r]);

            // Make sure that the column we just filled isn’t identical to an earlier column. We can check this one row early because the last digit is determined by the rest
            if (y == size - 2)
                for (int c = 0; c < x; c++)
                    if (Enumerable.Range(0, size - 2).All(r => current[c + size * r] == current[x + size * r]))
                        valid.Remove(current[c + size * (size - 2)]);

            if (valid.Count > 1 && Rnd.Next(0, 2) == 0)
                for (int i = 0; i < valid.Count; i++)
                    valid[i] = !valid[i];

            for (int i = 0; i < valid.Count; i++)
                if (!given[ix] || valid[i] == current[ix])
                {
                    current[ix] = valid[i];
                    foreach (var result in generatePuzzle(size, current, given, ix + 1))
                        yield return result;
                }
        }
    }
}