using AttendanceManagement.Web.ViewModels.Companies;

namespace AttendanceManagement.Web.ViewModels.Auth;

public class AuthLandingViewModel
{
    public AdminLoginViewModel Login { get; set; } = new();
    public CompanyFormViewModel Company { get; set; } = new();
    public bool HasAnyCompany { get; set; }
    public string? ReturnUrl { get; set; }

    public bool RequiresCompanySetup => !HasAnyCompany;
}
