using System;
using System.IO;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace Qoph
{
    static class Drumheads
    {
        public static void GenerateHTML()
        {
            General.ReplaceInFile(@"D:\c\Qoph\EnigmorionFiles\drumheads.html", "<!--%%-->", "<!--%%%-->", Enumerable.Range(0, 10).Select(ix => $"<img src='data:image/jpeg;base64,{Convert.ToBase64String(File.ReadAllBytes($@"D:\c\Qoph\DataFiles\Drumheads\{ix + 1}.jpg"))}' />").JoinString());
        }
    }
}