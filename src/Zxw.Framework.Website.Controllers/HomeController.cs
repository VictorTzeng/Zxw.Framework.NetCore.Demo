using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.CodeGenerator;
using Zxw.Framework.NetCore.Options;
using Zxw.Framework.Website.IRepositories;

namespace Zxw.Framework.Website.Controllers
{
    [Ignore]
    public class HomeController : BaseController
    {
        private ISysMenuRepository menuRepository;
        public HomeController(ISysMenuRepository menuRepository)
        {
            //CodeGenerator.Generate();//生成所有实体类对应的Repository和Service层代码文件
            this.menuRepository = menuRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Title"] = "Readme";
            ViewBag.PageHeader = "README.md";
            ViewBag.PageDescription = "项目简介";
            return View();
        }

        [AllowAnonymous]
        public IActionResult Init([FromServices]IOptions<CodeGenerateOption> options)
        {
            menuRepository.InitSysMenus(options.Value.ControllersNamespace);
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                menuRepository.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
