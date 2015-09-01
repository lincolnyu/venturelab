using System;
using SecurityAccess.Asx;
using SecurityAccess;

namespace SecurityAnalysisConsole
{
    class Program
    {
        static void ReorganiseFiles(string srcDir, string dstDir, bool append=false)
        {
            var fr = new FileReorganiser(srcDir, dstDir, Console.Out, append);
            fr.Reorganise();
        }

        static void SuckIntoStatistic(string srcDir, string dstDir)
        {
            ExtractHelper.ProcessFiles(srcDir, dstDir, Console.Out);
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
                else if (args[0].Equals("-reorganise-inc", StringComparison.OrdinalIgnoreCase))
                {
                    // args[1]: src, args[2]: dst
                    ReorganiseFiles(args[1], args[2], true);
                }
                else if (args[0].Equals("-suck", StringComparison.OrdinalIgnoreCase))
                {
                    SuckIntoStatistic(args[1], args[2]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} occurred: {1}", e.GetType().Name, e.Message);
            }
        }
    }
}
