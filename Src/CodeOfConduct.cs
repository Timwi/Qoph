using System;
using System.Linq;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace PuzzleStuff
{
    static class CodeOfConductEsperanto
    {
        public static void Do()
        {
            var transforms = Ut.NewArray<Func<string, ConsoleColoredString>>(
                // PELIKANO → FELIĈAN: ŝanĝu 1an al F, 5an al Ĉ, forigu lastan
                s => "F" + s.Remove(4, 1).Insert(4, "Ĉ").Substring(1, s.Length - 2),
                // DENASKULO → PASKON: ŝanĝu unuajn 3 al P kaj lastajn 3 al ON
                s => "P" + s.Substring(3, s.Length - 6) + "ON",
                // ALUMINIO → AL: forigu ĉion krom la unuaj 2
                s => s.Substring(0, 2),
                // ĈINUJO → ĈIUJ: forigu la 3an kaj la lastan
                s => s.Substring(0, s.Length - 1).Remove(2, 1)
            );
            var words = @"1=BIELEFELD,2=WETZLAR,3=RASTATT,5=HOPSTEN,6=TODTMOOS,13=DUDERSTADT,30=XANTEN,31=WINTERBERG,34=PLAUEN"
                .Split(',')
                .Select(piece => piece.Split('='))
                .Select(arr => (id: arr[0], str: arr[1]))
                .ToArray();
            var tt = new TextTable { ColumnSpacing = 2 };
            for (int i = 0; i < words.Length; i++)
            {
                tt.SetCell(0, i, words[i].id.Color(ConsoleColor.White));
                //tt.SetCell(1, i, words[i].str.Color(ConsoleColor.Cyan));
                for (int j = 0; j < transforms.Length; j++)
                    tt.SetCell(j + 2, i, transforms[j](words[i].str));
            }
            tt.WriteToConsole();
        }
    }
}