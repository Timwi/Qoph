using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff.BombDisposal
{
    static class Kurioza
    {
        public static void Generate()
        {
            var consonantCode = new Dictionary<char, (string lines, int fDx, int fDy, string filled)>
            {
                ['b'] = (@"", 0, 0, @"h1v1h-1z"),
                ['c'] = (@"", 0, 0, @"h1v1h-1z m 1,1 h1v1h-1z"),
                ['ĉ'] = (@"", 1, 0, @"h1v1h-1z m -1,1 h1v1h-1z"),
                ['d'] = (@"", 0, 1, @"h2v1h-2z"),
                ['f'] = (@"m 0,1 h2", 0, 0, @""),
                ['g'] = (@"m 0,1 h2 m -1,0 v-1", 0, 0, @""),
                ['ĝ'] = (@"m 0,1 h2 m -1,0 v1", 0, 0, @""),
                ['h'] = (@"m 1,0 v2 m 0,-1 h-1", 0, 0, @""),
                ['ĥ'] = (@"m 1,0 v2 m 0,-1 h1", 0, 0, @""),
                ['j'] = (@"", 0, 2, @"v-2h2z"),
                ['ĵ'] = (@"", 2, 0, @"v2h-2z"),
                ['k'] = (@"", 0, 1, @"h1v1h-1z"),
                ['l'] = (@"", 0, 0, @"h1v2h-1z"),
                ['m'] = (@"m 1,0 v2", 0, 0, @""),
                ['n'] = (@"", 0, 0, @""),
                ['p'] = (@"", 1, 0, @"h1v1h-1z"),
                ['r'] = (@"", 1, 0, @"h1v2h-1z"),
                ['s'] = (@"l 2,2", 0, 0, @""),
                ['ŝ'] = (@"m 2,0 l -2,2", 0, 0, @""),
                ['t'] = (@"m 1,0 v2 m -1,-1 h1", 0, 0, @""),
                ['ŭ'] = (@"", 0, 0, @"h2v1h-2z"),
                ['v'] = (@"", 1, 1, @"h1v1h-1z"),
                ['z'] = (@"l 2,2 m 0,-2 l -2,2", 0, 0, @"")
            };
            var vowelCode = new Dictionary<char, (int pDx, int pDy, string lines, int vDx, int vDy, int cDx, int cDy)>
            {
                ['a'] = (-1, 0, "v-2", 0, -2, -1, -2),
                ['e'] = (0, 0, "l 2,-2", 2, -2, 0, -2),
                ['i'] = (0, 1, "h2", 2, 0, 0, -1),
                ['o'] = (0, 2, "l 2,2", 2, 2, 0, 0),
                ['u'] = (-1, 2, "v2", 0, 2, -1, 0)
            };

            var data = @"valve	klapo	clap	aplaŭdo	1
gooseberry	groso	gross	groco	2
bread	pano	pan	pato	2
adultery	adulto	adult	plenkreskulo	5
house	domo	dome	kupolo	2
dragonfly	libelo	libel	kalumnio	1
employment	dungo	dung	sterko	1
cricket	grilo	grill	krado	2
estimate	taksi	tax	imposti	4
verify	kontroli	control	regi	1
absence	foresto	forest	arbaro	1
factory	fabriko	fabric	teksaĵo	4
ask	demandi	demand	postuli	1
puddle	flako	flake	floko	3
effort	peno	pen	skribilo	1
rapier	spado	spade	fosilo	3
order	mendi	mend	ripari	2
woodpecker	pego	peg	kejlo	2
present	nuno	nun	monakino	1
remain	resti	rest	ripozi	1
arrow	sago	sage	salvio	1
lentil	lento	lent	karesmo	3
soap	sapo	sap	suko	2
temptation	tento	tent	tendo	3
wolverine	gulo	gull	mevo	2
magpie	pigo	pig	porko	2
hand	mano	man	viro	3
string	kordo	cord	ŝnuro	6
lynx	linko	link	ligilo	1
queen	damo	dam	baraĵo	3
mastiff	dogo	dog	hundo	3
stew	stufi	stuff	farĉi	3
thistle	kardo	card	karto	4".Replace("\r", "").Split('\n').Select(line => line.Split('\t')).Select(arr => (eoLeft: arr[1], enLeft: arr[2], eoRight: arr[3], enRight: arr[0], index: int.Parse(arr[4]))).ToArray();

            (string svg, int dx, int dy) renderWord(string word)
            {
                var svgLines = new StringBuilder();
                var svgFilled = new StringBuilder();
                int cx = 0, cy = 0, vx = 0, vy = 0;
                bool lastWasVowel = false;
                foreach (var ch in word)
                {
                    if (vowelCode.ContainsKey(ch))
                    {
                        var (pDx, pDy, lines, vDx, vDy, cDx, cDy) = vowelCode[ch];
                        if (!lastWasVowel)
                        {
                            vx = cx + pDx;
                            vy = cy + pDy;
                            svgLines.Append($"M {vx},{vy}");
                        }
                        vx += vDx;
                        vy += vDy;
                        cx = vx + cDx;
                        cy = vy + cDy;
                        svgLines.Append(lines);
                        lastWasVowel = true;
                    }
                    else if (consonantCode.ContainsKey(ch))
                    {
                        var (lines, fDx, fDy, filled) = consonantCode[ch];
                        svgLines.Append($"M {cx} {cy} h2v2h-2z {lines}");
                        svgFilled.Append($"M {cx + fDx} {cy + fDy} {filled}");
                        cx += 2;
                        lastWasVowel = false;
                    }
                    else
                        Debugger.Break();
                }
                return ($"<path d='{svgLines}' fill='none' stroke='black' stroke-width='.1' /><path d='{svgFilled}' />", vx, vy);
            }

            var allSvg = new StringBuilder();

            // Left side
            for (var i = 0; i < data.Length; i++)
            {
                var (eoLeft, enLeft, eoRight, enRight, index) = data[i];
                var (svg, dx, dy) = renderWord(eoLeft);
                allSvg.Append($"<g transform='translate({100 - dx} {7 * i - dy})'>{svg}<circle cx='{dx}' cy='{dy}' r='.5' /></g>");
            }

            // Right side
            var sortedData = data.OrderBy(d => d.enRight).ToArray();
            for (var i = 0; i < data.Length; i++)
            {
                var (eoLeft, enLeft, eoRight, enRight, index) = sortedData[i];
                var (svg, dx, dy) = renderWord(eoRight);
                var (dx2, dy2) = "aeiou".Contains(eoRight[0]) ? (2, 0) : (1, -1);
                allSvg.Append($"<g transform='translate({200 + dx2} {7 * i + dy2})'>{svg}<circle cx='{-dx2}' cy='{-dy2}' r='.5' /></g>");
            }

            // Solution lines
            for (var i = 0; i < data.Length; i++)
            {
                var otherIx = sortedData.IndexOf(data[i]);
                allSvg.Append($"<path fill='none' stroke='black' stroke-width='.1' d='M 100 {7 * i} L 200 {7 * otherIx}' />");
            }

            File.WriteAllText(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Kurioza\Kurioza.svg", $@"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 -10 300 240'>{allSvg}</svg>");
        }
    }
}