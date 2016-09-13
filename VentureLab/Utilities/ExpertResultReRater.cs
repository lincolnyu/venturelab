using System;
using System.Collections.Generic;
using System.Linq;
using VentureCommon.Helpers;
using VentureLab.Helpers;
using static VentureLab.QbGaussianMethod.Helpers.PredictionCommon;
using PredictionSample = VentureCommon.Helpers.ExpertParser.Prediction;

namespace VentureLab.Utilities
{
    public class ExpertResultReRater
    {
        public delegate Expert.Result GetExpertResult(string code);

        public static readonly int[] Periods = new[]
        { 1,2,5,10,20,65};

        public class Result : PredictionResult
        {
            public Result(int outLen) : base(outLen)
            {
            }

            public string Code { get; set; }
        }

        public IEnumerable<string> ReRate(IEnumerable<string> lines, GetExpertResult getExpertResult)
        {
            int rank;
            string code = null;
            var resList = new List<Expert.Result>();
            foreach (var line in lines)
            {
                string c;
                if (line.ParseAsHeader(out rank, out c))
                {
                    code = c;
                    continue;
                }
                else if (code != null)
                {
                    var res = getExpertResult(code);
                    if (ParseSample(line, res))
                    {
                        res.UpdateScore();
                        resList.Add(res);
                    }
                    code = null;
                }
                else
                {
                    yield return line; // pass through
                }
            }
            resList.Sort((a, b) => a.Score.CompareTo(b.Score));
            rank = 1;
            foreach (var res in resList)
            {
                yield return $"{rank}:{res.Item.Stock.Code}";
                var y = res.Y;
                var yy = res.YY;
                for (var i = 0; i < y.Count && i < yy.Count; i++)
                {
                    var yi = y[i];
                    var yyi = yy[i];
                    var stdyi = StatisticsHelper.GetStandardVariance(yi, yyi);
                    var yiperc = (Math.Pow(2, yi) - 1) * 100;
                    var pmperc = (Math.Pow(2, stdyi) - 1) * 100;
                    var yiperStr = GetFormattedStringOfDouble(yiperc, 2);
                    var pmpercStr = GetFormattedStringOfNonNegativeDouble(pmperc, 2);
                    yield return $"+{Periods[i]}: {yiperStr}+/-{pmpercStr}%; ";
                }
                var clstr = GetScientificExp(res.Strength, 2);
                yield return $"Confidence level: {clstr}";

                rank++;
            }
        }

        public static string GetFormattedStringOfDouble(double value, int decimals)
        {
            if (double.IsPositiveInfinity(value))
            {
                return "+Inf";
            }
            else if (double.IsNegativeInfinity(value))
            {
                return "-Inf";
            }
            var zeros = new string('0', decimals);
            var s = $"{{0:0.{zeros}}}";
            var fmt = string.Format(s, value);
            return fmt;
        }

        public static string GetFormattedStringOfNonNegativeDouble(double value, int decimals)
        {
            if (double.IsInfinity(value))
            {
                return "Inf";
            }
            var zeros = new string('0', decimals);
            var s = $"{{0:0.{zeros}}}";
            var fmt = string.Format(s, value);
            return fmt;
        }

        public static string GetScientificExp(double value, int decimals)
        {
            if (double.IsPositiveInfinity(value))
            {
                return "+Inf";
            }
            else if (double.IsNegativeInfinity(value))
            {
                return "-Inf";
            }
            var s = $"{{0:E{decimals}}}";
            var fmt = string.Format(s, value);
            return fmt;
        }

        private static bool ParseSample(string line, Expert.Result result)
        {
            var samples = ExpertParser.ParseSample<PredictionSample>(line).ToList();
            if (samples.Count > result.Y.Count) return false;
            for (var i = 0; i < samples.Count; i++)
            {
                var sample = samples[i];
                if (sample.Days != Periods[i]) return false;
                result.Y[i] = sample.Y;
                var v = sample.StdVar * sample.StdVar;
                var yy = v + sample.Y * sample.Y;
                result.YY[i] = yy;
            }
            var iconf = line.LastIndexOf(';');
            if (iconf >= 0)
            {
                var expectConfText = line.Substring(iconf + 1);
                var split = expectConfText.Split(':');
                if (split.Length == 2 && split[0].ToLower() == "confidence level")
                {
                    double expectConf;
                    if (double.TryParse(split[1], out expectConf))
                    {
                        result.Strength = expectConf;
                    }
                }
            }
            return true;
        }
    }
}
