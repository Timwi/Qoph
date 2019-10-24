using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace PuzzleStuff.Esperanto
{
    static class Deorigine
    {
        public static void Do()
        {
            var solvo = @"ĈINUJO";
            var trompo = @"KMERŜA";
            var rnd = new Random(10);
            var solvPunktoj = new PointD[] { new PointD(19.06, 15.28), new PointD(15.40, 14.16), new PointD(14.70, 6.66), new PointD(8.35, 9.72), new PointD(5.08, 19.26), new PointD(13.53, 2.5) };
            //Ut.NewArray(solvo.Length, i => new PointD(1 + rnd.NextDouble() * 19, 1 + rnd.NextDouble() * 19));
            Clipboard.SetText($@"new PointD[]{{ {solvPunktoj.Select(p => $"new PointD({p.X:0.00},{p.Y:0.00})").JoinString(", ")} }}");
            var indikFaktoroj = new double[] { 10.64, 13.77, 19.66, 6.24, 2.25, 4.71 };
            //Ut.NewArray(solvo.Length, i => 1 + rnd.NextDouble() * 19);
            Clipboard.SetText($@"new double[]{{ {indikFaktoroj.Select(f => $"{f:0.00}").JoinString(", ")} }}");
            var indikPunktoj = Ut.NewArray(solvo.Length, i => solvPunktoj[i].Unit() * indikFaktoroj[i]);
            var trompPunktoj = new PointD[] { new PointD(19.01, 4.55), new PointD(3.77, 6.73), new PointD(13.96, 1.00), new PointD(13.92, 19.20), new PointD(11.10, 17.79), new PointD(6.87, 3.56) };
            //Ut.NewArray(solvo.Length, i => new PointD(1 + rnd.NextDouble() * 19, 1 + rnd.NextDouble() * 19));
            Clipboard.SetText($@"new PointD[]{{ {trompPunktoj.Select(p => $"new PointD({p.X:0.00},{p.Y:0.00})").JoinString(", ")} }}");
            File.WriteAllText(@"D:\temp\temp.svg", $@"
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='-1.5 -.5 24 23' text-anchor='middle' font-size='1' font-family='Trebuchet MS'>
                    {solvPunktoj.Select((p, ix) => $"<circle cx='{p.X}' cy='{21 - p.Y}' r='.1' /><text x='{p.X}' y='{21 - p.Y - .3}'>{solvo[ix]}</text>").JoinString()}
                    {indikPunktoj.Select((p, ix) => $"<circle cx='{p.X}' cy='{21 - p.Y}' r='.1' /><text x='{p.X}' y='{21 - p.Y - .3}'>{ix + 1}</text>").JoinString()}
                    {trompPunktoj.Select((p, ix) => $"<circle cx='{p.X}' cy='{21 - p.Y}' r='.1' /><text x='{p.X}' y='{21 - p.Y - .3}'>{trompo[ix]}</text>").JoinString()}
                    <line x1='-1' y1='21' x2='21' y2='21' fill='none' stroke='black' stroke-width='.05' />
                    <line x1='0' y1='22' x2='0' y2='1' fill='none' stroke='black' stroke-width='.05' />
                    <path d='M21,20.5 22,21 21,21.5z M-.5,1 0,0 .5,1' />
                </svg>");
        }
    }
}