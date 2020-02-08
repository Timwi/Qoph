using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Serialization;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff
{
    static class PuzzleBoat
    {
        sealed class ElephantClue
        {
            public string Id;
            public int[] Numbers;
            public string Answer;
            public ElephantClue(string id, int[] numbers, string answer)
            {
                Id = id;
                Numbers = numbers;
                Answer = answer;
            }
            private ElephantClue() { }  // for Classify
        }

        public static void Elephant_PrepareFile()
        {
            var lines = File.ReadAllLines(@"D:\Daten\Puzzles\Puzzle Boat\Elephant.txt");
            var clues = new List<ElephantClue>();
            foreach (var line in lines)
            {
                var m = Regex.Match(line, @"^(.*?)\.\s*(( \d+)+)(.+)?$");
                Console.WriteLine($"{m.Groups[1].Value} = {m.Groups[2].Value.Trim().Split(' ').JoinString("/")}{(m.Groups[4].Success ? $" = {m.Groups[4].Value.Trim()}" : "")}");
                var numbers = m.Groups[2].Value.Trim().Split(' ').Select(int.Parse).ToArray();
                clues.Add(new ElephantClue(m.Groups[1].Value.Trim(), numbers, m.Groups[4].Success ? m.Groups[4].Value.Trim() : new string('.', numbers.Length)));
            }
            ClassifyJson.SerializeToFile(clues, @"D:\Daten\Puzzles\Puzzle Boat\Elephant.json");
        }

        public static void Elephant_Run()
        {
            var clues = ClassifyJson.DeserializeFile<List<ElephantClue>>(@"D:\Daten\Puzzles\Puzzle Boat\Elephant.json");
            var highlighted = new List<string>();
            var showingNumbers = false;
            while (true)
            {
                static int trix(int number) => number > 206 ? number + 3 : number - 1;
                ClassifyJson.SerializeToFile(clues, @"D:\Daten\Puzzles\Puzzle Boat\Elephant.json");

                Console.Clear();

                // Output grid
                var characters = Ut.NewArray(450, _ => '_');
                characters[206] = '1';
                characters[207] = '9';
                characters[208] = '7';
                characters[209] = '5';
                var clueIds = Ut.NewArray(450, _ => "_");
                foreach (var cl in clues)
                    for (var ii = 0; ii < cl.Numbers.Length; ii++)
                    {
                        characters[trix(cl.Numbers[ii])] = cl.Answer[ii];
                        clueIds[trix(cl.Numbers[ii])] = cl.Id;
                    }

                string extraHighlight = null;
                ConsoleColor bkg(int ix) => clueIds[ix] == extraHighlight ? ConsoleColor.DarkYellow : highlighted.Contains(clueIds[ix]) ? ConsoleColor.DarkCyan : characters[ix] == '.' ? ConsoleColor.DarkMagenta : ConsoleColor.DarkBlue;
                (ConsoleColoredString[] grid1, ConsoleColoredString[] grid2, ConsoleColoredString[] grid3) getGrids()
                {
                    var grid1 = Enumerable.Range(0, 450).Split(18).Select(row => row.Select(ix => $" {characters[ix]} ".Color(ConsoleColor.White, bkg(ix))).JoinColoredString()).ToArray();
                    var grid2 = Enumerable.Range(0, 450).Split(18).Select(row => row.Select(ix => $" {(ix < 206 ? (ix + 1).ToString() : ix < 210 ? "" : (ix - 3).ToString()).PadLeft(3)} ".Color(ConsoleColor.White, bkg(ix))).JoinColoredString()).ToArray();
                    var grid3 = Enumerable.Range(0, 450).Split(18).Select(row => row.Select(ix => $" {clueIds[ix].PadLeft(2)} ".Color(ConsoleColor.White, bkg(ix))).JoinColoredString()).ToArray();
                    return (grid1, grid2, grid3);
                }
                var (g1, g2, g3) = getGrids();
                ConsoleUtil.WriteLine(g1.Zip(showingNumbers ? g2 : g3, (l1, l2) => l1 + "     " + l2).JoinColoredString("\n"));
                var command = Console.ReadLine();
                if (command == "exit")
                    break;

                if (command == "gr")
                {
                    showingNumbers = !showingNumbers;
                    continue;
                }

                if (command == "list")
                {
                    foreach (var c in clues)
                        Console.WriteLine($"{c.Id}. {c.Answer} ({c.Answer.Length} letters)");
                    Console.ReadLine();
                    continue;
                }

                var m2 = Regex.Match(command, @"^(\d+)=(.)$");
                if (m2.Success && int.TryParse(m2.Groups[1].Value, out var i) && m2.Groups[2].Value.Length == 1)
                {
                    var clue2 = clues.FirstOrDefault(c => c.Numbers.Contains(i));
                    if (clue2 == null)
                        continue;
                    var pos = clue2.Numbers.IndexOf(i);
                    clue2.Answer = clue2.Answer.Remove(pos, 1).Insert(pos, m2.Groups[2].Value.ToUpperInvariant());
                    continue;
                }

                m2 = Regex.Match(command, @"^(\d+)-(\d+)=(.+)$");
                if (m2.Success && int.TryParse(m2.Groups[1].Value, out i) && int.TryParse(m2.Groups[2].Value, out var j) && i < j && m2.Groups[3].Value.Length == j - i + 1)
                {
                    for (var nm = i; nm <= j; nm++)
                    {
                        var clue2 = clues.FirstOrDefault(c => c.Numbers.Contains(nm));
                        if (clue2 == null)
                            continue;
                        var pos = clue2.Numbers.IndexOf(nm);
                        clue2.Answer = clue2.Answer.Remove(pos, 1).Insert(pos, m2.Groups[3].Value.ToUpperInvariant()[nm - i].ToString());
                    }
                    continue;
                }

                var clue = clues.FirstOrDefault(c => c.Id.EqualsNoCase(command));
                if (clue != null)
                {
                    Console.Clear();
                    extraHighlight = clue.Id;
                    var (gr1, gr2, _) = getGrids();
                    ConsoleUtil.WriteLine(gr1.Zip(gr2, (l1, l2) => l1 + "     " + l2).JoinColoredString("\n"));
                    extraHighlight = null;
                    Console.WriteLine($"Clue: {clue.Id}");

                    tryAgain:
                    ConsoleUtil.WriteLine($"{clue.Numbers.Select((n, ix) => $" {n.ToString().PadLeft(3)}".Color(ConsoleColor.White, bkg(trix(clue.Numbers[ix])))).JoinColoredString()} ({clue.Answer.Length} letters)", null);
                    ConsoleUtil.WriteLine($"{clue.Answer.Select((ch, ix) => $" {ch.ToString().PadLeft(3)}".Color(ConsoleColor.White, bkg(trix(clue.Numbers[ix])))).JoinColoredString()} ({clue.Answer.Length} letters)", null);
                    var newAnswer = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(newAnswer))
                        continue;
                    if (newAnswer.Length != clue.Answer.Length)
                    {
                        Console.WriteLine($"Wrong length");
                        goto tryAgain;
                    }
                    clue.Answer = newAnswer.ToUpperInvariant();
                    continue;
                }

                var m = Regex.Match(command, @"^h (.*)$");
                if (m.Success && (clue = clues.FirstOrDefault(c => c.Id.EqualsNoCase(m.Groups[1].Value))) != null)
                {
                    if (highlighted.Contains(clue.Id))
                        highlighted.Remove(clue.Id);
                    else
                        highlighted.Add(clue.Id);
                    continue;
                }
            }
        }
    }
}
