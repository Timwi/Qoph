using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace Qoph
{
    static class BigBangTheoryEpisodes
    {
        private static (string first, string second)[] getEpisodes()
        {
            return File.ReadAllLines(@"D:\c\Qoph\DataFiles\Big Bang Theory episode titles.txt")
                .Select(e => (str: e, match: Regex.Match(e, @"^The (.*) (\w+)$")))
                .Select(e => e.match.Success ? e : throw new InvalidOperationException(e.str))
                .Select(e => (first: e.match.Groups[1].Value, second: e.match.Groups[2].Value))
                .Where(e => (e.first + e.second).All(ch => ch == ' ' || (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z')))
                .ToArray();
        }

        private static IEnumerable<string> getEpisodeWords()
        {
            return getEpisodes().SelectMany(tup => new[] { tup.first, tup.second }).Select(w => w.ToUpperInvariant().Where(ch => ch >= 'A' && ch <= 'Z').JoinString()).Distinct();
        }

        sealed class Candidate
        {
            public string CandidateName;
            public string PuzzleSolution;
        }

        public static void GenerateSolutionCandidates()
        {
            // Alternates between first and second part of the episode title

            var epi = generateSolutions();
            foreach (var row in epi)
            {
                Console.WriteLine(row.Letter1);
                Console.WriteLine(row.LeftCandidates
                    .OrderBy(lc => lc.PuzzleSolution[0] == lc.CandidateName[0] ? "ZZ" + lc.PuzzleSolution : lc.PuzzleSolution)
                    .Select(lc => $"{(lc.PuzzleSolution[0] == lc.CandidateName[0] ? "~" : "")}{lc.PuzzleSolution}")
                    .JoinString("\n"));
                Console.WriteLine();
                Console.WriteLine(row.Letter2);
                Console.WriteLine(row.RightCandidates
                    .OrderBy(lc => lc.PuzzleSolution[0] == lc.CandidateName[0] ? "ZZ" + lc.PuzzleSolution : lc.PuzzleSolution)
                    .Select(lc => $"{(lc.PuzzleSolution[0] == lc.CandidateName[0] ? "~" : "")}{lc.PuzzleSolution}")
                    .JoinString("\n"));
                Console.WriteLine();
            }
        }

        sealed class Row
        {
            public char Letter1;
            public char Letter2;
            public Candidate[] LeftCandidates;
            public Candidate[] RightCandidates;
        }

        private static Row[] generateSolutions()
        {
            var episodes = getEpisodes();

            var results = new List<Row>();
            foreach (var pair in "PA,NI,CA,VE,RT,ED".Split(','))
            {
                var leftCandidates = episodes.Where(e => e.first.StartsWith(pair[0]) && !e.second.StartsWith(pair[1]) && episodes.Count(e2 => e2.second.EqualsIgnoreCase(e.second)) == 1).ToArray();
                if (leftCandidates.Length == 0)
                    Debugger.Break();
                var rightCandidates = episodes.Where(e => e.second.StartsWith(pair[1]) && !e.first.StartsWith(pair[0]) && episodes.Count(e2 => e2.first.EqualsIgnoreCase(e.first)) == 1).ToArray();
                if (rightCandidates.Length == 0)
                    Debugger.Break();

                results.Add(new Row
                {
                    Letter1 = pair[0],
                    Letter2 = pair[1],
                    LeftCandidates = leftCandidates.Select(lc => new Candidate { CandidateName = lc.first, PuzzleSolution = lc.second }).ToArray(),
                    RightCandidates = rightCandidates.Select(rc => new Candidate { CandidateName = rc.second, PuzzleSolution = rc.first }).ToArray()
                });
            }

            return results.ToArray();
        }
    }
}
