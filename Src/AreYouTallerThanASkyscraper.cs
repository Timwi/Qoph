using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PuzzleSolvers;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace Qoph
{
    static class AreYouTallerThanASkyscraper
    {
        public static void FindArrangement()
        {
            var topRights = new[] { 0, 1, 1, 2 };
            var topLefts = new[] { 3, 0, 2, 3 };
            foreach (var latinSquare in new LatinSquare(4, 0).Solve())
            {
                var topRightMatches = topRights.Select((other, num) => Enumerable.Range(4, 12).Where(ix => (ix % 4 != 3) && latinSquare[ix] == num && latinSquare[ix - 3] == other).ToArray()).ToArray();
                var topLeftMatches = topLefts.Select((other, num) => Enumerable.Range(4, 12).Where(ix => (ix % 4 != 0) && latinSquare[ix] == num && latinSquare[ix - 5] == other).ToArray()).ToArray();

                for (var i = 0; i < 4; i++)
                    if (topRightMatches[i].Length == 0 || topLeftMatches[i].Length == 0 || (topRightMatches[i].Length == 1 && topLeftMatches[i].Length == 1 && topRightMatches[i][0] == topLeftMatches[i][0]))
                        goto busted;

                Console.WriteLine(latinSquare.Split(4).Select(row => row.JoinString(" ")).JoinString("\n"));
                Console.WriteLine();

                busted:;
            }
        }

        public static void Generate()
        {
            // .    :    /     #
            // 1-4  5-8  9-12  13-16
            var structure = @"
////::::####....
////::::####....
////::::####....
////0:::###2....
::::....////####
::::....////####
::::....////####
::::C..02//1####
....####::::////
....####::::////
....####::::////
....###O1:::////
####////....::::
####////....::::
####////....::::
####////....::::".Trim().Replace("\r", "").Replace("\n", "");

            var sums = new[] { 'I', 'N', 'Y' }.Select(ch => ch - 'A' + 1).ToArray();

            var valid4x4s = getValid4x4SkyscraperPuzzles().ToArray();

            var puzzle = new LatinSquare(16, 1);
            var chs = ".:/#";
            // Each cell must be in range
            foreach (var ix in Enumerable.Range(0, structure.Length))
            {
                var pos = chs.IndexOf(structure[ix]);
                if (pos != -1)
                    puzzle.AddConstraint(new OneCellLambdaConstraint(ix, val => val > 4 * pos && val <= 4 * (pos + 1)));
                else if (structure[ix] >= 'A' && structure[ix] <= 'Z')
                    puzzle.AddConstraint(new OneCellLambdaConstraint(ix, val => val == structure[ix] - 'A' + 1));
            }
            // Cells that must sum
            for (var sumIx = 0; sumIx < sums.Length; sumIx++)
                puzzle.AddConstraint(new SumConstraint(sums[sumIx], structure.SelectIndexWhere(ch => ch - '0' == sumIx)));
            // Each 4×4 must be a valid Skyscraper subpuzzle
            for (var ssq = 0; ssq < 16; ssq++)
            {
                var tl = 16 * 4 * (ssq / 4) + 4 * (ssq % 4);
                var ch = chs.Contains(structure[tl]) ? structure[tl] : structure[tl + 1];
                var additive = chs.IndexOf(ch) * 4;
                puzzle.AddConstraint(new CombinationsConstraint(Enumerable.Range(0, 16).Select(ix => 16 * 4 * (ssq / 4) + 4 * (ssq % 4) + 16 * (ix / 4) + ix % 4), valid4x4s.Select(comb => comb.Select(v => v + additive).ToArray()).ToArray()));
            }

            var rnd = new Random(47);
            var randomSolution = puzzle.Solve(new SolverInstructions { Randomizer = rnd }).First();

            var topColumnClues = Enumerable.Range(0, 16).Select(col => getSkyscraperClue(Enumerable.Range(0, 16).Select(y => randomSolution[y * 16 + col]))).ToArray();
            var bottomColumnClues = Enumerable.Range(0, 16).Select(col => getSkyscraperClue(Enumerable.Range(0, 16).Select(y => randomSolution[y * 16 + col]).Reverse())).ToArray();
            var leftRowClues = Enumerable.Range(0, 16).Select(row => getSkyscraperClue(Enumerable.Range(0, 16).Select(x => randomSolution[row * 16 + x]))).ToArray();
            var rightRowClues = Enumerable.Range(0, 16).Select(row => getSkyscraperClue(Enumerable.Range(0, 16).Select(x => randomSolution[row * 16 + x]).Reverse())).ToArray();

            var tt = new TextTable { ColumnSpacing = 1 };
            for (var x = 0; x < 16; x++)
            {
                tt.SetCell(x + 1, 0, topColumnClues[x].ToString().Color(ConsoleColor.Yellow), alignment: HorizontalTextAlignment.Right);
                tt.SetCell(x + 1, 17, bottomColumnClues[x].ToString().Color(ConsoleColor.Yellow), alignment: HorizontalTextAlignment.Right);
                tt.SetCell(0, x + 1, leftRowClues[x].ToString().Color(ConsoleColor.Yellow), alignment: HorizontalTextAlignment.Right);
                tt.SetCell(17, x + 1, rightRowClues[x].ToString().Color(ConsoleColor.Yellow), alignment: HorizontalTextAlignment.Right);
                for (var y = 0; y < 16; y++)
                {
                    var i = randomSolution[y * 16 + x];
                    tt.SetCell(x + 1, y + 1, i.ToString().Color(!chs.Contains(structure[y * 16 + x]) ? ConsoleColor.Yellow : ConsoleColor.DarkGray),
                        alignment: HorizontalTextAlignment.Right, background: (ConsoleColor) ((i - 1) / 4 + 1));
                }
            }
            tt.WriteToConsole();
            Console.WriteLine();

            var subsquaresOrder = new[] { /* . */ 5, 8, 14, 3, /* : */ 15, 4, 10, 1, /* / */ 6, 13, 11, 0, /* # */ 2, 9, 7, 12 };
            if (subsquaresOrder.Distinct().Count() != 16)
                Debugger.Break();

            var subsquares = subsquaresOrder.Select(ssq => Enumerable.Range(0, 16).Select(ix => randomSolution[16 * 4 * (ssq / 4) + 4 * (ssq % 4) + 16 * (ix / 4) + ix % 4]).ToArray()).ToArray();

            // Random spread
            rnd.NextDouble();

            // Generate the sub-puzzles
            var allSvg = new StringBuilder();
            var clip = Ut.NewArray<string>(27, 27);
            for (var ssqIx = 0; ssqIx < subsquares.Length; ssqIx++)
            {
                if (ssqIx % 4 == 0)
                    allSvg.AppendLine($"<text text-anchor='start' font-size='1.2' x='-5' y='{ssqIx / 4 * 6 + 2.4}'>{4 * (ssqIx / 4) + 1}–{4 * (ssqIx / 4) + 4}</text>");
                var sq = subsquares[ssqIx];
                var allClues = new List<(Side side, int where, int clue)>();
                for (var i = 0; i < 4; i++)
                {
                    allClues.Add((Side.Top, i, getSkyscraperClue(Enumerable.Range(0, 4).Select(row => sq[4 * row + i]))));
                    allClues.Add((Side.Bottom, i, getSkyscraperClue(Enumerable.Range(0, 4).Select(row => sq[4 * row + i]).Reverse())));
                    allClues.Add((Side.Left, i, getSkyscraperClue(Enumerable.Range(0, 4).Select(col => sq[4 * i + col]))));
                    allClues.Add((Side.Right, i, getSkyscraperClue(Enumerable.Range(0, 4).Select(col => sq[4 * i + col]).Reverse())));
                }
                var clues = Ut.ReduceRequiredSet(allClues.Shuffle(rnd), test: state => !makeSkyscraperPuzzle(state.SetToTest).Solve().Skip(1).Any()).ToList();

                // Changes to the clues that I’ve decided I want
                if (ssqIx == 1)
                    clues.Add((Side.Right, 2, 2));

                if (makeSkyscraperPuzzle(clues).Solve().Skip(1).Any())
                    Debugger.Break();

                foreach (var (side, i, clue) in clues)
                {
                    var sx = (ssqIx % 4) * 7 + side switch { Side.Left => 0, Side.Right => 5, _ => i + 1 };
                    var sy = (ssqIx / 4) * 7 + side switch { Side.Top => 0, Side.Bottom => 5, _ => i + 1 };
                    clip[sy][sx] = clue.ToString();
                }

                var st = new TextTable { ColumnSpacing = 1 };
                for (var i = 0; i < 16; i++)
                    st.SetCell((i % 4) + 1, (i / 4) + 1, sq[i].ToString(), alignment: HorizontalTextAlignment.Right);
                foreach (var (side, i, clue) in clues)
                    st.SetCell(
                        col: side switch { Side.Left => 0, Side.Right => 5, _ => i + 1 },
                        row: side switch { Side.Top => 0, Side.Bottom => 5, _ => i + 1 },
                        clue.ToString().Color(ConsoleColor.Yellow), alignment: HorizontalTextAlignment.Right);
                st.WriteToConsole();

                allSvg.AppendLine($@"<g transform='translate({ssqIx % 4 * 7} {ssqIx / 4 * 6})'>
    <path d='M0 0h4v4h-4z' fill='white' stroke='black' stroke-width='.075' />{((ssqIx + 1) % 4 < 2 ? "" : $@"
    <path d='{Ut.NewArray(
                                                "M4 0l.2 -.2M4.05 -.5a.45 .45 0 1 0 .9 0a.45 .45 0 1 0 -.9 0",
                                                (ssqIx < 4 ? "M0 0l-.2 -.2M-.05 -.5a.45 .45 0 1 0 -.9 0a.45 .45 0 1 0 .9 0"
                                                                : "M0 0l-.2 -.2M-.05 -.5a.45 .45 0 1 0 -.9 0a.45 .45 0 1 0 .9 0M-.5 -1.1a.6 .6 0 1 1 0 1.2h-2a.6 .6 0 1 1 0 -1.2zM-1.5 -.2v-.6m-.3 .3h.6"))[ssqIx % 4 - 1]}' fill='none' stroke='black' stroke-width='.05' />")}
    <path d='M1 0v4M2 0v4M3 0v4M0 1h4M0 2h4M0 3h4' fill='none' stroke='black' stroke-width='.025' />
{clues.Select(clue => $@"    <text x='{clue.side switch { Side.Left => -.5, Side.Right => 4.5, _ => clue.where + .5 }}' y='{clue.side switch { Side.Top => -.2, Side.Bottom => 4.8, _ => clue.where + .8 }}'>{clue.clue}</text>").JoinString("\r\n")}
</g>");
            }

            // CLIPBOARD: Small skyscapers
            Clipboard.SetText(clip.Select(row => row.JoinString("\t")).JoinString("\n"));

            var bigClues = Ut.NewArray<(Side side, int where, int clue)>(
                (Side.Top, 5, 4), (Side.Top, 14, 5),
                (Side.Right, 6, 2), (Side.Right, 8, 6),
                (Side.Bottom, 5, 4), (Side.Bottom, 11, 11),
                (Side.Left, 4, 8), (Side.Left, 9, 4), (Side.Left, 13, 3)
            );
            allSvg.Append($@"<g transform='translate(4.5, 25)'>
    <path d='M0 0h16v16h-16z' fill='white' stroke='black' stroke-width='.075' />
    <path d='{Enumerable.Range(1, 15).Select(x => $"M{x} 0v16").JoinString()}{Enumerable.Range(1, 15).Select(y => $"M0 {y}h16").JoinString()}' fill='none' stroke='black' stroke-width='.025' />
{bigClues.Select(clue => $@"    <text x='{clue.side switch { Side.Left => -.5, Side.Right => 16.5, _ => clue.where + .5 }}' y='{clue.side switch { Side.Top => -.2, Side.Bottom => 16.8, _ => clue.where + .8 }}'>{clue.clue}</text>").JoinString("\r\n")}
</g>");
            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\Objectionable Ranking\Objectionable Ranking.html", "<!--@@-->", "<!--@@@-->",
                $"<svg style='width: 30cm; display: inline-block' viewBox='-5.1 -1.1 35.2 43.2' text-anchor='middle' font-size='.7'>{allSvg}</svg>");

            clip = Ut.NewArray<string>(18, 18);
            foreach (var (side, i, clue) in bigClues)
            {
                var sx = side switch { Side.Left => 0, Side.Right => 17, _ => i + 1 };
                var sy = side switch { Side.Top => 0, Side.Bottom => 17, _ => i + 1 };
                clip[sy][sx] = clue.ToString();
            }

            // CLIPBOARD: Big skyscaper grid
            Clipboard.SetText(clip.Select(row => row.JoinString("\t")).JoinString("\n"));

            clip = Ut.NewArray<string>(18, 18);
            for (var i = 0; i < 16; i++)
            {
                clip[i + 1][0] = leftRowClues[i].ToString();
                clip[i + 1][17] = rightRowClues[i].ToString();
                clip[0][i + 1] = topColumnClues[i].ToString();
                clip[17][i + 1] = bottomColumnClues[i].ToString();
            }

            // CLIPBOARD: Big skyscaper grid with ALL clues
            Clipboard.SetText(clip.Select(row => row.JoinString("\t")).JoinString("\n"));
        }

        enum Side { Top, Right, Bottom, Left }

        private static int getSkyscraperClue(IEnumerable<int> numbers) => numbers.Aggregate((clue: 0, biggestSeen: 0), (p, n) => n > p.biggestSeen ? (p.clue + 1, n) : p).clue;
        private static readonly int[][] _permutations1to4 = Enumerable.Range(1, 4).Permutations().Select(p => p.ToArray()).ToArray();

        private static IEnumerable<int[]> getValid4x4SkyscraperPuzzles()
        {
            return new LatinSquare(4, 1).Solve().Where(latinSquare =>
            {
                // Generate all row/column clues
                var allClues = new List<(Side side, int where, int clue)>();
                for (var i = 0; i < 4; i++)
                {
                    allClues.Add((Side.Top, i, getSkyscraperClue(Enumerable.Range(0, 4).Select(row => latinSquare[4 * row + i]))));
                    allClues.Add((Side.Bottom, i, getSkyscraperClue(Enumerable.Range(0, 4).Select(row => latinSquare[4 * row + i]).Reverse())));
                    allClues.Add((Side.Left, i, getSkyscraperClue(Enumerable.Range(0, 4).Select(col => latinSquare[4 * i + col]))));
                    allClues.Add((Side.Right, i, getSkyscraperClue(Enumerable.Range(0, 4).Select(col => latinSquare[4 * i + col]).Reverse())));
                }

                return !makeSkyscraperPuzzle(allClues).Solve().Skip(1).Any();
            });
        }

        private static Puzzle makeSkyscraperPuzzle(IEnumerable<(Side side, int where, int clue)> clues)
        {
            var puzzle = new LatinSquare(4, 1);
            foreach (var (side, i, clue) in clues)
            {
                puzzle.AddConstraint(new CombinationsConstraint(side switch
                {
                    Side.Top => Enumerable.Range(0, 4).Select(row => 4 * row + i),
                    Side.Bottom => Enumerable.Range(0, 4).Select(row => 4 * row + i).Reverse(),
                    Side.Left => Enumerable.Range(0, 4).Select(col => 4 * i + col),
                    _ => Enumerable.Range(0, 4).Select(col => 4 * i + col).Reverse()
                }, _permutations1to4.Where(p => getSkyscraperClue(p) == clue).ToArray()));
            }
            return puzzle;
        }

    }
}