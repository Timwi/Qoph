using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff
{
    static class BeePuzzle
    {
        public static void Do()
        {
            var words = File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt").ToHashSet();
            var script = File.ReadAllText(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Bee Movie script.txt");
            var elements = @"H,He,Li,Be,B,C,N,O,F,Ne,Na,Mg,Al,Si,P,S,Cl,Ar,K,Ca,Sc,Ti,V,Cr,Mn,Fe,Co,Ni,Cu,Zn,Ga,Ge,As,Se,Br,Kr,Rb,Sr,Y,Zr,Nb,Mo,Tc,Ru,Rh,Pd,Ag,Cd,In,Sn,Sb,Te,I,Xe,Cs,Ba,La,Ce,Pr,Nd,Pm,Sm,Eu,Gd,Tb,Dy,Ho,Er,Tm,Yb,Lu,Hf,Ta,W,Re,Os,Ir,Pt,Au,Hg,Tl,Pb,Bi,Po,At,Rn,Fr,Ra,Ac,Th,Pa,U,Np,Pu,Am,Cm,Bk,Cf,Es,Fm,Md,No,Lr,Rf,Db,Sg,Bh,Hs,Mt,Ds,Rg,Cn,Nh,Fl,Mc,Lv,Ts,Og".Split(',');

            var newScript = Regex.Replace(script, @"(?<=\s|\A)[a-zA-Z]+(?=[\s\.,!\?:;]|\z)", m =>
            {
                var word = m.Value;
                if (word.Length < 8 || word.EndsWith("s", StringComparison.InvariantCultureIgnoreCase))
                    return word;
                var modifiedWords = new List<string>();
                for (int i = 0; i < elements.Length; i++)
                {
                    //if (word.EqualsNoCase("city") && elements[i] == "I")
                    //    System.Diagnostics.Debugger.Break();
                    var p = word.IndexOf(elements[i], StringComparison.InvariantCultureIgnoreCase);
                    if (p != -1 && words.Contains(word.Remove(p, elements[i].Length).ToUpperInvariant()))
                        modifiedWords.Add(word.Remove(p, elements[i].Length).Insert(p, $@"[{elements[i]}]"));
                }
                return modifiedWords.Count == 0 ? word : modifiedWords.JoinString("/");
            });

            File.WriteAllText(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Bee Movie script with elements.txt", newScript);
        }
    }
}