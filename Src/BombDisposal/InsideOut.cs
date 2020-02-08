using System;
using RT.Util;
using RT.Util.ExtensionMethods;
using System.IO;
using System.Linq;
using RT.Util.Consoles;

namespace PuzzleStuff.BombDisposal
{
    static class InsideOut
    {
        public static void Do()
        {
            var words = File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt")
                .Concat(File.ReadAllLines(@"D:\Daten\Wordlists\peter_broda_wordlist_unscored.txt"))
                .Select(w => w.ToUpperInvariant().Where(ch => ch != '(' && ch != ')').JoinString())
                .OrderByDescending(w => w.Length)
                .ToHashSet();

            foreach (var word in words)
            {
                if (word.Length < 6 || word.Length % 2 != 0)
                    continue;
                if (word.Substring(word.Length / 2 - 1, 2) != "NI")
                    continue;

                for (var i = 1; i < word.Length / 2 - 1; i++)
                {
                    if (!words.Contains(word.Substring(i, word.Length - 2 * i)))
                        continue;
                    if (word.Substring(word.Length - i) == "S")
                        continue;
                    ConsoleUtil.WriteLine($"{word.ColorSubstring(i, word.Length - 2 * i, null, ConsoleColor.DarkBlue).ColorSubstring(word.Length / 2 - 1, 2, null, ConsoleColor.DarkGreen)} - {word.Substring(i, word.Length - 2 * i)}", null);
                }
            }
        }
    }
}