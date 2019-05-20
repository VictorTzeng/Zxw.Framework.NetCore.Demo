using System;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Repositories
{
    public class SysPermissionRepository : BaseRepository<SysPermission, string>, ISysPermissionRepository
    {
        public SysPermissionRepository(IDbContextCore dbContext) : base(dbContext)
        {
        }
    }
}