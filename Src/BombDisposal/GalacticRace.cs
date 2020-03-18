using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace PuzzleStuff.BombDisposal
{
    static class GalacticRace
    {
        public static void Generate()
        {
            const int f = 5;       // Full-size factor
            const int r = 6;    // Radius of the circle
            const int offset = 8;

            static int transformLetter(char ltr) => ((ltr - 'A') * (2 * offset - 1)) % 26;
            static double angle(char ltr) => Math.PI * 2.0 / 26 * transformLetter(ltr) + Math.PI * 1.5;
            static double getX(char ltr) => r * Math.Cos(angle(ltr));
            static double getY(char ltr) => r * Math.Sin(angle(ltr));
            static double controlX(char ltr) => r * .8 * Math.Cos(angle(ltr));
            static double controlY(char ltr) => r * .8 * Math.Sin(angle(ltr));
            const double width = f * (2 * r + 2);
            const double height = width;
            //"GUINANSORANORMAZUR",
            //"SORANGUINANORMAZUR",
            //"SORANMAZURORGUINAN",
            const string cluephrase = "SORANORMAZUR";
            var pairs = @"AQUINO,VASH;BRUNT,QUARK;CRETAK,KOVAL;DUKAT,THRAX;EZRI,RENHOL;FENTO,NURIA;GRILKA,WORF;HALIZ,TERSA".Split(';').Select(str => str.Split(',')).ToArray();
            var decoys = "PE,TE;YC,TE;YG,YC;YG,FC;FC,QD;PE,QD".Split(';').Select(r => r.Split(',')).ToArray();
            /**/

            string viewBox = $"{-width / 2} {-height / 2} {width} {height}";

            string makePath(string word, double x, double y, bool avoidCurves = false) =>
                $"<g transform='translate({x} {y})'>" +
                    $@"<path fill='none' d='M{getX(word[0])},{getY(word[0])}{(avoidCurves ? "L" : "C")}{Enumerable.Range(0, word.Length - 1)
                        .Select(ix => (p: word[ix], q: word[ix + 1]))
                        .Select(tup => avoidCurves ? $"{getX(tup.q)},{getY(tup.q)}" : $"{controlX(tup.p)},{controlY(tup.p)} {controlX(tup.q)},{controlY(tup.q)} {getX(tup.q)},{getY(tup.q)}")
                        .JoinString(" ")}' />" +
                    $"<circle cx='{getX(word[0])}' cy='{getY(word[0])}' r='.1' fill='black' />" +
                    $"<circle cx='0' cy='0' r='.5' fill='white' fill-opacity='0.7' />" +
                    //$"<text x='0' y='0' font-size='2'>{word}</text>" +
                $"</g>";

            File.WriteAllText(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Galactic Race\Galactic Race.svg",
                $@"<svg xmlns='http://www.w3.org/2000/svg' viewBox='{viewBox}' stroke='black' stroke-width='.1' stroke-linecap='round' text-anchor='middle' font-size='5'>" +
                $@"<g>{Enumerable.Range(0, 26).Select(i => (char) ('A' + i)).Select(ch =>
                    $@"<circle cx='{f * getX(ch)}' cy='{f * getY(ch)}' r='1' fill='{("AEIOU".Contains(ch) ? "black" : "none")}' />" +
                    //$@"<text x='{f * 1.1 * getX(ch)}' y='{f * 1.1 * getY(ch) + 1}' font-size='2'>{ch}</text>" +
                    $@"").JoinString()}
                </g>" +
                $@"<g transform='translate(28,-28)'>{makePath("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 0, 0, true)}</g>" +
                $@"<g>{(Enumerable.Range(0, pairs.Length).Zip(cluephrase.Split(2), (ix, b) => (names: pairs[ix], decoy: decoys[ix], clueLetters: b)).Select(tup =>
                {
                    var lineSegment = new EdgeD(f * getX(tup.clueLetters[0]), f * getY(tup.clueLetters[0]), f * getX(tup.clueLetters[1]), f * getY(tup.clueLetters[1]));
                    var decoy1 = new EdgeD(f * getX(tup.decoy[0][0]), f * getY(tup.decoy[0][0]), f * getX(tup.decoy[0][1]), f * getY(tup.decoy[0][1]));
                    var loc1 = Intersect.LineWithLine(decoy1, lineSegment);
                    var decoy2 = new EdgeD(f * getX(tup.decoy[1][0]), f * getY(tup.decoy[1][0]), f * getX(tup.decoy[1][1]), f * getY(tup.decoy[1][1]));
                    var loc2 = Intersect.LineWithLine(decoy2, lineSegment);

                    return
                        //$"<line x1='{lineSegment.Start.X}' y1='{lineSegment.Start.Y}' x2='{lineSegment.End.X}' y2='{lineSegment.End.Y}' stroke-width='.5' stroke='cornflowerblue' />" +
                        makePath(tup.names[0], loc1.X, loc1.Y) +
                        makePath(tup.names[1], loc2.X, loc2.Y);
                }).JoinString())}
                </g>" +
                $@"</svg>");
        }
    }
}
