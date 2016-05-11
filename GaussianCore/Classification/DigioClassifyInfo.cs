using System.IO;

namespace GaussianCore.Classification
{
    public class DigioClassifyInfo
    {
        public DigioClassifyInfo(double score, int count, int endOfOne, int endOfTwo)
        {
            Score = score;
            Count = count;
            EndOfOne = endOfOne;
            EndOfTwo = endOfTwo;
        }

        private DigioClassifyInfo()
        {
        }

        public static DigioClassifyInfo Default { get; } = new DigioClassifyInfo();

        public double Score { get; set; }

        public int Count { get; set; }

        public int EndOfOne { get; set; }

        public int EndOfTwo { get; set; }

        public void WriteToBinary(BinaryWriter bw)
        {
            bw.Write(Score);
            bw.Write(Count);
            bw.Write(EndOfOne);
            bw.Write(EndOfTwo);
        }

        public static DigioClassifyInfo ReadFromBinary(BinaryReader br)
        {
            var dci = new DigioClassifyInfo
            {
                Score = br.ReadDouble(),
                Count = br.ReadInt32(),
                EndOfOne = br.ReadInt32(),
                EndOfTwo = br.ReadInt32()
            };
            return dci;
        }
    }
}
