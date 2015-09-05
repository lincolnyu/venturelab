using System;

namespace SecurityAccess.Asx
{
    public interface IOutputFile
    {
        #region Methods

        void WriteDailyStockEntry(DailyStockEntry dse);

        void WriteDailyStockEntry(string code, DateTime date,
                double open, double high, double low, double close, double volume);

        #endregion
    }
}
