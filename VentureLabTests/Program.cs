using System;
using System.Linq;
using SecurityAccess.Asx;

namespace VentureLabTests
{
    class Program
    {
        private static void DisplayByDateFileNames()
        {
            foreach (var n in Bootstrap.GetByDateFileNames().Select(x=>x.StringToDate()))
            {
                Console.WriteLine(n);
            }
        }
    
        static void Main(string[] args)
        {
            Bootstrap.InitTesting();
            DisplayByDateFileNames();
        }
    }
}
