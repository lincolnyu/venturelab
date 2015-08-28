using System;

namespace SecurityAccess
{
    public static class AsxHistoryHelper
    {
        #region Methods

        public static DateTime GetAppxNextTradingDay(this DateTime day)
        {
            DateTime nextDay;
            if (day.DayOfWeek == DayOfWeek.Friday)
            {
                nextDay = day.AddDays(2);
            }

            throw new NotImplementedException();
        }

        #endregion
    }
}
