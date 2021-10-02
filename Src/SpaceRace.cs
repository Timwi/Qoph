using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace Qoph
{
    static class SpaceRace
    {
        public static void Generate()
        {
            const int f = 5;       // Full-size factor
            const int r = 6;    // Radius of the circle
            const int offset = 6;

            {
                int transformLetter(char ltr) => ((ltr - 'A') * (2 * offset - 1)) % 26;
                double angle(char ltr) => Math.PI * 2.0 / 26 * transformLetter(ltr) + Math.PI * 1.5;
                double getX(char ltr) => r * Math.Cos(angle(ltr));
                double getY(char ltr) => r * Math.Sin(angle(ltr));
                double controlX(char ltr) => r * .8 * Math.Cos(angle(ltr));
                double controlY(char ltr) => r * .8 * Math.Sin(angle(ltr));
                const double width = f * (2 * r + 2);
                const double height = width;
                //const string cluephrase = "SORANORMAZUR";
                const string cluephrase = "SPACETHEFINALBLANK";
                //var pairs = @"AQUINO,VASH;BRUNT,QUARK;CRETAK,KOVAL;DUKAT,THRAX;EZRI,RENHOL;FENTO,NURIA;GRILKA,WORF;HALIZ,TERSA".Split(';').Select(str => str.Split(',')).ToArray();
                var pairs = @"ANIJ,SOJEF;BAREIL,RO;CRETAK,KOVAL;DUKAT,THRAX;ERIS,WEYOUN;FENTO,NURIA;GRILKA,WORF;HOGAN,VASH;ISHKA,QUARK".Split(';').Select(str => str.Split(',')).ToArray();
                var decoys = new Dictionary<string, string>
                {
                    ["SOJEF"] = "CI",
                    ["DUKAT"] = "CI",

                    ["ISHKA"] = "OP",
                    ["VASH"] = "OP",

                    ["WORF"] = "VU",
                    ["RO"] = "VU",

                    ["QUARK"] = "QD",
                    ["CRETAK"] = "QD",

                    ["ANIJ"] = "HN",
                    ["BAREIL"] = "HN",

                    ["HOGAN"] = "ER",
                    ["WEYOUN"] = "ER",

                    ["KOVAL"] = "OY",
                    ["ERIS"] = "OY",

                    ["GRILKA"] = "SY",
                    ["FENTO"] = "SY",

                    ["THRAX"] = "XY",
                    ["NURIA"] = "XY",
                };
                /**/

                string viewBox = $"{-width / 2} {-height / 2} {width} {height}";

                string makePath(string word, double x, double y, bool avoidCurves = false, string textColor = null) =>
                    $"<g transform='translate({x:.00} {y:.00})'>" +
                        $@"<path fill='none' d='M{getX(word[0]):.00},{getY(word[0]):.00}{(avoidCurves ? "L" : "C")}{Enumerable.Range(0, word.Length - 1)
                            .Select(ix => (p: word[ix], q: word[ix + 1]))
                            .Select(tup => avoidCurves ? $"{getX(tup.q):.00},{getY(tup.q):.00}" : $"{controlX(tup.p):.00},{controlY(tup.p):.00} {controlX(tup.q):.00},{controlY(tup.q):.00} {getX(tup.q):.00},{getY(tup.q):.00}")
                            .JoinString(" ")}' />" +
                        $"<circle cx='{getX(word[0]):.00}' cy='{getY(word[0]):.00}' r='.1' fill='black' />" +
                        $"<circle cx='0' cy='0' r='.5' fill='white' fill-opacity='0.7' />" +
                    //(textColor != null ? $"<text x='0' y='0' font-size='2' stroke-width='.3' stroke='white' paint-order='stroke' fill='{textColor}'>{word}</text>" : null) +
                    $"</g>";

                General.ReplaceInFile($@"D:\c\Qoph\EnigmorionFiles\space-race.html", "<!--%-->", "<!--%%-->",
                    $@"<svg style='width: 80%; margin: 1cm auto; display: block' xmlns='http://www.w3.org/2000/svg' viewBox='{viewBox}' stroke='black' stroke-width='.1' stroke-linecap='round' text-anchor='middle' font-size='5'>" +
                    $@"<g>{Enumerable.Range(0, 26).Select(i => (char) ('A' + i)).OrderBy(ch => getY(ch)).Select(ch =>
                        $@"<circle cx='{f * getX(ch):.00}' cy='{f * getY(ch):.00}' r='1' fill='{("AEIOU".Contains(ch) ? "black" : "none")}' />" +
                        //$@"<text x='{f * 1.1 * getX(ch)}' y='{f * 1.1 * getY(ch) + 1}' font-size='2'>{ch}</text>" +
                        $@"").JoinString()}" +
                    $@"</g>" +
                    $@"<g transform='translate(28,-28)'>{(makePath("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 0, 0, avoidCurves: true))}</g>" +
                    $@"<g>{(Enumerable.Range(0, pairs.Length).Zip(cluephrase.Split(2), (ix, b) => (names: pairs[ix], decoy: pairs[ix].Select(name => decoys.Get(name, null)).ToArray(), clueLetters: b)).SelectMany(tup =>
                    {
                        var lineSegment = new EdgeD(f * getX(tup.clueLetters[0]), f * getY(tup.clueLetters[0]), f * getX(tup.clueLetters[1]), f * getY(tup.clueLetters[1]));
                        var loc1 = tup.decoy[0] == null ? lineSegment.Start * 2 / 3 + lineSegment.End * 1 / 3 : Intersect.LineWithLine(new EdgeD(f * getX(tup.decoy[0][0]), f * getY(tup.decoy[0][0]), f * getX(tup.decoy[0][1]), f * getY(tup.decoy[0][1])), lineSegment);
                        var loc2 = tup.decoy[1] == null ? lineSegment.Start * 1 / 3 + lineSegment.End * 2 / 3 : Intersect.LineWithLine(new EdgeD(f * getX(tup.decoy[1][0]), f * getY(tup.decoy[1][0]), f * getX(tup.decoy[1][1]), f * getY(tup.decoy[1][1])), lineSegment);

                        return Ut.NewArray<(double y, string svg)>(
                            //$"<line x1='{lineSegment.Start.X}' y1='{lineSegment.Start.Y}' x2='{lineSegment.End.X}' y2='{lineSegment.End.Y}' stroke-width='.5' stroke='cornflowerblue' opacity='.25' />" +
                            (loc1.Y, makePath(tup.names[0], loc1.X, loc1.Y, textColor: tup.decoy[0] == null ? "red" : decoys.Count(p => p.Value == tup.decoy[0]) == 1 ? "#0000ff" : "black")),
                            (loc2.Y, makePath(tup.names[1], loc2.X, loc2.Y, textColor: tup.decoy[1] == null ? "red" : decoys.Count(p => p.Value == tup.decoy[1]) == 1 ? "#0000ff" : "black")));
                    }).OrderBy(tup => tup.y).Select(tup => tup.svg).JoinString())}" +
                    $@"</g>" +
                $@"</svg>");
            }
        }
    }
}