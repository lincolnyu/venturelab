﻿using System.IO;
using VentureLab.Asx;
using VentureLab.QbClustering;
using static VentureLab.Asx.StockManager;

namespace VentureLab.Helpers
{
    public static class ScoreTableHelper
    {
        public static void Save(this ScoreTable scoreTable, TextWriter writer)
        {
            foreach (var kvp in scoreTable.Data)
            {
                var key = kvp.Key;
                if (kvp.Value > 0)
                {
                    var strain1 = (StockItem)key.Strain1;
                    var strain2 = (StockItem)key.Strain2;
                    var code1 = strain1.Stock.Code;
                    var code2 = strain2.Stock.Code;
                    writer.WriteLine($"{code1},{code2},{kvp.Value}");
                }
            }
        }

        public static void Load(this ScoreTable scoreTable, StockManager sm, TextReader reader)
        {
            string line;
            scoreTable.Data.Clear();
            while ((line = reader.ReadLine()) != null)
            {
                var split = line.Split(',');
                var score = double.Parse(split[2]);
                if (score > 0)
                {
                    var code1 = split[0];
                    var code2 = split[1];
                    var strain1 = sm.Items[code1];
                    var strain2 = sm.Items[code2];
                    scoreTable[strain1, strain2] = score;
                }
            }
        }

        public static void LoadScoresDirect(this StockManager sm, TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var split = line.Split(',');
                var score = double.Parse(split[2]);
                if (score > 0)
                {
                    var code1 = split[0];
                    var code2 = split[1];
                    var strain1 = sm.Items[code1];
                    var strain2 = sm.Items[code2];
                    strain1.Weights[strain2] = score;
                    strain2.Weights[strain1] = score;
                }
            }
        }
    }
}
