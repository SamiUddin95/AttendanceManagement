namespace AttendanceManagement.Web.Services;

public static class TeamHierarchy
{
    public static bool HasCycle(int employeeId, IReadOnlyDictionary<int, int?> supervisors)
    {
        var visited = new HashSet<int>();
        int? current = employeeId;
        while (current.HasValue)
        {
            if (!visited.Add(current.Value)) return true;
            current = supervisors.GetValueOrDefault(current.Value);
        }
        return false;
    }
}
