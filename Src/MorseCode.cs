using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;

namespace KtanePuzzles
{
    static class MorseCode
    {
        public static (string morse, string ch)[] Morse =
            @".-|-...|-.-.|-..|.|..-.|--.|....|..|.---|-.-|.-..|--|-.|---|.--.|--.-|.-.|...|-|..-|...-|.--|-..-|-.--|--..".Split('|').Select((str, ix) => (str, ((char) ('A' + ix)).ToString()))
                .Concat(@"-----|.----|..---|...--|....-|.....|-....|--...|---..|----.".Split('|').Select((str, ix) => (str, ((char) ('0' + ix)).ToString())))
                //.Concat(@"Ä=.-.-|Á=.--.-|Ch=----|É=..-..|Ñ=--.--|Ö=---.|Ü=..--".Split('|').Select(t => t.Split('=')).Select(arr => (arr[1], arr[0])))
                .Concat(@".>.-.-.-|,>--..--|:>---...|?>..--..|'>.----.|->-....-|/>-..-.|(>-.--.|)>-.--.-|"">.-..-.|@>.--.-.|=>-...-|!>-.-.--".Split('|').Select(t => t.Split('>')).Select(arr => (arr[1], arr[0])))
                .ToArray();
        public static Dictionary<string, string> MorseFromCh = Morse.ToDictionary(m => m.ch, m => m.morse);
        public static Dictionary<string, string> ChFromMorse = Morse.ToDictionary(m => m.morse, m => m.ch);
    }
}