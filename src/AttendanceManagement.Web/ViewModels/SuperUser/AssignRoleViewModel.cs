using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AttendanceManagement.Web.ViewModels.SuperUser;

public class AssignRoleViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Assignment Type")]
    public string AssignmentType { get; set; } = "Group"; // "Group" or "Individual"

    [Required]
    [Display(Name = "Role")]
    public int RoleId { get; set; }

    [Display(Name = "Company")]
    public Guid CompanyId { get; set; }

    [Display(Name = "Group")]
    public int? GroupId { get; set; }

    [Display(Name = "Department")]
    public int? DepartmentId { get; set; }

    [Display(Name = "Employee")]
    public int? EmployeeId { get; set; }

    public bool IsActive { get; set; } = true;

    // Dropdown options
    public SelectList RoleOptions { get; set; } = new SelectList(new List<SelectListItem>());
    public SelectList CompanyOptions { get; set; } = new SelectList(new List<SelectListItem>());
    public SelectList GroupOptions { get; set; } = new SelectList(new List<SelectListItem>());
    public SelectList DepartmentOptions { get; set; } = new SelectList(new List<SelectListItem>());
    public SelectList EmployeeOptions { get; set; } = new SelectList(new List<SelectListItem>());

    // For display
    public string RoleName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNo { get; set; } = string.Empty;
}

public class AssignRoleListViewModel
{
    public List<AssignRoleViewModel> Assignments { get; set; } = new();
}
