using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using RT.Json;
using RT.Serialization;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff.BombDisposal
{
    static class Critical
    {
        public static void Do()
        {
            var file = JsonDict.Parse(File.ReadAllText(@"E:\other\Wikipedia article titles of people with D diacritic.json"));
            file["D"] = file["D"].GetList().Where(item => item.GetString().All(ch => !char.IsLetter(ch) || "ĎďĐđƊƋƌȡɖɗᵭᶁᶑḊḋḌḍḎḏḐḑḒḓabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(ch))).ToJsonList();
            File.WriteAllText(@"E:\other\Wikipedia article titles of people with D diacritic.json", file.ToStringIndented());

            //var d = ClassifyJson.DeserializeFile<Dictionary<char, List<string>>>(@"E:\other\Wikipedia article titles of people with one diacritic (sorted by page views).json");
            //Console.WriteLine(d['D'].Take(20).JoinString("\r\n"));

            //foreach (var word in @"DEALINGS,DILATORY".Split(','))
            //{
            //    ConsoleUtil.WriteLine(word.Color(ConsoleColor.White));
            //    foreach (var ch in word)
            //        Console.WriteLine($"{ch} - {d[ch].Take(20).JoinString(", ")}");
            //    Console.WriteLine();
            //}
        }

        public static void SortPeoplePagesByPageViews()
        {
            Console.WriteLine("Reading 1");
            var d = ClassifyJson.DeserializeFile<Dictionary<char, List<string>>>(@"E:\other\Wikipedia article titles of people with L diacritic.json");
            Console.WriteLine("Distinct");
            var uniqueTitles = d.SelectMany(p => p.Value).Distinct().ToHashSet();
            Console.WriteLine($" — {uniqueTitles.Count} unique titles");
            Console.WriteLine("Reading 2");
            var counts = new Dictionary<string, int>();
            var titlesFound = 0;
            foreach (var line in File.ReadLines(@"E:\other\pagecounts-2020-05-views-ge-5-totals"))
                if (line.StartsWith("en"))
                {
                    var m = Regex.Match(line, $@"^en\.[a-z] (.*) (\d+)$");
                    var title = m.Groups[1].Value.Replace("_", " ");
                    if (m.Success && uniqueTitles.Contains(title) && int.TryParse(m.Groups[2].Value, out var c))
                    {
                        Console.Write($"{titlesFound} found\r");
                        counts.IncSafe(title, c);
                        titlesFound++;
                    }
                }
            Console.WriteLine();

            foreach (var kvp in d)
            {
                Console.WriteLine($"Sorting {kvp.Key}");
                kvp.Value.Sort((a, b) => counts.Get(b, 5) - counts.Get(a, 5));
            }
            Console.WriteLine("Writing");
            ClassifyJson.SerializeToFile(d, @"E:\other\Wikipedia article titles of people with L diacritic (sorted by page views).json");
        }

        public static void FindPeoplePagesWithDDiacritic()
        {
            FindPeoplePagesWithSpecialLetter(@"E:\other\Wikipedia article titles of people with D diacritic.json", title =>
            {
                if (title.Any("ĎďĐđƊƋƌȡɖɗᵭᶁᶑḊḋḌḍḎḏḐḑḒḓ".Contains))
                    return 'D';
                return null;
            });
        }

        public static void FindPeoplePagesWithLDiacritic()
        {
            FindPeoplePagesWithSpecialLetter(@"E:\other\Wikipedia article titles of people with L diacritic.json", title =>
            {
                if (title.Any("ĹĺĻļŁłƚȴȽɫɬɭᶅḶḷḸḹḺḻḼḽⱠⱡⱢ".Contains))
                    if (title.All(ch => !char.IsLetter(ch) || "ĹĺĻļŁłƚȴȽɫɬɭᶅḶḷḸḹḺḻḼḽⱠⱡⱢabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(ch)))
                        return 'L';
                return null;
            });
        }

        public static void FindPeoplePagesWithOneDecomposableDiacritic()
        {
            FindPeoplePagesWithSpecialLetter(@"E:\other\Wikipedia article titles of people with one diacritic.json", title =>
            {
                var titleN = title.Normalize(NormalizationForm.FormD);
                if (titleN.Length != title.Length + 1)
                    return null;
                var low = 0;
                while (titleN[low] == title[low]) low++;
                var high = titleN.Length - 1;
                while (titleN[high] == title[high - 1]) high--;
                return char.ToUpperInvariant(titleN[low]);
            });
        }

        private static void FindPeoplePagesWithSpecialLetter(string path, Func<string, char?> isTitleSuitable)
        {
            var d = new Dictionary<char, List<string>>();
            Match m;
            var state = 0;
            var sb = new StringBuilder();
            string title = null;
            char letter = default;

            foreach (var line in File.ReadLines(@"E:\Other\enwiki-20200401-pages-articles-multistream.xml"))
            {
                switch (state)
                {
                    case 0: // Looking for <page>
                        if (line == "  <page>")
                        {
                            sb.Clear();
                            sb.AppendLine(line);
                            state = 1;
                        }
                        break;

                    case 1: // Expecting <title> to following immediately after <page>
                        if (line.StartsWith("    <title>") && (m = Regex.Match(line, @"^\s*<title>(.*)</title>$")).Success)
                        {
                            title = m.Groups[1].Value;
                            var possibleLetter = isTitleSuitable(title);
                            if (possibleLetter != null)
                            {
                                letter = possibleLetter.Value;
                                state = 2;
                                sb.AppendLine(line);
                                break;
                            }
                        }
                        state = 0;
                        goto case 0;

                    case 2: // Reading rest of the article XML until </page>
                        sb.AppendLine(line);
                        if (line == "  </page>")
                        {
                            var xml = XElement.Parse(sb.ToString());
                            var articleText = xml.Element("revision").Element("text").Value;
                            if ((m = Regex.Match(articleText, @"\[\[Category:(1\d{3})[_ ]births\]\]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
                            {
                                Console.WriteLine($"Found {m.Groups[1].Value} birth with {letter}: {title}");
                                d.AddSafe(letter, title);
                            }
                            state = 0;
                        }
                        break;
                }
            }
            ClassifyJson.SerializeToFile(d, path);
        }
    }
}