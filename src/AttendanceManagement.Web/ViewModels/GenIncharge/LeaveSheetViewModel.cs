using Microsoft.AspNetCore.Mvc.Rendering;

namespace AttendanceManagement.Web.ViewModels.GenIncharge;

public class LeaveSheetViewModel
{
    // ── Filter inputs ─────────────────────────────────────────────────────────

    public int? SelectedEmployeeId { get; set; }
    public SelectList EmployeeOptions { get; set; } = new SelectList(new List<object>());

    public int SelectedYear { get; set; } = DateTime.Now.Year;
    public List<int> AvailableYears { get; set; } = new();

    // ── Selected employee info ────────────────────────────────────────────────

    public string? EmployeeName { get; set; }
    public string? EmployeeNo { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public string? EmployeeType { get; set; }
    public string? Category { get; set; }
    public DateTime? JoiningDate { get; set; }

    // ── Leave sheet rows ──────────────────────────────────────────────────────

    public List<LeaveSheetRow> Rows { get; set; } = new();

    // ── Computed totals ───────────────────────────────────────────────────────

    public int TotalAllowed => Rows.Sum(r => r.Allowed);
    public int TotalTaken   => Rows.Sum(r => r.Taken);
    public int TotalBalance => Rows.Sum(r => r.Balance);

    public string PeriodDisplay => $"01 Jan {SelectedYear} — 31 Dec {SelectedYear}";
}

public class LeaveSheetRow
{
    public string LeaveType { get; set; } = string.Empty;
    public int Allowed { get; set; }
    public int Taken { get; set; }
    public bool CarryForward { get; set; }

    public int Balance => Math.Max(0, Allowed - Taken);
    public int OverUsed => Math.Max(0, Taken - Allowed);

    public decimal UtilizationPct =>
        Allowed > 0 ? Math.Min(100, Math.Round((decimal)Taken / Allowed * 100, 1)) : 0;

    public List<LeaveSheetApplication> Applications { get; set; } = new();
}

public class LeaveSheetApplication
{
    public string LeaveNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
}
