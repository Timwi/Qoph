using System;
using System.IO;
using System.Text;

namespace PuzzleStuff
{
    static class DrainPipe
    {
        public static void GenerateSvg()
        {
            var path = @"D:\c\PuzzleStuff\DataFiles\Drain Pipe\Output.svg";

            var dials = new (int[] dial, string color, string holeColor, double radius)[] {
                (new[] { 5, 7 }, "#008", "#08f", 1),
                (new[] { 7, 5 }, "#080", "#0f0", .9)
            };
            var svg = new StringBuilder();
            var ix = 0;
            foreach (var (dial, color, holeColor, radius) in dials)
            {
                svg.Append($@"<circle cx='{2 * ix}' fill='{color}' r='{radius}' />");
                for (int d = 0; d < dial.Length; d++)
                {
                    double ds = dial.Length + 1;
                    for (int h = 0; h < dial[d]; h++)
                        svg.Append($@"<circle cx='{2 * ix + (d + 1) / ds * Math.Cos(2 * Math.PI * h / dial[d])}' cy='{(d + 1) / ds * Math.Sin(2 * Math.PI * h / dial[d])}' r='.04' fill='{holeColor}' />");
                }
                ix++;
            }
            File.WriteAllText(path, $@"<svg viewBox='-1.1 -1.1 2.2 2.2'>{svg}</svg>");
        }
    }
}