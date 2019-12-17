using System;
using Zxw.Framework.NetCore.Web;
using Zxw.Framework.Website.Controllers.Filters;
using Zxw.Framework.Website.IRepositories;

namespace Zxw.Framework.Website.Controllers
{
    [ControllerDescription(Name = "用户管理")]
    public class SysUserController : BaseController
    {
        //private ISysUserRepository userRepository;
        
        public SysUserController(IWebContext webContext):base(webContext)
        {
            //this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }
	}
}