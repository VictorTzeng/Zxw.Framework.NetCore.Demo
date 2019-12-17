using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Web;
using Zxw.Framework.Website.Controllers.Filters;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Controllers
{
    [ControllerDescription(Name = "登录/注销")]
    public class AccountController:BaseController
    {

        public AccountController(IWebContext webContext):base(webContext)
        {
        }

        [AllowAnonymous]
        [ActionDescription(Name = "登录页面")]
        public IActionResult Index()
        {
            return View();
        }

        [ActionDescription(Name = "锁屏")]
        public IActionResult LockScreen()
        {
            return View();
        }

        [HttpPost, AjaxRequestOnly, ValidateAntiForgeryToken]
        [ActionDescription(Name = "屏幕解锁")]
        public async Task<IActionResult> Unlock(string password)
        {
            return await Task.Factory.StartNew(() => {
                var result = this.GetService<ISysUserRepository>().Exist(m =>
                    m.Active && m.SysUserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase) &&
                    m.SysPassword.Equals(password));
                return SucceedJson(string.Empty);
            });
        }

        [ HttpPost, AllowAnonymous, AjaxRequestOnly, ValidateAntiForgeryToken]
        [ActionDescription(Name = "账号登录")]
        public async Task<IActionResult> SignIn()
        {
            var account = Request.Form["account"][0];
            var password = Request.Form["password"][0];
            var msg= "登录成功";
            var result = this.GetService<ISysUserRepository>().Login(account, password, HttpContext.GetUserIp());

            if (result.Item2 == null)
            {
                msg = "对不起，您输入的用户名或者密码错误";
            }
            else if (!result.Item2.Active)
            {
                msg = "对不起，该账号已停用";
            }
            else
            {
                var user = new ClaimsPrincipal(
                    new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.Sid, result.Item2.Id),
                            new Claim(ClaimTypes.Name, result.Item2.SysUserName),
                        },
                        CookieAuthenticationDefaults.AuthenticationScheme));
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.Now.Add(TimeSpan.FromMinutes(30)) // 有效时间
                    });
            }
            return Json(new {success = result.Item1, msg});
        }

        [HttpPost, AjaxRequestOnly, ValidateAntiForgeryToken]
        [ActionDescription(Name = "编辑个人信息")]
        public Task<IActionResult> EditProfile(SysUser user)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var success = false;
                var msg = "数据验证失败";

                if (ModelState.IsValid)
                {
                    var result = this.GetService<ISysUserRepository>().EditProfile(user.Id, user.Telephone, user.SysUserName, user.EMail);
                    success = result.Item1;
                    msg = result.Item2;
                }
                return Json(new {success, msg});
            });
        }

        [HttpPost, AjaxRequestOnly, ValidateAntiForgeryToken]
        [ActionDescription(Name = "修改密码")]
        public Task<IActionResult> ChangePassword(string userId, string oldPwd, string newPwd)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var result = this.GetService<ISysUserRepository>().ChangePassword(userId, oldPwd, newPwd);
                return Json(new {success = result.Item1, msg = result.Item2});
            });
        }

        [HttpPost, AllowAnonymous, AjaxRequestOnly, ValidateAntiForgeryToken]
        [ActionDescription(Name = "账号注册")]
        public Task<IActionResult> SignUp(SysUser user)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var success = false;
                var msg = "数据验证失败";
                if (ModelState.IsValid)
                {
                    success = this.GetService<ISysUserRepository>().SignUp(user.Telephone, user.SysUserName, user.SysPassword, user.EMail);
                    msg = success?"注册成功":"注册失败";
                }
                return Json(new {success, msg});
            });
        }

        [ActionDescription(Name = "注销/退出")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
    }
}
