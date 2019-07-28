using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace PuzzleStuff
{
    static class Circles
    {
        public static void Generate()
        {
            const int len = 31;
            var morse = @"GUYLOMBARDOANDHISROYALCANADIANS";
            var angles = @"READDOTSONPERIMETERSASMORSECODE";
            var distances = @"JOINCENTERSTOORIGINANDSEEANGLES";
            var radii = @"SEEDISTANCESFROMCENTERSTOORIGIN";

            if (morse.Length != len || angles.Length != len || distances.Length != len || radii.Length != len)
                Debugger.Break();

            var cumulativeAngles = Enumerable.Range(0, angles.Length).Select(aIx => angles.Take(aIx).Select(a => a - 'A' + 1).Sum()).ToArray();
            var points = new List<PointD>();
            var centers = new List<PointD>();
            var longestMorseLength = 0;
            var longestMorse = "";
            for (int ix = 0; ix < len; ix++)
            {
                var mc = MorseCode.MorseFromCh[morse[ix].ToString()];
                var dotAngles = new List<int> { 0 };
                for (int i = 0; i < mc.Length; i++)
                    dotAngles.Add(dotAngles.Last() + (mc[i] == '-' ? 90 : 30));
                if (dotAngles.Last() > longestMorseLength)
                {
                    longestMorseLength = dotAngles.Last();
                    longestMorse = mc;
                }
                var center = (distances[ix] - 'A' + 1).Apply(d => (cumulativeAngles[ix] * Math.PI / 180).Apply(th => new PointD(d * Math.Cos(th), d * Math.Sin(th))));
                centers.Add(center);
                var radius = radii[ix] - 'A' + 1;
                foreach (var angle in dotAngles)
                    points.Add(new PointD(radius * Math.Cos(angle * Math.PI / 180), radius * Math.Sin(angle * Math.PI / 180)) + center);
            }

            Console.WriteLine($"{longestMorseLength} ({longestMorse})");

            for (int i = 0; i < points.Count; i++)
                points[i] = new PointD(points[i].X, -points[i].Y);

            var minX = points.Min(p => p.X);
            var maxX = points.Max(p => p.X);
            var minY = points.Min(p => p.Y);
            var maxY = points.Max(p => p.Y);
            //{Enumerable.Range(0, len).Select(ix => $"<circle cx='{centers[ix].X}' cy='{-centers[ix].Y}' r='{radii[ix] - 'A' + 1}' stroke='{(ix == 14 ? "red" : "black")}' stroke-width='.02' />").JoinString()}
            File.WriteAllText(@"D:\temp\temp2.html", $@"<!DOCTYPE html>
<html>
    <head>
        <title>Circles</title>
        <style>
            html, body {{ margin: 0; padding: 0; }}
        </style>
    </head>
    <body>
        <h1 style='text-align: center; border-bottom: 1px solid #ccc'>Circles</h1>
        <p style='text-align: center; font-style: italic'>Circles have radii.</p>
        <svg style='width: 99vw' viewBox='{minX - 2} {minY - 2} {maxX - minX + 4} {maxY - minY + 4}'>
            {points.Select(p => $"<circle cx='{p.X}' cy='{p.Y}' r='.1' />").JoinString()}
            <g fill='none' stroke='black'>
                <line x1='0' y1='{minY - 1}' x2='0' y2='{maxY + 1}' stroke-width='.05' />
                <line x1='{minX - 1}' y1='0' x2='{maxX + 1}' y2='0' stroke-width='.05' />
            </g>
            <path d='M .4 {minY - .5} h -.8 l .4 -1 z' />
            <path d='M {maxX + .5} .4 v -.8 l 1 .4 z' />
        </svg>
    </body>
</html>
");
        }
    }
}