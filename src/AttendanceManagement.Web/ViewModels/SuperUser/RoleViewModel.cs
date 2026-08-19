using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AttendanceManagement.Web.ViewModels.SuperUser;

public class RoleViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Role Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    [Display(Name = "Display Name")]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public List<RoleModuleViewModel> RoleModules { get; set; } = new();

    public List<ModuleViewModel> AvailableModules { get; set; } = new();
}

public class RoleModuleViewModel
{
    public int ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleDisplayName { get; set; } = string.Empty;
    public bool CanView { get; set; } = true;
    public bool CanCreate { get; set; } = false;
    public bool CanEdit { get; set; } = false;
    public bool CanDelete { get; set; } = false;
}

public class ModuleViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class RoleListViewModel
{
    public List<RoleViewModel> Roles { get; set; } = new();
}
