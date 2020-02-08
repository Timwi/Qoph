using System;
using System.Linq;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff
{
    static class WordSearch
    {
        public static bool Run(string gridChars, int width, int height, bool consoleOutput, params string[] words)
        {
            var grid = gridChars.Split(new[] { '\t', ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).JoinString();

            var used = new int[width * height];
            var directions = new (int dx, int dy)[] { (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1), (0, -1), (1, -1) };
            var allFound = true;

            foreach (var w in words)
            {
                var numMatches = 0;
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        foreach (var (dx, dy) in directions)
                        {
                            if (x + dx * w.Length >= -1 && x + dx * w.Length <= width && y + dy * w.Length >= -1 && y + dy * w.Length <= height
                                && Enumerable.Range(0, w.Length).All(i => grid[(x + dx * i) + width * (y + dy * i)] == w[i]))
                            {
                                numMatches++;
                                for (int i = 0; i < w.Length; i++)
                                    used[(x + dx * i) + width * (y + dy * i)]++;
                            }
                        }

                if (consoleOutput)
                    ConsoleUtil.WriteLine($"{w} found {numMatches} times".Color(numMatches == 0 ? ConsoleColor.Magenta : numMatches == 1 ? ConsoleColor.Green : ConsoleColor.Yellow));
                if (numMatches == 0)
                    allFound = false;
            }

            if (consoleOutput)
            {
                Console.WriteLine();
                for (int y = 0; y < height; y++)
                    ConsoleUtil.WriteLine(Enumerable.Range(0, width).Select(x => $"{grid[x + width * y]} ".Color(
                        used[x + width * y] == 0 ? ConsoleColor.White : (ConsoleColor) (used[x + width * y] + 8),
                        used[x + width * y] == 0 ? ConsoleColor.Black : (ConsoleColor) used[x + width * y])).JoinColoredString());
                Console.WriteLine();
                Console.WriteLine(Enumerable.Range(0, width * height).Where(i => used[i] == 0).Select(i => grid[i]).JoinString());
                Console.WriteLine();
            }

            return allFound;
        }
    }
}