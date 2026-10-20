using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Security;

public sealed class DatabaseConflictFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is UnauthorizedAccessException)
        {
            context.Result = new ForbidResult(); context.ExceptionHandled = true; return;
        }
        if (context.Exception is DbUpdateException { InnerException: SqlException sql } && sql.Number is 547 or 2601 or 2627)
        {
            context.Result = new ConflictObjectResult("This change conflicts with existing records. Referenced records cannot be deleted; deactivate them instead. Names or identifiers that must be unique cannot be duplicated.");
            context.ExceptionHandled = true;
        }
    }
}
