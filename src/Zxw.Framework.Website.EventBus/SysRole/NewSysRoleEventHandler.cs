using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Zxw.Framework.NetCore.EventBus;
using Zxw.Framework.NetCore.Helpers;
using Zxw.Framework.Website.IRepositories;

namespace Zxw.Framework.Website.EventBus.SysRole
{
    public class NewSysRoleEventHandler:BaseEventHandler<NewSysRoleEvent>
    {
        private ISysRoleRepository RoleRepository { get; set; }
        public NewSysRoleEventHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.RoleRepository = serviceProvider.GetService<ISysRoleRepository>();
        }

        public override async Task Execute(NewSysRoleEvent @event)
        {
            var role = await RoleRepository.GetSingleAsync(@event.Id);
            Log4NetHelper.WriteDebug(GetType(), @event.Id);
        }
    }
}
