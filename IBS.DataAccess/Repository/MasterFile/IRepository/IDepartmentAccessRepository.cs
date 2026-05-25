using IBS.DataAccess.Repository.IRepository;
using IBS.Models.MasterFile;
using Microsoft.AspNetCore.Mvc;

namespace IBS.DataAccess.Repository.MasterFile.IRepository
{
    public interface IDepartmentAccessRepository : IRepository<DepartmentAccess>
    {
        Task<List<(string[] Department, string Module, string Action)>> GetDepartmentAccessListAsync();
    }
}
