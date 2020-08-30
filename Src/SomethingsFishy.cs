using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace Qoph
{
    static class SomethingsFishy
    {
        // 0 = up; going clockwise
        private static readonly (int left, int right)[] _semaphoreOrientations = new[] { (5, 4), (6, 4), (7, 4), (0, 4), (4, 1), (4, 2), (4, 3), (6, 5), (5, 7), (0, 2), (5, 0), (5, 1), (5, 2), (5, 3), (6, 7), (6, 0), (6, 1), (6, 2), (6, 3), (7, 0), (7, 1), (0, 3), (1, 2), (1, 3), (7, 2), (3, 2) };

        public unsafe static void Generate()
        {
            const string solution = @"AMPHIBIA";
            const int w = 6;
            const int h = 5;
            var directions = new (int dx, int dy)[] { (0, -1), (1, -1), (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1) }.Select(d => (dx: 2 * d.dx, dy: 2 * d.dy)).ToArray();

            IEnumerable<string[]> recurse(string[] sofar, int solutionIx)
            {
                if (solutionIx == solution.Length)
                {
                    yield return sofar;
                    yield break;
                }

                var (left, right) = _semaphoreOrientations[solution[solutionIx] - 'A'];
                var (ldx, ldy) = directions[left];
                var (rdx, rdy) = directions[right];

                for (int x = 0; x < w; x++)
                    if (x + ldx >= 0 && x + ldx < w && x + rdx >= 0 && x + rdx < w)
                        for (int y = 0; y < h; y++)
                            if (y + ldy >= 0 && y + ldy < h && y + rdy >= 0 && y + rdy < h)
                            {
                                if (sofar[x + w * y] != null || sofar[x + ldx + w * (y + ldy)] != null || sofar[x + rdx + w * (y + rdy)] != null)
                                    continue;
                                var sofarCopy = (string[]) sofar.Clone();
                                var ch = (char) (solutionIx + 'A');
                                sofarCopy[x + w * y] = ch + "1";
                                sofarCopy[x + ldx + w * (y + ldy)] = ch + "2";
                                sofarCopy[x + rdx + w * (y + rdy)] = ch + "3";
                                foreach (var sol in recurse(sofarCopy, solutionIx + 1))
                                    yield return sol;
                            }
            }

            var result = recurse(new string[w * h], 0)
                // no adjacent blank spaces
                .Where(arr => !Enumerable.Range(0, w * h).Any(i => (arr[i] == null && (i % w < w - 1) && arr[i + 1] == null) || (arr[i] == null && (i / w < h - 1) && arr[i + w] == null)))
                .ToArray()
                .Shuffle(new Random(47))
                .First();
            ConsoleUtil.WriteLine(result.Split(w).Select(row => row.Select(v => (v ?? "").PadLeft(3).Color(ConsoleColor.White, (ConsoleColor) " ABCDEFGH".IndexOf((v ?? " ")[0]))).JoinColoredString(" ")).JoinColoredString("\n"));
            Console.WriteLine($"Red herrings needed: {w * h - 3 * solution.Length}");
            var redHerring = 1;
            File.WriteAllText(@"D:\c\Qoph\DataFiles\Something’s Fishy\Something’s Fishy.html", $@"
<html>
<head>
    <style>
        td {{ text-align: center; vertical-align: center; padding: 0; }}
    </style>
</head>
<body>
    <table style='border-spacing: .2cm'>
        {(result.Select(name => (name ?? $"R{redHerring++}").Apply(filename =>
            {
                var path = $@"D:\c\Qoph\DataFiles\Something’s Fishy\{filename}.jpg";
                var byteData = File.ReadAllBytes(path);
                using var bitmap = new Bitmap(path);
                var rect = GraphicsUtil.FitIntoMaintainAspectRatio(bitmap.Size, new Rectangle(0, 0, 200, 150));
                var s = bitmap.Width * bitmap.Height;
                return $"<td style='background: #acd; border: 5px solid {(filename.EndsWith("1") && !filename.StartsWith("R") ? "#4499ff" : "transparent")}'><img width='{rect.Width}' height='{rect.Height}' src='data:image/jpg;base64,{Convert.ToBase64String(byteData)}' /></td>";
            })).Split(w).Select(row => $"<tr>{row.JoinString()}</tr>").JoinString())}
    </table>
</body>
</html>
");
        }

        public static void ExamineGraphics()
        {
            var tt = new TextTable { ColumnSpacing = 2 };
            var row = 0;
            foreach (var file in new DirectoryInfo(@"D:\c\Qoph\DataFiles\Something’s Fishy").EnumerateFiles("*.jpg"))
            {
                using var bmp = new Bitmap(file.FullName);
                tt.SetCell(0, row, Path.GetFileNameWithoutExtension(file.Name));
                tt.SetCell(1, row, bmp.Width.ToString(), alignment: HorizontalTextAlignment.Right);
                tt.SetCell(2, row, bmp.Height.ToString(), alignment: HorizontalTextAlignment.Right);
                tt.SetCell(3, row, $"{bmp.Width / (double) bmp.Height:0.##}", alignment: HorizontalTextAlignment.Right);
                row++;
            }
            tt.WriteToConsole();
        }
    }
}