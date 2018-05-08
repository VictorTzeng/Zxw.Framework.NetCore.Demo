using System;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Repositories
{
    public class SysPermissionRepository : BaseRepository<SysPermission, Int32>, ISysPermissionRepository
    {
        public SysPermissionRepository(IDbContextCore dbContext) : base(dbContext)
        {
        }
    }
}