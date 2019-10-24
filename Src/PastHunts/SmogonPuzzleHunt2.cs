using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff
{
    static partial class SmogonPuzzleHunt2
    {
        public static void DoStuff()
        {
            FindGolfCourse("SPORE");
        }

        private static void FindGolfCourse(string word)
        {
            var list = File.ReadAllLines(@"D:\Daten\Puzzles\Smogon Puzzle Hunt 2\Hyperlinks.txt");
            var regex = Enumerable.Range(0, word.Length).Select(i => word.Remove(i, 1).Insert(i, ".")).JoinString("|");
            regex = $@"({regex})";
            foreach (var line in list)
            {
                var m = Regex.Match(line, @"^\d+\. (.*)$");
                if (Regex.IsMatch(m.Groups[1].Value, regex, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase))
                    Console.WriteLine(line);
            }
        }

        private static void Smogon2_Sudokus()
        {
            var sudokus = new[] {
                @"5	3	6	4	2	8	7	1	9
1	2	9	6	7	5	8	4	3
4	7	8	9	1	3	2	6	5
3	1	7	2	8	6	9	5	4
2	6	4	5	9	7	3	8	1
9	8	5	3	4	1	6	2	7
6	9	1	7	5	2	4	3	8
7	5	2	8	3	4	1	9	6
8	4	3	1	6	9	5	7	2",
                @"8	2	3	4	7	6	9	1	5
1	6	9	8	5	2	4	7	3
4	7	5	9	1	3	2	8	6
3	1	6	2	4	7	8	5	9
2	8	7	6	9	5	3	4	1
9	5	4	3	8	1	6	2	7
6	4	1	7	2	9	5	3	8
5	3	8	1	6	4	7	9	2
7	9	2	5	3	8	1	6	4",
                @"2	8	5	9	1	4	3	7	6
9	3	7	6	2	8	1	5	4
4	1	6	3	5	7	2	8	9
3	6	8	1	4	5	9	2	7
5	7	4	2	9	3	8	6	1
1	2	9	7	8	6	4	3	5
6	5	1	4	3	2	7	9	8
8	9	2	5	7	1	6	4	3
7	4	3	8	6	9	5	1	2"
            }
                .Select(su => su.Replace("\r", "").Replace("\n", "").Replace("\t", ""))
                .ToArray();

            for (int i = 0; i < 81; i++)
            {
                if (i % 9 == 0)
                    Console.WriteLine();
                if (sudokus.All(s => s[i] == sudokus[0][i]))
                    Console.Write(sudokus[0][i] + " ");
                else
                    Console.Write("· ");
            }
        }

        private static string[] _gridFromCountryRoad = @" a a a a|d d d d d|e
     -   - -   - -
 a a|b|a a a|d|f f|e
   -   - -   -   -
 a|b b b|c|a a|f|e e
   -   -   -     -
 a a|b|c c c|a|f f|e
 -   - -   -   - -
 i|a a|h|c|a a|g g|e
   - -   - - -   - -
 i|h h h h|g g g|m m
     - - -   - -   -
 i|h|k k|g g|m m m|n
   -     - -     -
 i|k k k k|m m m|l|n
   -     - - - -
 i i|k k|l l l l l|n
 - - - - - - - - -
 j j j j j j j j|n n".Replace("\r", "").Split('\n').Select(row => row.PadRight(21, ' ')).ToArray();

        private static string[] _gridFromHeyajilin = @" a a a a|c c|d d d d
   - -       -
 a|b b|a|c c|e|d d d
           -   -
 a|b b|a|c|e e e|d d
 -     -
 b b b b|c|e e e|d d
       -
 b b b|c c|e e e|d d
   - - - - - - -
 b|f f|g g|j j j|d d
 -   - -
 f f|h h|g|j j j|d d
   -     -
 f|h h h h|j j j|d d
   - -       - -
 f|i i|h h|j|d d d d
   - -   -
 f|h h h|j j|d d d d".Replace("\r", "").Split('\n').Select(row => row.PadRight(21, ' ')).ToArray();

        private static string[] _lettersFromHeyajilin = @",U,N,S,,,,,,,,L,A,T,,,,,,,,Y,R,E,,,,,,,,V,A,R,T,P,E,,,,,,,E,R,Y,X,,,,,,,O,I,C,S,,,,,,,S,E,M,H,O,W,,,,,,,,W,S,Q,,,,,,,,U,R,E,,,,,,,,B,E,L,".Split(',');

        private static string[] _gridFromStarBattle = @" ·|·|· ·|· · · · · · 
             -   - - 
 ·|·|· ·|· ·|·|·|· · 
         -           
 ·|·|· ·|·|·|·|·|· · 
 -   -     -   -     
 · · ·|·|· · · · · · 
 -       - - - -     
 ·|· ·|·|· · · ·|· · 
     -         - - - 
 ·|·|·|·|· · ·|· · · 
   -         -       
 · · ·|·|· ·|· · · · 
     -     - -       
 · ·|· ·|·|· ·|· · · 
     - - -       - - 
 · · · ·|· · ·|·|· · 
             - -     
 · · · ·|· ·|· · · · ".Replace("\r", "").Split('\n').Select(row => row.PadRight(21, ' ')).ToArray();

        private static string[] _lettersFromStarBattle = @",,,,,B,R,S,D,Q,,,,,,L,I,O,I,U,,,,,,A,S,U,N,I,,,,,,M,K,N,A,L,,,,,,E,S,D,R,L,G,N,V,D,M,,,,,,R,E,I,E,A,,,,,,U,X,N,L,D,,,,,,F,U,Y,T,A,,,,,,F,S,L,A,M,,,,,".Split(',');

        private static void Smogon2_RegionalTours_Heyajilin(string[] grid, string[] letters)
        {
            IEnumerable<bool[]> recurse(bool[] sofar, int ix)
            {
                if (ix == 100)
                {
                    yield return sofar.ToArray();
                    yield break;
                }

                //if (ix == 90)
                //{
                //    sofar[ix] = true;
                //    foreach (var solution in recurse(sofar, ix + 1))
                //        yield return solution;
                //    yield break;
                //}

                //if (ix == 45)
                //{
                //    // Check the “3” room
                //    if (Enumerable.Range(0, 45).Where(i => grid[2 * (i / 10)][2 * (i % 10) + 1] == '3').Count(i => sofar[i]) != 3)
                //        yield break;
                //}

                //if (ix == 96)
                //{
                //    // Check the “2” room
                //    if (Enumerable.Range(0, 96).Where(i => grid[2 * (i / 10)][2 * (i % 10) + 1] == '2').Count(i => sofar[i]) != 2)
                //        yield break;
                //}

                var canBeUnshaded = true;
                var x = ix % 10;
                var y = ix / 10;
                var numWalls = 0;
                for (var ty = y - 1; ty >= 0 && canBeUnshaded; ty--)
                {
                    if (sofar[x + 10 * ty])
                        break;
                    if (grid[2 * ty + 1][2 * x + 1] == '-')
                    {
                        numWalls++;
                        if (numWalls > 1)
                        {
                            canBeUnshaded = false;
                            break;
                        }
                    }
                }
                numWalls = 0;
                for (var tx = x - 1; tx >= 0 && canBeUnshaded; tx--)
                {
                    if (sofar[tx + 10 * y])
                        break;
                    if (grid[2 * y][2 * tx + 2] == '|')
                    {
                        numWalls++;
                        if (numWalls > 1)
                        {
                            canBeUnshaded = false;
                            break;
                        }
                    }
                }

                if (canBeUnshaded)
                {
                    sofar[ix] = false;
                    foreach (var solution in recurse(sofar, ix + 1))
                        yield return solution;
                }

                // If can be shaded
                if (!((y > 0 && sofar[x + 10 * (y - 1)]) || (x > 0 && sofar[x - 1 + 10 * y])))
                {
                    // Check for up-facing dead end
                    if ((x >= 2 && sofar[ix - 2] && y >= 1 && sofar[ix - 11]) || (x == 1 && y >= 1 && sofar[ix - 11]) || (x == 8 && y >= 1 && sofar[ix - 9]))
                        yield break;
                    // Check for left-facing dead-end
                    if ((x >= 1 && y >= 2 && sofar[ix - 11] && sofar[ix - 20]) || (y == 1 && x >= 1 && sofar[ix - 11]) || (y == 9 && x < 9 && sofar[ix - 9]))
                        yield break;
                    // Check for right-facing dead-end
                    if ((x < 9 && y >= 2 && sofar[ix - 20] && sofar[ix - 9]) || (y == 1 && x < 9 && sofar[ix - 9]) || (y == 9 && x >= 1 && sofar[ix - 11]))
                        yield break;
                    // Check for down-facing dead-end
                    if ((x >= 1 && x < 9 && y >= 1 && sofar[ix - 11] && sofar[ix - 9]) || (x == 0 && y >= 1 && sofar[ix - 9]) || (x == 9 && y >= 1 && sofar[ix - 11]))
                        yield break;

                    sofar[ix] = true;
                    foreach (var solution in recurse(sofar, ix + 1))
                        yield return solution;
                }
            }

            bool?[] definite = null;
            var count = 0;
            var set = new HashSet<string>();
            foreach (var solution in recurse(new bool[100], 0))
            {
                if (Enumerable.Range(0, 100).Where(i => letters[i] != "").Count(i => solution[i]) != 8)
                    continue;
                set.Add(Enumerable.Range(0, 100).Where(i => solution[i]).Select(i => letters[i]).JoinString());
                count++;
                if (definite == null)
                {
                    definite = solution.Cast<bool?>().ToArray();
                    ConsoleUtil.WriteLine(definite.Split(10).Select((row, y) => row.Select((b, x) => (b == null ? "??" : b.Value ? "██" : "░░").Color(grid[2 * y][2 * x + 1] == '2' ? ConsoleColor.Magenta : grid[2 * y][2 * x + 1] == '3' ? ConsoleColor.Yellow : ConsoleColor.Cyan)).JoinColoredString()).JoinColoredString("\n"));
                    Console.WriteLine();
                }
                else
                    for (int i = 0; i < 100; i++)
                    {
                        if (definite[i] != solution[i])
                            definite[i] = null;
                    }
            }
            ConsoleUtil.WriteLine(definite.Split(10).Select((row, y) => row.Select((b, x) => (b == null ? "??" : b.Value ? "██" : "░░").Color(grid[2 * y][2 * x + 1] == '2' ? ConsoleColor.Magenta : grid[2 * y][2 * x + 1] == '3' ? ConsoleColor.Yellow : ConsoleColor.Cyan)).JoinColoredString()).JoinColoredString("\n"));
            Console.WriteLine();
            Console.WriteLine(count);
            foreach (var word in set)
                Console.WriteLine(word);
        }


        [Flags]
        enum CellContent
        {
            None = 0,
            Left = 1 << 0,
            Up = 1 << 1,
            Right = 1 << 2,
            Down = 1 << 3,

            LeftRight = Left | Right,
            UpDown = Up | Down,
            UpRight = Up | Right,
            DownRight = Down | Right,
            DownLeft = Down | Left,
            UpLeft = Up | Left
        }

        enum RoomStatus
        {
            Unvisited,
            CurrentlyIn,
            Visited,

            StartingRoomStart,
            StartingRoomOut,
            StartingRoomIn
        }

        private static void Smogon2_RegionalTours_CountryRoad(string[] grid)
        {
            var possible = new[] { CellContent.None, CellContent.LeftRight, CellContent.UpDown, CellContent.UpRight, CellContent.DownRight, CellContent.DownLeft, CellContent.UpLeft };
            var directions1 = new[] { CellContent.Left, CellContent.Right, CellContent.Up, CellContent.Down };
            var directions2 = new (int dx, int dy)[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
            var iterations = 0;

            IEnumerable<CellContent[]> recurse(CellContent[] sofar, RoomStatus[] roomsVisited, int x, int y, int dx, int dy)
            {
                if (dx == 0 && dy == 0)
                {
                    roomsVisited[grid[2 * y][2 * x + 1] - 'a'] = RoomStatus.StartingRoomStart;
                    if (x > 0)
                    {
                        sofar[x + 10 * y] = CellContent.Left;
                        foreach (var solution in recurse(sofar, roomsVisited, x - 1, y, -1, 0))
                            yield return solution;
                    }
                    if (x < 9)
                    {
                        sofar[x + 10 * y] = CellContent.Right;
                        foreach (var solution in recurse(sofar, roomsVisited, x + 1, y, 1, 0))
                            yield return solution;
                    }
                    if (y > 0)
                    {
                        sofar[x + 10 * y] = CellContent.Up;
                        foreach (var solution in recurse(sofar, roomsVisited, x, y - 1, 0, -1))
                            yield return solution;
                    }
                    if (y < 9)
                    {
                        sofar[x + 10 * y] = CellContent.Down;
                        foreach (var solution in recurse(sofar, roomsVisited, x, y + 1, 0, 1))
                            yield return solution;
                        yield break;
                    }
                }

                // Did we come back to the start of the loop?
                if (sofar[x + 10 * y] == CellContent.Left ||
                    sofar[x + 10 * y] == CellContent.Right ||
                    sofar[x + 10 * y] == CellContent.Up ||
                    sofar[x + 10 * y] == CellContent.Down)
                {
                    var prev = sofar[x + 10 * y];
                    sofar[x + 10 * y] |= dx == -1 ? CellContent.Right : dx == 1 ? CellContent.Left : dy == -1 ? CellContent.Down : CellContent.Up;

                    if (iterations == 100000)
                    {
                        iterations = 0;
                        Console.CursorLeft = 0;
                        Console.CursorTop = 0;
                        Console.WriteLine(sofar.Select(cc =>
                        {
                            switch (cc)
                            {
                                case CellContent.LeftRight: return "──";
                                case CellContent.UpDown: return "│ ";
                                case CellContent.UpRight: return "└─";
                                case CellContent.DownRight: return "┌─";
                                case CellContent.DownLeft: return "┐ ";
                                case CellContent.UpLeft: return "┘ ";
                                case CellContent.None: return "  ";
                                default: return "??";
                            }
                        }).Split(10).Select(row => row.JoinString()).JoinString("\n"));
                    }
                    iterations++;

                    // Check that none of the walls have empty spaces on both sides
                    var valid = true;
                    for (int xx = 0; xx < 10 && valid; xx++)
                        for (int yy = 0; yy < 9 && valid; yy++)
                            if (grid[2 * yy + 1][2 * xx + 1] == '-' && sofar[xx + 10 * yy] == CellContent.None && sofar[xx + 10 * (yy + 1)] == CellContent.None)
                                valid = false;
                    for (int xx = 0; xx < 9 && valid; xx++)
                        for (int yy = 0; yy < 10 && valid; yy++)
                            if (grid[2 * yy][2 * xx + 2] == '|' && sofar[xx + 10 * yy] == CellContent.None && sofar[xx + 1 + 10 * yy] == CellContent.None)
                                valid = false;

                    if (valid)
                    {
                        var player = new SoundPlayer(@"E:\Music\Themes\Theme - Looney Tunes.wav");
                        player.Play();
                        yield return sofar.ToArray();
                    }
                    sofar[x + 10 * y] = prev;
                    yield break;
                }

                // Did we crash into another part of the loop?
                if (sofar[x + 10 * y] != CellContent.None)
                    yield break;

                var orig = CellContent.None;
                switch (dx)
                {
                    case -1:
                        orig = CellContent.Right;
                        if (grid[2 * y][2 * x + 2] == '|')
                            roomsVisited[grid[2 * y][2 * x + 3] - 'a'] = roomsVisited[grid[2 * y][2 * x + 3] - 'a'] == RoomStatus.StartingRoomStart ? RoomStatus.StartingRoomOut : RoomStatus.Visited;
                        break;
                    case 1:
                        orig = CellContent.Left;
                        if (grid[2 * y][2 * x] == '|')
                            roomsVisited[grid[2 * y][2 * x - 1] - 'a'] = roomsVisited[grid[2 * y][2 * x - 1] - 'a'] == RoomStatus.StartingRoomStart ? RoomStatus.StartingRoomOut : RoomStatus.Visited;
                        break;
                    default:
                        switch (dy)
                        {
                            case -1:
                                orig = CellContent.Down;
                                if (grid[2 * y + 1][2 * x + 1] == '-')
                                    roomsVisited[grid[2 * y + 2][2 * x + 1] - 'a'] = roomsVisited[grid[2 * y + 2][2 * x + 1] - 'a'] == RoomStatus.StartingRoomStart ? RoomStatus.StartingRoomOut : RoomStatus.Visited;
                                break;
                            case 1:
                                orig = CellContent.Up;
                                if (grid[2 * y - 1][2 * x + 1] == '-')
                                    roomsVisited[grid[2 * y - 2][2 * x + 1] - 'a'] = roomsVisited[grid[2 * y - 2][2 * x + 1] - 'a'] == RoomStatus.StartingRoomStart ? RoomStatus.StartingRoomOut : RoomStatus.Visited;
                                break;
                        }
                        break;
                }
                var curRoomStatus = roomsVisited[grid[2 * y][2 * x + 1] - 'a'];
                if (roomsVisited[grid[2 * y][2 * x + 1] - 'a'] == RoomStatus.Unvisited)
                    roomsVisited[grid[2 * y][2 * x + 1] - 'a'] = curRoomStatus = RoomStatus.CurrentlyIn;
                else if (roomsVisited[grid[2 * y][2 * x + 1] - 'a'] == RoomStatus.StartingRoomOut)
                    roomsVisited[grid[2 * y][2 * x + 1] - 'a'] = curRoomStatus = RoomStatus.StartingRoomIn;
                for (int i = 0; i < 4; i++)
                    if (directions1[i] != orig)
                    {
                        var (ndx, ndy) = directions2[i];
                        // Don’t leave the grid
                        if (x + ndx > 9 || x + ndx < 0 || y + ndy > 9 || y + ndy < 0 ||
                            // Don’t walk back into an already-visited room
                            roomsVisited[grid[2 * y + 2 * ndy][2 * x + 1 + 2 * ndx] - 'a'] == RoomStatus.Visited ||
                            // Don’t leave the start room if we’ve already been in it twice
                            curRoomStatus == RoomStatus.StartingRoomIn && roomsVisited[grid[2 * y + 2 * ndy][2 * x + 1 + 2 * ndx] - 'a'] != RoomStatus.StartingRoomIn)
                            continue;
                        sofar[x + 10 * y] = orig | directions1[i];
                        foreach (var solution in recurse(sofar, roomsVisited.ToArray(), x + ndx, y + ndy, ndx, ndy))
                            yield return solution;
                        sofar[x + 10 * y] = CellContent.None;
                    }
            }

            var count = 0;
            for (int startPos = 95; startPos <= 96; startPos++)
            {
                foreach (var solution in recurse(new CellContent[100], new RoomStatus[24], startPos % 10, startPos / 10, 0, 0))
                {
                    var solutionStr = solution.Select(cc =>
                    {
                        switch (cc)
                        {
                            case CellContent.LeftRight: return "──";
                            case CellContent.UpDown: return "│ ";
                            case CellContent.UpRight: return "└─";
                            case CellContent.DownRight: return "┌─";
                            case CellContent.DownLeft: return "┐ ";
                            case CellContent.UpLeft: return "┘ ";
                            case CellContent.None: return "  ";
                            default: return "??";
                        }
                    }).Split(10).Select(row => row.JoinString()).JoinString("\n");
                    Console.WriteLine(solutionStr);
                    Console.WriteLine();
                    File.AppendAllText(@"D:\temp\RegionalTours_Solutions.txt", solutionStr + "\r\n\r\n");
                    count++;
                }
            }
            Console.WriteLine(count);
        }
    }
}
