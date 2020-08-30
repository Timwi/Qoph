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
        public static void Generate()
        {
            var structure = @"////^^^^....####
////^^^^....####
////^^^^....####
////^^^^....####
....####///N^^^^
....####////^^^^
....####////^^^^
....C###////^^^^
####////I^^^....
####////^^^^....
####////^^^^....
####///O^^^^....
^^^^....####////
^^^^....####////
^^^^....####////
^^^^....####////".Replace("\r", "").Replace("\n", "");

            var valid4x4s = getValid4x4SkyscraperPuzzles().ToArray();

            var puzzle = new LatinSquare(16, 1);
            var chs = "#.^/";
            foreach (var ix in Enumerable.Range(0, structure.Length))
                puzzle.AddConstraint(chs.IndexOf(structure[ix]).Apply(pos => new OneCellLambdaConstraint(ix, val => pos == -1 ? val == structure[ix] - 'A' + 1 : val > 4 * pos && val <= 4 * (pos + 1))));
            for (var ssq = 0; ssq < 16; ssq++)
            {
                var tl = 16 * 4 * (ssq / 4) + 4 * (ssq % 4);
                var ch = chs.Contains(structure[tl]) ? structure[tl] : structure[tl + 1];
                var additive = chs.IndexOf(ch) * 4;
                puzzle.AddConstraint(new CombinationsConstraint(Enumerable.Range(0, 16).Select(ix => 16 * 4 * (ssq / 4) + 4 * (ssq % 4) + 16 * (ix / 4) + ix % 4), valid4x4s.Select(comb => comb.Select(v => v + additive).ToArray()).ToArray()));
            }

            var rnd = new Random(47);
            var randomSolution = puzzle.Solve(new SolverInstructions { Randomizer = rnd }).First();
            ConsoleUtil.WriteLine(randomSolution.Split(16).Select(row => row.Select(i => $"{i:00}".Color(i <= 4 ? ConsoleColor.White : ConsoleColor.Gray, i <= 4 ? ConsoleColor.DarkBlue : ConsoleColor.Black)).JoinColoredString(" ")).JoinColoredString("\n"));
            Console.WriteLine();

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
                    tt.SetCell(x + 1, y + 1, i.ToString().Color(i <= 4 ? ConsoleColor.White : ConsoleColor.Gray), alignment: HorizontalTextAlignment.Right, background: i <= 4 ? ConsoleColor.DarkBlue : ConsoleColor.Black);
                }
            }
            tt.WriteToConsole();
            Console.WriteLine();

            var subsquares = Enumerable.Range(0, 16).Select(ssq => Enumerable.Range(0, 16).Select(ix => randomSolution[16 * 4 * (ssq / 4) + 4 * (ssq % 4) + 16 * (ix / 4) + ix % 4]).ToArray())
                .Select((sq, ix) => (sq, ix))
                .OrderBy(inf => inf.ix == 3 ? 3 : inf.ix == 5 ? 2 : inf.ix == 8 ? 0 : inf.ix == 14 ? 1 : inf.sq[0])
                .Select(inf => inf.sq)
                .ToArray();

            // Generate the sub-puzzles
            var allSubpuzzles = new StringBuilder();
            var markers = new[] { "tr", "tl", "br", "bl" };
            var clip = Ut.NewArray<string>(27, 27);
            for (var ssqIx = 0; ssqIx < subsquares.Length; ssqIx++)
            {
                if (ssqIx % 4 == 0)
                    allSubpuzzles.AppendLine($"<div class='skyscraper-numbers'>{4 * (ssqIx / 4) + 1}–{4 * (ssqIx / 4) + 4}</div>");
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
                    clues.Add((Side.Right, 1, 1));
                if (ssqIx == 0)
                    clues.Add((Side.Left, 1, 4));
                if (ssqIx == 14)
                {
                    if (!clues.Remove((Side.Left, 0, 2)))
                        Debugger.Break();
                    clues.Add((Side.Left, 1, 4));
                }
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

                //M4 0l.2 -.2M4.05 -.5a.45 .45 0 1 0 .9 0a.45 .45 0 1 0 -.9 0
                allSubpuzzles.AppendLine($@"<svg class='skyscraper' viewBox='-1.1 -1.1 6.2 6.2' text-anchor='middle' font-size='.7'>
    <path d='M0 0h4v4h-4z' fill='white' stroke='black' stroke-width='.075' />{(ssqIx >= 4 ? "" : $@"
    <path d='{Ut.NewArray(
                                                "M4 0l.2 -.2M4.05 -.5a.45 .45 0 1 0 .9 0a.45 .45 0 1 0 -.9 0",
                                                "M0 0l-.2 -.2M-.05 -.5a.45 .45 0 1 0 -.9 0a.45 .45 0 1 0 .9 0",
                                                "M4 4l.2 .2M4.05 4.5a.45 .45 0 1 0 .9 0a.45 .45 0 1 0 -.9 0",
                                                "M0 4l-.2 .2M-.05 4.5a.45 .45 0 1 0 -.9 0a.45 .45 0 1 0 .9 0")[ssqIx]}' fill='none' stroke='black' stroke-width='.05' />")}
    <path d='M1 0v4M2 0v4M3 0v4M0 1h4M0 2h4M0 3h4' fill='none' stroke='black' stroke-width='.025' />
{clues.Select(clue => $@"    <text x='{clue.side switch { Side.Left => -.5, Side.Right => 4.5, _ => clue.where + .5 }}' y='{clue.side switch { Side.Top => -.2, Side.Bottom => 4.8, _ => clue.where + .8 }}'>{clue.clue}</text>").JoinString("\r\n")}
</svg>");
                if (ssqIx % 4 == 3)
                    allSubpuzzles.AppendLine("<br>");
            }
            Clipboard.SetText(clip.Select(row => row.JoinString("\t")).JoinString("\n"));

            var bigClues = Ut.NewArray<(Side side, int where, int clue)>(
                (Side.Top, 1, 2), (Side.Top, 7, 3), (Side.Top, 10, 2), (Side.Top, 13, 4),
                (Side.Right, 2, 11), (Side.Right, 8, 8),
                (Side.Bottom, 1, 5), (Side.Bottom, 7, 3), (Side.Bottom, 11, 7),
                (Side.Left, 6, 5), (Side.Left, 11, 3)
            );
            allSubpuzzles.Append($@"<svg style='width: 18.2cm; display: inline-block' viewBox='-1.1 -1.1 18.2 18.2' text-anchor='middle' font-size='.7'>
    <path d='M0 0h16v16h-16z' fill='white' stroke='black' stroke-width='.075' />
    <path d='{Enumerable.Range(1, 15).Select(x => $"M{x} 0v16").JoinString()}{Enumerable.Range(1, 15).Select(y => $"M0 {y}h16").JoinString()}' fill='none' stroke='black' stroke-width='.025' />
{bigClues.Select(clue => $@"    <text x='{clue.side switch { Side.Left => -.5, Side.Right => 16.5, _ => clue.where + .5 }}' y='{clue.side switch { Side.Top => -.2, Side.Bottom => 16.8, _ => clue.where + .8 }}'>{clue.clue}</text>").JoinString("\r\n")}
</svg>");
            General.ReplaceInFile(@"D:\c\Qoph\DataFiles\Objectionable Ranking\Objectionable Ranking.html", "<!--@@-->", "<!--@@@-->", allSubpuzzles.ToString());
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