using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff.BombDisposal
{
    static class RectangleMangle
    {
        public static void RectangleMangle_Find()
        {
            var allData = @"
NORTH/!BOREAL,SOUTH/!AUSTRAL,WEST/!OCCIDENTAL,EAST/!ORIENTAL
WINTER/!HIBERNAL,SPRING/!VERNAL,SUMMER/!ESTIVAL,AUTUMN/!AUTUMNAL
DONATELLO,MICHELANGELO,RAPHAEL,LEONARDO
#ARAMIS,PORTHOS,ATHOS,DARTAGNAN
CONQUEST,WAR,FAMINE,DEATH
ADDITION/ADD/PLUS/SUM/ADDEND/ADDEND,SUBTRACTION/SUBTRACT/MINUS/DIFFERENCE/MINUEND/SUBTRAHEND,MULTIPLICATION/MULTIPLY/TIMES/PRODUCT/FACTOR/FACTOR,DIVISION/DIVIDE/OVER/QUOTIENT/DIVIDEND/DIVISOR
FIRE,WATER,EARTH,AIR
MATTHEW,MARK,LUKE,JOHN
JOHNLENNON/LENNON,PAULMCCARTNEY/MCCARTNEY,GEORGEHARRISON/HARRISON,RINGOSTARR/STARR
LEFTVENTRICLE,RIGHTVENTRICLE,LEFTAURICLE,RIGHTAURICLE
TINKYWINKY,DIPSY,LAALAA,PO
ADENINE,THYMINE,GUANINE,CYTOSINE
SPADES,HEARTS,CLUBS,DIAMONDS
MERCURY,VENUS,EARTH,MARS
JUPITER,SATURN,URANUS,NEPTUNE
#CALLISTO,EUROPA,GANYMEDE,IO
#BLACKBILE/CHOLERIC,YELLOWBILE/MELANCHOLIC,PHLEGM/PHLEGMATIC,BLOOD/SANGUINE
#RAIDERSOFTHELOSTARK,TEMPLEOFDOOM,LASTCRUSADE,KINGDOMOFTHECRYSTALSKULL
ENGLAND,SCOTLAND,WALES,NORTHERNIRELAND
JUSTICE,PRUDENCE,FORTITUDE,TEMPERANCE
JERRY,GEORGE,ELAINE,KRAMER
#RED,GREEN,BLUE,YELLOW
CYAN,MAGENTA,YELLOW,KEY
#DUKKHA,SAMUDAYA,NIRODHA,MAGGA
#SARAH,REBEKAH,LEAH,RACHEL
#RIGVEDA,SAMAVEDA,YAJURVEDA,ATHARVAVEDA
A,B,AB,O
SOLID,LIQUID,GAS,PLASMA
GRAVITY,ELECTROMAGNETISM,WEAK,STRONG
INTAKE,COMPRESSION,POWER,EXHAUST
FIRSTBASE,SECONDBASE,THIRDBASE,HOMEPLATE
COLORADO,UTAH,NEWMEXICO,ARIZONA
MRFANTASTIC,THEINVISIBLEWOMAN,THEHUMANTORCH,THETHING
GRYFFINDOR,HUFFLEPUFF,RAVENCLAW,SLYTHERIN
HARRYPOTTER,FLEURDELACOUR,CEDRICDIGGORY,VIKTORKRUM
#WESTEROS,ESSOS,SOTHORYOS,ULTHOS
PETERVENKMAN,RAYSTANTZ,EGONSPENGLER,WINSTONZEDDEMORE
STANMARSH/STAN,KYLEBROFLOVSKI/KYLE,KENNYMCCORMICK/KENNY,ERICCARTMAN/ERIC
INKY,BLINKY,PINKY,CLYDE
MURDOCK,BABARACUS,HANNIBAL,FACEMAN
PLUM,ORCHID,CHRYSANTHEMUM,BAMBOO"
                .Trim().Replace("\r", "").Split('\n')
                .Where(row => row.Length > 0 && !row.StartsWith("#"))
                .Select(row => row.TrimStart('#').Split(',', '/').Where(str => !str.StartsWith("!")).Select(str => str.TrimStart('!')).ToArray())
                .ToArray();

            var set = new List<(string fragment, string item1, int dataIx1, string item2, int dataIx2)>();
            for (int i = 0; i < allData.Length; i++)
                for (int j = i + 1; j < allData.Length; j++)
                    for (int ii = 0; ii < allData[i].Length; ii++)
                        for (int jj = 0; jj < allData[j].Length; jj++)
                            if (allData[i][ii].Length == allData[j][jj].Length)
                            {
                                var commonalities = Enumerable.Range(0, allData[i][ii].Length).Where(ix => allData[i][ii][ix] == allData[j][jj][ix]).Select(ix => allData[i][ii][ix]).JoinString();
                                if (commonalities.Length > 0)
                                    set.Add((commonalities, allData[i][ii], i, allData[j][jj], j));
                            }

            ConsoleUtil.WriteParagraphs(set.Select(s => s.fragment).GroupBy(s => s[0]).OrderBy(gr => gr.Key).Select(gr => new ConsoleColoredString($"{(gr.Key + ":").Color(ConsoleColor.White)} {gr.Distinct().Order().JoinString(", ").Color(ConsoleColor.DarkGreen)}")).JoinColoredString("\n"));
            Console.WriteLine();
            var phrase = "EUROPAIOCALLISTO";

            IEnumerable<int[]> recurse(int[] sofar, string phraseLeft, int[] available)
            {
                if (phraseLeft == "")
                {
                    yield return sofar;
                    yield break;
                }

                for (int len = phraseLeft.Length; len > 0; len--)
                {
                    var substr = phraseLeft.Substring(0, len);
                    for (int p = 0; p < available.Length; p++)
                        if (set[available[p]].fragment == substr)
                            foreach (var solution in recurse(
                                sofar.Insert(sofar.Length, available[p]),
                                phraseLeft.Substring(len),
                                available
                                    .Where(av => set[av].dataIx1 != set[available[p]].dataIx1 && set[av].dataIx1 != set[available[p]].dataIx2 && set[av].dataIx2 != set[available[p]].dataIx1 && set[av].dataIx2 != set[available[p]].dataIx2)
                                    .ToArray()))
                                yield return solution;
                }
            }

            // GRID FOR A SOLUTION WITH 14 ELEMENTS ONLY!
            var gridRaw = RectangleMangle_getGrid();
            var grid = Enumerable.Range(0, 14).Select(ix =>
            {
                var numPositions = Enumerable.Range(0, gridRaw.Length).Where(grIx => gridRaw[grIx] != null && gridRaw[grIx].Value.val == ix && gridRaw[grIx].Value.grp1 == false).ToArray();
                var ltrPositions = Enumerable.Range(0, gridRaw.Length).Where(grIx => gridRaw[grIx] != null && gridRaw[grIx].Value.val == ix && gridRaw[grIx].Value.grp1 == true).ToArray();
                var x = numPositions.Aggregate(0, (prev, next) => prev ^ (next % 10));
                var x2 = ltrPositions.Aggregate(0, (prev, next) => prev ^ (next % 10));
                var y = numPositions.Aggregate(0, (prev, next) => prev ^ (next / 10));
                var y2 = ltrPositions.Aggregate(0, (prev, next) => prev ^ (next / 10));
                if (numPositions.Length != 3 || ltrPositions.Length != 3 || x != x2 || y != y2)
                    Debugger.Break();
                return (ix, x, y);
            })
                .OrderBy(loc => loc.y).ThenBy(loc => loc.x)
                .ToArray();

            foreach (var solution in recurse(new int[0], phrase, Enumerable.Range(0, set.Count).ToArray()))
            {
                if (solution.Length != 14)
                {
                    // ABOVE GRID WORKS ONLY FOR A SOLUTION WITH 14 ELEMENTS
                    continue;
                }

                ConsoleUtil.WriteLine($"{solution.Select(ix => set[ix].fragment).JoinString(", ").Color(ConsoleColor.White)} ({solution.Length.ToString().Color(ConsoleColor.Yellow)})", null);
                var sb = new StringBuilder();
                for (var solIx = 0; solIx < solution.Length; solIx++)
                {
                    var setIx = solution[solIx];
                    var (fragment, item1, dataIx1, item2, dataIx2) = set[setIx];
                    ConsoleUtil.WriteLine($"    {fragment.Color(ConsoleColor.Yellow)} = {item1.Color(ConsoleColor.Green)} + {item2.Color(ConsoleColor.Cyan)}", null);
                    var (ix, x, y) = grid[solIx];
                    string remainders(int dataIx, string exception)
                    {
                        var batchLen = allData[dataIx].Length / 4;
                        var ixInBatch = allData[dataIx].IndexOf(exception) % batchLen;
                        return Enumerable.Range(0, 4).Select(i => allData[dataIx][ixInBatch + i * batchLen]).Where(i => i != exception).JoinString("\t");
                    }
                    sb.AppendLine($"{(char) ('A' + ix)}\t{remainders(dataIx1, item1)}\t{item1}\t{ix + 1}\t{remainders(dataIx2, item2)}\t{item2}\t{fragment}");
                }
                Clipboard.SetText(sb.ToString());
                Console.ReadLine();
                Console.WriteLine();
            }
        }

        private static (string valStr, int val, bool grp1)?[] RectangleMangle_getGrid() => @" 6 6 N M M11 J11 J N
13 512 M██ H██12 H E
10 F 3 F 3████11 J K
 6██ 2 F░░ 7 E 2 7 E
 814██ 1 1██ B B H N
 8 412 A 4 8 L██░░ A
 9 5████ 3 C 5 G G I
101414██ 1 710 G██ A
 9██ 2 9 4 K B██ D K
13 D C I13 C L L D I".Replace("\r", "").Split('\n').SelectMany(row => row.Split(2).Select(str =>
                          int.TryParse(str, out int value) ? (valStr: str.Trim(), val: value - 1, grp1: false).Nullable() :
                          str[1] >= 'A' && str[1] <= 'Z' ? (valStr: str.Trim(), val: str[1] - 'A', grp1: true).Nullable() : null).ToArray()).ToArray();

        public static void RectangleMangle_ConstructGrid()
        {
            const int w = 10;
            const int h = 10;

            IEnumerable<(int[] board, int num)> recurse(int[] sofar, int ix)
            {
                var spaces = Enumerable.Range(0, w * h).Where(i => (i % w) > 0 && (i % w) < w - 1 && (i / w) > 0 && (i / w) < h - 1 && sofar[i] == 0).ToArray().Shuffle();
                var any = false;
                foreach (var space in spaces)
                {
                    var x = space % w;
                    var y = space / w;
                    var candidates = Enumerable.Range(0, w)
                        .Where(x2 => x2 != x && sofar[x2 + w * y] == 0)
                        .SelectMany(x2 => Enumerable.Range(0, h).Where(y2 => y2 != y && sofar[x + w * y2] == 0 && sofar[x2 + w * y2] == 0).Select(y2 => (x2, y2)))
                        .ToArray();
                    var candidatePairs = candidates.UniquePairs()
                        .Select(tup => (x1: tup.Item1.x2, y1: tup.Item1.y2, tup.Item2.x2, tup.Item2.y2))
                        .Where(tup => tup.x1 != tup.x2 && tup.y1 != tup.y2)
                        .ToArray().Shuffle();
                    foreach (var (x1, y1, x2, y2) in candidatePairs)
                    {
                        sofar[space] = -1;
                        sofar[x1 + w * y1] = 2 * ix + 1;
                        sofar[x + w * y1] = 2 * ix + 1;
                        sofar[x1 + w * y] = 2 * ix + 1;
                        sofar[x2 + w * y] = 2 * ix + 2;
                        sofar[x + w * y2] = 2 * ix + 2;
                        sofar[x2 + w * y2] = 2 * ix + 2;

                        foreach (var result in recurse(sofar, ix + 1))
                        {
                            yield return result;
                            any = true;
                        }

                        sofar[space] = 0;
                        sofar[x1 + w * y1] = 0;
                        sofar[x + w * y1] = 0;
                        sofar[x1 + w * y] = 0;
                        sofar[x2 + w * y] = 0;
                        sofar[x + w * y2] = 0;
                        sofar[x2 + w * y2] = 0;
                    }
                }
                if (!any)
                {
                    yield return (sofar.ToArray(), ix);
                    yield break;
                }
            }

            var best = 0;
            foreach (var (solution, num) in recurse(new int[w * h], 0))
            {
                if (num > best)
                {
                    best = num;
                    ConsoleUtil.WriteLine($"{num}".Color(ConsoleColor.White));
                    ConsoleUtil.WriteLine(solution.Split(w)
                        .Select(row => row
                            .Select(i => (i == -1 ? "██" : i == 0 ? "░░" : i % 2 == 0 ? ((char) ('A' + (i - 1) / 2)).ToString().PadLeft(2) : ((i + 1) / 2).ToString().PadLeft(2))
                                .Color(i == -1 ? ConsoleColor.DarkGray : ConsoleColor.White, i < 1 ? ConsoleColor.Black : (ConsoleColor) (((i + 1) / 2 - 1) % 15 + 1)))
                            .JoinColoredString())
                        .JoinColoredString("\n"));
                    Console.WriteLine();
                }
            }
        }

        public static void RectangleMangle_ConstructGridWithSquares()
        {
            const int w = 10;
            const int h = 10;

            IEnumerable<(int[] board, int num)> recurse(int[] sofar, int ix)
            {
                var spaces = Enumerable.Range(0, w * h).Where(i => (i % w) > 0 && (i % w) < w - 1 && (i / w) > 0 && (i / w) < h - 1 && sofar[i] == 0).ToArray().Shuffle();
                var any = false;
                foreach (var space in spaces)
                {
                    var x = space % w;
                    var y = space / w;
                    var candidates = Enumerable.Range(0, w)
                        .Where(x2 => x2 != x && y + (x2 - x) >= 0 && y + (x2 - x) < h && sofar[x2 + w * y] == 0 && sofar[x + w * (y + x2 - x)] == 0 && sofar[x2 + w * (y + x2 - x)] == 0)
                        .Select(x2 => (x2, y2: y + x2 - x))
                        .ToArray();
                    var candidatePairs = candidates.UniquePairs()
                        .Select(tup => (x1: tup.Item1.x2, y1: tup.Item1.y2, tup.Item2.x2, tup.Item2.y2))
                        .Where(tup => tup.x1 != tup.x2 && tup.y1 != tup.y2)
                        .ToArray().Shuffle();
                    foreach (var (x1, y1, x2, y2) in candidatePairs)
                    {
                        sofar[space] = -1;
                        sofar[x1 + w * y1] = 2 * ix + 1;
                        sofar[x + w * y1] = 2 * ix + 1;
                        sofar[x1 + w * y] = 2 * ix + 1;
                        sofar[x2 + w * y] = 2 * ix + 2;
                        sofar[x + w * y2] = 2 * ix + 2;
                        sofar[x2 + w * y2] = 2 * ix + 2;

                        foreach (var result in recurse(sofar, ix + 1))
                        {
                            yield return result;
                            any = true;
                        }

                        sofar[space] = 0;
                        sofar[x1 + w * y1] = 0;
                        sofar[x + w * y1] = 0;
                        sofar[x1 + w * y] = 0;
                        sofar[x2 + w * y] = 0;
                        sofar[x + w * y2] = 0;
                        sofar[x2 + w * y2] = 0;
                    }
                }
                if (!any)
                {
                    yield return (sofar.ToArray(), ix);
                    yield break;
                }
            }

            var best = 0;
            foreach (var (solution, num) in recurse(new int[w * h], 0))
            {
                if (num > best)
                {
                    best = num;
                    ConsoleUtil.WriteLine($"{num}".Color(ConsoleColor.White));
                    ConsoleUtil.WriteLine(solution.Split(w)
                        .Select(row => row
                            .Select(i => (i == -1 ? "██" : i == 0 ? "░░" : i % 2 == 0 ? ((char) ('A' + (i - 1) / 2)).ToString().PadLeft(2) : ((i + 1) / 2).ToString().PadLeft(2))
                                .Color(i == -1 ? ConsoleColor.DarkGray : ConsoleColor.White, i < 1 ? ConsoleColor.Black : (ConsoleColor) (((i + 1) / 2 - 1) % 15 + 1)))
                            .JoinColoredString())
                        .JoinColoredString("\n"));
                    Console.WriteLine();
                }
            }
        }

        public static void RectangleMangle_GeneratePuzzle()
        {
            var grid = RectangleMangle_getGrid();
            var values = @"M	VENUS	EARTHPLANET	MARS
E	SOUTH	WEST	EAST
K	CYAN	MAGENTA	KEY
J	INTAKE	COMPRESSION	EXHAUST
F	FIRE	WATER	AIR
N	JOHNLENNON	PAULMCCARTNEY	GEORGEHARRISON
H	DEATH	WAR	FAMINE
L	PETERVENKMAN	EGONSPENGLER	WINSTONZEDDEMORE
C	MICHELANGELO	RAPHAEL	LEONARDO
I	TINKYWINKY	DIPSY	PO
A	SUBTRACTION	MULTIPLICATION	DIVISION
G	WINTER	SPRING	AUTUMN
D	MARK	LUKE	JOHN
B	COLORADO	UTAH	NEWMEXICO
13	JUPITER	SATURN	URANUS
5	GEORGE	ELAINE	KRAMER
11	PLUM	ORCHID	CHRYSANTHEMUM
10	INKY	BLINKY	CLYDE
6	ENGLAND	SCOTLAND	NORTHERNIRELAND
14	HARRYPOTTER	FLEURDELACOUR	CEDRICDIGGORY
8	ADENINE	THYMINE	GUANINE
12	KYLEBROFLOVSKI	KENNYMCCORMICK	ERICCARTMAN
3	GRYFFINDOR	HUFFLEPUFF	SLYTHERIN
9	SOLID	GAS	PLASMA
1	MRFANTASTIC	THEINVISIBLEWOMAN	THEHUMANTORCH
7	GRAVITY	ELECTROMAGNETISM	WEAK
4	PRUDENCE	FORTITUDE	TEMPERANCE
2	BABARACUS	HANNIBAL	FACEMAN".Replace("\r", "").Split('\n').Select(row => row.Split('\t')).Select(arr => (valStr: arr[0], names: arr.Skip(1).ToArray())).ToArray();
            var counts = new Dictionary<string, int>();
            const int targetWidth = 105;
            const int targetHeight = 105;
            File.WriteAllText(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Rectangle Mangle\Rectangle Mangle.html", $@"<!DOCTYPE html>
<html>
    <head>
        <style>
            .image {{
                width: {targetWidth}px;
            }}
        </style>
    </head>
    <body>
        <table class='puzzle'>
            {Enumerable.Range(0, 10).Select(row => $@"<tr>{Enumerable.Range(0, 10).Select(col =>
            {
                if (grid[col + 10 * row] == null)
                    return "<td></td>";
                var (valStr, names) = values.First(vd => vd.valStr == grid[col + 10 * row].Value.valStr);
                var count = counts.Get(valStr, 0);
                var name = names[count];
                var filename = "png,jpg,jpeg,bmp".Split(',').Select(ext => $@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Rectangle Mangle\{name}.{ext}").First(f => File.Exists(f));
                counts.IncSafe(valStr);
                using (var bmp = new Bitmap(filename))
                using (var mem = new MemoryStream())
                {
                    GraphicsUtil.DrawBitmap(targetWidth, targetHeight, g =>
                    {
                        g.DrawImage(bmp, GraphicsUtil.FitIntoMaintainAspectRatio(bmp.Size, new Rectangle(0, 0, targetWidth, targetHeight)));
                    }).Save(mem, ImageFormat.Png);
                    return $@"<td><img src='data:image/png;base64,{Convert.ToBase64String(mem.ToArray())}' class='image' /></td>";
                }
            }).JoinString()}</tr>").JoinString()}
        </table>
    </body>
</html>
");
        }
    }
}
