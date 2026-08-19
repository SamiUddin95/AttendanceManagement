using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class LeaveApplication
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required, MaxLength(50)]
    public string LeaveType { get; set; } = string.Empty; // Annual, Sick, Casual, etc.

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public int TotalDays { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    // Approval status
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Cancelled

    // Incharge approval
    public int? InchargeId { get; set; }
    public DateTime? InchargeApprovedAt { get; set; }
    [MaxLength(20)]
    public string? InchargeApprovalStatus { get; set; } // Pending, Approved, Rejected

    // Cancellation
    public DateTime? CancelledAt { get; set; }
    public int? CancelledBy { get; set; }
    [MaxLength(500)]
    public string? CancellationReason { get; set; }

    // Leave reference number for tracking
    [Required, MaxLength(50)]
    public string LeaveNumber { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    // Navigation properties
    public Company? Company { get; set; }
    public Employee? Employee { get; set; }
    public Employee? Incharge { get; set; }
    public Employee? CancelledByEmployee { get; set; }
}
