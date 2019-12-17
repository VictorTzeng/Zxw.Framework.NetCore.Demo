using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.NetCore.Web;
using Zxw.Framework.Website.Controllers.Filters;

namespace Zxw.Framework.Website.Controllers
{
    [Authorize, RequestFilter]
    public abstract class BaseController : Zxw.Framework.NetCore.Mvc.BaseController
    {
        public BaseController(IWebContext webContext) : base(webContext)
        {
        }
    }
}