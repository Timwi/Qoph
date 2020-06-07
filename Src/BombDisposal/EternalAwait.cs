using System;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff.BombDisposal
{
    static class EternalAwait
    {
        public static void Do()
        {
            var braille = @"A=1 B=12 C=14 D=145 E=15 F=124 G=1245 H=125 I=24 J=245 K=13 L=123 M=134 N=1345 O=135 P=1234 Q=12345 R=1235 S=234 T=2345 U=136 V=1236 W=2456 X=1346 Y=13456 Z=1356"
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(bit => Regex.Match(bit, @"^([A-Z])=(\d+)$"))
                .Select(m => (letter: m.Groups[1].Value[0], dots: m.Groups[2].Value))
                .ToArray();

            string letter2braille(char letter) => braille.First(b => b.letter == letter).dots;
            bool[] bitmap(string str)
            {
                if (str.Length != 3)
                    throw new ArgumentException("Length of the string must be 3.", nameof(str));
                var ret = new bool[18];
                for (var i = 0; i < 3; i++)
                    foreach (var digit in letter2braille(str[i]))
                        ret[6 * ((digit - '1') % 3) + 2 * i + ((digit - '1') / 3)] = true;
                return ret;
            }

            var cluephrase = @"FIR,STW,ORD,OFL,UVI,SRA,GET,WOT,RAC,KSE,VEN".Split(',');
            var counts = new int[18];
            for (var i = 0; i < cluephrase.Length; i++)
            {
                var bm = bitmap(cluephrase[i]);
                Console.WriteLine(bm.Select(b => b ? "█" : "░").Split(6).Select(row => row.JoinString(" ")).JoinString("\n"));
                Console.WriteLine();
                for (var j = 0; j < 18; j++)
                    if (bm[j])
                        counts[j]++;
            }

            Console.WriteLine(counts.Select(c => $"{c,3}").Split(6).Select(row => row.JoinString()).JoinString("\n"));
        }
    }
}