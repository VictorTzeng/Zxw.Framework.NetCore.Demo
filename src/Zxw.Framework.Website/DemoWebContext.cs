using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zxw.Framework.NetCore.Web;

namespace Zxw.Framework.Website
{
    public class DemoWebContext : WebContext
    {
        public DemoWebContext(IHttpContextAccessor accessor):base(accessor)
        {

        }
        public override T GetService<T>()
        {
            return base.GetService<T>();
        }
    }
}
