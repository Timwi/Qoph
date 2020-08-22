using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace PuzzleStuff.BombDisposal
{
    static class SapphicStripes
    {
        public static void FindWords()
        {
            var words = File.ReadAllLines(@"D:\Daten\Wordlists\English words.txt").Union(File.ReadAllLines(@"D:\Daten\Wordlists\peter_broda_wordlist_unscored.txt")).Order().ToArray();

            for (var mod = 27; mod <= 48; mod++)
            {
                ConsoleUtil.WriteLine($"Modulo {mod}".Color(ConsoleColor.White));
                var found = Ut.NewArray(6, _ => new List<string>());
                foreach (var word in words)
                {
                    if (word.Length >= 3 && word.Length <= 8 && word.All(ch => "LESBIAN".Contains(ch)))
                    {
                        var value = word.Aggregate(0, (p, n) => p * 7 + "LESBIAN".IndexOf(n));
                        if (value % mod == "HAZARD"[word.Length - 3] - 'A' + 1)
                            found[word.Length - 3].Add(word);
                    }
                }
                if (found.All(s => s.Count > 0))
                {
                    var tt = new TextTable { ColumnSpacing = 2 };
                    for (var x = 0; x < 6; x++)
                    {
                        tt.SetCell(x, 0, "HAZARD".Substring(x, 1).Color(ConsoleColor.Yellow));
                        for (var y = 0; y < found[x].Count; y++)
                            tt.SetCell(x, y + 1, found[x][y]);
                    }
                    tt.WriteToConsole();
                }
                Console.WriteLine();
            }
        }
    }
}