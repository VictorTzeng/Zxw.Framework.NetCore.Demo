using System;
using System.Threading.Tasks;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.EventBus;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.EventBus.SysRole;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Repositories
{
    public class SysRoleRepository : BaseRepository<SysRole, string>, ISysRoleRepository
    {
        private IEventPublisher EventPublisher { get; set; }
        public SysRoleRepository(IDbContextCore dbContext, IEventPublisher eventPublisher) : base(dbContext)
        {
            EventPublisher = eventPublisher;
        }

        public override async Task<int> AddAsync(SysRole entity)
        {
            var ret = await base.AddAsync(entity);
            await EventPublisher.PublishAsync(new NewSysRoleEvent() {Id = entity.Id});
            return ret;
        }
    }
}