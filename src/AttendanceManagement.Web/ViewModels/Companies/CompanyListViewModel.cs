using System;
using System.Collections.Generic;
using AttendanceManagement.Application.Contracts.Companies;

namespace AttendanceManagement.Web.ViewModels.Companies;

public class CompanyListViewModel
{
    public IEnumerable<CompanyDto> Companies { get; set; } = Array.Empty<CompanyDto>();
    public string? Search { get; set; }
}
