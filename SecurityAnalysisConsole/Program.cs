using System;
using SecurityAccess.Asx;

namespace SecurityAnalysisConsole
{
    class Program
    {
        static void ReorganiseFiles(string srcDir, string dstDir)
        {
            var fr = new FileReorganiser(Console.Out);
            fr.Reorganise(srcDir, dstDir);
        }

        static void Main(string[] args)
        {
            try
            {
                if (args[0].Equals("-reorganise", StringComparison.OrdinalIgnoreCase))
                {
                    // args[1]: src, args[2]: dst
                    ReorganiseFiles(args[1], args[2]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} occurred: {1}", e.GetType().Name, e.Message);
            }
        }
    }
}
