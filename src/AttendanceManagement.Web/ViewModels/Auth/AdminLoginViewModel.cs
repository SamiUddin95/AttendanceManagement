using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Web.ViewModels.Auth;

public class AdminLoginViewModel
{
    [Required]
    [Display(Name = "Company ID / Domain")]
    public string CompanyIdentifier { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Admin User ID")]
    public string AdminUserId { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
}
