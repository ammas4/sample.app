using AppSample.Domain.Interfaces;
using AppSample.Domain.Models.AdminUsers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace AppSample.Admin.Helpers;

public class RequiresAuthenticationAttribute : ActionFilterAttribute
{
    public AdminUserRole[]? Roles;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await base.OnActionExecutionAsync(context, next);
        return;
        AdminUserRole? currentRole = null;

        var httpContext = context.HttpContext;
        if (httpContext.User.Identity.IsAuthenticated)
        {
            var userName = httpContext.User.Identity.Name;
            var adminSettings = context.HttpContext.RequestServices.GetService<IOptions<AdminSettings>>();
            var adminSettingsValue = adminSettings.Value;

            if (adminSettingsValue.UsersAsList.Contains(userName, StringComparer.InvariantCultureIgnoreCase))
            {
                currentRole = AdminUserRole.Admin;
            }
            else
            {
                var userService = context.HttpContext.RequestServices.GetService<IAdminUserService>();
                var dbUser = await userService.GetByLogin(userName);
                if (dbUser != null)
                {
                    currentRole = dbUser.Role;
                }
            }
        }

        bool isValid = currentRole.HasValue && (currentRole == AdminUserRole.Admin || Roles != null && Roles.Contains(currentRole.Value));

        if (isValid == false)
        {
            context.Result = new ViewResult
            {
                ViewName = @"~/Views/Home/NotAuthorized.cshtml",
            };
            return;
        }

        await base.OnActionExecutionAsync(context, next);
    }
}