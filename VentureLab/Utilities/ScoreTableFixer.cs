using System.IO;

namespace VentureLab
{
    public static class ScoreTableFixer
    {
        /// <summary>
        ///  Try to fix a score table stream corrupt or from previous version problematic or not
        ///  to be compliant with the current
        /// </summary>
        /// <param name="sr">The input stream</param>
        /// <param name="sw">The fixed output stream</param>
        public static void Fix(StreamReader sr, StreamWriter sw)
        {
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (line == null) return;
                var split = line.Split(',');
                var valstr = split[2];
                double val = 0;
                double.TryParse(valstr, out val);
                if (val > 0)
                {
                    sw.WriteLine(line);
                }
            }
        }
    }
}
