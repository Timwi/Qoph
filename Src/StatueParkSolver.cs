using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace Qoph
{
    static class StatueParkSolver
    {
        public static IEnumerable<bool[][]> Solve(bool?[][] grid, string[] pieces, bool allowFlip = false)
        {
            /*
                EXAMPLE PIECES:
                Ut.NewArray(
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
            */

            return Solve(grid, pieces
                .Select(str => str.Split('/'))
                .Select(arr => arr.Select(row => row.PadRight(arr.Max(str => str.Length), '.').Select(ch => ch == '#').ToArray()).ToArray())
                .ToArray(), allowFlip);
        }

        public static IEnumerable<bool[][]> Solve(bool?[][] grid, bool[][][] pieces, bool allowFlip = false)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            if (grid.Length == 0 || grid.Contains(null))
                throw new ArgumentException("Grid cannot be empty or contain null entries.", nameof(grid));
            var width = grid[0].Length;
            if (grid.Any(ar => ar.Length != grid[0].Length))
                throw new ArgumentException("Grid rows must have consistent widths.", nameof(grid));
            var height = grid.Length;
            var flipsAllowed = allowFlip ? new[] { false, true } : new[] { false };

            IEnumerable<bool[][]> recurse(bool?[][] grid, bool[][] placed, int pieceIx)
            {
                if (pieceIx == pieces.Length)
                {
                    for (var y = 0; y < height; y++)
                        for (var x = 0; x < width; x++)
                            if (grid[y][x] == true && !placed[y][x])
                                yield break;

                    yield return grid.Select(row => row.Select(b => b ?? false).ToArray()).ToArray();
                    yield break;
                }

                var alreadyPiece = new HashSet<string>();
                for (var rot = 0; rot < 4; rot++)
                    foreach (var flip in flipsAllowed)
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

                        for (var x = 0; x <= width - w; x++)
                            for (var y = 0; y <= height - h; y++)
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
                                        if (x + xx < width - 1 && (xx == w - 1 || !piece[yy][xx + 1]) && grid[y + yy][x + xx + 1] == true)
                                            goto busted;
                                        if (y + yy > 0 && (yy == 0 || !piece[yy - 1][xx]) && grid[y + yy - 1][x + xx] == true)
                                            goto busted;
                                        if (y + yy < height - 1 && (yy == h - 1 || !piece[yy + 1][xx]) && grid[y + yy + 1][x + xx] == true)
                                            goto busted;
                                    }

                                (int x, int y)? adj = null;
                                var gridCopy = grid.Select(arr => (bool?[]) arr.Clone()).ToArray();
                                var placedCopy = placed.Select(arr => (bool[]) arr.Clone()).ToArray();
                                for (var xx = 0; xx < w; xx++)
                                    for (var yy = 0; yy < h; yy++)
                                        if (piece[yy][xx])
                                        {
                                            gridCopy[y + yy][x + xx] = true;
                                            placedCopy[y + yy][x + xx] = true;
                                            if (adj != null)
                                                continue;
                                            if (y + yy > 0 && gridCopy[y + yy - 1][x + xx] != true && (yy == 0 || !piece[yy - 1][xx]))
                                                adj = (x + xx, y + yy - 1);
                                            else if (y + yy < height - 1 && gridCopy[y + yy + 1][x + xx] != true && (yy == piece.Length - 1 || !piece[yy + 1][xx]))
                                                adj = (x + xx, y + yy + 1);
                                            else if (x + xx > 0 && gridCopy[y + yy][x + xx - 1] != true && (xx == 0 || !piece[yy][xx - 1]))
                                                adj = (x + xx - 1, y + yy);
                                            else if (x + xx < width - 1 && gridCopy[y + yy][x + xx + 1] != true && (xx == piece[yy].Length - 1 || !piece[yy][xx + 1]))
                                                adj = (x + xx + 1, y + yy);
                                        }

                                var amountOfWhite = gridCopy.Sum(row => row.Count(b => b != true));
                                var q = new Queue<(int x, int y)>();
                                q.Enqueue(adj.Value);
                                var already = new HashSet<(int x, int y)>();
                                while (q.Count > 0)
                                {
                                    var (tx, ty) = q.Dequeue();
                                    if (!already.Add((tx, ty)))
                                        continue;
                                    if (tx > 0 && gridCopy[ty][tx - 1] != true)
                                        q.Enqueue((tx - 1, ty));
                                    if (tx < width - 1 && gridCopy[ty][tx + 1] != true)
                                        q.Enqueue((tx + 1, ty));
                                    if (ty > 0 && gridCopy[ty - 1][tx] != true)
                                        q.Enqueue((tx, ty - 1));
                                    if (ty < height - 1 && gridCopy[ty + 1][tx] != true)
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

            return recurse(grid, Ut.NewArray<bool>(height, width), 0);
        }
    }
}