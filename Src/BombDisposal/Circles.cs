using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace PuzzleStuff.BombDisposal
{
    static class Circles
    {
        public static void Generate()
        {
            var distances = @"ABCDEFGHI";   // for sorting
            var len = distances.Length;
            var radii = @"SAAHTSEIE";
            var angles = @"PRNOACFCN";
            var angleMult = 4;
            var tap = @"EMRSTOFIT";     // = SPEARMANRHOSTATSCOEFFICIENT (= CORRELATION)

            var rnd = new Random(1);

            if (tap.Length != len || angles.Length != len || distances.Length != len || radii.Length != len)
                Debugger.Break();

            var cumulativeAngles = Enumerable.Range(0, angles.Length).Select(aIx => angles.Take(aIx).Select(a => a - 'A' + 1).Sum() * angleMult).ToArray();
            var points = new List<(PointD pt, int th)>();
            var centers = new List<PointD>();

            var offsets = new int?[]
            {
                120,
                -100,
                30,
                50,
                10,
                0,
                0,
                40,
                -90
            };
            if (offsets.Length != len)
                Debugger.Break();
            var highlight = offsets.IndexOf(value => value == null);

            for (int ix = 0; ix < len; ix++)
            {
                var (_, row, col) = TapCode.TapFromCh[tap[ix]];
                var dotAngles = Enumerable.Range(0, row).Select(r => 20 * r).ToList();
                dotAngles.AddRange(Enumerable.Range(0, col).Select(c => 20 * row + 60 + 20 * c));
                var center = (distances[ix] - 'A' + 1).Apply(d => (cumulativeAngles[ix] * Math.PI / 180).Apply(θ => new PointD(d * Math.Cos(θ), d * Math.Sin(θ))));
                centers.Add(center);
                var radius = radii[ix] - 'A' + 1;
                var offset = offsets[ix] ?? 0;
                foreach (var angle in dotAngles)
                    points.Add((new PointD(radius * Math.Cos((angle + offset) * Math.PI / 180), radius * Math.Sin((angle + offset) * Math.PI / 180)) + center, angle + offset));
            }

            for (int i = 0; i < points.Count; i++)
                points[i] = (new PointD(points[i].pt.X, -points[i].pt.Y), points[i].th);

            var minX = points.Min(p => p.pt.X);
            var maxX = points.Max(p => p.pt.X);
            var minY = points.Min(p => p.pt.Y);
            var maxY = points.Max(p => p.pt.Y);
            for (var step = 0; step < 3; step++)
            {
                File.WriteAllText($@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\Circles\Circles{(step > 0 ? step.ToString() : "")}.html", $@"<!DOCTYPE html>
<html>
    <head>
        <title>Circles</title>
        <style>
            html, body {{ margin: 0; padding: 0; }}
        </style>
    </head>
    <body>
        <h1 style='text-align: center; border-bottom: 1px solid #ccc'>Circles</h1>
        <p style='text-align: center; font-style: italic'>Center to origin, widdershins.</p>
        <svg style='width: 99vw' viewBox='{minX - 2} {minY - 2} {maxX - minX + 4} {maxY - minY + 4}'>
            {points.Select(p => $"<rect x='{p.pt.X - .1}' y='{p.pt.Y - .1}' width='.2' height='.2' transform='rotate({-p.th} {p.pt.X} {p.pt.Y})' />").JoinString()}
            <g fill='none' stroke='black'>
                {(step > 0 ? Enumerable.Range(0, len).Select(ix => $"<circle cx='{centers[ix].X}' cy='{-centers[ix].Y}' r='{radii[ix] - 'A' + 1}' stroke='{(ix == highlight ? "#F00" : "#248")}' stroke-width='{(ix == highlight ? ".1" : ".02")}' />").JoinString() : "")}
                {(step > 1 ? Enumerable.Range(0, len).Select(ix => $"<line x1='{centers[ix].X}' y1='{-centers[ix].Y}' x2='0' y2='0' stroke='#822' stroke-width='.02' />").JoinString() : "")}
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
}
