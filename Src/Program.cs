using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using RT.PostBuild;

[assembly: AssemblyTitle("PuzzleStuff")]
[assembly: AssemblyDescription("Contains some ancillary code used in the creation of puzzle hunts.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("PuzzleStuff")]
[assembly: AssemblyCopyright("Copyright © Timwi 2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("95055383-2e25-42be-97b7-e1411a695e1d")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace PuzzleStuff
{
    partial class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
                Console.InputEncoding = Encoding.UTF8;
            }
            catch { }

            if (args.Length == 2 && args[0] == "--post-build-check")
                return PostBuildChecker.RunPostBuildChecks(args[1], Assembly.GetExecutingAssembly());


            BombDisposal.BattleshipsAndPitfalls.Do();


            Console.WriteLine("Done.");
            Console.ReadLine();
            return 0;
        }
    }
}
