using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace Qoph
{
    static class HomeDepot
    {
        public static void FindPaths()
        {
            var fodder = @"ALLEN WRENCH; GERBIL FEEDER; TOILET SEAT; ELECTRIC HEATER
TRASH COMPACTOR; JUICE EXTRACTOR; SHOWER ROD; WATER METER
WALKIE-TALKIE; COPPER WIRE; SAFETY GOGGLE; RADIAL TIRE
BB PELLET; RUBBER MALLET
PICTURE HANGER; PAPER CUTTER; WAFFLE IRON; WINDOW SHUTTER
PAINT REMOVER
KITCHEN FAUCET; FOLDING TABLE; WEATHER STRIPPING; JUMPER CABLE
POWER FOGGER
HIGH-PERFORMANCE LUBRICATION
WATER PROOFING
AIR COMPRESSOR; BRASS CONNECTOR; WRECKING CHISEL; SMOKE DETECTOR
TIRE GAUGE; HAMSTER CAGE; BUG DEFLECTOR
TRAILER HITCH DEMAGNETIZER; AUTOMATIC CIRCUMCISER
TENNIS RACKET; ANGLE BRACKET
SOFFIT PANEL; CIRCUIT BREAKER; VACUUM CLEANER
CALCULATOR; GENERATOR".Replace("\r", "").Split('\n');
            var phrase = "ANSWERSIDEMIRROR";

            if (fodder.Length != phrase.Length)
                Debugger.Break();

            var allDirections = new (int dx, int dy, string direction)[] { (0, -1, "N"), (1, 0, "E"), (0, 1, "S"), (-1, 0, "W") };

            IEnumerable<(ConsoleColoredString[] solution, string[] directions, int[] dists)> recurse(ConsoleColoredString[] sofar, string[] dirs, int[] dists, int fodderIx, int boardIx, bool[] used, int forbiddenDirectionIx)
            {
                if (!fodder[fodderIx].Contains(phrase[boardIx]))
                    yield break;

                sofar = sofar.Insert(sofar.Length, new ConsoleColoredString($"{phrase[boardIx].ToString().Color(ConsoleColor.White)} {boardIx % 4},{boardIx / 4} {fodder[fodderIx].Split("; ").Where(s => s.Contains(phrase[boardIx])).JoinString(", ").Select(ch => ch == phrase[boardIx] ? ch.Color(ConsoleColor.White, ConsoleColor.DarkBlue) : ch).JoinColoredString()}"));

                if (fodderIx == fodder.Length - 1)
                {
                    yield return (sofar, dirs, dists);
                    yield break;
                }

                used[boardIx] = true;

                for (var dirIx = 0; dirIx < allDirections.Length; dirIx++)
                {
                    if (dirIx == forbiddenDirectionIx)
                        continue;
                    var (dx, dy, direction) = allDirections[dirIx];
                    var x = boardIx % 4;
                    var y = boardIx / 4;
                    var dist = 0;
                    do
                    {
                        x += dx;
                        y += dy;
                        dist++;
                    }
                    while (x >= 0 && y >= 0 && x < 4 && y < 4 && used[x + 4 * y]);
                    if (x < 0 || y < 0 || x >= 4 || y >= 4)
                        continue;
                    foreach (var solution in recurse(sofar, dirs.Insert(dirs.Length, direction), dists.Insert(dists.Length, dist), fodderIx + 1, x + 4 * y, used, (dirIx + 2) % 4))
                        yield return solution;
                }

                used[boardIx] = false;
            }

            var maxDist = 0;
            var count = 0;
            foreach (var (solution, directions, distances) in recurse(new ConsoleColoredString[0], new string[0], new int[0], 0, 9, new bool[16], -1))
            {
                count++;
                //if (maxDist >= distances.Sum())
                //    //if (distances.Sum() != 20)
                //    continue;
                maxDist = distances.Sum();
                ConsoleUtil.WriteLine(solution.JoinColoredString("\n"));
                Console.WriteLine(directions.JoinString());
                Console.WriteLine($"{distances.JoinString("+")} = {distances.Sum()}");
                //Console.ReadLine();
                Console.WriteLine();
            }
            Console.WriteLine(count);
        }
    }
}