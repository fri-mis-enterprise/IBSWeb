using Microsoft.AspNetCore.Authorization;

namespace IBS.Services.Attributes
{
    public class RequiredDepartment : IAuthorizationRequirement
    {
        public RequiredDepartment(List<string> department)
        {
            Department = department;
        }

        public List<string> Department { get; }
    }
}
