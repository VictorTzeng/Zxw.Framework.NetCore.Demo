using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.Website.Models;
using Zxw.Framework.Website.IRepositories;
using System.Threading.Tasks;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Models;
using System.Linq;

namespace Zxw.Framework.Website.Controllers
{
    public class SysRoleController : Controller
    {
        private ISysRoleRepository SysRoleRepository;
        
        public SysRoleController(ISysRoleRepository repository)
        {
            SysRoleRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        #region Views

        public IActionResult Index()
        {
            return View();
        }

        #endregion

        [AjaxRequestOnly, HttpGet]
        public Task<IActionResult> GetEntities()
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                    var rows = SysRoleRepository.Get().ToList();
                    return Json(ExcutedResult.SuccessResult(rows));
            });
        }

        [AjaxRequestOnly]
        public Task<IActionResult> GetEntitiesByPaged(int pageSize, int pageIndex)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var total = SysRoleRepository.Count(m => true);
                var rows = SysRoleRepository.GetByPagination(m => true, pageSize, pageIndex, true,
                    m => m.Id).ToList();
                return Json(PaginationResult.PagedResult(rows, total, pageSize, pageIndex));
            });
        }
        /// <summary>
        /// 新建
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AjaxRequestOnly,HttpPost,ValidateAntiForgeryToken]
        public Task<IActionResult> Add(SysRole model)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                if(!ModelState.IsValid)
                    return Json(ExcutedResult.FailedResult("数据验证失败"));
                SysRoleRepository.AddAsync(model, false);
                return Json(ExcutedResult.SuccessResult());
            });
        }
        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AjaxRequestOnly, HttpPost]
        public Task<IActionResult> Edit(SysRole model)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                if (!ModelState.IsValid)
                    return Json(ExcutedResult.FailedResult("数据验证失败"));
                SysRoleRepository.Edit(model, false);
                return Json(ExcutedResult.SuccessResult());
            });
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly]
        public Task<IActionResult> Delete(int id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                SysRoleRepository.Delete(id, false);
                return Json(ExcutedResult.SuccessResult("成功删除一条数据。"));
            });
        }
	}
}