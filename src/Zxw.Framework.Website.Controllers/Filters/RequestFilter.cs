using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.IoC;
using Zxw.Framework.Website.IRepositories;

namespace Zxw.Framework.Website.Controllers.Filters
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
    public class RequestFilter:ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var identity = context.RouteData.Values["controller"] + "/" + context.RouteData.Values["action"];
            if (!context.Filters.Contains(new IgnoreAttribute()))
            {
                var repository =
                    AspectCoreContainer.Resolve<ISysMenuRepository>();
                if (repository.Count(
                        m => m.Identity.Equals(identity, StringComparison.OrdinalIgnoreCase) && m.Activable) <= 0)
                {
                    if (context.HttpContext.Request.IsAjaxRequest())
                    {
                        context.Result = new JsonResult(new {success = false, msg = "您请求的地址不存在，或者已被停用."});
                    }
                    else
                    {
                        context.Result = new ViewResult() {ViewName = "NotFound"};
                        context.HttpContext.Response.StatusCode = HttpStatusCode.NotFound.GetHashCode();
                    }
                }
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
