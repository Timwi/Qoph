using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using RT.PostBuild;

namespace Qoph
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


            FaceToFace.GatherAllData();


            Console.WriteLine("Done.");
            Console.ReadLine();
            return 0;
        }
    }
}
