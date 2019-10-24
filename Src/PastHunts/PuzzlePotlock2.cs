using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PuzzleStuff
{
    static class PuzzlePotlock2
    {
        public static void MurderWeapon()
        {
            var lines = File.ReadAllLines(@"D:\temp\Access.log");
            Console.WriteLine(lines.First());
            Console.WriteLine(lines.Last());
            var numHtml = 0;
            var numPdf = 0;
            var distinctHtml = new HashSet<string>();
            var distinctPdf = new HashSet<string>();
            int pos;
            foreach (var line in lines)
                if (!line.EndsWith("/"))
                {
                    if ((pos = line.IndexOf(@"Get https://ktane.timwi.de/HTML/")) != -1 && line.EndsWith(".html"))
                    {
                        numHtml++;
                        distinctHtml.Add(line.Substring(pos));
                    }
                    else if ((pos = line.IndexOf(@"Get https://ktane.timwi.de/PDF/")) != -1)
                    {
                        numPdf++;
                        distinctPdf.Add(line.Substring(pos));
                    }
                }
            Console.WriteLine($@"HTML accesses: {numHtml} ({distinctHtml.Count} distinct)");
            Console.WriteLine($@"PDF accesses: {numPdf} ({distinctPdf.Count} distinct)");
        }
    }
}