using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VentureLabTests
{
    public static class Bootstrap
    {
        const string TestsDir = @"..\..\..\tests\";

        public static void InitTesting()
        {
            // remove all folders except "bydate"
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var testsDirStr = Path.Combine(appDir, TestsDir);
            var testsDir = new DirectoryInfo(testsDirStr);
            foreach (var f in testsDir.GetFiles())
            {
                f.Delete();
            }
            foreach (var d in testsDir.GetDirectories().Where(x=>x.Name != "bydate"))
            {
                d.Delete(true);
            }
        }

        public static IEnumerable<string> GetByDateFileNames()
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var byDateDirStr = Path.Combine(Path.Combine(appDir, TestsDir), "bydate");
            var byDateDir = new DirectoryInfo(byDateDirStr);
            // we assume all files in that folder are by-date files
            return byDateDir.GetFiles().OrderBy(x=>x.Name).Select(f => Path.GetFileNameWithoutExtension(f.Name));
        }
    }
}
