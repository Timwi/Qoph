using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RT.Dijkstra;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace PuzzleStuff
{
    class GalacticPuzzleHunt2019
    {
        public static void Colors()
        {
            var colorNames = new Dictionary<string, string>
            {
                { "AliceBlue", "#f0f8ff" },
                { "AntiqueWhite", "#faebd7" },
                { "Aqua", "#0ff" },
                { "Aquamarine", "#7fffd4" },
                { "Azure", "#f0ffff" },
                { "Beige", "#f5f5dc" },
                { "Bisque", "#ffe4c4" },
                { "Black", "#000" },
                { "BlanchedAlmond", "#ffebcd" },
                { "Blue", "#00f" },
                { "BlueViolet", "#8a2be2" },
                { "Brown", "#a52a2a" },
                { "BurlyWood", "#deb887" },
                { "CadetBlue", "#5f9ea0" },
                { "Chartreuse", "#7fff00" },
                { "Chocolate", "#d2691e" },
                { "Coral", "#ff7f50" },
                { "CornflowerBlue", "#6495ed" },
                { "Cornsilk", "#fff8dc" },
                { "Crimson", "#dc143c" },
                { "Cyan", "#0ff" },
                { "DarkBlue", "#00008b" },
                { "DarkCyan", "#008b8b" },
                { "DarkGoldenrod", "#b8860b" },
                { "DarkGray", "#a9a9a9" },
                { "DarkGreen", "#006400" },
                { "DarkKhaki", "#bdb76b" },
                { "DarkMagenta", "#8b008b" },
                { "DarkOliveGreen", "#556b2f" },
                { "DarkOrange", "#ff8c00" },
                { "DarkOrchid", "#9932cc" },
                { "DarkRed", "#8b0000" },
                { "DarkSalmon", "#e9967a" },
                { "DarkSeaGreen", "#8fbc8f" },
                { "DarkSlateBlue", "#483d8b" },
                { "DarkSlateGray", "#2f4f4f" },
                { "DarkTurquoise", "#00ced1" },
                { "DarkViolet", "#9400d3" },
                { "DeepPink", "#ff1493" },
                { "DeepSkyBlue", "#00bfff" },
                { "DimGray", "#696969" },
                { "DodgerBlue", "#1e90ff" },
                { "FireBrick", "#b22222" },
                { "FloralWhite", "#fffaf0" },
                { "ForestGreen", "#228b22" },
                { "Fuchsia", "#f0f" },
                { "Gainsboro", "#dcdcdc" },
                { "GhostWhite", "#f8f8ff" },
                { "Gold", "#ffd700" },
                { "Goldenrod", "#daa520" },
                { "Gray", "#808080" },
                { "Green", "#008000" },
                { "GreenYellow", "#adff2f" },
                { "Honeydew", "#f0fff0" },
                { "HotPink", "#ff69b4" },
                { "IndianRed", "#cd5c5c" },
                { "Indigo", "#4b0082" },
                { "Ivory", "#fffff0" },
                { "Khaki", "#f0e68c" },
                { "Lavender", "#e6e6fa" },
                { "LavenderBlush", "#fff0f5" },
                { "LawnGreen", "#7cfc00" },
                { "LemonChiffon", "#fffacd" },
                { "LightBlue", "#add8e6" },
                { "LightCoral", "#f08080" },
                { "LightCyan", "#e0ffff" },
                { "LightGoldenrodYellow", "#fafad2" },
                { "LightGreen", "#90ee90" },
                { "LightGrey", "#d3d3d3" },
                { "LightPink", "#ffb6c1" },
                { "LightSalmon", "#ffa07a" },
                { "LightSeaGreen", "#20b2aa" },
                { "LightSkyBlue", "#87cefa" },
                { "LightSlateGray", "#789" },
                { "LightSteelBlue", "#b0c4de" },
                { "LightYellow", "#ffffe0" },
                { "Lime", "#0f0" },
                { "LimeGreen", "#32cd32" },
                { "Linen", "#faf0e6" },
                { "Magenta", "#f0f" },
                { "Maroon", "#800000" },
                { "MediumAquamarine", "#66cdaa" },
                { "MediumBlue", "#0000cd" },
                { "MediumOrchid", "#ba55d3" },
                { "MediumPurple", "#9370db" },
                { "MediumSeaGreen", "#3cb371" },
                { "MediumSlateBlue", "#7b68ee" },
                { "MediumSpringGreen", "#00fa9a" },
                { "MediumTurquoise", "#48d1cc" },
                { "MediumVioletRed", "#c71585" },
                { "MidnightBlue", "#191970" },
                { "MintCream", "#f5fffa" },
                { "MistyRose", "#ffe4e1" },
                { "Moccasin", "#ffe4b5" },
                { "NavajoWhite", "#ffdead" },
                { "Navy", "#000080" },
                { "OldLace", "#fdf5e6" },
                { "Olive", "#808000" },
                { "OliveDrab", "#6b8e23" },
                { "Orange", "#ffa500" },
                { "OrangeRed", "#ff4500" },
                { "Orchid", "#da70d6" },
                { "PaleGoldenrod", "#eee8aa" },
                { "PaleGreen", "#98fb98" },
                { "PaleTurquoise", "#afeeee" },
                { "PaleVioletRed", "#db7093" },
                { "PapayaWhip", "#ffefd5" },
                { "PeachPuff", "#ffdab9" },
                { "Peru", "#cd853f" },
                { "Pink", "#ffc0cb" },
                { "Plum", "#dda0dd" },
                { "PowderBlue", "#b0e0e6" },
                { "Purple", "#800080" },
                { "Red", "#f00" },
                { "RosyBrown", "#bc8f8f" },
                { "RoyalBlue", "#4169e1" },
                { "SaddleBrown", "#8b4513" },
                { "Salmon", "#fa8072" },
                { "SandyBrown", "#f4a460" },
                { "SeaGreen", "#2e8b57" },
                { "Seashell", "#fff5ee" },
                { "Sienna", "#a0522d" },
                { "Silver", "#c0c0c0" },
                { "SkyBlue", "#87ceeb" },
                { "SlateBlue", "#6a5acd" },
                { "SlateGray", "#708090" },
                { "Snow", "#fffafa" },
                { "SpringGreen", "#00ff7f" },
                { "SteelBlue", "#4682b4" },
                { "Tan", "#d2b48c" },
                { "Teal", "#008080" },
                { "Thistle", "#d8bfd8" },
                { "Tomato", "#ff6347" },
                { "Turquoise", "#40e0d0" },
                { "Violet", "#ee82ee" },
                { "Wheat", "#f5deb3" },
                { "White", "#fff" },
                { "WhiteSmoke", "#f5f5f5" },
                { "Yellow", "#ff0" },
                { "YellowGreen", "#9acd32" },
            };
            foreach (var kvp in colorNames.ToArray())
                if (kvp.Value.Length == 4)
                    colorNames[kvp.Key] = $"#{kvp.Value[1]}{kvp.Value[1]}{kvp.Value[2]}{kvp.Value[2]}{kvp.Value[3]}{kvp.Value[3]}";

            var strings = Ut.NewArray(6, _ => "");
            for (int i = 0; i < 7; i++)
            {
                using (var bmp = new Bitmap($@"D:\temp\galactic\Colors\{i}.png"))
                    for (int j = 0; j < 6; j++)
                    {
                        var color = bmp.GetPixel(bmp.Width / 2, bmp.Height / 12 * (2 * j + 1));
                        strings[j] += ($"{i}\t{j}\t\"'{color.R:X2}\"\t\"'{color.G:X2}\"\t\"'{color.B:X2}\"\t{colorNames.FirstOrDefault(kvp => kvp.Value == $"#{color.R:X2}{color.G:X2}{color.B:X2}".ToLowerInvariant()).Key}\t\t");
                        //strings[j] += ($"{i}\t{j}\t\"'{Convert.ToString(color.R, 2).PadLeft(8, '0').Replace("1", "█").Replace("0", "░")}\"\t\"'{Convert.ToString(color.G, 2).PadLeft(8, '0').Replace("1", "█").Replace("0", "░")}\"\t\"'{Convert.ToString(color.B, 2).PadLeft(8, '0').Replace("1", "█").Replace("0", "░")}\"\t\t");
                    }
            }
            Clipboard.SetText(strings.JoinString("\n"));
        }

        public static void RaceForTheGalaxy_Grid()
        {
            var words = File.ReadAllLines(@"D:\Daten\sowpods.txt").Where(w => w.Length == 6).ToHashSet();
            words.Add("RASIST");    // apparently that’s a word according to them
            var input = Clipboard.GetText().Split(new[] { '\t', '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries).JoinString().Split(3).ToArray();
            if (input.Length != 36 || input.Any(str => str.Length != 3 || str.Any(c => c < 'A' || c > 'Z')))
                Debugger.Break();

            IEnumerable<string> recurse(string[] inp, int ix, bool col)
            {
                if (ix == 6)
                    return !col ? recurse(inp, 0, true) : (new[] { inp.JoinString() });

                var fragments = col ? Enumerable.Range(0, 6).Select(i => inp[ix + 6 * i]).ToArray() : Enumerable.Range(0, 6).Select(i => inp[i + 6 * ix]).ToArray();
                var regex = new Regex($"^{fragments.Select(frg => $"[{frg}]").JoinString()}$");
                return words.Where(w => regex.IsMatch(w)).SelectMany(word =>
                {
                    var newInp = inp.Select((str, i) => (col ? (i % 6) : (i / 6)) == ix ? str.Remove(str.IndexOf(word[col ? (i / 6) : (i % 6)]), 1) : str).ToArray();
                    return recurse(newInp, ix + 1, col);
                });
            }

            foreach (var result in recurse(input, 0, false))
                Console.WriteLine(result);
        }

        public static void RaceForTheGalaxy_WordSearch()
        {
            //var origGrid = @"TEZZKIXZMNZZEZZBQFZKIACTUEWOTOUYJJMKTUZTXZBFMZDZFBXKMEAINEMUIAFKOYPAMMOMKEDBXBOXOANIFOPSZSPWFHMXXUUKHSRPEMLBMXBBRFMPZNUABAYAXKKHJFMXRDOUAARAAKAX";

            var origGrid = Clipboard.GetText().Split(new[] { '\t', ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).JoinString();
            var words = @"AFFLUENT;BOUNCER;COLORFULLY;COMPLEX;FIASCO;HORSEMANSHIP;LAUREATE;MEASLY;PAYDAY;PLAYACT;SERVICE;SULTANATE;VIETNAMESE;WHIPLASH".Split(';').OrderByDescending(w => w.Length).ToArray();

            IEnumerable<string> recurse((char ch, char orig, bool tr, bool mark)[] grid, int ix)
            {
                if (ix == words.Length)
                {
                    var trnsl = new Dictionary<char, char>();
                    foreach (var (ch, orig, tr, mark) in grid)
                        if (tr)
                            trnsl[orig] = ch;
                    Console.WriteLine(trnsl.OrderBy(k => k.Key).Select(k => $"{k.Key}→{k.Value}").JoinString(", "));
                    Console.WriteLine($"Undiscovered: {Enumerable.Range(0, 26).Select(i => (char) (i + 'A')).Where(c => !trnsl.ContainsKey(c)).JoinString()} → {Enumerable.Range(0, 26).Select(i => (char) (i + 'A')).Where(c => !trnsl.Values.Contains(c)).JoinString()}");
                    Console.WriteLine(grid.Where(c => !c.mark).Select(c => c.orig).JoinString());
                    Console.WriteLine(grid.Where(c => !c.mark).Select(c => c.tr ? c.ch : '?').JoinString());
                    yield return "";
                    yield break;
                }

                for (var x = 0; x < 12; x++)
                    for (var y = 0; y < 12; y++)
                        foreach (var w in new[] { words[ix], words[ix].Reverse().JoinString() })
                        {
                            // Horizontal
                            if (x + w.Length <= 12)
                            {
                                for (int i = 0; i < w.Length; i++)
                                {
                                    if (grid[x + i + 12 * y].tr)
                                    {
                                        if (grid[x + i + 12 * y].ch != w[i])
                                            goto bustedH;
                                    }
                                    else
                                    {
                                        for (int j = i + 1; j < w.Length; j++)
                                            if ((!grid[x + j + 12 * y].tr && grid[x + i + 12 * y].ch == grid[x + j + 12 * y].ch) != (w[i] == w[j]))
                                                goto bustedH;
                                    }
                                }

                                var trl = new Dictionary<char, char>();
                                for (int i = 0; i < w.Length; i++)
                                    if (!grid[x + i + 12 * y].tr)
                                        trl[grid[x + i + 12 * y].ch] = w[i];
                                var newGrid = grid.Select(c => c.tr || !trl.ContainsKey(c.ch) ? c : (ch: trl[c.ch], c.orig, tr: true, c.mark)).ToArray();
                                for (int i = 0; i < w.Length; i++)
                                    newGrid[x + i + 12 * y] = (newGrid[x + i + 12 * y].ch, newGrid[x + i + 12 * y].orig, true, true);

                                foreach (var res in recurse(newGrid, ix + 1))
                                    yield return res;
                            }
                            bustedH:;

                            // Vertical
                            if (y + w.Length <= 12)
                            {
                                for (int i = 0; i < w.Length; i++)
                                {
                                    if (grid[x + 12 * (y + i)].tr)
                                    {
                                        if (grid[x + 12 * (y + i)].ch != w[i])
                                            goto bustedV;
                                    }
                                    else
                                    {
                                        for (int j = i + 1; j < w.Length; j++)
                                            if ((!grid[x + 12 * (y + j)].tr && grid[x + 12 * (y + i)].ch == grid[x + 12 * (y + j)].ch) != (w[i] == w[j]))
                                                goto bustedV;
                                    }
                                }

                                var trl = new Dictionary<char, char>();
                                for (int i = 0; i < w.Length; i++)
                                    if (!grid[x + 12 * (y + i)].tr)
                                        trl[grid[x + 12 * (y + i)].ch] = w[i];
                                var newGrid = grid.Select(c => c.tr || !trl.ContainsKey(c.ch) ? c : (ch: trl[c.ch], c.orig, tr: true, c.mark)).ToArray();
                                for (int i = 0; i < w.Length; i++)
                                    newGrid[x + 12 * (y + i)] = (newGrid[x + 12 * (y + i)].ch, newGrid[x + 12 * (y + i)].orig, true, true);

                                foreach (var res in recurse(newGrid, ix + 1))
                                    yield return res;
                            }
                            bustedV:;

                            // Forward diagonal ( \ )
                            if (x + w.Length <= 12 && y + w.Length <= 12)
                            {
                                for (int i = 0; i < w.Length; i++)
                                {
                                    if (grid[x + i + 12 * (y + i)].tr)
                                    {
                                        if (grid[x + i + 12 * (y + i)].ch != w[i])
                                            goto bustedFwD;
                                    }
                                    else
                                    {
                                        for (int j = i + 1; j < w.Length; j++)
                                            if ((!grid[x + j + 12 * (y + j)].tr && grid[x + i + 12 * (y + i)].ch == grid[x + j + 12 * (y + j)].ch) != (w[i] == w[j]))
                                                goto bustedFwD;
                                    }
                                }

                                var trl = new Dictionary<char, char>();
                                for (int i = 0; i < w.Length; i++)
                                    if (!grid[x + i + 12 * (y + i)].tr)
                                        trl[grid[x + i + 12 * (y + i)].ch] = w[i];
                                var newGrid = grid.Select(c => c.tr || !trl.ContainsKey(c.ch) ? c : (ch: trl[c.ch], c.orig, tr: true, c.mark)).ToArray();
                                for (int i = 0; i < w.Length; i++)
                                    newGrid[x + i + 12 * (y + i)] = (newGrid[x + i + 12 * (y + i)].ch, newGrid[x + i + 12 * (y + i)].orig, true, true);

                                foreach (var res in recurse(newGrid, ix + 1))
                                    yield return res;
                            }
                            bustedFwD:;

                            // Backward diagonal ( / )
                            if (x + w.Length <= 12 && y + w.Length <= 12)
                            {
                                for (int i = 0; i < w.Length; i++)
                                {
                                    if (grid[x + i + 12 * (y + w.Length - 1 - i)].tr)
                                    {
                                        if (grid[x + i + 12 * (y + w.Length - 1 - i)].ch != w[i])
                                            goto bustedBkD;
                                    }
                                    else
                                    {
                                        for (int j = i + 1; j < w.Length; j++)
                                            if ((!grid[x + j + 12 * (y + w.Length - 1 - j)].tr && grid[x + i + 12 * (y + w.Length - 1 - i)].ch == grid[x + j + 12 * (y + w.Length - 1 - j)].ch) != (w[i] == w[j]))
                                                goto bustedBkD;
                                    }
                                }

                                var trl = new Dictionary<char, char>();
                                for (int i = 0; i < w.Length; i++)
                                    if (!grid[x + i + 12 * (y + w.Length - i)].tr)
                                        trl[grid[x + i + 12 * (y + w.Length - i)].ch] = w[i];
                                var newGrid = grid.Select(c => c.tr || !trl.ContainsKey(c.ch) ? c : (ch: trl[c.ch], c.orig, tr: true, c.mark)).ToArray();
                                for (int i = 0; i < w.Length; i++)
                                    newGrid[x + i + 12 * (y + w.Length - 1 - i)] = (newGrid[x + i + 12 * (y + w.Length - 1 - i)].ch, newGrid[x + i + 12 * (y + w.Length - 1 - i)].orig, true, true);

                                foreach (var res in recurse(newGrid, ix + 1))
                                    yield return res;
                            }
                            bustedBkD:;
                        }
            }

            foreach (var solution in recurse(origGrid.Select(ch => (ch, ch, false, false)).ToArray(), 0))
                Console.WriteLine(solution);
            Console.WriteLine();
        }

        public static void RaceForTheGalaxy_Logic()
        {
            while (true)
            {
                var line = Console.ReadLine();
                if (line == "")
                    return;

                var pieces = line.Split(';').Select(piece => Regex.Match(piece, @"^(?<t>[tf])|(?<n>![a-e])|(?<op>[a-e][\|\&\@\#\^][a-e])|[a-e]$")).ToArray();
                if (pieces.Any(m => !m.Success))
                {
                    Console.WriteLine($"Item #{pieces.IndexOf(m => !m.Success)} invalid syntax.");
                    continue;
                }

                var equations = pieces.Select(m => m.Value).ToArray();
                for (int comb = 0; comb < 32; comb++)
                {
                    var values = Enumerable.Range(0, 5).Select(i => (comb & (1 << i)) != 0).ToArray();
                    // verify all statements
                    for (int i = 0; i < equations.Length; i++)
                    {
                        if (equations[i] == "t")
                        {
                            if (!values[i])
                                goto busted;
                            else
                                continue;
                        }

                        if (equations[i] == "f")
                        {
                            if (values[i])
                                goto busted;
                            else
                                continue;
                        }

                        if (equations[i].Length == 1)
                        {
                            if (values[equations[i][0] - 'a'] != values[i])
                                goto busted;
                            else
                                continue;
                        }

                        if (equations[i].StartsWith("!"))
                        {
                            if (values[equations[i][1] - 'a'] == values[i])
                                goto busted;
                            else
                                continue;
                        }

                        var op1 = values[equations[i][0] - 'a'];
                        var op2 = values[equations[i][2] - 'a'];
                        switch (equations[i][1])
                        {
                            case '|': if ((op1 | op2) != values[i]) goto busted; break;
                            case '&': if ((op1 & op2) != values[i]) goto busted; break;
                            case '@': if (!(op1 & op2) != values[i]) goto busted; break;
                            case '#': if (!(op1 | op2) != values[i]) goto busted; break;
                            case '^': if ((op1 ^ op2) != values[i]) goto busted; break;
                            default: goto busted;
                        }
                    }
                    Console.WriteLine($"Solution: {values.Select(v => v ? "1" : "0").JoinString()} ({comb}) = {(char) ('A' + values.Aggregate(0, (p, n) => (p << 1) | (n ? 1 : 0)) - 1)}");

                    busted:;
                }
            }
        }

        public static void Cuspidation_RunSimulation()
        {
            var horizWords = new[] { "caNDy", "WAsh", "jAVelIN", "DUb", "haBIT", "frAMIng", "wARM", "qUEEN" };
            var vertWords = new[] { "EL", "TAxON", "vISoR", "WIkI", "SEvERE", "LoCK", "ABRAdE", "ReAM" };

            /* First step of the puzzle: times deduced from given constraints
            var horizTimes = new[] {
                4,// CANDY    
                13,// WASH
                0,// JAVELIN  
                19,// DUB
                1,// HABIT    
                12,// FRAMING 
                3,// WARM     
                3// QUEEN     
            };
            var vertTimes = new[] {
                17,// EL
                2,// TAXON    
                4,// VISOR    
                17,// WIKI
                12,// SEVERE   
                18,// LOCK
                11,// ABRADE
                16,// REAM
            };

            /*/ // Second step: givens added to solution
            var horizTimes = new[] { 19, 5, 13, 9, 20, 23, 5, 14 };
            var vertTimes = new[] { 15, 6, 12, 1, 21, 14, 3, 8 };
            /**/

            var trains =
                horizWords.Select((word, ix) => (word, x: -word.Length - 1, y: ix, horiz: true, timeTillStart: horizTimes[ix])).Concat(
                vertWords.Select((word, ix) => (word, x: ix, y: -word.Length - 1, horiz: false, timeTillStart: vertTimes[ix]))).ToArray();

            var sb = new StringBuilder();
            for (int time = 0; time < 51; time++)
            {
                for (int i = 0; i < trains.Length; i++)
                {
                    var (word, x, y, horiz, timeTillStart) = trains[i];
                    for (int j = 0; j < word.Length; j++)
                        foreach (var otherTrain in trains.Where(t2 => t2.horiz != horiz))
                        {
                            if (
                                // Is the other train’s arrow on the same height as this train?
                                (otherTrain.horiz ? otherTrain.x : otherTrain.y) + otherTrain.word.Length == (horiz ? y : x) &&
                                // Does the other train’s arrow hit this letter j?
                                (otherTrain.horiz ? otherTrain.y : otherTrain.x) == (horiz ? x : y) + j
                            )
                                word = word.Substring(0, j) + (char.IsUpper(word[j]) ? '!' : char.ToUpperInvariant(word[j])) + word.Substring(j + 1);
                            //else if (
                            //    // Is the other train’s arrow below this train?
                            //    (otherTrain.horiz ? otherTrain.x : otherTrain.y) + otherTrain.word.Length > (horiz ? y : x) &&
                            //    // Is the other train’s beginning above this train?
                            //    (otherTrain.horiz ? otherTrain.x : otherTrain.y) <= (horiz ? y : x) &&
                            //    // Does the other train’s arrow hit this letter j?
                            //    (otherTrain.horiz ? otherTrain.y : otherTrain.x) == (horiz ? x : y) + j &&
                            //    char.IsUpper(word[j])
                            //)
                            //    word = word.Substring(0, j) + "!" + word.Substring(j + 1);
                        }
                    trains[i] = (word, x, y, horiz, timeTillStart);
                }

                sb.Append($@"{(time == 0 ? "" : $"<div><a href='#time_{time}'>Next</a></div>")}<h1 id='time_{time}'>Time {time}</h1><svg viewBox='-9 -9 26 26' fill='none' stroke='black' font-size='.7' text-anchor='middle'>
                    {Enumerable.Range(1, 7).Select(x => $"<line x1='{x}' x2='{x}' y1='0' y2='8' stroke-width='.05' />").JoinString()}
                    {Enumerable.Range(1, 7).Select(y => $"<line y1='{y}' y2='{y}' x1='0' x2='8' stroke-width='.05' />").JoinString()}
                    <rect x='0' y='0' width='8' height='8' stroke-width='.1' />
                    {trains.Select(train =>
                {
                    var tipX = train.x + (train.horiz ? train.word.Length : 0) + .5;
                    var tipY = train.y + (train.horiz ? 0 : train.word.Length) + .5;
                    var d = .3;
                    var s = $"<path d='M{train.x + .5} {train.y + .5} L {tipX} {tipY} M {tipX} {tipY} L {tipX - d} {tipY - d} M {tipX} {tipY} L {(train.horiz ? tipX - d : tipX + d)} {(train.horiz ? tipY + d : tipY - d)}' stroke-width='.2' stroke-linecap='round' />";
                    for (int i = 0; i < train.word.Length; i++)
                    {
                        s += $"<circle cx='{train.x + (train.horiz ? i : 0) + .5}' cy='{train.y + (train.horiz ? 0 : i) + .5}' r='.4' stroke-width='.05' fill='{(train.word[i] == '!' ? "#f88" : char.IsLower(train.word[i]) ? "#8f8" : "white")}' />";
                        s += $"<text x='{train.x + (train.horiz ? i : 0) + .5}' y='{train.y + (train.horiz ? 0 : i) + .75}' fill='black' stroke='none'>{char.ToUpperInvariant(train.word[i])}</text>";
                        //if (char.IsLower(train.word[i]))
                        //    s += $"<line transform='translate({train.x + (train.horiz ? i : 0) + .5} {train.y + (train.horiz ? 0 : i) + .5}) rotate(45)' x1='0' x2='0' y1='-.4' y2='.4' stroke-width='.05' stroke='#f00' />";
                    }
                    return s;
                }).JoinString()}
                </svg><div><img src='Cuspidation timeline.png'/></div>");

                for (int i = 0; i < trains.Length; i++)
                {
                    var (word, x, y, horiz, timeTillStart) = trains[i];
                    if (timeTillStart > 0)
                        timeTillStart--;
                    else if (x < 8 && y < 8)
                    {
                        if (horiz)
                            x++;
                        else
                            y++;
                    }
                    trains[i] = (word, x, y, horiz, timeTillStart);
                }
            }
            General.ReplaceInFile(@"D:\temp\galactic\Cuspidation.html", "<!--%%-->", "<!--%%%-->", sb.ToString());
        }

        public static void SomethingIsOff_1_Gaps()
        {
            IEnumerable<bool[]> recurse(bool[] grid, int ix)
            {
                if (ix == 64)
                    return new[] { grid.ToArray() };

                var col = ix % 8;
                var row = ix / 8;

                var possible = new List<bool> { false, true };

                if (col > 0)
                {
                    if (grid[ix - 1] || (row > 0 && grid[ix - 9]))
                        possible.Remove(true);
                }
                if (row > 0 && grid[ix - 8])
                    possible.Remove(true);
                if (col < 7 && row > 0 && grid[ix - 7])
                    possible.Remove(true);

                var numInCol = Enumerable.Range(0, row).Count(r => grid[col + 8 * r]);
                if (numInCol > 2)
                    return Enumerable.Empty<bool[]>();
                if (numInCol == 2)
                    possible.Remove(true);
                //if (numInCol == 1 && col == 1)
                //    possible.Remove(row != Enumerable.Range(0, row).First(r => grid[col + 8 * r]) + 2);
                if (row >= 6 + numInCol)
                    possible.Remove(false);

                var numInRow = Enumerable.Range(0, col).Count(c => grid[c + 8 * row]);
                if (numInRow > 2)
                    return Enumerable.Empty<bool[]>();
                if (numInRow == 2)
                    possible.Remove(true);
                //if (row == 2 && numInRow == 1)
                //    possible.Remove(col != Enumerable.Range(0, col).First(c => grid[c + 8 * row]) + 2);
                if (col >= 6 + numInRow)
                    possible.Remove(false);

                return possible.SelectMany(b =>
                {
                    grid[ix] = b;
                    return recurse(grid, ix + 1);
                });
            }

            var words = File.ReadAllLines(@"D:\Daten\sowpods.txt").ToHashSet();
            var theGrid = new bool[64];
            var letters = new string(' ', 16) + "TSHAL    OUCLM    PEDRI    ELNER" + new string(' ', 16);
            var solutions = new HashSet<string>();
            foreach (var solution in recurse(theGrid, 0))
            {
                Console.WriteLine(solution.Select(b => b ? "█" : "░").Split(8).Select(chunk => chunk.JoinString("")).JoinString("\n"));
                var word = solution.Select((b, ix) => b ? letters[ix].ToString().Replace(" ", "") : "").JoinString();
                Console.WriteLine(word);
                Console.WriteLine();
            }
        }

        public static void SomethingIsOff_2_Heteromino()
        {
            var shapes = Ut.NewArray(
                new (int x, int y)[] { (0, 0), (0, 1), (1, 1) },
                new (int x, int y)[] { (0, 0), (0, 1), (-1, 1) },
                new (int x, int y)[] { (0, 0), (1, 0), (1, 1) },
                new (int x, int y)[] { (0, 0), (1, 0), (0, 1) },
                new (int x, int y)[] { (0, 0), (1, 0), (2, 0) },
                new (int x, int y)[] { (0, 0), (0, 1), (0, 2) });
            var blocks = new[] { 2, 4, 15, 17, 19, 31, 45 };
            var letters = new string(' ', 28) + "SN   HTAI   EOCN   KT";

            void output(int[] sol)
            {
                ConsoleUtil.WriteLine(sol.Select((s, ix) => s.ToString().PadLeft(3).Color(ConsoleColor.White, blocks.Contains(ix) ? ConsoleColor.DarkGray : (ConsoleColor) (s + 1))).Split(7).Select(chunk => chunk.JoinColoredString(" ")).JoinColoredString("\n"));
                ConsoleUtil.WriteLine(Enumerable.Range(0, 49)
                    .Where(i => new[] { i % 7 > 0 && sol[i - 1] == sol[i], i / 7 > 0 && sol[i - 7] == sol[i], i % 7 < 6 && sol[i + 1] == sol[i], i / 7 < 6 && sol[i + 7] == sol[i] }.Count(b => b) >= 2)
                    .Select(i => letters[i])
                    .JoinString()
                    .Replace(" ", ""));
            }

            IEnumerable<int[]> recurse(int[] grid, int ix, int id)
            {
                while (ix < 49 && (blocks.Contains(ix) || grid[ix] >= 0))
                    ix++;

                if (ix == 49)
                {
                    yield return grid.ToArray();
                    yield break;
                }

                var c = ix % 7;
                var r = ix / 7;
                foreach (var sh in shapes)
                {
                    if (!sh.All(t => c + t.x >= 0 && c + t.x < 7 && r + t.y >= 0 && r + t.y < 7 && grid[c + t.x + 7 * (r + t.y)] == -1 && !blocks.Contains(c + t.x + 7 * (r + t.y))))
                        continue;

                    // place a suitable triomino
                    var adj = new HashSet<int>();
                    foreach (var (x, y) in sh)
                    {
                        grid[c + x + 7 * (r + y)] = id;
                        if (c + x > 0)
                            adj.Add(grid[c + x - 1 + 7 * (r + y)]);
                        if (c + x < 6)
                            adj.Add(grid[c + x + 1 + 7 * (r + y)]);
                        if (r + y > 0)
                            adj.Add(grid[c + x + 7 * (r + y - 1)]);
                        if (r + y < 6)
                            adj.Add(grid[c + x + 7 * (r + y + 1)]);
                    }

                    string getShapeStr(int shapeId) => grid.Select(b => b == shapeId ? '#' : ' ').JoinString().Trim();

                    var thisShape = getShapeStr(id);

                    //Console.WriteLine("Just placed:");
                    //output(grid);
                    if (adj.Any(a => a != -1 && a != id && getShapeStr(a) == thisShape))
                    {
                        //Console.WriteLine("Invalid!");
                        //Console.ReadLine();
                        goto cleanup;
                    }
                    //Console.ReadLine();

                    foreach (var res in recurse(grid, ix + 1, id + 1))
                        yield return res;

                    cleanup:
                    foreach (var (x, y) in sh)
                        grid[c + x + 7 * (r + y)] = -1;
                }
            }

            foreach (var solution in recurse(Ut.NewArray(49, _ => -1), 0, 0))
                output(solution);
        }

        public static void SomethingIsOff_4_YinYang()
        {
            var givens = @"#SHT#OARIBC@GNUE#APREBF#S#W@RNA#OR#IC@UTLI#@@L@E@ERETA#YTNEVEI@WR#A#OGCR#U#IKERHGDN#BS#E@MUMOSI#NRT#";
            var anyBlack = givens.IndexOf('#');
            var anyWhite = givens.IndexOf('@');

            IEnumerable<bool[]> recurse(bool?[] grid)
            {
                // Find patterns
                bool anyFound;
                (int x, int y) candidate = (-1, -1);
                do
                {
                    anyFound = false;
                    for (int x = 0; x < 10; x++)
                        for (int y = 0; y < 10; y++)
                        {
                            if (candidate.x == -1 && grid[x + 10 * y] == null)
                                candidate = (x, y);
                            if (x == 9 || y == 9)
                                continue;

                            if (grid[x + 10 * y] != null && grid[x + 10 * y] == grid[x + 1 + 10 * (y + 1)])
                            {
                                if (grid[x + 10 * y] == grid[x + 1 + 10 * y] && grid[x + 10 * y] == grid[x + 10 * (y + 1)])
                                    // Found a 2×2 square, which is not allowed
                                    yield break;
                                if (grid[x + 1 + 10 * y] == null)
                                {
                                    if (grid[x + 10 * (y + 1)] != null)
                                    {
                                        grid[x + 1 + 10 * y] = !grid[x + 10 * (y + 1)].Value;
                                        anyFound = true;
                                    }
                                    else
                                        candidate = (x + 1, y);
                                }
                                else if (grid[x + 10 * (y + 1)] == null)
                                {
                                    if (grid[x + 1 + 10 * y] != null)
                                    {
                                        grid[x + 10 * (y + 1)] = !grid[x + 1 + 10 * y].Value;
                                        anyFound = true;
                                    }
                                    else
                                        candidate = (x + 1, y);
                                }
                            }
                            else if (grid[x + 1 + 10 * y] != null && grid[x + 1 + 10 * y] == grid[x + 10 * (y + 1)])
                            {
                                if (grid[x + 10 * y] == null)
                                {
                                    if (grid[x + 1 + 10 * (y + 1)] != null)
                                    {
                                        grid[x + 10 * y] = !grid[x + 1 + 10 * (y + 1)].Value;
                                        anyFound = true;
                                    }
                                    else
                                        candidate = (x, y);
                                }
                                else if (grid[x + 1 + 10 * (y + 1)] == null)
                                {
                                    if (grid[x + 10 * y] != null)
                                    {
                                        grid[x + 1 + 10 * (y + 1)] = !grid[x + 10 * y].Value;
                                        anyFound = true;
                                    }
                                    else
                                        candidate = (x, y);
                                }
                            }
                        }
                }
                while (anyFound);

                // Check black and white are both contiguous
                var numContiguous = new int[2];

                foreach (var isBlack in new[] { false, true })
                {
                    var visited = new HashSet<int>();
                    var q = new Queue<int>();
                    q.Enqueue(isBlack ? anyBlack : anyWhite);
                    while (q.Count > 0)
                    {
                        var elem = q.Dequeue();
                        if (!visited.Add(elem))
                            continue;
                        if (elem % 10 > 0 && grid[elem - 1] != !isBlack)
                            q.Enqueue(elem - 1);
                        if (elem % 10 < 9 && grid[elem + 1] != !isBlack)
                            q.Enqueue(elem + 1);
                        if (elem / 10 > 0 && grid[elem - 10] != !isBlack)
                            q.Enqueue(elem - 10);
                        if (elem / 10 < 9 && grid[elem + 10] != !isBlack)
                            q.Enqueue(elem + 10);
                    }

                    if (visited.Count != grid.Count(g => g != !isBlack))
                        yield break;
                }

                if (grid.All(b => b != null))
                {
                    yield return grid.Select(g => g.Value).ToArray();
                    yield break;
                }

                if (candidate.x == -1)
                    Debugger.Break();

                grid[candidate.x + 10 * candidate.y] = true;    // black
                foreach (var res in recurse((bool?[]) grid.Clone()))
                    yield return res;
                grid[candidate.x + 10 * candidate.y] = false;    // white
                foreach (var res in recurse((bool?[]) grid.Clone()))
                    yield return res;
            }

            foreach (var solution in recurse(givens.Select(ch => ch == '#' ? true : ch == '@' ? false : (bool?) null).ToArray()))
            {
                ConsoleUtil.WriteLine(solution.Split(10).Select(row => row.Select(b => " ● ".Color(b ? ConsoleColor.Black : ConsoleColor.White, b ? ConsoleColor.Gray : ConsoleColor.Black)).JoinColoredString()).JoinColoredString("\n"));
                Console.WriteLine(solution.Select((b, ix) =>
                {
                    if (!b)
                        return "";
                    var neigh = 0;
                    if (ix % 10 > 0 && solution[ix - 1])
                        neigh++;
                    if (ix % 10 < 9 && solution[ix + 1])
                        neigh++;
                    if (ix / 10 > 0 && solution[ix - 10])
                        neigh++;
                    if (ix / 10 < 9 && solution[ix + 10])
                        neigh++;
                    return neigh == 1 ? givens[ix].ToString() : "";
                }).JoinString().Replace("#", ""));
                Console.WriteLine();
            }
        }

        public static void SomethingIsOff_6_Kurodoko()
        {
            var numbers = new (int x, int y, int value)[] { (0, 0, 2), (4, 0, 8), (6, 1, 7), (2, 2, 4), (3, 3, 7), (1, 4, 3), (4, 4, 8), (7, 4, 2), (6, 6, 4), (2, 7, 4), (4, 8, 7), (8, 8, 4) };
            var letters = "{0}OWC HARDE{1}NATESPIRT{0}".Fmt(new string(' ', 27), new string(' ', 9));
            var directions = new (int dx, int dy)[] { (-1, 0), (1, 0), (0, -1), (0, 1) };

            IEnumerable<bool[]> recurse(bool?[] grid, int ix)
            {
                if (ix == numbers.Length)
                {
                    yield return grid.Select(g => g ?? false).ToArray();
                    yield break;
                }

                var c = numbers[ix].x;
                var r = numbers[ix].y;
                var maxDistances = new int[directions.Length];
                for (int i = 0; i < directions.Length; i++)
                    for (int x = c + directions[i].dx, y = r + directions[i].dy; x >= 0 && y >= 0 && x < 9 && y < 9 && grid[x + 9 * y] != true; x += directions[i].dx, y += directions[i].dy)
                        maxDistances[i]++;
                var totalMax = 1 + maxDistances.Sum();

                if (totalMax < numbers[ix].value)
                    yield break;

                if (totalMax == numbers[ix].value)
                {
                    // Shortcut
                    var gridCopy = (bool?[]) grid.Clone();
                    for (int i = 0; i < directions.Length; i++)
                        for (int x = c + directions[i].dx, y = r + directions[i].dy; x >= 0 && y >= 0 && x < 9 && y < 9 && grid[x + 9 * y] != true; x += directions[i].dx, y += directions[i].dy)
                            gridCopy[x + 9 * y] = false;
                    foreach (var res in recurse(gridCopy, ix + 1))
                        yield return res;
                    yield break;
                }

                var distances = new int[maxDistances.Length];
                for (int i = 0; i < maxDistances.Length; i++)
                    maxDistances[i] = Math.Min(maxDistances[i], numbers[ix].value);
                while (true)
                {
                    int j = 0;
                    while (j < distances.Length && (maxDistances[j] == 0 || distances[j] == maxDistances[j]))
                        distances[j++] = 0;
                    if (j == distances.Length)
                        break;
                    distances[j]++;
                    if (1 + distances.Sum() != numbers[ix].value)
                        continue;

                    var gridCopy = (bool?[]) grid.Clone();
                    for (int i = 0; i < directions.Length; i++)
                    {
                        for (int k = 0; k < distances[i]; k++)
                            gridCopy[c + directions[i].dx * (k + 1) + 9 * (r + directions[i].dy * (k + 1))] = false;
                        var cc = c + directions[i].dx * (distances[i] + 1);
                        var rr = r + directions[i].dy * (distances[i] + 1);
                        if (cc >= 0 && cc < 9 && rr >= 0 && rr < 9)
                        {
                            if (gridCopy[cc + 9 * rr] == false || directions.Any(d => cc + d.dx >= 0 && cc + d.dx < 9 && rr + d.dy >= 0 && rr + d.dy < 9 && gridCopy[cc + d.dx + 9 * (rr + d.dy)] == true))
                                goto busted;
                            gridCopy[cc + 9 * rr] = true;
                        }
                    }
                    foreach (var res in recurse(gridCopy, ix + 1))
                        yield return res;
                    busted:;
                }
            }

            foreach (var result in recurse(Ut.NewArray(81, i => numbers.Any(num => num.x == i % 9 && num.y == i / 9) ? false : (bool?) null), 0))
            {
                // “false” cells must form a contiguous area
                var visited = new HashSet<(int x, int y)>();
                var q = new Queue<(int x, int y)>();
                q.Enqueue((numbers[0].x, numbers[0].y));
                while (q.Count > 0)
                {
                    var elem = q.Dequeue();
                    if (!visited.Add(elem))
                        continue;
                    if (elem.x > 0 && result[elem.x - 1 + 9 * elem.y] == false)
                        q.Enqueue((elem.x - 1, elem.y));
                    if (elem.x < 8 && result[elem.x + 1 + 9 * elem.y] == false)
                        q.Enqueue((elem.x + 1, elem.y));
                    if (elem.y > 0 && result[elem.x + 9 * (elem.y - 1)] == false)
                        q.Enqueue((elem.x, elem.y - 1));
                    if (elem.y < 8 && result[elem.x + 9 * (elem.y + 1)] == false)
                        q.Enqueue((elem.x, elem.y + 1));
                }
                if (visited.Count != result.Count(b => !b))
                    continue;

                ConsoleUtil.WriteLine(Enumerable.Range(0, 9).Select(row => Enumerable.Range(0, 9).Select(col => (numbers.IndexOf(n => n.x == col && n.y == row).Apply(ix => ix == -1 ? "•" : numbers[ix].value.ToString())).Color(ConsoleColor.White, result[col + 9 * row] ? ConsoleColor.DarkGreen : ConsoleColor.Black)).JoinColoredString()).JoinColoredString("\n"));
                Console.WriteLine(Enumerable.Range(0, 9).Select(row => Enumerable.Range(0, 9).Select(col => result[col + 9 * row] ? letters[col + 9 * row] : ' ').JoinString()).JoinString().Replace(" ", ""));
                Console.WriteLine();
            }
        }

        public static void SomethingIsOff_7_Scrabble()
        {
            var letters = "PASERTNEUSHWHGNLOMTERVIEXUECNARWPSOT";
            var words = "aadB,abac,acbc,dcCc,adbcb,bdAca,cabCd,Ccbad,ababca,cbbaca,ccbabc,cdabaa".Split(',').Reverse().Select(word => word.Select(ch => (letter: char.ToLowerInvariant(ch), isSpecial: char.IsUpper(ch))).ToArray()).ToArray();

            IEnumerable<(char? letter, bool isSpecial)[]> recurse((char? letter, bool isSpecial)[] grid, int ix)
            {
                if (ix == words.Length)
                {
                    yield return grid;
                    yield break;
                }

                for (int x = 0; x < 6; x++)
                    for (int y = 0; y < 6; y++)
                    {
                        // Horizontal?
                        if (x + words[ix].Length <= 6 && Enumerable.Range(0, words[ix].Length).All(i => grid[x + i + 6 * y].letter == null || grid[x + i + 6 * y].letter.Value == words[ix][i].letter))
                        {
                            var prev = new (char? letter, bool isSpecial)[words[ix].Length];
                            for (int i = 0; i < words[ix].Length; i++)
                            {
                                prev[i] = grid[x + i + 6 * y];
                                grid[x + i + 6 * y] = (words[ix][i].letter, isSpecial: grid[x + i + 6 * y].isSpecial || words[ix][i].isSpecial);
                            }
                            foreach (var res in recurse(grid, ix + 1))
                                yield return res;
                            for (int i = 0; i < prev.Length; i++)
                                grid[x + i + 6 * y] = prev[i];
                        }

                        // Vertical?
                        if (y + words[ix].Length <= 6 && Enumerable.Range(0, words[ix].Length).All(i => grid[x + 6 * (y + i)].letter == null || grid[x + 6 * (y + i)].letter.Value == words[ix][i].letter))
                        {
                            var prev = new (char? letter, bool isSpecial)[words[ix].Length];
                            for (int i = 0; i < words[ix].Length; i++)
                            {
                                prev[i] = grid[x + 6 * (y + i)];
                                grid[x + 6 * (y + i)] = (words[ix][i].letter, isSpecial: grid[x + 6 * (y + i)].isSpecial || words[ix][i].isSpecial);
                            }
                            foreach (var res in recurse(grid, ix + 1))
                                yield return res;
                            for (int i = 0; i < prev.Length; i++)
                                grid[x + 6 * (y + i)] = prev[i];
                        }
                    }
            }

            var originalWords = words.Select(w => w.Select(c => c.letter).JoinString()).ToHashSet();
            foreach (var solution in recurse(new (char? letter, bool isSpecial)[36], 0))
            {
                var allWords = new HashSet<string>();
                for (int i = 0; i < 6; i++)
                {
                    var column = Enumerable.Range(0, 6).Select(y => solution[i + 6 * y].letter ?? ' ').JoinString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var row = Enumerable.Range(0, 6).Select(x => solution[x + 6 * i].letter ?? ' ').JoinString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in column.Concat(row))
                        if (word.Length > 1 && (!allWords.Add(word) || !originalWords.Contains(word)))
                            goto busted;
                }

                ConsoleUtil.WriteLine(solution.Split(6).Select(row => row.Select(ch => ch.letter.ToString().Color(ch.isSpecial ? ConsoleColor.Magenta : ConsoleColor.Green)).JoinColoredString()).JoinColoredString("\n"));
                ConsoleUtil.WriteLine(Enumerable.Range(0, 36).Where(i => solution[i].isSpecial).Select(i => letters[i]).JoinString());
                busted:;
            }
        }

        public static void SomethingIsOff_9_Battleship()
        {
            var size = 8;
            var ships = new[] { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 }.Where(x => x != 0).OrderByDescending(x => x).ToArray();
            bool[][] solution = null;

            var rowCounts = new int?[] { 4, 2, 3, 3, null, null, 3, 3 };
            var colCounts = new int?[] { 4, 3, 1, 2, 2, 1, 3, 4 };
            var grid = Ut.NewArray(size, size, (x, y) => (x == 2 || x == 5) && (y == 0 || y == 7) ? false : (bool?) null);

            var rowsDone = new bool[size];
            var colsDone = new bool[size];
            var hypotheses = new Stack<(int X, int Y, bool?[][] Grid, bool[] RowsDone, bool[] ColsDone)>();

            nextIter:
            if (rowsDone.All(b => b) && colsDone.All(b => b))
                goto tentativeSolution;

            // Diagonal from a true is a false
            for (int c = 0; c < size; c++)
                for (int r = 0; r < size; r++)
                    if (grid[c][r] == true)
                    {
                        if (r > 0 && c > 0)
                            grid[c - 1][r - 1] = false;
                        if (r > 0 && c < size - 1)
                            grid[c + 1][r - 1] = false;
                        if (r < size - 1 && c > 0)
                            grid[c - 1][r + 1] = false;
                        if (r < size - 1 && c < size - 1)
                            grid[c + 1][r + 1] = false;
                    }

            var anyDeduced = false;

            // Check if a row can be filled in unambiguously
            for (int r = 0; r < size; r++)
                if (rowCounts[r] != null && !rowsDone[r])
                {
                    var cnt = Enumerable.Range(0, size).Count(c => grid[c][r] != false);
                    if (cnt < rowCounts[r].Value)
                        goto contradiction;

                    if (cnt == rowCounts[r].Value)
                    {
                        for (int c = 0; c < size; c++)
                            if (grid[c][r] == null)
                                grid[c][r] = true;
                        rowsDone[r] = true;
                        anyDeduced = true;
                    }

                    cnt = Enumerable.Range(0, size).Count(c => grid[c][r] == true);
                    if (cnt > rowCounts[r].Value)
                        goto contradiction;

                    if (cnt == rowCounts[r].Value)
                    {
                        for (int c = 0; c < size; c++)
                            if (grid[c][r] == null)
                                grid[c][r] = false;
                        rowsDone[r] = true;
                        anyDeduced = true;
                    }
                }
                else if (rowCounts[r] == null && !rowsDone[r] && Enumerable.Range(0, size).All(c => grid[c][r] != null))
                {
                    rowsDone[r] = true;
                    anyDeduced = true;
                }

            // Check if a column can be filled in unambiguously
            for (int c = 0; c < size; c++)
                if (colCounts[c] != null && !colsDone[c])
                {
                    var cnt = Enumerable.Range(0, size).Count(r => grid[c][r] != false);
                    if (cnt < colCounts[c].Value)
                        goto contradiction;

                    if (cnt == colCounts[c].Value)
                    {
                        for (int r = 0; r < size; r++)
                            if (grid[c][r] == null)
                                grid[c][r] = true;
                        colsDone[c] = true;
                        anyDeduced = true;
                    }

                    cnt = Enumerable.Range(0, size).Count(r => grid[c][r] == true);
                    if (cnt > colCounts[c].Value)
                        goto contradiction;

                    if (cnt == colCounts[c].Value)
                    {
                        for (int r = 0; r < size; r++)
                            if (grid[c][r] == null)
                                grid[c][r] = false;
                        colsDone[c] = true;
                        anyDeduced = true;
                    }
                }
                else if (colCounts[c] == null && !colsDone[c] && Enumerable.Range(0, size).All(r => grid[c][r] != null))
                {
                    colsDone[c] = true;
                    anyDeduced = true;
                }

            if (anyDeduced)
                goto nextIter;

            // No obvious deduction. Explore a hypothesis by placing a ship in the first undeduced space
            var unfinishedCol = Array.IndexOf(colsDone, false);
            var unfinishedRow = Array.IndexOf(grid[unfinishedCol], null);
            hypotheses.Push((X: unfinishedCol, Y: unfinishedRow, Grid: Ut.NewArray(size, size, (x, y) => grid[x][y]), RowsDone: (bool[]) rowsDone.Clone(), ColsDone: (bool[]) colsDone.Clone()));
            grid[unfinishedCol][unfinishedRow] = true;
            goto nextIter;

            contradiction:
            if (hypotheses.Count == 0)
            {
                Console.WriteLine("All solutions exhausted.");
                goto done;
            }

            // Backtrack to the last hypothesis and place water instead
            var prevHypo = hypotheses.Pop();
            grid = prevHypo.Grid;
            rowsDone = prevHypo.RowsDone;
            colsDone = prevHypo.ColsDone;
            grid[prevHypo.X][prevHypo.Y] = false;
            goto nextIter;

            tentativeSolution:

            // Check that the tentative solution is correct by counting all the ships.
            var unaccountedFor = ships.OrderByDescending(x => x).ToList();
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    int? thisLen = null;
                    if (grid[x][y] == true && (x == 0 || grid[x - 1][y] == false) && (x == size - 1 || grid[x + 1][y] == false) && (y == 0 || grid[x][y - 1] == false) && (y == size - 1 || grid[x][y + 1] == false))
                        thisLen = 1;
                    if (thisLen == null && grid[x][y] == true && (x == 0 || grid[x - 1][y] == false))
                    {
                        var len = 0;
                        while (x + len < size && grid[x + len][y] == true)
                            len++;
                        if (len > 1 && (x + len == size || grid[x + len][y] == false))
                            thisLen = len;
                    }
                    if (thisLen == null && grid[x][y] == true && (y == 0 || grid[x][y - 1] == false))
                    {
                        var len = 0;
                        while (y + len < size && grid[x][y + len] == true)
                            len++;
                        if (len > 1 && (y + len == size || grid[x][y + len] == false))
                            thisLen = len;
                    }
                    // Are there too many ships of this length?
                    if (thisLen != null && !unaccountedFor.Remove(thisLen.Value))
                        goto contradiction;
                }

            // Is there a ship length unaccounted for? (This should never happen because if it is so, then another ship length must have too many, so the previous check should have caught it.)
            if (unaccountedFor.Count > 0)
                goto contradiction;

            // Found a solution. 
            Console.WriteLine(@"Solution found:");
            Console.WriteLine("   {0}\n{1}".Fmt(
                Enumerable.Range(0, size).Select(col => colCounts[col].ToString().PadLeft(2)).JoinString(),
                Enumerable.Range(0, size).Select(row => rowCounts[row].ToString().PadLeft(3) + " " + Enumerable.Range(0, size).Select(col => grid[col][row].Value ? "# " : "· ").JoinString()).JoinString("\n")));
            var letters = @"{0}SSNA    TORTI    AENLM    REIAG    NSERD    PEES{0}".Fmt(new string(' ', 8));
            Console.WriteLine(Enumerable.Range(0, size).Select(row => Enumerable.Range(0, size).Select(col => grid[col][row].Value ? letters[col + size * row] : ' ').JoinString()).JoinString().Replace(" ", ""));

            // Now keep searching to see if there’s another.
            solution = Ut.NewArray(size, size, (i, j) => grid[i][j] ?? false);
            goto contradiction;

            done:;
        }

        public static void SomethingIsOff_10_Skyscrapers()
        {
            foreach (var grid in GridGenerator.GenerateGrid(5, 5, 1, 5))
            {
                int count(Func<int, int> getter)
                {
                    var tallest = 0;
                    var result = 0;
                    for (var i = 0; i < 5; i++)
                    {
                        var g = getter(i);
                        if (g > tallest)
                        {
                            tallest = g;
                            result++;
                        }
                    }
                    return result;
                }

                if (count(i => grid[i]) != 4)
                    continue;
                if (count(i => grid[i + 3 * 5]) != 3)
                    continue;
                if (count(i => grid[(4 - i) + 5]) != 3)
                    continue;
                if (count(i => grid[(4 - i) + 4 * 5]) != 4)
                    continue;
                if (count(i => grid[2 + 5 * i]) != 3)
                    continue;
                if (count(i => grid[2 + 5 * (4 - i)]) != 3)
                    continue;

                Console.WriteLine(Enumerable.Range(0, 25).Where(i => grid[i] == 2).Select(i => @"FBCTPLORESAEIOURANISTKSHD"[i]).JoinString());
            }
        }

        [Flags]
        enum PeachCharacters
        {
            Bowsette = 1 << 0,
            ShyGal = 1 << 1,
            BulletteBill = 1 << 2,
            Bloopette = 1 << 3,
            Wigglette = 1 << 4,
            Booette = 1 << 5,
            PiranhaPlantette = 1 << 6,
            Lakitette = 1 << 7
        }

        sealed class PeachNode : Node<int, string>
        {
            public string CurrentWord { get; private set; }
            public string FinalWord { get; private set; }
            public PeachCharacters AvailableCharacters { get; private set; }
            public PeachNode(string word, string finalWord, PeachCharacters available)
            {
                CurrentWord = word;
                FinalWord = finalWord;
                AvailableCharacters = available;
            }

            public override bool IsFinal => CurrentWord == FinalWord;

            public override IEnumerable<Edge<int, string>> Edges
            {
                get
                {
                    Edge<int, string> newEdge(string name, PeachNode targetNode) => new Edge<int, string>(1, $"{CurrentWord} → {name} → {targetNode.CurrentWord}", targetNode);

                    //Bowsette (Bw) :             Deletes letters in alphabetic orders
                    if ((AvailableCharacters & PeachCharacters.Bowsette) != 0 && CurrentWord.Length > 0)
                        yield return newEdge("Bowsette", new PeachNode(CurrentWord.Replace(CurrentWord.Min().ToString(), ""), FinalWord, AvailableCharacters));

                    //Shy Gal (Sg) :                Swaps L and R
                    if ((AvailableCharacters & PeachCharacters.ShyGal) != 0)
                        yield return newEdge("Shy Gal", new PeachNode(CurrentWord.Select(ch => ch == 'L' ? 'R' : ch == 'R' ? 'L' : ch).JoinString(), FinalWord, AvailableCharacters));

                    //Bullette Bill (Bb) :           Replaces the first letter with Charge
                    if ((AvailableCharacters & PeachCharacters.BulletteBill) != 0 && CurrentWord.Length > 0)
                        for (char charge = 'A'; charge <= 'Z'; charge++)
                            yield return newEdge($"Bullette Bill ({charge})", new PeachNode(charge + CurrentWord.Substring(1), FinalWord, AvailableCharacters));

                    //Bloopette (Bp) :              Inserts Charge at the second and second to last positions
                    if ((AvailableCharacters & PeachCharacters.Bloopette) != 0 && CurrentWord.Length > 0)
                        for (char charge = 'A'; charge <= 'Z'; charge++)
                        {
                            var chargeStr = charge.ToString();
                            yield return newEdge($"Bloopette ({charge})", new PeachNode(CurrentWord.Insert(CurrentWord.Length - 1, chargeStr).Insert(1, chargeStr), FinalWord, AvailableCharacters));
                        }

                    //Wigglette (Wg) :             Replace Charge with the two letters alphabetically behind and in front of it
                    if ((AvailableCharacters & PeachCharacters.Wigglette) != 0 && CurrentWord.Length > 0)
                        for (var charge = 0; charge < CurrentWord.Length; charge++)
                        {
                            var chCh = CurrentWord[charge];
                            var next = (char) ((chCh - 'A' + 1) % 26 + 'A');
                            var prev = (char) ((chCh - 'A' + 25) % 26 + 'A');
                            yield return newEdge($"Wigglette ({charge})", new PeachNode(CurrentWord.Remove(charge, 1).Insert(charge, next + "" + prev), FinalWord, AvailableCharacters));
                        }

                    //Booette (Bo) :                 Replaces each vowel with the two vowels in front and behind them (e.g. A → UE)
                    if ((AvailableCharacters & PeachCharacters.Booette) != 0 && CurrentWord.Length > 0)
                    {
                        var v = "AEIOU";
                        var newWord = CurrentWord.Select(ch => v.Contains(ch) ? v[(v.IndexOf(ch) + 4) % 5] + "" + v[(v.IndexOf(ch) + 1) % 5] : ch.ToString()).JoinString();
                        yield return newEdge($"Booette", new PeachNode(newWord, FinalWord, AvailableCharacters));
                    }

                    //Piranha Plantette (Pp) :  Swaps the first and second half (also splits if not already split)
                    if ((AvailableCharacters & PeachCharacters.PiranhaPlantette) != 0 && CurrentWord.Length > 0)
                    {
                    }

                    //Lakitette (Lk) :                Puts Charge + everything behind it in front of string			
                    if ((AvailableCharacters & PeachCharacters.Lakitette) != 0 && CurrentWord.Length > 0)
                        for (int charge = 0; charge < CurrentWord.Length; charge++)
                            yield return newEdge($"Lakitette ({charge})", new PeachNode(CurrentWord.Substring(charge) + CurrentWord.Substring(0, charge), FinalWord, AvailableCharacters));
                }
            }

            public override bool Equals(Node<int, string> other) => other is PeachNode p && p.CurrentWord == CurrentWord;
            public override int GetHashCode() => CurrentWord.GetHashCode();
            public override string ToString() => CurrentWord;
        }

        public static void Peach()
        {
            // new PeachNode("TURF", "PARFAIT", PeachCharacters.BulletteBill | PeachCharacters.Booette | PeachCharacters.Wigglette | PeachCharacters.Lakitette),
            var results = DijkstrasAlgorithm.Run(
                new PeachNode("SHRINE", "FESHRONE", PeachCharacters.Bowsette | PeachCharacters.ShyGal | PeachCharacters.Booette | PeachCharacters.Wigglette | PeachCharacters.Bloopette | PeachCharacters.Lakitette),
                0,
                (a, b) => a + b,
                out var totalWeight
            );
            Console.WriteLine(results.JoinString("\n"));
        }

        sealed class Holistic_Newton_Node : Node<int, string>
        {
            public bool[] Board { get; private set; }
            public bool[] Solved { get; private set; }
            public int X { get; private set; }
            public int Y { get; private set; }
            public Holistic_Newton_Node(bool[] board, bool[] solved, int x, int y)
            {
                Board = board;
                Solved = solved;
                X = x;
                Y = y;
            }

            public override bool IsFinal => Solved.All(b => b);

            public override IEnumerable<Edge<int, string>> Edges
            {
                get
                {
                    // Up
                    if (Y > 0)
                    {
                        if (!Board[X + 7 * (Y - 1)])    // just moving
                            yield return new Edge<int, string>(1, "Up", new Holistic_Newton_Node(Board, Solved, X, Y - 1));
                        else if (X.IsBetween(1, 5) && !Solved[X + 1]) // pushing a piece up
                        {
                            // Only valid if there’s no piece in between
                            for (int y = 0; y < Y - 1; y++)
                                if (Board[X + 7 * y])
                                    goto busted;
                            var newSolved = (bool[]) Solved.Clone();
                            newSolved[X + 1] = true;
                            var newBoard = (bool[]) Board.Clone();
                            newBoard[X + 7 * (Y - 1)] = false;
                            yield return new Edge<int, string>(1, "Up", new Holistic_Newton_Node(newBoard, newSolved, X, Y - 1));
                            busted:;
                        }
                    }

                    // Left
                    if (X > 0)
                    {
                        if (!Board[X - 1 + 7 * Y])  // just moving
                            yield return new Edge<int, string>(1, "Left", new Holistic_Newton_Node(Board, Solved, X - 1, Y));
                        else if ((Y == 3 && !Solved[1]) || (Y == 5 && !Solved[0])) // pushing a piece left
                        {
                            var newSolved = (bool[]) Solved.Clone();
                            newSolved[Y == 3 ? 1 : 0] = true;
                            var newBoard = (bool[]) Board.Clone();
                            newBoard[X - 1 + 7 * Y] = false;
                            for (int x = X - 2; x >= 0; x--)
                                if (newBoard[x + 7 * Y])
                                {
                                    newBoard[x + 7 * Y] = false;
                                    newBoard[x + 1 + 7 * Y] = true;
                                }
                            yield return new Edge<int, string>(1, "Left", new Holistic_Newton_Node(newBoard, newSolved, X - 1, Y));
                        }
                    }

                    // Right
                    if (X < 6)
                    {
                        if (!Board[X + 1 + 7 * Y])  // just moving
                            yield return new Edge<int, string>(1, "Right", new Holistic_Newton_Node(Board, Solved, X + 1, Y));
                        else if ((Y == 3 && !Solved[7]) || (Y == 5 && !Solved[8])) // pushing a piece right
                        {
                            var newSolved = (bool[]) Solved.Clone();
                            newSolved[Y == 3 ? 7 : 8] = true;
                            var newBoard = (bool[]) Board.Clone();
                            newBoard[X + 1 + 7 * Y] = false;
                            for (int x = X + 2; x <= 6; x++)
                                if (newBoard[x + 7 * Y])
                                {
                                    newBoard[x + 7 * Y] = false;
                                    newBoard[x - 1 + 7 * Y] = true;
                                }
                            yield return new Edge<int, string>(1, "Right", new Holistic_Newton_Node(newBoard, newSolved, X + 1, Y));
                        }
                    }

                    // Down
                    if (Y < 6 && !Board[X + 7 * (Y + 1)]) // just moving
                        yield return new Edge<int, string>(1, "Down", new Holistic_Newton_Node(Board, Solved, X, Y + 1));
                }
            }
            public override bool Equals(Node<int, string> other) => other is Holistic_Newton_Node hnn && hnn.X == X && hnn.Y == Y && hnn.Board.SequenceEqual(Board) && hnn.Solved.SequenceEqual(Solved);
            public override int GetHashCode() => Ut.ArrayHash(Board, Solved, X, Y);
        }

        public static void Holistic_Newton()
        {
            var result = DijkstrasAlgorithm.Run(new Holistic_Newton_Node(Ut.NewArray(49, i => (i % 7) % 2 == 1 && (i / 7) % 2 == 1), new bool[9], 3, 4), 1, (a, b) => a + b, out var totalWeight);
            Console.WriteLine(totalWeight);
            Console.WriteLine(result.JoinString(", "));
        }

        sealed class Holistic_Gabelstapler_Node : Node<int, ConsoleColoredString>
        {
            public int X { get; private set; }
            public int Height { get; private set; }
            public int[] Crates { get; private set; }

            public Holistic_Gabelstapler_Node(int x, int height, int[] crates)
            {
                if (height == 10)
                    Debugger.Break();
                X = x;
                Height = height;
                Crates = crates;
            }

            static readonly int[] _finalState = new int[100];
            static Holistic_Gabelstapler_Node()
            {
                _finalState[7] = 1;
                _finalState[17] = 1;
                _finalState[27] = 1;
                _finalState[8] = 2;
                _finalState[9] = 3;
                _finalState[19] = 3;
            }

            public override bool IsFinal => Crates.SequenceEqual(_finalState);

            public override IEnumerable<Edge<int, ConsoleColoredString>> Edges
            {
                get
                {
                    // Move left
                    if (X > 0 && Crates[X - 1] == 0 && (X == 2 ? Height <= 2 : X == 3 ? Height <= 3 : true))
                    {
                        var newCrates = Crates;
                        // Are we carrying crates?
                        if (Height > 0 && Crates[X + 1 + 10 * Height] != 0 && Crates[X + 1 + 10 * (Height - 1)] == 0)
                        {
                            newCrates = (int[]) Crates.Clone();
                            if (X == 2)    // Hitting the block
                            {
                                for (int y = Height; y < 3; y++)
                                {
                                    newCrates[X + 10 * y] = newCrates[X + 1 + 10 * y];
                                    newCrates[X + 1 + 10 * y] = 0;
                                }
                                for (int y = 0; y < 10; y++)
                                    newCrates[X + 1 + 10 * y] = y + 3 < 10 ? newCrates[X + 1 + 10 * (y + 3)] : 0;
                            }
                            else
                            {
                                for (int y = Height; y < 10; y++)
                                {
                                    newCrates[X + 10 * y] = newCrates[X + 1 + 10 * y];
                                    newCrates[X + 1 + 10 * y] = 0;
                                }
                            }
                        }

                        var node = new Holistic_Gabelstapler_Node(X - 1, Height, newCrates);
                        yield return new Edge<int, ConsoleColoredString>(1, "Left\n" + node.ToCString(), node);
                    }

                    // Move right
                    if (X < 8 && (Height == 0 || Crates[X + 1] == 0) && (X == 0 ? Height != 3 : X == 1 ? Height <= 2 : true) && (Crates[X + 1 + 10 * Height] == 0 || Crates[X + 2 + 10 * Height] == 0))
                    {
                        var newCrates = Crates;
                        // Are we carrying or pushing crates?
                        if (Crates[X + 1 + 10 * Height] != 0)
                        {
                            newCrates = (int[]) Crates.Clone();
                            for (int y = Height; y < 10; y++)
                            {
                                newCrates[X + 2 + 10 * y] = newCrates[X + 1 + 10 * y];
                                newCrates[X + 1 + 10 * y] = 0;
                            }
                        }

                        var node = new Holistic_Gabelstapler_Node(X + 1, Height, newCrates);
                        yield return new Edge<int, ConsoleColoredString>(1, "Right\n" + node.ToCString(), node);
                    }

                    // Up (increase height)
                    if (
                        // At the far left AND carrying a crate? => Go up to above the highest crate
                        (X == 0 && Crates[1 + 10 * Height] != 0) ? (Height <= 3 || Crates[2 + 10 * Height] != 0) :
                        // At the far left, NOT carrying a crate, and there’s nothing on the block? => optimize away
                        (X == 0 && Crates[2 + 10 * 4] == 0) ? false :
                        // At the far left and NOT carrying a crate? => Go up to the highest crate only
                        (X == 0) ? (Height <= 3 || Crates[2 + 10 * (Height + 1)] != 0) :
                        // Column 1 and carrying a crate?
                        (X == 1 && Crates[2 + 10 * Height] != 0) ? (
                            // Allow carrying crates below the block
                            (Height <= 1 && Crates[2 + 10 * 2] == 0) ||
                            // Allow lifting crates from ABOVE the block
                            (Height >= 4 && Crates[2 + 10 * Height] != 0 && (Height == 4 || Crates[2 + 10 * (Height - 1)] != 0))
                        ) :
                        // Column 1 and NOT carrying a crate? => Go up to the highest crate in column 2 only
                        (X == 1) ? (Height <= 1 && Crates[3 + 10 * (Height + 1)] != 0) :
                        // Maximum lift height in column 2
                        (X == 2) ? (Height <= 2) :
                        // In any other case:
                        // Lifting a crate from a stack is always allowed
                        (Crates[X + 1 + 10 * Height] != 0 && (Height == 0 || Crates[X + 1 + 10 * (Height - 1)] != 0)) ? true :
                        // Carrying a crate? => Go up to above the highest crate on the immediate right
                        (Crates[X + 1 + 10 * Height] != 0) ? (Crates[X + 2 + 10 * Height] != 0) :
                        // Not carrying a crate? => Go only up to the highest crate on the immediate right
                        (Height < 9 && Crates[X + 2 + 10 * (Height + 1)] != 0)
                    )
                    {
                        var newCrates = Crates;
                        if (Crates[X + 1 + 10 * Height] != 0)
                        {
                            newCrates = (int[]) Crates.Clone();
                            for (int y = 9; y > Height; y--)
                                newCrates[X + 1 + 10 * y] = newCrates[X + 1 + 10 * (y - 1)];
                            newCrates[X + 1 + 10 * Height] = 0;
                        }
                        var node = new Holistic_Gabelstapler_Node(X, Height + 1, newCrates);
                        yield return new Edge<int, ConsoleColoredString>(1, "Up\n" + node.ToCString(), node);
                    }

                    // Down (decrease height)
                    if (Height > 0 && !(X == 1 && Height == 4) && Crates[X + 1 + 10 * (Height - 1)] == 0)
                    {
                        var newCrates = Crates;
                        if (Crates[X + 1 + 10 * Height] != 0)
                        {
                            newCrates = (int[]) Crates.Clone();
                            int y = Height - 1;
                            for (; y < 9 && newCrates[X + 1 + 10 * (y + 1)] != 0; y++)
                                newCrates[X + 1 + 10 * y] = newCrates[X + 1 + 10 * (y + 1)];
                            newCrates[X + 1 + 10 * y] = 0;
                        }
                        var node = new Holistic_Gabelstapler_Node(X, Height - 1, newCrates);
                        yield return new Edge<int, ConsoleColoredString>(1, "Down\n" + node.ToCString(), node);
                    }
                }
            }

            public override bool Equals(Node<int, ConsoleColoredString> other) => other is Holistic_Gabelstapler_Node hg && hg.X == X && hg.Height == Height && hg.Crates.SequenceEqual(Crates);
            public override int GetHashCode() => Ut.ArrayHash(X, Height, Crates);

            public ConsoleColoredString ToCString()
            {
                return Enumerable.Range(0, 10).Select(row => Enumerable.Range(0, 10).Select(col =>
                {
                    var ix = col + 10 * (9 - row);
                    var str = Crates[ix] != 0 ? Crates[ix].ToString() + " " : "  ";
                    var gabelstapler = col == X && row == 9;
                    var gabel = col == X + 1 && row == 9 - Height;
                    return str.Color(ConsoleColor.White, gabelstapler ? ConsoleColor.DarkYellow : gabel ? ConsoleColor.DarkRed : ConsoleColor.Black);
                }).JoinColoredString()).JoinColoredString("\n");
            }
        }

        public static void Holistic_Gabelstapler()
        {
            var crates = new int[10 * 10];
            crates[4] = 1;
            crates[5] = 1;
            crates[6] = 1;
            crates[14] = 2;
            crates[15] = 3;
            crates[16] = 3;
            var result = DijkstrasAlgorithm.Run(new Holistic_Gabelstapler_Node(1, 0, crates), 1, (a, b) => a + b, out var totalWeight);
            Console.WriteLine(totalWeight);
            ConsoleUtil.WriteLine(result.JoinColoredString("\n\n"));
        }

        public static void TheLastDatabender()
        {
            //var sb = new StringBuilder();
            //foreach (var file in new DirectoryInfo(@"D:\temp\galactic\TheLastDatabender").EnumerateFiles("*.txt")
            //    .Where(f => int.TryParse(Path.GetFileNameWithoutExtension(f.Name), out var number) && number % 3 != 0)
            //    .Select(f => (file: f, number: int.Parse(Path.GetFileNameWithoutExtension(f.Name))))
            //    .OrderBy(f => f.number % 26).ThenBy(f => f.number)
            //)
            //    sb.AppendLine($"{file.number % 26,2} {File.ReadAllText(file.file.FullName)}");
            //Clipboard.SetText(sb.ToString());

            for (int i = 0; i < 26; i++)
            {
                var str = @"FOHDU";
                Console.WriteLine(str.Select(ch => (char) ((ch - 'A' + i) % 26 + 'A')).JoinString());
            }
        }

        public static unsafe void Observatory()
        {
            using (var bmp = new Bitmap(@"D:\temp\galactic\Observatory-moved-new.png"))
            {
                var dots = new List<(int x, int y, int r, int g, int b)>();
                var input = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                var prev = (byte*) 0;
                for (int y = 0; y < bmp.Height; y++)
                {
                    var p = (byte*) (input.Scan0 + y * input.Stride);
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        if (p[4 * x] < 0xa0 || (y > 0 && prev[4 * x] >= 0xa0) || (x > 0 && p[4 * (x - 1)] >= 0xa0))
                            continue;
                        dots.Add((x, y, p[4 * x + 2], p[4 * x + 1], p[4 * x]));
                    }
                    prev = p;
                }
                bmp.UnlockBits(input);

                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    //foreach (var group in dots.GroupBy(d => (d.r, d.g, d.b)))
                    //{
                    //    byte fnc(int val) => (byte) ((val - 0xc0) * 3);
                    //    var c = Color.FromArgb(fnc(group.Key.r), fnc(group.Key.g), fnc(group.Key.b));
                    //    var avgX = group.Average(dot => (float) dot.x);
                    //    var avgY = group.Average(dot => (float) dot.y);
                    //    g.FillEllipse(new SolidBrush(c), avgX - 10, avgY - 10, 23, 23);
                    //}

                    for (int i = 0; i < dots.Count; i++)
                    {
                        var dot = dots[i];
                        byte fnc(int val) => (byte) ((val - 0xc0) * 4);
                        var c = Color.FromArgb(fnc(dot.r), fnc(dot.g), fnc(dot.b));
                        g.DrawEllipse(new Pen(c, 5f), dot.x - 10, dot.y - 10, 23, 23);
                        g.DrawString($"{i}", new Font(new FontFamily("Arial"), 24f), new SolidBrush(c), dot.x + 2, dot.y + 26, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                    }
                }

                bmp.Save(@"D:\temp\galactic\Observatory-moved-new-marked.png");
            }
        }

        public static unsafe void Observatory2()
        {
            List<(int x, int y, int r, int g, int b)> getDots(string bitmapPath)
            {
                using (var bmp = new Bitmap(bitmapPath))
                {
                    var dots = new List<(int x, int y, int r, int g, int b)>();
                    var input = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                    var prev = (byte*) 0;
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        var p = (byte*) (input.Scan0 + y * input.Stride);
                        for (int x = 0; x < bmp.Width; x++)
                        {
                            if (p[4 * x] < 0xa0 || (y > 0 && prev[4 * x] >= 0xa0) || (x > 0 && p[4 * (x - 1)] >= 0xa0))
                                continue;
                            dots.Add((x, y, p[4 * x + 2], p[4 * x + 1], p[4 * x]));
                        }
                        prev = p;
                    }
                    bmp.UnlockBits(input);
                    return dots;
                }
            }

            var oldDots = getDots(@"D:\temp\galactic\Observatory-moved-old.png");
            var newDots = getDots(@"D:\temp\galactic\Observatory-moved-new.png");

            var oldDotsOrder = new[] {
                new[] { 1, 10, 5, 4, 14 },
                new[] { 3, 16, 11, 9, 21 },
                new[] { 0, 8, 12, 2, 15 },
                new[] { 7, 26, 24, 20, 28 },
                new[] { 23, 33, 38, 27, 42 },
                new[] { 31, 44, 35, 39, 41 },
                new[] { 30, 40, 37, 34, 43 },
                new[] { 17, 32, 29, 22, 36 },
                new[] { 6, 25, 13, 18, 19 }
            };
            var newDotsOrder = new[] {
                new[] { 3, 6, 0, 15, 8 },
                new[] { 2, 14, 7, 18, 1 },
                new[] { 5, 11, 12, 9, 4 },
                new[] { 16, 24, 22, 25, 23 },
                new[] { 26, 39, 37, 31, 34 },
                new[] { 43, 44, 36, 42, 33 },
                new[] { 40, 41, 32, 38, 35 },
                new[] { 28, 30, 29, 27, 21 },
                new[] { 10, 20, 17, 19, 13 }
            };
            var names = @"Red;Orange;Chartreuse;Green;Teal;Turquoise;Blue;Purple;Pink".Split(';');

            using (var bmp = new Bitmap(@"D:\temp\galactic\Observatory-moved-new.png"))
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                for (int i = 0; i < 9; i++)
                {
                    Console.Write($"{names[i],15}: ");
                    foreach (var clockwise in new[] { true, false })
                    {
                        for (int jj = 0; jj < 5; jj++)
                        {
                            var j = clockwise ? jj : 4 - jj;
                            var lambda = clockwise
                                ? new EdgeD(oldDots[oldDotsOrder[i][j]].x, oldDots[oldDotsOrder[i][j]].y, oldDots[oldDotsOrder[i][(j + 1) % oldDotsOrder[i].Length]].x, oldDots[oldDotsOrder[i][(j + 1) % oldDotsOrder[i].Length]].y).LambdaOfPointDroppedPerpendicularly(new PointD(newDots[newDotsOrder[i][j]].x, newDots[newDotsOrder[i][j]].y))
                                : new EdgeD(oldDots[oldDotsOrder[i][(j + 1) % oldDotsOrder[i].Length]].x, oldDots[oldDotsOrder[i][(j + 1) % oldDotsOrder[i].Length]].y, oldDots[oldDotsOrder[i][j]].x, oldDots[oldDotsOrder[i][j]].y).LambdaOfPointDroppedPerpendicularly(new PointD(newDots[newDotsOrder[i][j]].x, newDots[newDotsOrder[i][j]].y));
                            var intL = (int) Math.Round(lambda * 27);
                            //Console.WriteLine($" - {lambda * 27,7:0.000} = {intL,2} = {(char) ('A' + (intL + 25) % 26)} / {26 - intL,2} = {(char) ('Z' - intL)}");
                            Console.Write($"{(char) ('A' + (intL + 25) % 26)}");

                            var newDot = newDots[newDotsOrder[i][j]];
                            byte fnc(int val) => (byte) val;// (byte) ((val - 0xc0) * 4);
                            g.DrawString($"{(char) ('A' + (intL + 25) % 26)}", new Font(new FontFamily("Arial"), 24f), new SolidBrush(Color.FromArgb(fnc(newDot.r), fnc(newDot.g), fnc(newDot.b))), newDot.x + 2, newDot.y + (clockwise ? -26 : 26), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                            if (!clockwise)
                                g.DrawLine(Pens.White, oldDots[oldDotsOrder[i][j]].x, oldDots[oldDotsOrder[i][j]].y, oldDots[oldDotsOrder[i][(j + 1) % 5]].x, oldDots[oldDotsOrder[i][(j + 1) % 5]].y);
                        }
                        Console.Write("  ");
                    }
                    Console.WriteLine();
                    //bmp.Save($@"D:\temp\galactic\Observatory-moved-new-lettered-{(clockwise ? "cw" : "ccw")}.png");
                }
            }
        }

        public static void Cuspidation_FindWords()
        {
            var patterns = new[] { "EL", "?UEEN", "SE?ERE", "WI?I", "ABRA?E", "?AV??IN", "TA?ON", "L?CK", "WA??", "DU?", "??BIT", "??AMI??", "?ARM", "?IS?R", "R?AM", "??ND?" };
            var allLetters = "LXAEOHWQYAJVCGVDSHBENFKRO";
            var words = File.ReadLines(@"D:\Daten\sowpods.txt").Concat(new[] { "WIKI" }).ToArray();
            var candidates = patterns.Select(ptrn => words.Where(w => Regex.IsMatch(w, "^" + ptrn.Replace("?", ".") + "$")).ToArray()).ToArray();

            var bestMatch = 0;

            IEnumerable<string> recurse(string letters, IEnumerable<string> soFar, int ix)
            {
                if (ix > bestMatch)
                {
                    bestMatch = ix;
                    Console.WriteLine(soFar.JoinString(", ") + " / " + letters);
                }

                if (ix == patterns.Length)
                {
                    yield return soFar.JoinString(", ");
                    yield break;
                }

                foreach (var word in candidates[ix].Where(word => new Regex("^" + patterns[ix].Replace("?", "[" + letters + "]") + "$").IsMatch(word)))
                {
                    var newLetters = letters;
                    for (int i = 0; i < patterns[ix].Length; i++)
                        if (patterns[ix][i] == '?')
                        {
                            var pos = newLetters.IndexOf(word[i]);
                            if (pos == -1)
                                goto busted;
                            newLetters = newLetters.Remove(pos, 1);
                        }
                    foreach (var sol in recurse(newLetters, soFar.Concat(word), ix + 1))
                        yield return sol;
                    busted:;
                }
            }

            foreach (var solution in recurse(allLetters, new string[0], 0))
                Console.WriteLine(solution);
        }

        private static void Colors_Test()
        {
            var strings = Ut.NewArray(6, _ => "");
            var colors = new List<(int r, int g, int b)>();
            for (int i = 0; i < 7; i++)
            {
                using (var bmp = new Bitmap($@"D:\temp\galactic\Colors\{i}.png"))
                    for (int j = 0; j < 6; j++)
                    {
                        var color = bmp.GetPixel(bmp.Width / 2, bmp.Height / 12 * (2 * j + 1));
                        //strings[j] += ($"{i}\t{j}\t\"'{color.R:X2}\"\t\"'{color.G:X2}\"\t\"'{color.B:X2}\"\t{colorNames.FirstOrDefault(kvp => kvp.Value == $"#{color.R:X2}{color.G:X2}{color.B:X2}".ToLowerInvariant()).Key}\t\t");
                        strings[j] += ($"{i}\t{j}\t\"'{Convert.ToString(color.R, 2).PadLeft(8, '0').Replace("1", "█").Replace("0", "░")}\"\t\"'{Convert.ToString(color.G, 2).PadLeft(8, '0').Replace("1", "█").Replace("0", "░")}\"\t\"'{Convert.ToString(color.B, 2).PadLeft(8, '0').Replace("1", "█").Replace("0", "░")}\"\t\t");
                        if (colors.Count < 6)
                            colors.Add((color.R, color.G, color.B));
                        else
                            colors[j] = (colors[j].r ^ color.R, colors[j].g ^ color.G, colors[j].b ^ color.B);
                    }
            }
            for (int j = 0; j < 6; j++)
            {
                var (r, g, b) = colors[j];
                //strings[j] += ($"{i}\t{j}\t\"'{color.R:X2}\"\t\"'{color.G:X2}\"\t\"'{color.B:X2}\"\t{colorNames.FirstOrDefault(kvp => kvp.Value == $"#{color.R:X2}{color.G:X2}{color.B:X2}".ToLowerInvariant()).Key}\t\t");
                strings[j] += ($"\"'{Convert.ToString(r, 2).PadLeft(8, '0').Replace("1", "█").Replace("0", "░")}\"\t\"'{Convert.ToString(g, 2).PadLeft(8, '0').Replace("1", "█").Replace("0", "░")}\"\t\"'{Convert.ToString(b, 2).PadLeft(8, '0').Replace("1", "█").Replace("0", "░")}\"\t\t");
            }
            Clipboard.SetText(strings.JoinString("\n"));
        }

        public static void TheMetaPuzzle_ARGENTINA()
        {
            var offsets = Ut.NewArray<(int x, int y)>(
                //(-15, 0), (-15, 5), (-14, 2), (-11, 2), (-11, 3), (-5, -3), (-5, -1), (-4, 1), (-3, -1), (-2, -2), (-2, 0), (-2, 2), (0, -1), (1, -2), (1, -2), (1, 1), (2, 1), (2, 3), (3, -1), (3, -1), (3, 0), (4, -1), (8, -1), (12, -5), (12, 2), (14, -2), (15, 3), (17, 0)
                (-15, 0), (-2, 2), (0, -1), (1, -2), (1, 1), (3, -1), (3, -1), (12, 2), (17, 0)
            );

            var table = @"H;;;;;;;;;;;;;;;;;He
Li;Be;;;;;;;;;;;B;C;N;O;F;Ne
Na;Mg;;;;;;;;;;;Al;Si;P;S;Cl;Ar
K;Ca;Sc;Ti;V;Cr;Mn;Fe;Co;Ni;Cu;Zn;Ga;Ge;As;Se;Br;Kr
Rb;Sr;Y;Zr;Nb;Mo;Tc;Ru;Rh;Pd;Ag;Cd;In;Sn;Sb;Te;I;Xe
Cs;Ba;La;Hf;Ta;W;Re;Os;Ir;Pt;Au;Hg;Tl;Pb;Bi;Po;At;Rn
Fr;Ra;Ac;Rf;Db;Sg;Bh;Hs;Mt;Ds;Rg;Cn;Nh;Fl;Mc;Lv;Ts;Og".Replace("\r", "").Split('\n').Select(row => row.Split(';')).ToArray();
            var lookup = table.SelectMany((row, rowIx) => row.Select((elem, colIx) => (elem, colIx, rowIx)).Where(tup => tup.elem.Length > 0)).ToDictionary(tup => tup.elem.ToUpperInvariant());

            var solutions = File.ReadLines(@"D:\Daten\sowpods.txt").SelectMany(word =>
            {
                IEnumerable<string> recurse(string fragment, string soFar, string lastElement, (int x, int y)[] remainingOffsets)
                {
                    if (fragment.Length == 0)
                    {
                        yield return soFar;
                        yield break;
                    }

                    for (int ln = 1; ln <= 2; ln++)
                    {
                        if (fragment.Length < ln || !lookup.TryGetValue(fragment.Substring(0, ln), out var tup))
                            continue;

                        var ix = lastElement == null ? -2 : remainingOffsets.IndexOf(of => of.x == tup.colIx - lookup[lastElement].colIx && of.y == tup.rowIx - lookup[lastElement].rowIx);
                        if (ix == -1)
                            continue;

                        var newRemainingOffsets = remainingOffsets;
                        var newSoFar = soFar;
                        if (ix != -2)
                        {
                            newRemainingOffsets = remainingOffsets.Take(ix).Concat(remainingOffsets.Skip(ix + 1)).ToArray();
                            newSoFar += $" ({remainingOffsets[ix].x}, {remainingOffsets[ix].y})→ ";
                        }

                        foreach (var res in recurse(fragment.Substring(ln), newSoFar + tup.elem, fragment.Substring(0, ln), newRemainingOffsets))
                            yield return res;
                    }
                }

                return recurse(word, "", null, offsets).Select(solution => $"{word} = {solution}");
            });
            foreach (var solution in solutions) //.OrderBy(s => s.Count(ch => ch == '→'));
                Console.WriteLine(solution);
        }

        public static void TheMetaPuzzle_BLACK_SUN()
        {
            int fromCard(string cardStr) => "a23456789tjqk".IndexOf(char.ToLowerInvariant(cardStr[0])) + 13 * ("cd♣♦hs♥♠".IndexOf(cardStr[1]) / 4);

            var allWords = @"SUN,TIE,DOT,INK,RAW".Split(',');
            var allGivens = Ut.NewArray(
                @"K♦ T♥ J♥",
                @"9♣ T♣ A♠",
                @"T♦ J♦ Q♥",
                @"Q♣ K♣ 4♠",
                @"5♣ J♣ J♠",
                @"8♦ Q♦ 8♥").Select(str => str.Split(' ').Select(fromCard).ToArray()).ToArray();

            IEnumerable<(char?[], int[][])> recurse(char?[] known, int[][] remainingGivens, int wordIx)
            {
                if (wordIx == allWords.Length)
                {
                    yield return (known.ToArray(), remainingGivens);
                    yield break;
                }

                for (int givenIx = 0; givenIx < remainingGivens.Length; givenIx++)
                {
                    var given = remainingGivens[givenIx];
                    foreach (var cmb in allWords[wordIx].Permutations())
                    {
                        var word = cmb.JoinString();
                        for (int i = 0; i < 3; i++)
                        {
                            if (known[given[i]] != null && known[given[i]].Value != word[i])
                                goto busted;
                            for (int j = i + 1; j < 3; j++)
                                if ((word[i] == word[j]) != (given[i] == given[j]))
                                    goto busted;
                        }
                        var newKnown = (char?[]) known.Clone();
                        for (int i = 0; i < 3; i++)
                            newKnown[given[i]] = word[i];

                        foreach (var sol in recurse(newKnown, remainingGivens.Remove(givenIx, 1), wordIx + 1))
                            yield return sol;

                        busted:;
                    }
                }
            }

            var desired = @"th 5d 8c ts qd kc kd ts".Split(' ').Select(fromCard).ToArray();
            var runs = "IWON,SIN,NAS".Split(',');

            foreach (var (solution, remGivens) in recurse(new char?[26], allGivens, 0))
            {
                var reverse = Enumerable.Range(0, 26).ToDictionary(letter => (char) (letter + 'A'), letter => solution.IndexOf((char) (letter + 'A')));
                // T & I same rank?
                if (reverse['T'] != -1 && reverse['I'] != -1 && reverse['T'] % 13 != reverse['I'] % 13)
                    continue;
                // runs all same suit?
                if (runs.Any(run => run.Skip(1).Any(ch => reverse[ch] / 13 != reverse[run[0]] / 13)))
                    continue;

                string convertToCards(string word) => word.Select(ch => reverse[ch] == -1 ? "???" : "A23456789TJQK"[reverse[ch] % 13] + new[] { "♣♥", "♠♦" }[reverse[ch] / 13]).JoinString();
                string convertToCards2(int[] gv) => gv.Select(g => "A23456789TJQK"[g % 13] + new[] { "♣♥", "♠♦" }[g / 13]).JoinString();
                Console.WriteLine($"{desired.Select(i => solution[i] ?? '?').JoinString()} ({solution.Select(ch => ch ?? '?').JoinString()}) (runs: {runs.Select(word => $"{word}={convertToCards(word)}").JoinString("/")})");
                foreach (var given in allGivens)
                    Console.WriteLine($" — {convertToCards2(given)} = {given.Select(card => solution[card] ?? '?').JoinString()}");
            }
        }

        public static void TheMetaPuzzle_ANTIGO_WISCONSIN()
        {
            var c = 0;
            //foreach (var word in File.ReadAllLines(@"D:\Daten\sowpods.txt"))
            //    if (word.Length == 6 && !(word.Contains("I") || word.Contains("O")) && Regex.IsMatch(word, @"^[^I][^I]V[^I][^O][^O]$"))
            //    {
            //        Console.WriteLine(word);
            //        c++;
            //    }
            foreach (var word in File.ReadAllLines(@"D:\Daten\sowpods.txt"))
                if (Regex.IsMatch(word, @"^[^AEIOU]*I[^AEIOU]*I[^AEIOU]*O[^AEIOU]*$"))
                {
                    Console.WriteLine(word);
                    c++;
                }
            Console.WriteLine(c);
        }

        private static int LevenshteinDistance(string a, string b)
        {
            int lengthA = a.Length;
            int lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];
            for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
            for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

            for (int i = 1; i <= lengthA; i++)
                for (int j = 1; j <= lengthB; j++)
                    distances[i, j] = Math.Min(Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1), distances[i - 1, j - 1] + (b[j - 1] == a[i - 1] ? 0 : 1));
            return distances[lengthA, lengthB];
        }

        public static void WhereAreThey()
        {
            var countries = @"AFGHANISTAN;ALBANIA;ALGERIA;ANDORRA;ANGOLA;ANTIGUAANDBARBUDA;ARGENTINA;ARMENIA;AUSTRALIA;AUSTRIA;AZERBAIJAN;BAHAMAS;BAHRAIN;BANGLADESH;BARBADOS;BELARUS;BELGIUM;BELIZE;BENIN;BHUTAN;BOLIVIA;BOSNIAANDHERZEGOVINA;BOTSWANA;BRAZIL;BRUNEI;BULGARIA;BURKINAFASO;BURUNDI;CABOVERDE;CAMBODIA;CAMEROON;CANADA;CENTRALAFRICANREPUBLIC;CHAD;CHILE;CHINA;COLOMBIA;COMOROS;CONGO;COSTARICA;CÔTED'IVOIRE;CROATIA;CUBA;CYPRUS;CZECHREPUBLIC;DEMOCRATICREPUBLICOFTHECONGO;DENMARK;DJIBOUTI;DOMINICA;DOMINICANREPUBLIC;ECUADOR;EGYPT;ELSALVADOR;EQUATORIALGUINEA;ERITREA;ESTONIA;ESWATINI;ETHIOPIA;FEDERATEDSTATESOFMICRONESIA;FIJI;FINLAND;FRANCE;GABON;GAMBIA;GEORGIA;GERMANY;GHANA;GREECE;GRENADA;GUATEMALA;GUINEA;GUINEA-BISSAU;GUYANA;HAITI;HONDURAS;HUNGARY;ICELAND;INDIA;INDONESIA;IRAN;IRAQ;IRELAND;ISRAEL;ITALY;JAMAICA;JAPAN;JORDAN;KAZAKHSTAN;KENYA;KIRIBATI;KOSOVO;KUWAIT;KYRGYZSTAN;LAOS;LATVIA;LEBANON;LESOTHO;LIBERIA;LIBYA;LIECHTENSTEIN;LITHUANIA;LUXEMBOURG;MADAGASCAR;MALAWI;MALAYSIA;MALDIVES;MALI;MALTA;MARSHALLISLANDS;MAURITANIA;MAURITIUS;MEXICO;MOLDOVA;MONACO;MONGOLIA;MONTENEGRO;MOROCCO;MOZAMBIQUE;MYANMAR;NAMIBIA;NAURU;NEPAL;NETHERLANDS;NEWZEALAND;NICARAGUA;NIGER;NIGERIA;NORTHKOREA;NORTHMACEDONIA;NORWAY;OMAN;PAKISTAN;PALAU;PANAMA;PAPUANEWGUINEA;PARAGUAY;PERU;PHILIPPINES;POLAND;PORTUGAL;QATAR;ROMANIA;RUSSIA;RWANDA;SAINTKITTSANDNEVIS;SAINTLUCIA;SAINTVINCENTANDTHEGRENADINES;SAMOA;SANMARINO;SÃOTOMÉANDPRÍNCIPE;SAUDIARABIA;SENEGAL;SERBIA;SEYCHELLES;SIERRALEONE;SINGAPORE;SLOVAKIA;SLOVENIA;SOLOMONISLANDS;SOMALIA;SOUTHAFRICA;SOUTHKOREA;SOUTHSUDAN;SPAIN;SRILANKA;SUDAN;SURINAME;SWEDEN;SWITZERLAND;SYRIANARABREPUBLIC;TAIWAN;TAJIKISTAN;TANZANIA;THAILAND;TIMOR-LESTE;TOGO;TONGA;TRINIDADANDTOBAGO;TUNISIA;TURKEY;TURKMENISTAN;TUVALU;UGANDA;UKRAINE;UNITEDARABEMIRATES;UNITEDKINGDOM;UNITEDSTATES;URUGUAY;UZBEKISTAN;VANUATU;VATICANCITY;VENEZUELA;VIETNAM;YEMEN;ZAMBIA;ZIMBABWE"
                .Split(';');

            foreach (var item in @"CUP;SNORKEL;BINOCULARS;TEAKETTLE;SLEEPINGBAG;SHOVEL;BACKPACK;CAMERA;MALLET;BAG;BELT;WALKINGSTICK".Split(';'))
                Console.WriteLine($"{countries.MinElement(c => LevenshteinDistance(c, item))}");
        }

        public static void ResearchCenter()
        {
            var file = File.ReadAllLines(@"D:\temp\galactic\Research Center.txt");
            var width = file.Max(f => f.Length);
            var height = file.Length;
            var field = file.Select(line => line.PadRight(width, ' ').ToCharArray()).ToArray();

            var bubbles = new List<(int x, int y, int size)>();

            while (true)
            {
                int y = 0, x = 0;
                for (y = 0; y < height; y++)
                {
                    for (x = 0; x < width; x++)
                    {
                        if (!" /\\_|".Contains(field[y][x]))
                        {
                            goto found;
                        }
                    }
                }
                break;

                found:;
                var q = new Queue<(int x, int y)>();
                q.Enqueue((x, y));
                var already = new HashSet<(int x, int y)>();
                var area = 0;

                while (q.Count > 0)
                {
                    var (tx, ty) = q.Dequeue();
                    if (!already.Add((tx, ty)))
                        continue;

                    if (!"/\\_|".Contains(field[ty][tx]))
                    {
                        area += 1;
                        if (tx > 0)
                            q.Enqueue((tx - 1, ty));
                        if (tx < width - 1)
                            q.Enqueue((tx + 1, ty));
                        if (ty > 0)
                            q.Enqueue((tx, ty - 1));
                        if (ty < height - 1)
                            q.Enqueue((tx, ty + 1));
                    }
                    else
                        area += 1;
                }
                bubbles.Add((x, y, area));

                foreach (var (tx, ty) in already)
                    field[ty][tx] = ' ';
            }

            file = File.ReadAllLines(@"D:\temp\galactic\Research Center.txt");
            foreach (var (x, y, size) in bubbles)
            {
                var sizeStr = $"[{size}]";
                file[y] = file[y].Substring(0, x) + sizeStr + file[y].Substring(x + sizeStr.Length);
            }
            File.WriteAllLines(@"D:\temp\galactic\Research Center sizes.txt", file);
        }
    }
}
