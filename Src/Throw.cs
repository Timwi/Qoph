using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace Qoph
{
    static class Throw
    {
        enum Bed
        {
            Bullseye,
            Bullring,
            SingleInner,
            Treble,
            SingleOuter,
            Double
        }

        public static void Generate()
        {
            var segmentValues = new[] { 20, 1, 18, 4, 13, 6, 10, 15, 2, 17, 3, 19, 7, 16, 8, 11, 14, 9, 12, 5 };
            var players = @"
GLEDURRANT
1. G: 7/SingleInner; 7/SingleOuter
2. L: 4/Treble; 6/Double; 12/SingleInner; 12/SingleOuter
3. E: 5/SingleOuter; 5/SingleInner
4. D: 2/Double; 4/SingleInner; 4/SingleOuter
5. U: 7/Treble
6. R: 9/Double; 6/Treble; 18/SingleOuter; 18/SingleInner
7. R: 6/Treble; 18/SingleInner; 18/SingleOuter
8. A: 1/SingleInner; 1/SingleOuter
9. N: 14/SingleOuter; 7/Double; 14/SingleInner
10. T: 10/Double; 20/SingleOuter; 20/SingleInner

KYLANDERSON
1. K: 11/SingleOuter; 11/SingleInner
2. Y: 25/Bullring
3. L: 4/Treble; 12/SingleInner; 12/SingleOuter; 6/Double
4. A: 1/SingleOuter; 1/SingleInner
5. N: 7/Double; 14/SingleInner; 14/SingleOuter
6. D: 2/Double; 4/SingleInner; 4/SingleOuter
7. E: 5/SingleInner; 5/SingleOuter
8. R: 6/Treble; 18/SingleInner; 18/SingleOuter; 9/Double
9. S: 19/SingleInner; 19/SingleOuter
10. O: 5/Treble; 15/SingleInner; 15/SingleOuter
11. N: 14/SingleInner; 14/SingleOuter

ARKFROST
1. A: 1/SingleOuter; 1/SingleInner
2. R: 18/SingleInner; 6/Treble; 18/SingleOuter; 9/Double
3. K: 11/SingleInner; 11/SingleOuter
4. F: 6/SingleInner; 6/SingleOuter; 3/Double; 2/Treble
5. R: 9/Double; 18/SingleInner; 18/SingleOuter
6. O: 15/SingleOuter; 15/SingleInner; 5/Treble
7. S: 19/SingleOuter; 19/SingleInner
8. T: 20/SingleInner; 20/SingleOuter; 10/Double

RBCROSS
1. R: 6/Treble; 9/Double; 18/SingleInner; 18/SingleOuter
2. B: 2/SingleInner; 2/SingleOuter; 1/Double
3. C: 1/Treble; 3/SingleInner; 3/SingleOuter
4. R: 9/Double; 18/SingleInner; 18/SingleOuter
5. O: 5/Treble; 15/SingleInner; 15/SingleOuter
6. S: 19/SingleInner
7. S: 19/SingleOuter

STEHENBUNTING
1. S: 19/SingleOuter; 19/SingleInner
2. T: 20/SingleOuter; 10/Double
3. E: 5/SingleInner
4. H: 8/SingleOuter; 8/SingleInner; 4/Double
5. E: 5/SingleOuter
6. N: 7/Double
7. B: 2/SingleInner; 2/SingleOuter; 1/Double
8. U: 7/Treble
9. N: 14/SingleOuter
10. T: 10/Double; 20/SingleInner
11. I: 3/Treble; 9/SingleInner; 9/SingleOuter
12. N: 14/SingleInner
13. G: 7/SingleInner; 7/SingleOuter

TONECCLES
1. T: 10/Double; 20/SingleOuter; 20/SingleInner
2. O: 15/SingleInner; 15/SingleOuter; 5/Treble
3. N: 14/SingleOuter; 14/SingleInner; 7/Double
4. E: 5/SingleOuter
5. C: 1/Treble; 3/SingleInner; 3/SingleOuter
6. C: 3/SingleOuter; 1/Treble
7. L: 4/Treble; 12/SingleInner; 12/SingleOuter; 6/Double
8. E: 5/SingleInner
9. S: 19/SingleInner; 19/SingleOuter
".Replace("'\r", "").Split('\n')
                .Select(str => new { Line = str, Match = Regex.Match(str.Trim(), @"^\d+\. .: (\d+)/([A-Za-z]+)(;|$)") })
                .GroupConsecutiveBy(inf => inf.Match.Success)
                .Where(gr => gr.Key)
                .Select(gr => gr.Select(inf => (segment: Array.IndexOf(segmentValues, int.Parse(inf.Match.Groups[1].Value)), bed: EnumStrong.Parse<Bed>(inf.Match.Groups[2].Value))).ToArray())
                .ToArray();

            var segments = Ut.NewArray(
                @"M 2.5,0 A 2.5,2.5 0 0 1 0,2.5 2.5,2.5 0 0 1 -2.5,0 2.5,2.5 0 0 1 0,-2.5 2.5,2.5 0 0 1 2.5,0 Z",  // bullseye
                @"M 0,-6.25 A 6.25,6.25 0 0 0 -6.25,0 6.25,6.25 0 0 0 0,6.25 6.25,6.25 0 0 0 6.25,0 6.25,6.25 0 0 0 0,-6.25 Z M 0,-2.5 A 2.5,2.5 0 0 1 2.5,0 2.5,2.5 0 0 1 0,2.5 2.5,2.5 0 0 1 -2.5,0 2.5,2.5 0 0 1 0,-2.5 Z",   // bullring
                @"m 0,-38.5 a 38.5,38.5 0 0 0 -6.019531,0.49609 l 5.042969,31.83594 A 6.25,6.25 0 0 1 0,-6.25 6.25,6.25 0 0 1 0.976562,-6.166 L 6.015625,-37.98047 A 38.5,38.5 0 0 0 0,-38.5 Z", // singles (1)
                @"m 0,-42 a 42,42 0 0 0 -6.5625,0.56836 l 0.542969,3.42773 A 38.5,38.5 0 0 1 0,-38.5 a 38.5,38.5 0 0 1 6.015625,0.51953 l 0.550781,-3.47851 A 42,42 0 0 0 0,-42 Z", // doubles
                @"m 0,-63.5 a 63.5,63.5 0 0 0 -9.921875,0.85742 l 3.355469,21.1836 A 42,42 0 0 1 0,-42 a 42,42 0 0 1 6.5625,0.56836 l 3.365234,-21.25 A 63.5,63.5 0 0 0 0,-63.5 Z", // singles (2)
                @"m 0,-67 a 67,67 0 0 0 -10.46875,0.90625 l 0.541016,3.41211 A 63.5,63.5 0 0 1 0,-63.5 a 63.5,63.5 0 0 1 9.921875,0.85742 l 0.552734,-3.49414 A 67,67 0 0 0 0,-67 Z" // trebles
            );

            var rnd = new Random(47);
            var svgs = new StringBuilder();

            foreach (var player in players)
            {
                var svg = new StringBuilder();
                for (var ix = 0; ix < player.Length; ix++)
                {
                    var (segment, bed) = player[ix];
                    //var num = player[ix] - 'A' + 1;
                    //var representations = new List<(int? segment, Bed bed)>();
                    //if (num >= 1 && num <= 20)
                    //{
                    //    representations.Add((segment: segmentValues.IndexOf(num), bed: Bed.SingleInner));
                    //    representations.Add((segment: segmentValues.IndexOf(num), bed: Bed.SingleOuter));
                    //}
                    //if (num % 2 == 0 && num / 2 <= 20)
                    //    representations.Add((segment: segmentValues.IndexOf(num / 2), bed: Bed.Double));
                    //if (num % 3 == 0 && num / 3 <= 20)
                    //    representations.Add((segment: segmentValues.IndexOf(num / 3), bed: Bed.Treble));
                    //if (num == 25)
                    //    representations.Add((segment: null, bed: Bed.Bullring));
                    //representations.RemoveAll(rep => elements.Contains(rep));

                    svg.Append($"<path fill='#{"{0}0{0}0{0}0".Fmt("123456789ABCDEF"[ix])}' d='{segments[(int) bed]}'{(segment != -1 ? $" transform='rotate({18 * segment})'" : "")} />");
                }
                svgs.AppendLine($@"<svg viewBox='-68 -68 136 136'>{svg}</svg>");
            }
            General.ReplaceInFile(@"D:\c\Qoph\EnigmorionFiles\throw.html", "<!--%%-->", "<!--%%%-->", svgs.ToString());
        }
    }
}