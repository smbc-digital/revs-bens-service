using System;

namespace revs_bens_service.Extensions
{
    public static class DateExtensions
    {
        private const int April = 4;

        public static int ToFinancialYear(this DateTime date)
        {
            if (date.Month < April)
            {
                return date.Year - 1;
            }

            return date.Year;
        }

        public static DateTime ToDate(this string date)
        {
            DateTime dateToUse;
            DateTime.TryParse(date, out dateToUse);
            return dateToUse;
        }
    }
}
