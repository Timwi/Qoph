using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace Qoph
{
    static class OneCanHope
    {
        private static readonly Dictionary<char, (string lines, int fDx, int fDy, string filled)> consonantCode = new Dictionary<char, (string lines, int fDx, int fDy, string filled)>
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
        private static readonly Dictionary<char, (int pDx, int pDy, string lines, int vDx, int vDy, int cDx, int cDy)> vowelCode = new Dictionary<char, (int pDx, int pDy, string lines, int vDx, int vDy, int cDx, int cDy)>
        {
            ['a'] = (-1, 0, "v-1", 0, -1, -1, -2),
            ['e'] = (0, 0, "l 1,-1", 1, -1, 0, -2),
            ['i'] = (0, 1, "h1", 1, 0, 0, -1),
            ['o'] = (0, 2, "l 1,1", 1, 1, 0, 0),
            ['u'] = (-1, 2, "v1", 0, 1, -1, 0)
        };

        private static (string svg, int dx, int dy, (int x, int y)[] cornerPoints) renderWord(string word)
        {
            var svgLines = new StringBuilder();
            var svgFilled = new StringBuilder();
            var cornerPoints = new List<(int x, int y)> { (0, 0) };
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
                    cornerPoints.Add((cx, cy));
                    cornerPoints.Add((cx + 2, cy));
                    cornerPoints.Add((cx, cy + 2));
                    cornerPoints.Add((cx + 2, cy + 2));
                    cx += 2;
                    lastWasVowel = false;
                }
                else
                    Debugger.Break();
            }
            cornerPoints.Add((cx, cy));
            return ($"<path d='{svgLines}' fill='none' stroke='black' stroke-width='.1' /><path d='{svgFilled}' />", vx, vy, cornerPoints.ToArray());
        }

        private static readonly (string eoLeft, string enLeft, string eoRight, string enRight, int index)[] data = @"valve	klapo	clap	aplaŭdo	1
gooseberry	groso	gross	groco	2
bread	pano	pan	pato	2
wait	atendi	attend	ĉeesti	4
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

        public static void Generate_OLD()
        {
            const double leftX = 100;
            const double rightX = 200;

            var allSvg = new StringBuilder();

            // Left side
            var (y, prevDx, prevDy) = (15, 0, 0);
            (int dx, int dy)[] prevCs = null;
            var leftYs = new int[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                var prevY = y;
                y += 2;
                var (eoLeft, enLeft, eoRight, enRight, index) = data[i];
                var (svg, dx, dy, cs) = renderWord(eoLeft);
                if (i > 0)
                    foreach (var (cdx, cdy) in cs)
                        foreach (var (pdx, pdy) in prevCs)
                            if (Math.Abs((pdx - prevDx) - (cdx - dx)) <= 1)
                                if (y - dy + cdy < prevY - prevDy + pdy + 3)
                                    y = prevY - prevDy + pdy + 3 + dy - cdy;
                leftYs[i] = y;
                allSvg.Append($"<g transform='translate({leftX - dx - 1} {y - dy})'>{svg}<circle cx='{dx + 1}' cy='{dy}' r='.5' fill='white' stroke='black' stroke-width='.1' /></g>");
                prevDx = dx;
                prevDy = dy;
                prevCs = cs;
            }

            // Right side
            var sortedData = data.OrderBy(d => d.enRight).ToArray();
            var rightYs = new int[data.Length];
            y = -2;
            for (var i = 0; i < data.Length; i++)
            {
                var prevY = y;
                y += 2;
                var (eoLeft, enLeft, eoRight, enRight, index) = sortedData[i];
                var (svg, dx, dy, cs) = renderWord(eoRight);
                var (dx2, dy2) = eoRight[0] == 'a' ? (2, 0) : (1, -1);
                cs = cs.Select(tup => (tup.x + dx2, tup.y + dy2)).ToArray();
                if (i > 0)
                    foreach (var (cdx, cdy) in cs)
                        foreach (var (pdx, pdy) in prevCs)
                            if (Math.Abs(pdx - cdx) <= 1)
                                if (y + cdy < prevY + pdy + 3)
                                    y = prevY + pdy + 3 - cdy;
                rightYs[i] = y;
                allSvg.Append($"<g transform='translate({rightX + dx2} {y + dy2})'>{svg}<circle cx='{-dx2}' cy='{-dy2}' r='.5' fill='white' stroke='black' stroke-width='.1' /></g>");
                prevDx = dx;
                prevDy = dy;
                prevCs = cs;
            }

            // Find the solution lines
            var lines = new List<(int leftY, int rightY, int needsPoints)>();
            for (var i = 0; i < data.Length; i++)
            {
                var otherIx = sortedData.IndexOf(data[i]);

                // SOLUTION
                allSvg.Append($"<path fill='none' stroke='hsl({60 * (data[i].index - 1)}, 50%, 50%)' stroke-width='.2' d='M {leftX} {leftYs[i]} L {rightX} {rightYs[otherIx]}' />");

                lines.Add((leftYs[i], rightYs[otherIx], data[i].index));
            }

            // Find all points of intersection
            var intersections = new List<(PointD pt, int line1, int line2)>();
            for (var l1Ix = 0; l1Ix < lines.Count; l1Ix++)
                for (var l2Ix = l1Ix + 1; l2Ix < lines.Count; l2Ix++)
                {
                    var edge1 = new EdgeD(leftX, lines[l1Ix].leftY, rightX, lines[l1Ix].rightY);
                    var edge2 = new EdgeD(leftX, lines[l2Ix].leftY, rightX, lines[l2Ix].rightY);
                    Intersect.LineWithLine(ref edge1, ref edge2, out double l1l, out double l2l);
                    if (!double.IsNaN(l1l) && l1l > 0 && l1l < 1 && l2l > 0 && l2l < 1)
                        intersections.Add((edge1.Start + l1l * (edge1.End - edge1.Start), l1Ix, l2Ix));
                }

            var groupedIntersections = new Dictionary<PointD, List<(int line1, int line2)>>();
            foreach (var (pt, line1, line2) in intersections)
            {
                var key = groupedIntersections.Keys.FirstOrNull(p => p.Distance(pt) < .0001);
                if (key == null)
                    groupedIntersections[pt] = new List<(int line1, int line2)> { (line1, line2) };
                else
                    groupedIntersections[key.Value].Add((line1, line2));
            }

            // Filter out points of intersection that are too close to other points of intersection
            var keysToRemove = groupedIntersections.Keys.UniquePairs().Where(tup => tup.Item1.Distance(tup.Item2) < 3).SelectMany(tup => new[] { tup.Item1, tup.Item2 }).ToArray();
            foreach (var key in keysToRemove)
                groupedIntersections.Remove(key);

            var pointsSelected = lines.Select(_ => new List<PointD>()).ToArray();
            //foreach (var grInter in groupedIntersections.ToArray().Shuffle().OrderByDescending(kvp => kvp.Value.Count))
            //{
            //    var linesAffected = grInter.Value.SelectMany(tup => new[] { tup.line1, tup.line2 }).Distinct().ToArray();
            //    if (linesAffected.Any(l => pointsSelected[l].Count == lines[l].needsPoints))
            //        continue;
            //    foreach (var lineIx in linesAffected)
            //        pointsSelected[lineIx].Add(grInter.Key);
            //}

            var rnd = new Random(47);
            foreach (var lineIx in Enumerable.Range(0, lines.Count).ToArray().Shuffle(rnd).OrderByDescending(ix => lines[ix].needsPoints))
                foreach (var grInter in groupedIntersections.Where(gi => gi.Value.Any(tup => tup.line1 == lineIx || tup.line2 == lineIx)).ToArray().Shuffle(rnd).OrderByDescending(kvp => kvp.Value.Count))
                {
                    var linesAffected = grInter.Value.SelectMany(tup => new[] { tup.line1, tup.line2 }).Distinct().ToArray();
                    if (linesAffected.Any(l => pointsSelected[l].Count == lines[l].needsPoints))
                        continue;
                    foreach (var lineIx2 in linesAffected)
                        pointsSelected[lineIx2].Add(grInter.Key);
                    groupedIntersections.Remove(grInter.Key);
                }
            for (var lineIx = 0; lineIx < lines.Count; lineIx++)
                if (pointsSelected[lineIx].Count < lines[lineIx].needsPoints)
                {
                    var numPointsMissing = lines[lineIx].needsPoints - pointsSelected[lineIx].Count;
                    var (pt1, pt2) = intersections.Where(inter => inter.line1 == lineIx || inter.line2 == lineIx).Select(inter => inter.pt).Concat(new[] { new PointD(leftX, lines[lineIx].leftY), new PointD(rightX, lines[lineIx].rightY) }).OrderBy(pt => pt.X).ConsecutivePairs(closed: false).MaxElement(pair => pair.Item2.Distance(pair.Item1));
                    for (var i = 0; i < numPointsMissing; i++)
                        pointsSelected[lineIx].Add(pt1 + (pt2 - pt1) / (numPointsMissing + 1) * (i + 1));
                }

            var allPoints = new List<PointD>(pointsSelected.SelectMany(l => l).Distinct());
            var density = 100;
            const double decoyDistance = 5;
            var candidateDecoys = Enumerable.Range(1, density - 1).SelectMany(x => Enumerable.Range(0, density + 1).Select(y =>
            {
                var topY = (rightYs.First() - leftYs.First()) * (double) x / density + leftYs.First();
                var bottomY = (rightYs.Last() - leftYs.Last()) * (double) x / density + leftYs.Last();
                return new PointD((rightX - leftX) * x / density + leftX, (bottomY - topY) * y / density + topY);
            }))
                .Where(pt => allPoints.All(p2 => p2.Distance(pt) > decoyDistance) && lines.All(l => new EdgeD(leftX, l.leftY, rightX, l.rightY).Distance(pt) > decoyDistance))
                .ToList();
            while (candidateDecoys.Count > 0)
            {
                var decoy = candidateDecoys.PickRandom(rnd);
                allPoints.Add(decoy);
                candidateDecoys.RemoveAll(p => p.Distance(decoy) <= decoyDistance);
            }

            foreach (var pt in allPoints)
                allSvg.Append($"<circle cx='{pt.X}' cy='{pt.Y}' r='.75' />");

            File.WriteAllText(@"D:\c\Qoph\DataFiles\One Can Hope\One Can Hope.svg", $@"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 -10 300 240'>{allSvg}</svg>");
        }

        public static void Generate()
        {
            var htmlCodeTop = new StringBuilder();
            var htmlCodeBottom = new StringBuilder();

            static (int x, int y, int width, int height, string svg) getRect(string word)
            {
                var (svg, dx, dy, points) = renderWord(word);
                var minX = points.Min(p => p.x);
                var minY = points.Min(p => p.y);
                var maxX = points.Max(p => p.x);
                var maxY = points.Max(p => p.y);
                return (minX, minY, maxX - minX, maxY - minY, svg);
            }
            var maxWidth = data.Max(d => Math.Max(getRect(d.eoLeft).width, getRect(d.eoRight).width));
            var maxHeight = data.Max(d => Math.Max(getRect(d.eoLeft).height, getRect(d.eoRight).height));
            var padding = .5d;

            foreach (var (eoLeft, enLeft, _, _, _) in data)
            {
                var (x, y, width, height, svg) = getRect(eoLeft);
                htmlCodeTop.Append($"<div class='box'><svg viewBox='{x + width / 2d - maxWidth / 2d - padding} {y + height - maxHeight - padding} {maxWidth + 2 * padding} {maxHeight + 2 * padding}'>{svg}</svg><div>{enLeft.ToUpperInvariant()}</div></div>");
            }
            foreach (var (_, _, eoRight, enRight, index) in data.OrderBy(d => d.enRight))
            {
                var (x, y, width, height, svg) = getRect(eoRight);
                htmlCodeBottom.Append($"<div class='box'><svg viewBox='{x + width / 2d - maxWidth / 2d - padding} {y + height - maxHeight - padding} {maxWidth + 2 * padding} {maxHeight + 2 * padding}'>{svg}</svg><div>{enRight.Select(ch => "_ ").JoinString().Trim()} ({index})</div></div>");
            }

            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\One Can Hope\One Can Hope.html", "<!--%%top-->", "<!--%%%top-->", htmlCodeTop.ToString());
            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\One Can Hope\One Can Hope.html", "<!--%%bottom-->", "<!--%%%bottom-->", htmlCodeBottom.ToString());
        }
    }
}