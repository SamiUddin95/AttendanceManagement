using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Web.Services;

public static class MarkDayDates
{
    public static IEnumerable<DateTime> GetOccurrences(MarkDay markDay, DateTime from, DateTime to)
    {
        if (!markDay.RepeatsYearly)
        {
            var start = markDay.FromDate.Date > from.Date ? markDay.FromDate.Date : from.Date;
            var end = markDay.ToDate.Date < to.Date ? markDay.ToDate.Date : to.Date;
            for (var date = start; date <= end; date = date.AddDays(1)) yield return date;
            yield break;
        }

        var duration = (markDay.ToDate.Date - markDay.FromDate.Date).Days;
        for (var year = from.Year; year <= to.Year; year++)
        {
            if (year < markDay.FromDate.Year) continue;
            var startDay = Math.Min(markDay.FromDate.Day, DateTime.DaysInMonth(year, markDay.FromDate.Month));
            var start = new DateTime(year, markDay.FromDate.Month, startDay);
            if (start > to.Date) continue;
            var end = start.AddDays(duration);
            for (var date = start < from.Date ? from.Date : start; date <= end && date <= to.Date; date = date.AddDays(1))
                yield return date;
        }
    }
}
