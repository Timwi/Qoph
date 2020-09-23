using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace Qoph
{
    static class Comebacks
    {
        static readonly string[][] _insults = Ut.NewArray(
            Ut.NewArray(
                "Have you stopped wearing diapers yet?",
                "I’m not going to take your insolence sitting down!",
                "You make me want to puke.",
                "People fall at my feet when they see me coming.",
                "You have the manners of a beggar.",
                "You fight like a dairy farmer.",
                "I once owned a dog that was smarter than you.",
                "My handkerchief will wipe up your blood!",
                "Nobody’s ever drawn blood from me and nobody ever will!",
                "There are no words for how disgusting you are.",
                "Soon you’ll be wearing my sword like a shish kebab!",
                "You’re no match for my brains, you poor fool.",
                "I got this scar on my face during a mighty struggle!",
                "This is the END for you, you gutter-crawling cur!",
                "I’ve spoken with apes more polite than you.",
                "I’ve heard you are a contemptible sneak."),
            Ut.NewArray(
                "I hope you have a boat ready for a quick escape.",
                "You are a pain in the backside, sir!",
                "If your brother’s like you, better to marry a pig.",
                "My wisest enemies run away at the first sight of me!",
                "Every word you say to me is stupid.",
                "I will milk every drop of blood from your body!",
                "Only once have I met such a coward!",
                "My name is feared in every dirty corner of this island!",
                "No one will ever catch ME fighting as badly as you do.",
                "There are no clever moves that can help you now.",
                "My tongue is sharper than any sword.",
                "I’ve got the courage and skill of a master swordsman!",
                "My last fight ended with my hands covered with blood.",
                "I’ve got a long, sharp lesson for you to learn today.",
                "Now I know what filth and stupidity really are.",
                "My sword is famous all over the Caribbean!"));

        static readonly string[] _comebacks = Ut.NewArray(
            "Why, did you want to borrow one?",
            "Your hemorrhoids are flaring up again, eh?",
            "You make me think somebody already did.",
            "Even BEFORE they smell your breath?",
            "I wanted to make sure you’d feel comfortable with me.",
            "How appropriate. You fight like a cow.",
            "He must have taught you everything you know.",
            "So you got that job as janitor, after all.",
            "You run THAT fast?",
            "Yes, there are. You just never learned them.",
            "First you’d better stop waving it like a feather-duster.",
            "I’d be in real trouble if you ever used them.",
            "I hope now you’ve learned to stop picking your nose.",
            "And I’ve got a little TIP for you. Get the POINT?",
            "I’m glad to hear you attended your family reunion.",
            "Too bad no one’s ever heard of YOU at all.");

        static readonly string[] _instructions = Ut.NewArray(
            "FIFTH WORD, FIRST LETTER",
            "FOURTH WORD, LAST LETTER",
            "FOURTEENTH LETTER",
            "TWENTIETH LETTER",
            "LETTER AFTER V",
            "SEVENTH LETTER, ATBASH CIPHERED",
            "LETTER BEFORE THE FIRST A, ATBASH CIPHERED",
            "LETTER AFTER LAST Y",
            "FIFTH WORD, SECOND LETTER",
            "TWENTY-SECOND LETTER",
            "FIFTH WORD, FIRST LETTER",
            "NINTH LETTER",
            "THIRD WORD, SECOND LETTER, ATBASH CIPHERED",
            "FIRST LETTER",
            "LETTER AFTER K",
            "LETTER BEFORE H, ROT THIRTEEN");

        public static void FindInstructions()
        {
            var clues = new[] { "DOTHESWORDMASTER", "ANSWERSCATTERING" };
            if (clues[0].Length != clues[1].Length)
                Debugger.Break();

            var usable = Ut.NewArray<bool>(16, 16);
            var sb = new StringBuilder();
            for (var clueIx = 0; clueIx < clues[0].Length; clueIx++)
            {
                var letters = clues.Select(c => c[clueIx]).ToArray();
                var numPhr = _insults[0].Length;
                if (_insults[1].Length != numPhr)
                    Debugger.Break();

                var possibleInstrs = new List<string>();
                var phrIx = clueIx;
                //for (var phrIx = 0; phrIx < numPhr; phrIx++)
                {
                    IEnumerable<(string instruction, char ch1, char ch2)> extract()
                    {
                        var words = _insults.Select(phrs => phrs[phrIx].Split(' ').Select(w => Regex.Replace(w.ToUpperInvariant(), "[^A-Z]", "")).ToArray()).ToArray();
                        if (_insults.All(phrs => !phrs[phrIx].Contains('-')))
                        {
                            for (var wordIx = 0; wordIx < words.Min(ws => ws.Length); wordIx++)
                            {
                                string w1 = words[0][wordIx], w2 = words[1][wordIx];
                                for (var ltrIx = 0; ltrIx < words.Min(ws => ws[wordIx].Length); ltrIx++)
                                    yield return ($"word {wordIx + 1} letter {ltrIx + 1}", w1[ltrIx], w2[ltrIx]);
                                yield return ($"word {wordIx + 1} last letter", w1.Last(), w2.Last());
                                if (w1.Length % 2 == 1 && w2.Length % 2 == 1)
                                    yield return ($"word {wordIx + 1} middle letter", w1[w1.Length / 2], w2[w2.Length / 2]);
                            }
                            string lw1 = words[0].Last(), lw2 = words[1].Last();
                            for (var ltrIx = 0; ltrIx < words.Min(ws => ws.Last().Length); ltrIx++)
                                yield return ($"last word letter {ltrIx + 1}", lw1[ltrIx], lw2[ltrIx]);
                            if (lw1.Length % 2 == 1 && lw2.Length % 2 == 1)
                                yield return ($"last word middle letter", lw1[lw1.Length / 2], lw2[lw2.Length / 2]);
                        }

                        var fullPhrase = _insults.Select(phr => Regex.Replace(phr[phrIx].ToUpperInvariant(), "[^A-Z]", "")).ToArray();
                        for (var ltrIx = 0; ltrIx < fullPhrase.Min(f => f.Length); ltrIx++)
                            yield return ($"letter {ltrIx + 1}", fullPhrase[0][ltrIx], fullPhrase[1][ltrIx]);
                        if (fullPhrase[0].Length % 2 == 1 && fullPhrase[1].Length % 2 == 1)
                            yield return ($"middle letter", fullPhrase[0][fullPhrase[0].Length / 2], fullPhrase[1][fullPhrase[1].Length / 2]);

                        var consonants = fullPhrase.Select(fp => fp.IndexOf('Y').Apply(yPos => yPos == -1 ? fp : fp.Remove(yPos)).Where(c => "BCDFGHJKLMNPQRSTVWXZ".Contains(c)).JoinString()).ToArray();
                        for (var consonantIx = 0; consonantIx < consonants.Min(f => f.Length); consonantIx++)
                            yield return ($"consonant {consonantIx + 1}", consonants[0][consonantIx], consonants[1][consonantIx]);
                        var lastConsonants = fullPhrase.Select(fp => fp.LastIndexOf(ch => "BCDFGHJKLMNPQRSTVWXYZ".Contains(ch)).Apply(cPos => fp[cPos] == 'Y' ? -1 : cPos)).ToArray();
                        if (!lastConsonants.Contains(-1))
                            yield return ($"last consonant", fullPhrase[0][lastConsonants[0]], fullPhrase[1][lastConsonants[1]]);

                        var vowels = fullPhrase.Select(fp => fp.IndexOf('Y').Apply(yPos => yPos == -1 ? fp : fp.Remove(yPos)).Where(c => "AEIOU".Contains(c)).JoinString()).ToArray();
                        for (var vowelIx = 0; vowelIx < vowels.Min(f => f.Length); vowelIx++)
                            yield return ($"vowel {vowelIx + 1}", vowels[0][vowelIx], vowels[1][vowelIx]);
                        var lastVowels = fullPhrase.Select(fp => fp.LastIndexOf(ch => "AEIOUY".Contains(ch)).Apply(cPos => fp[cPos] == 'Y' ? -1 : cPos)).ToArray();
                        if (!lastVowels.Contains(-1))
                            yield return ($"last vowel", fullPhrase[0][lastVowels[0]], fullPhrase[1][lastVowels[1]]);

                        for (var ltr = 'A'; ltr <= 'Z'; ltr++)
                        {
                            var ps = fullPhrase.Select(phr => phr.IndexOf(ltr)).ToArray();
                            if (ps.Select((p, i) => p != -1 && p < fullPhrase[i].Length - 1).All(b => b))
                                yield return ($"letter after first {ltr}", fullPhrase[0][ps[0] + 1], fullPhrase[1][ps[1] + 1]);
                            if (ps.Select((p, i) => p != -1 && p > 0).All(b => b))
                                yield return ($"letter before first {ltr}", fullPhrase[0][ps[0] - 1], fullPhrase[1][ps[1] - 1]);

                            ps = fullPhrase.Select(phr => phr.LastIndexOf(ltr)).ToArray();
                            if (ps.Select((p, i) => p != -1 && p < fullPhrase[i].Length - 1).All(b => b))
                                yield return ($"letter after last {ltr}", fullPhrase[0][ps[0] + 1], fullPhrase[1][ps[1] + 1]);
                            if (ps.Select((p, i) => p != -1 && p > 0).All(b => b))
                                yield return ($"letter before last {ltr}", fullPhrase[0][ps[0] - 1], fullPhrase[1][ps[1] - 1]);

                            if (fullPhrase.All(phr => phr.Count(ch => ch == ltr) == 3))
                            {
                                ps = fullPhrase.Select(phr => phr.SelectIndexWhere(ch => ch == ltr).Skip(1).First()).ToArray();
                                if (ps.Select((p, i) => p != -1 && p < fullPhrase[i].Length - 1).All(b => b))
                                    yield return ($"letter after middle {ltr}", fullPhrase[0][ps[0] + 1], fullPhrase[1][ps[1] + 1]);
                                if (ps.Select((p, i) => p != -1 && p > 0).All(b => b))
                                    yield return ($"letter before middle {ltr}", fullPhrase[0][ps[0] - 1], fullPhrase[1][ps[1] - 1]);
                            }
                        }
                    }

                    static char atbash(char letter) => (char) ('Z' - (letter - 'A'));
                    static char rot13(char letter) => (char) ((letter - 'A' + 13) % 26 + 'A');

                    var results = extract().ToList();
                    foreach (var (instruction, ch1, ch2) in results)
                    {
                        if (ch1 == letters[0] && ch2 == letters[1])
                            possibleInstrs.Add(instruction);
                        if (atbash(ch1) == letters[0] && atbash(ch2) == letters[1])
                            possibleInstrs.Add($"{instruction} atbash");
                        if (rot13(ch1) == letters[0] && rot13(ch2) == letters[1])
                            possibleInstrs.Add($"{instruction} rot13");
                    }
                }

                Console.WriteLine();
                Console.WriteLine(_insults.Select(phrs => phrs[phrIx]).JoinString("\n"));
                foreach (var instr in possibleInstrs)
                    Console.WriteLine($"    — {letters.JoinString("/")} = {instr}");
                sb.AppendLine($@"""{possibleInstrs.JoinString("\n")}""");
            }
            Clipboard.SetText(sb.ToString());
        }

        public static void GenerateDropquotes()
        {
            if (_insults[0].Length != 16 || _insults[1].Length != 16 || _comebacks.Length != 16 || _instructions.Length != 16)
                Debugger.Break();

            // Alphabetize them!
            var comebacks = _comebacks.ToArray();
            var instructions = _instructions.ToArray();
            Array.Sort(comebacks, instructions);

            const int w = 16;
            var allSvgs = new StringBuilder();
            var numBlackSquares = 0;
            var clipb = new List<string>();
            for (var i = 0; i < 16; i++)
            {
                var topChars = Ut.NewArray(w, _ => "");
                var blackSquares = new List<(int x, int y)>();
                var punctuation = new List<(int x, int y, char ch)>();

                var x = 0;
                var y = 0;
                var fullPhrase = $"{_insults[0][i]}  {comebacks[i]}  {instructions[i]}".ToUpperInvariant();
                for (var c = 0; c < fullPhrase.Length; c++)
                {
                    if (x >= w)
                    {
                        y++;
                        x -= w;
                    }

                    if (fullPhrase[c] == ' ')
                    {
                        blackSquares.Add((x, y));
                        x++;
                    }
                    else if (fullPhrase[c] >= 'A' && fullPhrase[c] <= 'Z')
                    {
                        topChars[x] += fullPhrase[c];
                        x++;
                    }
                    else
                    {
                        punctuation.Add((x, y, fullPhrase[c]));
                        x++;
                    }
                }
                for (; x < w; x++)
                    blackSquares.Add((x, y));

                var topRows = topChars.Max(c => c.Length);
                var totalRows = topRows + y + 1;
                var clip = Ut.NewArray<string>(totalRows, w);

                var svg = new StringBuilder();
                // Top letters
                for (var xx = 0; xx < w; xx++)
                {
                    var sortedChars = topChars[xx].Order().ToArray();
                    for (var yy = 0; yy < sortedChars.Length; yy++)
                    {
                        svg.Append($@"<text x='{xx + .5}' y='{topRows - sortedChars.Length + yy + .75}'>{sortedChars[yy]}</text>");
                        clip[topRows - sortedChars.Length + yy][xx] = sortedChars[yy].ToString();
                    }
                }
                // Frame
                svg.Append($@"<rect fill='white' stroke='black' stroke-width='.1' x='0' y='{topRows}' width='{w}' height='{y + 1}' />");
                // Black squares
                foreach (var (xx, yy) in blackSquares)
                {
                    svg.Append($@"<rect x='{xx}' y='{yy + topRows}' width='1' height='1' />");
                    clip[yy + topRows][xx] = "#";
                }

                // Horizontal lines
                for (var yy = topRows + 1; yy < totalRows; yy++)
                    svg.Append($@"<line x1='0' x2='{w}' y1='{yy}' y2='{yy}' stroke='black' stroke-width='.025' />");
                // Vertical lines (top)
                for (var xx = 1; xx < w; xx++)
                    svg.Append($@"<line x1='{xx}' x2='{xx}' y1='0' y2='{topRows}' stroke='black' stroke-width='.025' stroke-dasharray='0.025,0.075' />");
                // Vertical lines (bottom)
                for (var xx = 1; xx < w; xx++)
                    svg.Append($@"<line x1='{xx}' x2='{xx}' y1='{topRows}' y2='{totalRows}' stroke='black' stroke-width='.025' />");

                // Punctuation
                foreach (var (xx, yy, ch) in punctuation)
                {
                    svg.Append($@"<text x='{xx + .5}' y='{yy + topRows + .8}'>{ch}</text>");
                    clip[yy + topRows][xx] = ch.ToString();
                }

                allSvgs.Append($@"<svg viewBox='-.2 -.2 {w + .4} {totalRows + .4}' text-anchor='middle' font-size='.8'>{svg}</svg>");
                numBlackSquares += blackSquares.Count;
                //Console.WriteLine($"Dropquote {i + 1} = {totalRows} rows");
                clipb.Add(clip.Select(row => row.JoinString("\t")).JoinString("\n"));
                //clipb.AppendLine(fullPhrase.Replace("  ", "\t"));
            }
            Clipboard.SetText(clipb.JoinString("\n\n\n"));

            var path = $@"D:\c\Qoph\EnigmorionFiles\comebacks.html";
            General.ReplaceInFile(path, @"<!--%%-->", @"<!--%%%-->", allSvgs.ToString());
        }
    }
}