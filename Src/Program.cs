using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using RT.Util;
using RT.Util.ExtensionMethods;

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
                return Ut.RunPostBuildChecks(args[1], Assembly.GetExecutingAssembly());

            //BombDisposal.PolyhedralPuzzle_GenerateVertexClues();

            var sudoku = GridGenerator.GenerateSudoku(3);
            Console.WriteLine(sudoku.Split(9).Select(row => row.JoinString(" ")).JoinString("\n"));
            Console.WriteLine();
            var givenIxs = Enumerable.Range(0, 81).ToArray().Shuffle();
            var givens = Ut.ReduceRequiredSet(givenIxs, skipConsistencyTest: true, test: set =>
            {
                var arr = set.SetToTest.ToArray();
                Console.WriteLine(Enumerable.Range(0, 81).Select(c => arr.Contains(givenIxs[c]) ? "█" : "░").JoinString());
                return !GridGenerator.GenerateSudokus(3, Enumerable.Range(0, 81).Select(ix => arr.Contains(ix) ? sudoku[ix] : (int?) null).ToArray()).Skip(1).Any();
            });
            Console.WriteLine();
            Console.WriteLine(sudoku.Split(9).Select((row, rowIx) => row.Select((c, cIx) => givens.Contains(cIx + 9 * rowIx) ? c.ToString() : ".").JoinString(" ")).JoinString("\n"));
            Console.WriteLine();

            Console.WriteLine("Done.");
            Console.ReadLine();
            return 0;
        }
    }
}
