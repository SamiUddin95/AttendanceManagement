using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace AttendanceManagement.Web.Areas.Administrator.Controllers;
[Area("Administrator"), Authorize]
public class ListController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Employee");
}
