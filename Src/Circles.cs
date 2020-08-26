using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace Qoph
{
    static class Circles
    {
        public static void Generate()
        {
            const int numDotsPerCircle = 5;

            var ltrDistances = @"TSUOSIAAG";
            var ltrRadii = @"HOTNWTSRU";   // TODO: should end with RU instead of AU
            var ltrCircumferences = @"ELIIHEPAS";       // = THE SOLUTION IS WHITE ASPARAGUS
            var len = ltrDistances.Length;

            if (ltrCircumferences.Length != len || ltrDistances.Length != len || ltrRadii.Length != len)
                Debugger.Break();

            var distances = ltrDistances.Select(d => d - 'A' + 1d).ToArray();
            var radii = ltrRadii.Select(r => r - 'A' + 1d).ToArray();
            var circumferences = ltrCircumferences.Select(c => c - 'A' + 1d).ToArray();

            var points = new List<PointD>();
            var centers = Enumerable.Range(0, len).Select(ix => distances[ix].Apply(d => new PointD(d * Math.Cos(Math.PI * 2 / len * ix), -d * Math.Sin(Math.PI * 2 / len * ix)))).ToArray();

            for (int ix = 0; ix < len; ix++)
            {
                var dotDistance = (2 * Math.PI - circumferences[ix] / radii[ix]) / (numDotsPerCircle - 1);
                var dotAngles = Enumerable.Range(0, numDotsPerCircle).Select(dIx => dotDistance * dIx).ToArray();

                var bestOffsetDistance = 0d;
                PointD[] bestPoints = null;
                for (double offset = 0; offset < 1; offset += 1d / 512)
                {
                    var pts = dotAngles.Select(angle => new PointD(radii[ix] * Math.Cos(angle + offset), -radii[ix] * Math.Sin(angle + offset)) + centers[ix]).ToArray();
                    var closest = (from pt in pts
                                   from otherCircle in Enumerable.Range(0, len)
                                   where otherCircle != ix
                                   select Math.Abs(pt.Distance(centers[otherCircle]) - radii[otherCircle])).Min();
                    if (closest > bestOffsetDistance)
                    {
                        bestOffsetDistance = closest;
                        bestPoints = pts;
                    }
                }

                points.AddRange(bestPoints);
            }

            var minX = points.Min(p => p.X);
            var maxX = points.Max(p => p.X);
            var minY = points.Min(p => p.Y);
            var maxY = points.Max(p => p.Y);
            for (var step = 0; step < 3; step++)
            {
                File.WriteAllText($@"D:\c\Qoph\DataFiles\Circles\Circles{(step > 0 ? $" (solution step {step})" : "")}.html", $@"<!DOCTYPE html>
<html>
    <head>
        <title>Circles</title>
        <style>
            html, body {{ margin: 0; padding: 0; }}
        </style>
    </head>
    <body>
        <h1 style='text-align: center'>Circles</h1>
        <p style='text-align: center; font-style: italic'>Distance, radius, circumference, widdershins.</p>
        <svg style='width: 99vw' viewBox='{minX - 2} {minY - 2} {maxX - minX + 4} {maxY - minY + 4}'>
            <g fill='none' stroke='#ccc'>
                {(step > 0 ? Enumerable.Range(0, len).Select(ix => $"<circle cx='{centers[ix].X}' cy='{centers[ix].Y}' r='{radii[ix]}' stroke='#248' stroke-width='.02' />").JoinString() : "")}
                {(step > 1 ? Enumerable.Range(0, len).Select(ix => $"<line x1='{centers[ix].X}' y1='{centers[ix].Y}' x2='0' y2='0' stroke='#822' stroke-width='.02' />").JoinString() : "")}
                <line x1='0' y1='{minY - 1}' x2='0' y2='{maxY + 1}' stroke-width='.05' />
                <line x1='{minX - 1}' y1='0' x2='{maxX + 1}' y2='0' stroke-width='.05' />
            </g>
            <path fill='#ccc' d='M .4 {minY - .5} h -.8 l .4 -1 z' />
            <path fill='#ccc' d='M {maxX + .5} .4 v -.8 l 1 .4 z' />
            {
                //points.Select(p => $"<rect x='{p.pt.X - .1}' y='{p.pt.Y - .1}' width='.2' height='.2' transform='rotate({-p.th} {p.pt.X} {p.pt.Y})' />").JoinString()
                points.OrderBy(p => p.X).Select(p => $"<circle cx='{p.X}' cy='{p.Y}' r='.1' />").JoinString()
            }
        </svg>
    </body>
</html>
");
            }
        }
    }
}
