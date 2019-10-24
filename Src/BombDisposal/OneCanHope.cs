using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PuzzleStuff.BombDisposal
{
    static class OneCanHope
    {
        public static void Do()
        {
            // (potential puzzle idea with Esperanto words, some of which are similar to their English translation, some of which are false friends)

            int LevenshteinDistance(string a, string b)
            {
                int lengthA = a.Length;
                int lengthB = b.Length;
                var distances = new int[lengthA + 1, lengthB + 1];
                for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
                for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

                for (int i = 1; i <= lengthA; i++)
                    for (int j = 1; j <= lengthB; j++)
                        distances[i, j] = Math.Min(Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1), distances[i - 1, j - 1] + (b[j - 1] == a[i - 1] ? 0 : 1));
                return distances[lengthA, lengthB];
            }

            var input = File.ReadAllText(@"D:\temp\temp.txt");
            foreach (var (esp, eng, leven) in Regex.Matches(input, @"<p><b>([^<]+)</b>\s*</p>\s*<dl><dd>([^<]+)</dd></dl>", RegexOptions.Singleline)
                .Cast<Match>()
                .Select(m => (esp: m.Groups[1].Value, eng: Regex.Replace(m.Groups[2].Value, "^(the|a|to) ", "")))
                .Select(m => (m.esp, m.eng, leven: LevenshteinDistance(m.esp, m.eng) / m.esp.Length))
                .OrderBy(inf => inf.leven))
                Console.WriteLine($"{esp} = {eng} = {leven}");
        }
    }
}
