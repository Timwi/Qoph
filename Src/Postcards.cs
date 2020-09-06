using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RT.KitchenSink.Geometry;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace Qoph
{
    static class Postcards
    {
        public static void MakeVoronoiDiagram()
        {
            static double convDeg(int deg, int min, int sec) => sec / 60.0 / 60.0 + min / 60.0 + deg;
            double latFrom = 41;
            double latTo = 37;
            double longFrom = convDeg(109, 2, 48);
            double longTo = convDeg(102, 2, 48);
            double convLong(int deg, int min, int sec) => (convDeg(deg, min, sec) - longFrom) / (longTo - longFrom) * 7;
            double convLat(int deg, int min, int sec) => (convDeg(deg, min, sec) - latFrom) / (latTo - latFrom) * 4;
            var allTowns = File.ReadAllLines(@"D:\c\Qoph\DataFiles\Postcards\Towns.txt");
            var allCoords = File.ReadAllLines(@"D:\c\Qoph\DataFiles\Postcards\Coordinates.txt");
            var data = @"Brush,Cokedale,Craig,Crawford,Fairplay,Flagler,Marble,Nucla,Pagosa Springs,Springfield,Ovid,Kim,Moffat,Silverton,Lake City,Romeo,Dinosaur,Dolores,La Junta,Ophir,Eagle,Walden,Eads,Elizabeth,Fountain,Hugo,Holly,Ridgway,Ward,Parachute"
                .Split(',')
                .Select(town => (town, match: Regex.Match(allCoords[allTowns.IndexOf(town)], @"^(\d+)°(\d+)′(\d+)″N (\d+)°(\d+)′(\d+)″W$").Groups.Cast<Group>().Skip(1).Select(gr => int.Parse(gr.Value)).ToArray()))
                .Select(tup => (tup.town, pt: new PointD(convLong(tup.match[3], tup.match[4], tup.match[5]), convLat(tup.match[0], tup.match[1], tup.match[2]))))
                .ToArray();

            var diagram = VoronoiDiagram.GenerateVoronoiDiagram(data.Select(tup => tup.pt).ToArray(), new SizeF(7, 4), VoronoiDiagramFlags.IncludeEdgePolygons);
            var svg = new StringBuilder();
            foreach (var (site, polygon) in diagram.Polygons.ToTuples())
            {
                svg.Append($@"<path d='M {polygon.Vertices.Select(p => $"{p.X},{p.Y}").JoinString(" ")} z' fill='none' stroke='black' stroke-width='.02' />");
                svg.Append($@"<circle cx='{site.X}' cy='{site.Y}' r='.025' />");
                svg.Append($@"<text x='{site.X}' y='{site.Y - .05}'>{data.First(tup => tup.pt == site).town}</text>");
            }
            File.WriteAllText(@"D:\c\Qoph\DataFiles\Postcards\Voronoi.svg", $"<svg viewBox='-.1 -.1 7.2 4.2' xmlns='http://www.w3.org/2000/svg' text-anchor='middle' font-size='.1'>{svg}</svg>");


            var allPoints = allCoords
                .Select(c => Regex.Match(c, @"^(\d+)°(\d+)′(\d+)″N (\d+)°(\d+)′(\d+)″W$").Groups.Cast<Group>().Skip(1).Select(gr => int.Parse(gr.Value)).ToArray())
                .Select(match => new PointD(convLong(match[3], match[4], match[5]), convLat(match[0], match[1], match[2])))
                .ToArray();

            diagram = VoronoiDiagram.GenerateVoronoiDiagram(allPoints, new SizeF(7, 4), VoronoiDiagramFlags.IncludeEdgePolygons);
            svg = new StringBuilder();
            foreach (var (site, polygon) in diagram.Polygons.ToTuples())
            {
                svg.Append($@"<path d='M {polygon.Vertices.Select(p => $"{p.X * 10},{p.Y * 10}").JoinString(" ")} z' fill='none' stroke='black' stroke-width='.05' />");
                svg.Append($@"<text x='{site.X * 10}' y='{(site.Y - .025) * 10}'>{allTowns[allPoints.IndexOf(site)]}</text>");
                svg.Append($@"<circle cx='{site.X * 10}' cy='{site.Y * 10}' r='.1' />");
            }
            File.WriteAllText(@"D:\c\Qoph\DataFiles\Postcards\Voronoi all.svg", $"<svg viewBox='-1 -1 72 42' xmlns='http://www.w3.org/2000/svg' text-anchor='middle' font-size='.5'>{svg}</svg>");
        }
    }
}