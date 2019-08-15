using System;
using System.IO;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.Json;

namespace PuzzleStuff
{
    static class Prefectures
    {
        public static void Doodle()
        {
            var json = JsonList.Parse(File.ReadAllText(@"D:\c\PuzzleStuff\DataFiles\Prefectures\Prefectures.json"));
            var obj = new object();
            json.ParallelForEach(8, jv =>
            {
                var url = jv[0].GetString();
                var name = jv[1].GetString();
                url = Regex.Replace(url, @"/\d+px-", "/1024px-");
                var h = new HClient();
                File.WriteAllBytes($@"D:\c\PuzzleStuff\DataFiles\Prefectures\{name}.png", h.Get(url).Data);
                lock (obj)
                    Console.WriteLine(name);
            });
            Console.WriteLine("All done!");
        }
    }
}