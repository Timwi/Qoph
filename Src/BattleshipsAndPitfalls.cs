using System;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace Qoph
{
    static class BattleshipsAndPitfalls
    {
        public static void Do()
        {
            var grid = Ut.NewArray<bool?>(7, 7);
            grid[5][6] = false;
            grid[4][6] = false;

            var count = 0;
            bool?[][] commonalities = null;
            foreach (var solution in StatueParkSolver.Solve(grid, new[] { "#.#/###", "####/.#", "##/.##", "####/#", "..#/###", "#/##" }, allowFlip: false))
            {
                Console.WriteLine(solution.Select(row => row.Select(cell => cell ? "██" : "··").JoinString()).JoinString("\n"));
                Console.WriteLine();
                count++;

                if (commonalities == null)
                    commonalities = solution.Select(row => row.Select(b => b.Nullable()).ToArray()).ToArray();
                else
                    for (var y = 0; y < 7; y++)
                        for (var x = 0; x < 7; x++)
                            if (commonalities[y][x] != solution[y][x])
                                commonalities[y][x] = null;
            }
            Console.WriteLine(count);
            Console.WriteLine();
            if (commonalities != null)
                Console.WriteLine(commonalities.Select(row => row.Select(cell => cell == null ? "??" : cell == true ? "██" : "··").JoinString()).JoinString("\n"));
            Console.WriteLine();
        }
    }
}