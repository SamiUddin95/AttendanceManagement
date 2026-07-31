using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AttendanceManagement.Web.ViewModels.LeavePolicies;

public class LeavePolicyViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Active";

    [Required]
    public Guid CompanyId { get; set; }

    public string? CompanyName { get; set; }

    public List<SelectListItem>? Companies { get; set; }
}
