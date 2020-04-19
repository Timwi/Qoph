using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff
{
    static class StatueParkSolver
    {
        public static void Do()
        {
            var mainGrid = @"⚫					⚪	⚫		⚫	⚫	⚫	
⚫	⚫							⚪	⚫		
⚫			⚫	⚪							⚫
⚪		⚫				⚫			⚫		
	⚫			⚫			⚪			⚫	
						⚫	⚫			⚪	
	⚫							⚫			
			⚪						⚫		
⚫				⚫						⚫	⚪
						⚫			⚫	⚪	⚫
			⚫		⚫	⚫	⚫	⚪			
⚫		⚪				⚫					".Replace("\r", "").Split('\n').Select(row => row.Split('\t').Select(cell => cell == "⚪" ? false : cell == "⚫" ? true : (bool?) null).ToArray()).ToArray();

            var gridW = mainGrid[0].Length;
            var gridH = mainGrid.Length;
            var pieces = Ut.NewArray(
                @"#/#/#/#/#",
                @"#/##/.#/.#",
                @"###/#.#",
                @".##/##/#",
                @"#/###/..#",
                @"..#/###/..#",
                @".#/####",
                @"#/###/.#",
                @"#/#/#/##",
                @"##/###",
                @"..#/..#/###",
                @".#/###/.#"
            )
                .Select(str => str.Split('/'))
                .Select(arr => arr.Select(row => row.PadRight(arr.Max(str => str.Length), '.').Select(ch => ch == '#').ToArray()).ToArray())
                .ToArray();

            IEnumerable<bool[][]> recurse(bool?[][] grid, bool[][] placed, int pieceIx)
            {
                if (pieceIx == pieces.Length)
                {
                    if (grid.Sum(row => row.Count(c => c == true)) == 5 * 12)
                        yield return grid.Select(row => row.Select(b => b ?? false).ToArray()).ToArray();
                    yield break;
                }

                var alreadyPiece = new HashSet<string>();
                for (var rot = 0; rot < 4; rot++)
                    foreach (var flip in new[] { false, true })
                    {
                        var piece = pieces[pieceIx];
                        var w = piece[0].Length;
                        var h = piece.Length;
                        for (var r = 0; r < rot; r++)
                        {
                            piece = Ut.NewArray(w, h, (y, x) => piece[x][w - 1 - y]);
                            w = piece[0].Length;
                            h = piece.Length;
                        }
                        if (flip)
                            piece = Ut.NewArray(h, w, (y, x) => piece[y][w - 1 - x]);

                        var pieceStr = piece.Select(row => row.Select(ch => ch ? '#' : '.').JoinString()).JoinString("/");
                        if (!alreadyPiece.Add(pieceStr))
                            continue;

                        for (var x = 0; x <= grid[0].Length - w; x++)
                            for (var y = 0; y <= grid.Length - h; y++)
                            {
                                for (var xx = 0; xx < w; xx++)
                                    for (var yy = 0; yy < h; yy++)
                                    {
                                        if (!piece[yy][xx])
                                            continue;
                                        if (placed[y + yy][x + xx])
                                            goto busted;
                                        if (grid[y + yy][x + xx] == false)
                                            goto busted;
                                        if (x + xx > 0 && (xx == 0 || !piece[yy][xx - 1]) && grid[y + yy][x + xx - 1] == true)
                                            goto busted;
                                        if (x + xx < gridW - 1 && (xx == w - 1 || !piece[yy][xx + 1]) && grid[y + yy][x + xx + 1] == true)
                                            goto busted;
                                        if (y + yy > 0 && (yy == 0 || !piece[yy - 1][xx]) && grid[y + yy - 1][x + xx] == true)
                                            goto busted;
                                        if (y + yy < gridH - 1 && (yy == h - 1 || !piece[yy + 1][xx]) && grid[y + yy + 1][x + xx] == true)
                                            goto busted;
                                    }

                                var gridCopy = grid.Select(arr => (bool?[]) arr.Clone()).ToArray();
                                var placedCopy = placed.Select(arr => (bool[]) arr.Clone()).ToArray();
                                for (var xx = 0; xx < w; xx++)
                                    for (var yy = 0; yy < h; yy++)
                                        if (piece[yy][xx])
                                        {
                                            gridCopy[y + yy][x + xx] = true;
                                            placedCopy[y + yy][x + xx] = true;
                                        }

                                var amountOfWhite = gridCopy.Sum(row => row.Count(b => b != true));
                                var q = new Queue<(int x, int y)>();
                                q.Enqueue((0, 3));
                                var already = new HashSet<(int x, int y)>();
                                while (q.Count > 0)
                                {
                                    var (tx, ty) = q.Dequeue();
                                    if (!already.Add((tx, ty)))
                                        continue;
                                    if (tx > 0 && gridCopy[ty][tx - 1] != true)
                                        q.Enqueue((tx - 1, ty));
                                    if (tx < gridW - 1 && gridCopy[ty][tx + 1] != true)
                                        q.Enqueue((tx + 1, ty));
                                    if (ty > 0 && gridCopy[ty - 1][tx] != true)
                                        q.Enqueue((tx, ty - 1));
                                    if (ty < gridH - 1 && gridCopy[ty + 1][tx] != true)
                                        q.Enqueue((tx, ty + 1));
                                }
                                if (already.Count < amountOfWhite)
                                    goto busted;

                                foreach (var solution in recurse(gridCopy, placedCopy, pieceIx + 1))
                                    yield return solution;

                                busted:;
                            }
                    }
            }

            foreach (var solution in recurse(mainGrid, Ut.NewArray(gridW, gridH, (x, y) => false), 0))
            {
                Console.WriteLine("Solution found:");
                Console.WriteLine(solution.Select(row => row.Select(ch => ch ? "██" : "░░").JoinString()).JoinString("\n"));
                Console.WriteLine();
            }
        }
    }
}