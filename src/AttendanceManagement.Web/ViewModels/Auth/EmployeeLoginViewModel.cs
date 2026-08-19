using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Web.ViewModels.Auth;

public class EmployeeLoginViewModel
{
    [Required]
    [Display(Name = "Employee No")]
    public string EmployeeNo { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
}
