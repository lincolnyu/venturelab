using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SecurityAccess.Asx;
using static System.String;

namespace SecurityAccess.Helpers
{
    public static class ByDateFileHelper
    {
        public static IEnumerable<DailyStockEntry> GetStockEntriesFromDate(this DirectoryInfo byDateDir,
            string code, string dateTime)
        {
            foreach (var f in byDateDir.GetFiles().Where(x => x.Extension == ".txt")
                .OrderBy(x => x.Name).Where(x=>Compare(x.Name, dateTime, StringComparison.Ordinal) >= 0))
            {
                using (var fs = f.Open(FileMode.Open))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        var dses = sr.ReadDailyStockEntries();
                        var cd = dses.FirstOrDefault(x => x.Code == code);
                        if (cd != null)
                        {
                            yield return cd;
                        }
                    }
                }
            }
        }
    }
}
