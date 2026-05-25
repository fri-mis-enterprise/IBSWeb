using System.Linq.Expressions;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.MasterFile.IRepository;
using IBS.Models.MasterFile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IBS.DataAccess.Repository.MasterFile
{
    public class DepartmentAccessRepository : Repository<DepartmentAccess>, IDepartmentAccessRepository
    {
        private readonly ApplicationDbContext _db;

        public DepartmentAccessRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public override IQueryable<DepartmentAccess> GetAllQuery(Expression<Func<DepartmentAccess, bool>>? filter = null)
        {
            IQueryable<DepartmentAccess> query = dbSet
                .AsSplitQuery()
                .AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query;
        }

        public async Task<List<(string[] Department, string Module, string Action)>> GetDepartmentAccessListAsync()
        {
            var data = await _db.DepartmentAccesses
                .Select(x => new
                {
                    x.Department,
                    x.Module,
                    x.Action
                })
                .ToListAsync();

            return data
                .Select(x => (x.Department, x.Module, x.Action))
                .ToList();
        }
    }
}
