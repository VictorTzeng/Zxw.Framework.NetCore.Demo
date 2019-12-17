using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.IoC;
using Zxw.Framework.NetCore.Options;
using Zxw.Framework.NetCore.Web;
using Zxw.Framework.Website.Controllers.Filters;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Controllers
{
    [ControllerDescription(Name = "首页")]
    public class HomeController : BaseController
    {
        //private ISysMenuRepository menuRepository;
        //private ISysUserRepository userRepository;
        public HomeController(IWebContext webContext):base(webContext)
        {
            //CodeGenerator.Generate();//生成所有实体类对应的Repository和Service层代码文件
            //this.menuRepository = menuRepository;
            //this.userRepository = userRepository;
        }
        [ActionDescription(Name = "首页")]
        public IActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult Default()
        {
            return View();
        }

        [ActionDescription(Name = "项目简介")]
        public IActionResult About()
        {
            ViewData["Title"] = "Readme";
            ViewBag.PageHeader = "README.md";
            ViewBag.PageDescription = "项目简介";
            return View();
        }

        [AllowAnonymous,Ignore]
        [ActionDescription(Name = "初始化菜单")]
        public async Task<IActionResult> Init()
        {
            return await Task.Factory.StartNew(() => {
                var options = ServiceLocator.Resolve<IOptions<CodeGenerateOption>>();
                InitSysMenus(options.Value.ControllersNamespace);
                var userRepository = this.GetService<ISysUserRepository>();
                if (!userRepository.Exist(m => m.SysUserName == "admin" && m.Active))
                {
                    userRepository.Add(new SysUser()
                    {
                        Id = Guid.NewGuid().ToString(),
                        SysUserName = "admin",
                        Active = true,
                        EMail = "admin@demo.com",
                        SysPassword = "123456",
                        Telephone = "13888888888"
                    });
                }
                return RedirectToAction("Index", "Account");
            });
        }

        /// <summary>
        /// 初始化系统菜单
        /// </summary>
        private void InitSysMenus(string controllerAssemblyName)
        {
            var menuRepository = this.GetService<ISysMenuRepository>();
            var assembly = Assembly.Load(controllerAssemblyName);
            var types = assembly?.GetTypes();
            var list = types?.Where(t => !t.IsAbstract && t.IsPublic && t.IsSubclassOf(typeof(Controller))).ToList();
            if (list != null)
            {
                foreach (var type in list)
                {
                    var controllerName = type.Name.Replace("Controller", "");
                    var methods = type.GetMethods().Where(m =>
                        m.IsPublic && (m.ReturnType == typeof(IActionResult) ||
                                       m.ReturnType == typeof(Task<IActionResult>)));
                    var parentIdentity = $"{controllerName}";
                    if (menuRepository.Count(m => EF.Functions.Contains(m.Identity, parentIdentity)) == 0)
                    {
                        menuRepository.Add(new SysMenu()
                        {
                            Id = Guid.NewGuid().ToString(),
                            MenuName = type.GetCustomAttribute<ControllerDescriptionAttribute>()?.Name??parentIdentity,
                            Active = true,
                            Visible = true,
                            Identity = parentIdentity,
                            RouteUrl = "",
                            ParentId = String.Empty
                        });
                    }

                    foreach (var method in methods)
                    {
                        var identity = $"{controllerName}/{method.Name}";
                        if (menuRepository.Count(m => m.Identity.Equals(identity, StringComparison.OrdinalIgnoreCase)) == 0)
                        {
                            menuRepository.Add(new SysMenu()
                            {
                                Id = Guid.NewGuid().ToString(),
                                MenuName = method.GetCustomAttribute<ActionDescriptionAttribute>()?.Name ?? method.Name,
                                Active = true,
                                Visible = method.GetCustomAttribute<AjaxRequestOnlyAttribute>() == null,
                                Identity = identity,
                                RouteUrl = identity,
                                ParentId = identity.Equals(parentIdentity, StringComparison.OrdinalIgnoreCase)
                                    ? String.Empty
                                    : menuRepository.GetSingleOrDefault(x => x.Identity == parentIdentity)?.Id
                            });
                        }
                    }
                }
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
    }
}
