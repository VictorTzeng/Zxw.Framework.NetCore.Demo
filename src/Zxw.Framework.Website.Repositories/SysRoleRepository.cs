using System;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Repositories
{
    public class SysRoleRepository : BaseRepository<SysRole, Int32>, ISysRoleRepository
    {
        public SysRoleRepository(IDbContextCore dbContext) : base(dbContext)
        {
        }
    }
}