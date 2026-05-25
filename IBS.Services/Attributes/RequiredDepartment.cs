using Microsoft.AspNetCore.Authorization;

namespace IBS.Services.Attributes
{
    public class RequiredDepartment : IAuthorizationRequirement
    {
        public RequiredDepartment(List<string> department)
        {
            ArgumentNullException.ThrowIfNull(department);
            Department = department.ToList().AsReadOnly();
        }

        public IReadOnlyList<string> Department { get; }
    }
}
