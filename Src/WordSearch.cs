using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace Qoph
{
    static class WordSearch
    {
        public static (ConsoleColoredString output, bool allFound) Run(string gridChars, int width, int height, bool consoleOutput, params string[] words)
        {
            var grid = gridChars.Split(new[] { '\t', ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).JoinString();
            var output = new List<ConsoleColoredString>();

            var used = new int[width * height];
            var directions = new (int dx, int dy)[] { (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1), (0, -1), (1, -1) };
            var allFound = true;

            foreach (var w in words)
            {
                var coords = new List<(int x, int y, int dx, int dy)>();
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        foreach (var (dx, dy) in directions)
                        {
                            if (x + dx * w.Length >= -1 && x + dx * w.Length <= width && y + dy * w.Length >= -1 && y + dy * w.Length <= height
                                && Enumerable.Range(0, w.Length).All(i => grid[(x + dx * i) + width * (y + dy * i)] == w[i]))
                            {
                                coords.Add((x, y, dx, dy));
                                for (int i = 0; i < w.Length; i++)
                                    used[(x + dx * i) + width * (y + dy * i)]++;
                            }
                        }

                output.Add($"{w} found {coords.Count} times: {coords.Select(c => $"[{c.x}, {c.y}, {c.dx}, {c.dy}]").JoinString(", ")}".Color(coords.Count == 0 ? ConsoleColor.Magenta : coords.Count == 1 ? ConsoleColor.Green : ConsoleColor.Yellow));
                if (coords.Count == 0)
                    allFound = false;
            }

            if (consoleOutput)
            {
                output.Add("");
                for (int y = 0; y < height; y++)
                    output.Add(Enumerable.Range(0, width).Select(x => $"{grid[x + width * y]} ".Color(
                        used[x + width * y] == 0 ? ConsoleColor.White : (ConsoleColor) (used[x + width * y] + 8),
                        used[x + width * y] == 0 ? ConsoleColor.Black : (ConsoleColor) used[x + width * y])).JoinColoredString());
                output.Add("");
                output.Add($"{Enumerable.Range(0, width * height).Where(i => used[i] == 0).Select(i => grid[i]).JoinString()} ({Enumerable.Range(0, width * height).Where(i => used[i] == 0).Count()})");
                output.Add("");
            }

            return (output.JoinColoredString(Environment.NewLine), allFound);
        }

        public static void Generate(int width, int height, params string[] words)
        {
            var size = words.Sum(word => word.Length);
            Console.WriteLine($"Longest word: {words.Max(w => w.Length)}");
            Console.WriteLine($"Size: {width}×{height}");
            var dirs = new (int dx, int dy)[] { (-1, -1), (0, -1), (1, -1), (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0) };

            IEnumerable<char[]> recurse(char[] chs, Dictionary<string, List<(int x, int y, int dx, int dy)>> dic, bool firstIteration)
            {
                if (dic.Count == 0)
                {
                    yield return chs.Select(ch => ch == '\0' ? '?' : ch).ToArray();
                    yield break;
                }

                var (word, wordPlacements) = dic.MinElement(kvp => kvp.Value.Count);
                wordPlacements.Shuffle();
                foreach (var (x, y, dx, dy) in wordPlacements)
                {
                    var newChs = (char[]) chs.Clone();
                    for (var i = 0; i < word.Length; i++)
                        newChs[x + dx * i + width * (y + dy * i)] = word[i];
                    var newDic = new Dictionary<string, List<(int x, int y, int dx, int dy)>>();
                    foreach (var (otherWord, otherPlacements) in dic)
                        if (otherWord != word)
                        {
                            var newList = new List<(int x, int y, int dx, int dy)>();
                            foreach (var (xx, yy, ddx, ddy) in otherPlacements)
                            {
                                for (var i = 0; i < otherWord.Length; i++)
                                {
                                    var gridIx = xx + ddx * i + width * (yy + ddy * i);
                                    if (newChs[gridIx] != '\0' && newChs[gridIx] != otherWord[i])
                                        goto busted;
                                }
                                newList.Add((xx, yy, ddx, ddy));
                                busted:;
                            }
                            if (newList.Count == 0)
                                goto nextPlacement;
                            newDic[otherWord] = newList;
                        }

                    foreach (var solution in recurse(newChs, newDic, firstIteration: false))
                        yield return solution;

                    nextPlacement:;
                }
            }

            var allPlacements = words.ParallelSelect(word =>
            {
                var possiblePlacements = new List<(int x, int y, int dx, int dy)>();
                foreach (var (dx, dy) in dirs)
                    for (var x = 0; x < width; x++)
                        if (x + dx * (word.Length - 1) >= 0 && x + dx * (word.Length - 1) < width)
                            for (var y = 0; y < height; y++)
                                if (y + dy * (word.Length - 1) >= 0 && y + dy * (word.Length - 1) < height)
                                    possiblePlacements.Add((x, y, dx, dy));
                return Ut.KeyValuePair(word, possiblePlacements);
            }).ToDictionary();

            Enumerable.Range(0, Environment.ProcessorCount).ParallelForEach(processor =>
            {
                lock (allPlacements)
                    Console.WriteLine($"Processor {processor} started.");

                foreach (var result in recurse(new char[width * height], allPlacements.Select(kvp => Ut.KeyValuePair(kvp.Key, kvp.Value.ToList())).ToDictionary(), firstIteration: true))
                    lock (allPlacements)
                    {
                        Console.WriteLine(result == null ? "No result" : "Result:");
                        if (result != null)
                        {
                            var (output, allFound) = Run(result.JoinString(), width, height, true, words);
                            ConsoleUtil.WriteLine(output);
                            Console.WriteLine();
                            Console.WriteLine();
                            File.AppendAllText(@"D:\temp\temp.txt", $"{output}\r\n\r\n");
                        }
                    }

                lock (allPlacements)
                    Console.WriteLine($"Processor {processor} has ended.");
            });
        }
    }
}