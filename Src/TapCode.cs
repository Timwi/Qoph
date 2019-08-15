using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff
{
    static class TapCode
    {
        public static (char ch, int row, int col)[] Tap = "ABCDE|FGHIJ|LMNOP|QRSTU|VWXYZ".Split('|').SelectMany((row, rowIx) => row.Select((ch, chIx) => (ch: ch, row: rowIx + 1, col: chIx + 1))).ToArray();
        public static Dictionary<char, (char ch, int row, int col)> TapFromCh = Tap.ToDictionary(m => m.ch, m => m);
        //public static Dictionary<string, string> ChFromMorse = Morse.ToDictionary(m => m.morse, m => m.ch);
    }
}