using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AttendanceManagement.Web.ViewModels.Leaves;

public class LeaveViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid CompanyId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public int LeavePolicyId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public string? CompanyName { get; set; }
    public string? CategoryName { get; set; }
    public string? LeavePolicyName { get; set; }

    public List<LeaveDetailViewModel> LeaveDetails { get; set; } = new();

    public List<SelectListItem>? Categories { get; set; }
    public List<SelectListItem>? LeavePolicies { get; set; }
}
