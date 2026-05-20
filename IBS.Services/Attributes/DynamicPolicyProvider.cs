using IBS.DataAccess.Repository.MasterFile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IBS.Services.Attributes
{
    public class DynamicPolicyProvider
        : DefaultAuthorizationPolicyProvider
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DynamicPolicyProvider(
            IOptions<AuthorizationOptions> options,
            IServiceScopeFactory scopeFactory)
            : base(options)
        {
            _scopeFactory = scopeFactory;
        }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(
            string policyName)
        {
            var existingPolicy =
                await base.GetPolicyAsync(policyName);

            if (existingPolicy != null)
            {
                return existingPolicy;
            }

            using var scope = _scopeFactory.CreateScope();

            var repo = scope.ServiceProvider
                .GetRequiredService<DepartmentAccessRepository>();

            var departmentAccessList =
                await repo.GetDepartmentAccessListAsync();

            var matchedAccess = departmentAccessList
                .Where(x => x.Action == policyName)
                .SelectMany(x => x.Department)
                .ToList();

            if (!matchedAccess.Any())
            {
                return new AuthorizationPolicyBuilder()
                    .AddRequirements(new RequiredDepartment(new List<string>()))
                    .Build();
            }

            return new AuthorizationPolicyBuilder()
                .AddRequirements(
                    new RequiredDepartment(matchedAccess))
                .Build();
        }
    }
}
