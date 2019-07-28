using System;
using RT.Util;

namespace PuzzleStuff
{
    static class MinesweeperPuzzle
    {
        public static void Do()
        {
            var grid = Ut.NewArray<int>(7, 8);
            var solution = Ut.NewArray<bool?>(7, 8);
            solution[1][1] = true;
            solution[2][3] = true;
            solution[3][1] = true;
            solution[3][2] = true;
            solution[3][3] = true;
            solution[3][5] = true;
            solution[4][3] = true;
            solution[4][6] = true;
            solution[5][5] = true;

            solution[1][2] = false;
            solution[2][1] = false;
            solution[2][2] = false;
            solution[2][4] = false;
            solution[3][4] = false;
            solution[3][6] = false;
            solution[4][4] = false;
            solution[4][5] = false;
            solution[5][6] = false;

            for (int y = 0; y < 7; y++)
                for (int x = 0; x < 8; x++)
                    if (solution[y][x] == null)
                    {
                        var c = 0;
                        for (var xx = -1; xx <= 1; xx++)
                            for (var yy = -1; yy <= 1; yy++)
                                if (x + xx >= 0 && x + xx < 8 && y + yy >= 0 && y + yy < 7 && solution[y + yy][x + xx] == true)
                                    c++;
                        grid[y][x] = c;
                    }
        }
    }
}