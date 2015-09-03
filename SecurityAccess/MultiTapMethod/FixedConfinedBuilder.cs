using GaussianCore.Generic;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

namespace SecurityAccess.MultiTapMethod
{
    public class FixedConfinedBuilder
    {
        #region Constructors

        public FixedConfinedBuilder(FixedConfinedCoreManager fccm)
        {
            CoreManager = fccm;
            Codes = new List<string>();
        }

        #endregion

        #region Properties

        public FixedConfinedCoreManager CoreManager { get; private set; }

        public List<string> Codes { get; private set; }

        #endregion

        #region Methods

        public void Build(string statisticsDir, bool append=false)
        {
            if (!append)
            {
                CoreManager.Cores.Clear();
            }
            foreach (var code in Codes)
            {
                var fn = string.Format("{0}.txt", code);
                var path = Path.Combine(statisticsDir, fn);
                AddStatistics(path);
            }
            CoreManager.UpdateCoreCoeffs();
        }

        private void AddStatistics(string path)
        {
            using (var sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line == null)
                    {
                        continue;
                    }
                    var segs = line.Split(',');

                    var core = new GaussianConfinedCore(22, 6);
                    for (var i = 0; i < 22; i++)
                    {
                        var val = double.Parse(segs[i]);
                        core.CentersInput[i] = val;
                    }
                    for (var i = 0; i < 6; i++)
                    {
                        var val = double.Parse(segs[i + 22]);
                        core.CentersOutput[i] = val;
                    }
                    CoreManager.Cores.Add(core);
                }
            }
        }

        /// <summary>
        ///  Exports to text fille
        /// </summary>
        /// <param name="path">The path of the text file</param>
        public void ExportToText(string path)
        {
            using (var sw = new StreamWriter(path))
            {
                foreach (var core in CoreManager)
                {
                    var sb = new StringBuilder();
                    foreach (var v in core.CentersInput)
                    {
                        sb.AppendFormat("{0},", v);
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append(';');
                    foreach (var v in core.CentersOutput)
                    {
                        sb.AppendFormat("{0},", v);
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sw.WriteLine(sb);
                }
            }
        }

        public void Save(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(CoreManager.Cores.Count);
                    foreach (var core in CoreManager)
                    {
                        foreach (var v in core.CentersInput)
                        {
                            bw.Write(v);
                        }
                        foreach (var v in core.CentersOutput)
                        {
                            bw.Write(v);
                        }
                        // TODO core coeffs to improve loading speed...
                    }
                }
            }
        }

        public void Load(string path)
        {
            CoreManager.Cores.Clear();
            using (var fs = new FileStream(path, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    int count = br.ReadInt32();
                    CoreManager.Cores.Capacity = count;
                    for (var i = 0; i < count; i++)
                    {
                        var core = new GaussianConfinedCore(22, 6);
                        for (var j = 0; i < 22; j++)
                        {
                            var val = br.ReadDouble();
                            core.CentersInput[j] = val;
                        }
                        for (var j = 0; i < 6; i++)
                        {
                            var val = br.ReadDouble();
                            core.CentersOutput[j] = val;
                        }
                        CoreManager.Cores.Add(core);
                    }
                }
            }
            // TODO enhance this by storing core coefficients
            CoreManager.UpdateCoreCoeffs();
        }

        public void LoadCodeSelection(string file)
        {
            Codes.Clear();
            using (var sr = new StreamReader(file))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line == null)
                    {
                        continue;
                    }
                    var code = line.ToUpper();
                    Codes.Add(code);
                }
            }
            SortCodes();
        }

        private void SortCodes()
        {
            Codes.Sort();
            for (var i = Codes.Count-1; i>0; )
            {
                var j = i - 1;
                for (; j >= 0 && Codes[j] == Codes[i]; j--)
                {
                }
                if (i - j > 1)
                {
                    Codes.RemoveRange(j + 2, i - j - 1);
                }
                i = j;
            }
        }

        #endregion
    }
}
