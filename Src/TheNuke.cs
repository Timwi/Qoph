using System.Linq;
using RT.Util.ExtensionMethods;

namespace Qoph
{
    static class TheNuke
    {
        public static void Generate()
        {
            General.ReplaceInFile(@"D:\c\Qoph\EnigmorionFiles\the-nuke.html", "<!--@@-->", "<!--@@@-->", Enumerable.Range(0, 12)
                .Select(i => $"<circle cx='-.7' cy='{2 * i}' r='.5' fill='{(i % 2 != 0 ? "none" : "black")}'/><circle cx='.7' cy='{2 * i}' r='.5' fill='{(i % 2 == 0 ? "none" : "black")}'/>")
                .JoinString());
            General.ReplaceInFile(@"D:\c\Qoph\EnigmorionFiles\the-nuke.html", "<!--%%-->", "<!--%%%-->", Enumerable.Range(0, 12)
                .Select(i => $"<text x='-1.6' y='{2 * i + .2}'>The</text>")
                .JoinString());
        }
    }
}