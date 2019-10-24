using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff
{
    static class General
    {
        public static bool ContainsAll(this string str, out string remaining, params char[] characters)
        {
            remaining = str;
            for (int i = 0; i < characters.Length; i++)
            {
                var pos = remaining.IndexOf(characters[i]);
                if (pos == -1)
                    return false;
                remaining = remaining.Remove(pos, 1);
            }
            return true;
        }

        public static bool ContainsAll(this string str, params char[] characters) => ContainsAll(str, out _, characters);

        public static void ReplaceInFile(this string path, string startMarkerRegex, string endMarkerRegex, string newText)
        {
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), $@"(?<={startMarkerRegex})(\r?\n)*( *).*?(\r?\n *)?(?={endMarkerRegex})", m => m.Groups[1].Value + newText.Unindent().Indent(m.Groups[2].Length) + m.Groups[3].Value, RegexOptions.Singleline));
        }

        public static bool ContainsAll(this string str, string characters) => ContainsAll(str, out _, characters.ToCharArray());

        public static int[] ConsistsOfAny(this string str, params Func<char, bool>[] predicates) => consistsOfAnyRecurse(str, predicates, 0)?.ToArray();

        private static IEnumerable<int> consistsOfAnyRecurse(string str, Func<char, bool>[] predicates, int skip)
        {
            if (str.Length == 0)
                return Enumerable.Empty<int>();
            if (str.Length > predicates.Length - skip)
                return null;
            for (int i = skip; i < predicates.Length; i++)
            {
                if (predicates[i](str[0]))
                {
                    var t = predicates[i];
                    if (i != skip)
                    {
                        predicates[i] = predicates[skip];
                        predicates[skip] = t;
                    }

                    var result = consistsOfAnyRecurse(str.Substring(1), predicates, skip + 1);

                    if (i != skip)
                    {
                        predicates[skip] = predicates[i];
                        predicates[i] = t;
                    }

                    if (result != null)
                        return i.Concat(result.Select(ix => ix == i ? skip : ix));
                }
            }
            return null;
        }

        public static int LevenshteinDistance(string a, string b)
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
    }
}
