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
using Zxw.Framework.Website.Controllers.Filters;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Controllers
{
    public class AccountController:BaseController
    {
        private ISysUserRepository _userRepository;

        public AccountController(ISysUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LockScreen()
        {
            return View();
        }

        [ HttpPost, AllowAnonymous, AjaxRequestOnly,ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn()
        {
            var account = Request.Form["account"][0];
            var password = Request.Form["password"][0];
            var msg= "登录成功";
            var result = _userRepository.Login(account, password, HttpContext.GetUserIp());

            if (result.Item2 == null)
            {
                msg = "对不起，您输入的用户名或者密码错误";
            }
            else if (!result.Item2.Activable)
            {
                msg = "对不起，该账号已停用";
            }
            else
            {
                var user = new ClaimsPrincipal(
                    new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.Sid, result.Item2.UniqueId),
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
        public Task<IActionResult> EditProfile(SysUser user)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var success = false;
                var msg = "数据验证失败";

                if (ModelState.IsValid)
                {
                    var result = _userRepository.EditProfile(user.Id, user.Telephone, user.SysUserName, user.EMail);
                    success = result.Item1;
                    msg = result.Item2;
                }
                return Json(new {success, msg});
            });
        }

        [HttpPost, AjaxRequestOnly, ValidateAntiForgeryToken]
        public Task<IActionResult> ChangePassword(string userId, string oldPwd, string newPwd)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var result = _userRepository.ChangePassword(userId, oldPwd, newPwd);
                return Json(new {success = result.Item1, msg = result.Item2});
            });
        }

        [HttpPost, AllowAnonymous, AjaxRequestOnly, ValidateAntiForgeryToken]
        public Task<IActionResult> SignUp(SysUser user)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var success = false;
                var msg = "数据验证失败";
                if (ModelState.IsValid)
                {
                    success = _userRepository.SignUp(user.Telephone, user.SysUserName, user.SysPassword, user.EMail);
                    msg = success?"注册成功":"注册失败";
                }
                return Json(new {success, msg});
            });
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
    }
}
