using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Application.Contracts.Auth;

public class AdminLoginRequest
{
    [Required]
    public string CompanyIdentifier { get; set; } = string.Empty;

    [Required]
    public string AdminUserId { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
