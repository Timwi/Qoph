using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff.BombDisposal
{
    static class NatoSplit
    {
        public static void Do()
        {
            var words = File.ReadAllLines(@"D:\Daten\Wordlists\peter_broda_wordlist.txt")
                .Select(line => Regex.Match(line, @"^(.*);(\d+)$"))
                .Where(m => m.Success)
                .Select(m => (word: m.Groups[1].Value, score: int.Parse(m.Groups[2].Value)))
                .Concat(File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt").Select(word => (word, score: 100)))
                .GroupBy(m => m.word)
                .ToDictionary(gr => gr.Key, gr => gr.Max(t => t.score));

            var shard = "JULIET";
            for (var i = 1; i <= shard.Length - 1; i++)
            {
                var shard1 = shard.Substring(0, i);
                var shard2 = shard.Substring(i);
                ConsoleUtil.WriteLine($"{shard1} + {shard2}".Color(ConsoleColor.White));

                var bestScore = 0;

                void processWord(string w1, string w2)
                {
                    var surround = w1.Substring(0, w1.Length - shard1.Length) + w2.Substring(shard2.Length);
                    if (!words.TryGetValue(surround, out var sc))
                        return;
                    var score = words[w1] + words[w2] + sc;
                    if (score > bestScore)
                    {
                        bestScore = score;
                    }
                    if (score >= 100)
                        ConsoleUtil.WriteLine($"{score.ToString().Color(ConsoleColor.White)} = {(w1.Substring(0, w1.Length - shard1.Length) + w2.Substring(shard2.Length)).Color(ConsoleColor.Green)} = {w1.Color(ConsoleColor.DarkGray).ColorSubstring(0, w1.Length - shard1.Length, ConsoleColor.Yellow)} + {w2.Color(ConsoleColor.DarkGray).ColorSubstring(shard2.Length, ConsoleColor.Yellow)}", null);
                }

                if (shard1.Length > shard2.Length)
                {
                    foreach (var w1 in words.Keys)
                        if (w1.Length > shard1.Length && w1.EndsWith(shard1))
                            foreach (var w2 in words.Keys)
                                if (w2.Length > shard2.Length && w2.StartsWith(shard2))
                                    processWord(w1, w2);
                }
                else
                {
                    foreach (var w2 in words.Keys)
                        if (w2.Length > shard2.Length && w2.StartsWith(shard2))
                            foreach (var w1 in words.Keys)
                                if (w1.Length > shard1.Length && w1.EndsWith(shard1))
                                    processWord(w1, w2);
                }
            }
        }
    }
}