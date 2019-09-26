using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.Website.Controllers
{
    public class GeneratorController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}