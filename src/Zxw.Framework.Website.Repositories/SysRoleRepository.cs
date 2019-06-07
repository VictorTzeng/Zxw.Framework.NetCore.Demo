using System;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Repositories
{
    public class SysRoleRepository : BaseRepository<SysRole, string>, ISysRoleRepository
    {
        public SysRoleRepository(IDbContextCore dbContext) : base(dbContext)
        {
        }
    }
}