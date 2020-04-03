using System;
using System.Collections.Generic;
using System.Text;
using Zxw.Framework.NetCore.EventBus;

namespace Zxw.Framework.Website.EventBus.SysRole
{
    public class NewSysRoleEvent: IEvent
    {
        public string Id { get; set; }
    }
}
