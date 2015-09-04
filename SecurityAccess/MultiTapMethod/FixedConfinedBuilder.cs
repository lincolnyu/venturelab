﻿using GaussianCore.Generic;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
                    var gc = (GaussianConfinedCore)core;
                    foreach (var v in gc.CentersInput)
                    {
                        sb.AppendFormat("{0},", v);
                    }
                    sb.Remove(sb.Length - 1, 1);

                    sb.Append(';');
                    foreach (var v in gc.CentersOutput)
                    {
                        sb.AppendFormat("{0},", v);
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append(';');

                    foreach (var v in gc.K)
                    {
                        sb.AppendFormat("{0},", v);
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append(';');

                    foreach (var v in gc.L)
                    {
                        sb.AppendFormat("{0},", v);
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append(';');

                    sb.AppendFormat("{0}", gc.Multiple);

                    sw.WriteLine(sb);
                }
            }
        }
        
        /// <summary>
        ///  Save the built and updated core set to the spcecified file
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(CoreManager.Cores.Count);
                    foreach (var core in CoreManager)
                    {
                        var gc = (GaussianConfinedCore)core;
                        foreach (var v in gc.CentersInput)
                        {
                            bw.Write(v);
                        }
                        foreach (var v in gc.CentersOutput)
                        {
                            bw.Write(v);
                        }
                        foreach (var v in gc.K)
                        {
                            bw.Write(v);
                        }
                        foreach (var v in gc.L)
                        {
                            bw.Write(v);
                        }
                        bw.Write(gc.Multiple);
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
                        for (var j = 0; j < 22; j++)
                        {
                            var val = br.ReadDouble();
                            core.CentersInput[j] = val;
                        }
                        for (var j = 0; j < 6; i++)
                        {
                            var val = br.ReadDouble();
                            core.CentersOutput[j] = val;
                        }
                        for (var j = 0; j < 22; j++)
                        {
                            var val = br.ReadDouble();
                            core.K[j] = val;
                        }
                        for (var j = 0; j < 6; j++)
                        {
                            var val = br.ReadDouble();
                            core.L[j] = val;
                        }
                        core.Multiple = br.ReadDouble();
                        core.UpdateInvLCoeff();
                        CoreManager.Cores.Add(core);
                    }
                }
            }
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
