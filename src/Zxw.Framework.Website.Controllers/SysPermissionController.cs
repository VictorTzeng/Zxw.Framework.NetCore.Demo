using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.Website.Models;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.NetCore.Attributes;
using System.Threading.Tasks;
using Zxw.Framework.NetCore.Models;
using System.Linq;
using Zxw.Framework.Website.Controllers.Filters;

namespace Zxw.Framework.Website.Controllers
{
    [ControllerDescription(Name = "权限管理")]
    public class SysPermissionController : Controller
    {
        private ISysPermissionRepository SysPermissionRepository;
        
        public SysPermissionController(ISysPermissionRepository repository)
        {
            SysPermissionRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        #region Views

        [ActionDescription(Name = "权限列表")]
        public IActionResult Index()
        {
            return View();
        }

        #endregion

        [AjaxRequestOnly, HttpGet]
        [ActionDescription(Name = "获取权限列表")]
        public Task<IActionResult> GetEntities()
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                    var rows = SysPermissionRepository.Get().ToList();
                    return Json(ExcutedResult.SuccessResult(rows));
            });
        }

        [AjaxRequestOnly]
        [ActionDescription(Name = "分页获取权限列表")]
        public Task<IActionResult> GetEntitiesByPaged(int pageSize, int pageIndex)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var total = SysPermissionRepository.Count(m => true);
                var rows = SysPermissionRepository.GetByPagination(m => true, pageSize, pageIndex, true,
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
        [ActionDescription(Name = "新建")]
        public Task<IActionResult> Add(SysPermission model)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                if(!ModelState.IsValid)
                    return Json(ExcutedResult.FailedResult("数据验证失败"));
                SysPermissionRepository.AddAsync(model);
                return Json(ExcutedResult.SuccessResult());
            });
        }
        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AjaxRequestOnly, HttpPost]
        [ActionDescription(Name = "编辑")]
        public Task<IActionResult> Edit(SysPermission model)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                if (!ModelState.IsValid)
                    return Json(ExcutedResult.FailedResult("数据验证失败"));
                SysPermissionRepository.Edit(model);
                return Json(ExcutedResult.SuccessResult());
            });
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly]
        [ActionDescription(Name = "删除")]
        public Task<IActionResult> Delete(string id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                SysPermissionRepository.Delete(id);
                return Json(ExcutedResult.SuccessResult("成功删除一条数据。"));
            });
        }
	}
}