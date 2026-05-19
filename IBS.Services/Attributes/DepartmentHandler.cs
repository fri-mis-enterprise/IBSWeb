using IBS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace IBS.Services.Attributes
{
    public class DepartmentHandler : AuthorizationHandler<RequiredDepartment>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DepartmentHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RequiredDepartment requirement)
        {
            if (context.User.Identity?.Name is null)
            {
                return;
            }

            var user = await _userManager.FindByNameAsync(context.User.Identity.Name);

            if (user is null)
            {
                return;
            }

            if (requirement.Department.Contains(user.Department))
            {
                context.Succeed(requirement);
            }
        }
    }
}
