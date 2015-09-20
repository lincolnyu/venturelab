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

        public void BuildFromCoreSet(IEnumerable<ICore> cores, bool append, bool updateCoeffs)
        {
            if (!append)
            {
                CoreManager.Cores.Clear();
            }
            foreach (var core in cores.OfType<GaussianConfinedCore>())
            {
                CoreManager.Cores.Add(core);
            }
            if (updateCoeffs)
            {
                CoreManager.UpdateCoreCoeffs();
            }
        }

        public static void GetHeader(BinaryReader br, out int count, out Flags flag)
        {
            flag = (Flags)br.ReadUInt32();
            count = br.ReadInt32();
        }

        /// <summary>
        ///  read starting from the end of header
        /// </summary>
        /// <param name="br"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static IEnumerable<ICore> LoadCores(BinaryReader br, int start, int count, Flags flag)
        {
            // skip
            for (var i = 0; i < start; i++)
            {
                for (var j = 0; j < 28; j++)
                {
                    br.ReadDouble();
                }
                if (flag != Flags.InputOutputOnly)
                {
                    br.ReadDouble();
                }
                if (flag == Flags.Complete)
                {
                    for (var j = 0; j < 28; j++)
                    {
                        br.ReadDouble();
                    }
                }
            }

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
                yield return core;
            }
        }

        /// <summary>
        ///  Get cores from the binary file (with header information retrieved early on and passed as arguments)
        /// </summary>
        /// <param name="br"></param>
        /// <param name="count"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static IEnumerable<ICore> LoadCores(BinaryReader br, int count, Flags flag)
        {
            return LoadCores(br, 0, count, flag);
        }

        public static IEnumerable<ICore> LoadCores(StreamReader sr)
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

    
        /// <summary>
        ///  Save the cores to an existing file (with header and possible some data)
        ///  starting from the beginning of the file
        /// </summary>
        /// <param name="bw"></param>
        /// <param name="cores"></param>
        /// <param name="currentCount"></param>
        /// <param name="flag"></param>
        public static void SaveCores(BinaryWriter bw, IEnumerable<ICore> cores, int currentCount, Flags flag)
        {
            const int HeaderSize = sizeof(Flags) + sizeof(int);
            var blockSize = GetBlockSize(flag);
            bw.Seek(HeaderSize + blockSize * currentCount, SeekOrigin.Begin);

            Save(bw, cores, flag);
        }

        private static int GetBlockSize(Flags flag)
        {
            switch (flag)
            {
                case Flags.InputOutputOnly:
                    return sizeof(double)*28;
                case Flags.InputOutputAndMutiple:
                    return sizeof(double)*29;
                case Flags.Complete:
                    return sizeof(double)*(28*2+1);
                default:
                    throw new System.ArgumentException("Unknown flag");
            }
        }


        private void AddStatistics(string path)
        {
            using (var sr = new StreamReader(path))
            {
                var cores = LoadCores(sr).Cast<GaussianConfinedCore>();
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

        private static void Save(BinaryWriter bw, IEnumerable<ICore> cores, Flags flag = Flags.Complete)
        {
            foreach (var core in cores)
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

        public void Save(FileStream fs, Flags flag = Flags.Complete)
        {
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write((uint)flag);
                bw.Write(CoreManager.Cores.Count);
                Save(bw, CoreManager, flag);
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
                Save(fs, flag);
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
                    int count;
                    Flags flag;
                    GetHeader(br, out count, out flag);
                    CoreManager.Cores.Capacity = count;

                    var cores = LoadCores(br, count, flag);
                    foreach (var core in cores.OfType<GaussianConfinedCore>())
                    {
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
