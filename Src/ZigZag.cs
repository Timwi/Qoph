using System;
using System.IO;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace KtanePuzzles
{
    static class ZigZag
    {
        public static unsafe void FindPairs()
        {
            var wordGroups = File.ReadLines(@"D:\Daten\Wordlists\peter_broda_wordlist_unscored.txt")
                .Concat(File.ReadLines(@"D:\Daten\Wordlists\English words.txt").Select(w => w.ToUpperInvariant()).Where(str => str.All(ch => ch >= 'A' && ch <= 'Z')))
                .Concat(File.ReadLines(@"D:\Daten\Wordlists\sowpods.txt"))
                .Distinct()
                .GroupBy(w => w.Length)
                .Where(gr => gr.Key >= 3)
                .OrderByDescending(gr => gr.Key)
                .Select(gr => (len: gr.Key, words: gr.ToHashSet()))
                .ToList();

            var targetWords = "AIRPORT,NEWYORK".Split(',');
            var targetPairs = targetWords[0].Zip(targetWords[1].Reverse(), (a, b) => (a, b)).ToArray();

            foreach (var (a, b) in targetPairs)
            {
                Console.WriteLine();
                Console.WriteLine($"{a}-{b}:");
                foreach (var (len, words) in wordGroups)
                {
                    foreach (var word in words)
                    {
                        if (!word.StartsWith(a) || !word.EndsWith(b))
                            continue;
                        var reverse = word.Reverse().JoinString();
                        if (reverse != word && words.Contains(reverse))
                            Console.WriteLine($"{word} = {reverse}");
                    }
                }
            }
        }

        public static void Generate()
        {
            var clues = @"OneOpeningLettingLiquidEscape
FLAlternativeMusicianSinclair

PhysicistIsidorIsaac
EastMontenegrinRiver

CtrlYFunction
EuropeanRiver

ScreamTheTenthPixarMovie
RopedStrengthTransmitter

AbbrevForIAmEnRoute
UNAtmosphericAgency

MorallyWorseOrWronger
ExperienceASecondTime

MortisePartner
AssemblyOfNine".Replace("\r", "");
            foreach (var pair in clues.Split("\n\n"))
            {
                var words = pair.ToUpperInvariant().Split("\n");
                Console.WriteLine(words[0].Zip(words[1].Reverse(), (a, b) => $"{a}{b}").JoinString());
            }
        }
    }
}