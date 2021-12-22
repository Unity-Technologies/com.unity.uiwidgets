using System;
using System.Collections.Generic;

namespace Unity.UIWidgets.material {
    public static partial class utils {
        public static DateTime dateOnly(DateTime date) {
            return new DateTime(date.Year, date.Month, date.Day);
        }

        public static bool isSameDay(DateTime dateA, DateTime dateB) {
            return
                dateA.Year == dateB.Year &&
                dateA.Month == dateB.Month &&
                dateA.Day == dateB.Day;
        }

        public static int monthDelta(DateTime startDate, DateTime endDate) {
            return (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;
        }

        public static DateTime addMonthsToMonthDate(DateTime monthDate, int monthsToAdd) {
            return monthDate.AddMonths(monthsToAdd);
        }

        public static int firstDayOffset(int year, int month, MaterialLocalizations localizations) {
            // 0-based day of week for the month and year, with 0 representing Monday.
            int weekdayFromMonday = (int) ((new DateTime(year, month, 1)).DayOfWeek) - 1;

            // 0-based start of week depending on the locale, with 0 representing Sunday.
            int firstDayOfWeekIndex = localizations.firstDayOfWeekIndex;

            // firstDayOfWeekIndex recomputed to be Monday-based, in order to compare with
            // weekdayFromMonday.
            firstDayOfWeekIndex = (firstDayOfWeekIndex - 1) % 7;

            // Number of days between the first day of week appearing on the calendar,
            // and the day corresponding to the first of the month.
            return (weekdayFromMonday - firstDayOfWeekIndex) % 7;
        }

        public const int february = 2;

        public static int getDaysInMonth(int year, int month) {
            if (month == february) {
                bool isLeapYear = (year % 4 == 0) && (year % 100 != 0) ||
                                  (year % 400 == 0);
                if (isLeapYear)
                    return 29;
                return 28;
            }

            List<int> daysInMonth = new List<int> {
                31, -1, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31
            };
            return daysInMonth[month - 1];
        }
    }
}