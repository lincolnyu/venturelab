using System;
using System.IO;

namespace SecurityAccess
{
    /// <summary>
    ///  1d, 2d, 4d, 8d, 16d, 32d, 64d, 128d, 256d, 512d, 1024d, 2048d, 4096d 
    /// </summary>
    public class MultiTapMethod
    {
        #region Methods

        /// <summary>
        ///  Sucks data and creates model
        /// </summary>
        /// <param name="dir">The directory the files are in</param>
        /// <param name="code"></param>
        /// <param name="startingDate">The starting date</param>
        /// <param name="interval">interval in number of days</param>
        /// <param name="count">total number of samples</param>
        public void Suck(string dir, string code, DateTime startingDate, int interval, int count)
        {
            var date = startingDate;
            for (;; date.AddDays(1))
            {
                var fn = GetFileName(dir, date);
                if (!File.Exists(fn))
                {
                    continue;
                }
                throw new NotImplementedException();
            }
        }

        private string GetFileName(string dir, DateTime date)
        {
            var fn = string.Format("{0}{1:00}{2:00}.txt", date.Year, date.Month, date.Day);
            return Path.Combine(dir, fn);
        }

        

        #endregion
    }
}
