using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Web.ViewModels.InOutTimings;

namespace AttendanceManagement.Web.Services;

public static class PunchPairing
{
    // Pair by timestamp so an overnight exit belongs to the day its shift began.
    // Unmatched and repeated entries remain visible for manual correction.
    public static List<InOutTimingsItem> Build(IEnumerable<Attendance> punches)
    {
        var rows = new List<InOutTimingsItem>();
        Attendance? pending = null;
        void Add(Attendance? start, Attendance? end)
        {
            var duration = start != null && end != null ? end.PunchDateTime - start.PunchDateTime : (TimeSpan?)null;
            rows.Add(new() { SrNo = rows.Count + 1, Date = (start ?? end)!.PunchDateTime.Date,
                TimeIn = start?.PunchDateTime.TimeOfDay, TimeOut = end?.PunchDateTime.TimeOfDay,
                ExtraHours = duration > TimeSpan.FromHours(8) ? duration - TimeSpan.FromHours(8) : null,
                Status = start == null ? "Half Day (No In)" : end == null ? "Half Day (No Out)" : duration < TimeSpan.FromHours(4) ? "Half Day" : "Present" });
        }
        foreach (var punch in punches.OrderBy(p => p.PunchDateTime).ThenBy(p => p.Id))
        {
            if (punch.PunchType.Equals("In", StringComparison.OrdinalIgnoreCase))
            {
                if (pending != null) Add(pending, null);
                pending = punch;
            }
            else if (punch.PunchType.Equals("Out", StringComparison.OrdinalIgnoreCase))
            {
                if (pending != null && punch.PunchDateTime - pending.PunchDateTime > TimeSpan.FromHours(24))
                { Add(pending, null); pending = null; }
                Add(pending, punch); pending = null;
            }
        }
        if (pending != null) Add(pending, null);
        return rows;
    }
}
