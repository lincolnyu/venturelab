using GaussianCore;
using GaussianCore.Generic;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SecurityAccess.MultiTapMethod
{
    public class FixedConfinedBuilder
    {
        #region Enumerations

        public enum Flags : uint
        {
            InputOutputOnly = 0,
            InputOutputAndMutiple = 1,
            Complete = 2,// input/output, coeffs and mutiple
        }

        #endregion

        #region Constructors

        public FixedConfinedBuilder(FixedConfinedCoreManager fccm)
        {
            CoreManager = fccm;
        }

        #endregion

        #region Properties

        public FixedConfinedCoreManager CoreManager { get; private set; }

        #endregion

        #region Methods

        // TODO incremental build to be implemented

        public void BuildFromText(string statisticsDir, IEnumerable<string> codes, bool append=false)
        {
            if (!append)
            {
                CoreManager.Cores.Clear();
            }
            foreach (var code in codes)
            {
                var fn = string.Format("{0}.txt", code);
                var path = Path.Combine(statisticsDir, fn);
                AddStatistics(path);
            }
            CoreManager.UpdateCoreCoeffs();
        }

        public void BuildFromBinary(string statisticsDir, IEnumerable<string> codes, bool append = false)
        {
            if (!append)
            {
                CoreManager.Cores.Clear();
            }

            foreach (var code in codes)
            {
                var fn = string.Format("{0}.dat", code);
                var path = Path.Combine(statisticsDir, fn);
                Load(path, true, true);
            }
        }

        public static IEnumerable<ICore> GetCores(StreamReader sr)
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
                yield return core;
            }
        }

        private void AddStatistics(string path)
        {
            using (var sr = new StreamReader(path))
            {
                var cores = GetCores(sr).Cast<GaussianConfinedCore>();
                foreach (var core in cores)
                {
                    CoreManager.Cores.Add(core);
                }
            }
        }

        /// <summary>
        ///  Exports to text fille
        /// </summary>
        /// <param name="path">The path of the text file</param>
        public void ExportToText(string path, Flags flag = Flags.Complete)
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
                    foreach (var v in gc.CentersOutput)
                    {
                        sb.AppendFormat("{0},", v);
                    }
                    if (flag != Flags.InputOutputOnly)
                    {
                        sb.AppendFormat("{0},", gc.Weight);
                    }
                    if (flag == Flags.Complete)
                    {
                        foreach (var v in gc.K)
                        {
                            sb.AppendFormat("{0},", v);
                        }
                        foreach (var v in gc.L)
                        {
                            sb.AppendFormat("{0},", v);
                        }
                    }
                    sw.WriteLine(sb);
                }
            }
        }

        /// <summary>
        ///  Save the built and updated core set to the spcecified file
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path, Flags flag = Flags.Complete)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write((uint)flag);
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
                        if (flag != Flags.InputOutputOnly)
                        {
                            bw.Write(gc.Weight);
                        }
                        if (flag == Flags.Complete)
                        {
                            foreach (var v in gc.K)
                            {
                                bw.Write(v);
                            }
                            foreach (var v in gc.L)
                            {
                                bw.Write(v);
                            }
                        }
                    }
                }
            }
        }

        public void Load(string path, bool deduceCoeffs=false, bool append=false)
        {
            if (!append)
            {
                CoreManager.Cores.Clear();
            }
            using (var fs = new FileStream(path, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    var flag = (Flags)br.ReadUInt32();
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
                        for (var j = 0; j < 6; j++)
                        {
                            var val = br.ReadDouble();
                            core.CentersOutput[j] = val;
                        }
                        if (flag != Flags.InputOutputOnly)
                        {
                            core.Weight = br.ReadDouble();
                        }
                        if (flag == Flags.Complete)
                        {
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
                            core.UpdateInvLCoeff();
                        }
                        CoreManager.Cores.Add(core);
                    }
                    if (deduceCoeffs && flag != Flags.Complete)
                    {
                        CoreManager.UpdateCoreCoeffs();
                    }
                }
            }
        }

        public static List<string> LoadCodeSelection(string file)
        {
            var codes = new List<string>();
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
                    codes.Add(code);
                }
            }
            SortCodes(codes);
            return codes;
        }

        private static void SortCodes(List<string> codes)
        {
            codes.Sort();
            for (var i = codes.Count-1; i>0; )
            {
                var j = i - 1;
                for (; j >= 0 && codes[j] == codes[i]; j--)
                {
                }
                if (i - j > 1)
                {
                    codes.RemoveRange(j + 2, i - j - 1);
                }
                i = j;
            }
        }

        #endregion
    }
}
