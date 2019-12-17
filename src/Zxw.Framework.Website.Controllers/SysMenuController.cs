using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.NetCore.Web;
using Zxw.Framework.Website.Controllers.Filters;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;
using Zxw.Framework.Website.ViewModels;

namespace Zxw.Framework.Website.Controllers
{
    [ControllerDescription(Name = "菜单管理")]
    public class SysMenuController : BaseController
    {
        //private ISysMenuRepository menuRepository;
        
        public SysMenuController(IWebContext webContext):base(webContext)
        {
            //this.menuRepository = menuRepository ?? throw new ArgumentNullException(nameof(menuRepository));
        }

        #region Views
        [ActionDescription(Name = "菜单列表")]
        public IActionResult Index()
        {
            //menuRepository.GetMenusByCache(m => true);
            //menuRepository.GetMenusByCacheAsync(m => true);
            return View();
        }
        [ActionDescription(Name = "新建菜单")]
        public IActionResult Create()
        {
            return View();
        }
        [ActionDescription(Name = "编辑菜单")]
        public Task<IActionResult> Edit(string id)
        {
            return Task.Factory.StartNew<IActionResult>(() => View(this.GetService<ISysMenuRepository>().GetSingle(id)));
        }

        #endregion

        #region Methods

        [AjaxRequestOnly, HttpGet, ActionDescription(Description = "Ajax获取菜单列表", Name = "获取菜单列表")]
        public Task<IActionResult> GetMenus()
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var rows = this.GetService<ISysMenuRepository>()
                    .GetHomeMenusByTreeView(m => m.Active && m.Visible && string.IsNullOrEmpty(m.ParentId))
                    .OrderBy(m => m.SortIndex).ToList();
                return Json(ExcutedResult.SuccessResult(rows));
            });
        }
        [AjaxRequestOnly, HttpGet, ActionDescription(Description = "Ajax获取菜单列表", Name = "获取菜单列表")]
        public Task<IActionResult> GetVueMenus()
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var rows = this.GetService<ISysMenuRepository>()
                    .GetHomeMenusByTreeView(m => m.Active && m.Visible && string.IsNullOrEmpty(m.ParentId))
                    .OrderBy(m => m.SortIndex).Select(ToJsonViewModel).ToList();
                return Json(ExcutedResult.SuccessResult(rows));
            });
        }

        private SysMenuJsonViewModel ToJsonViewModel(SysMenuViewModel model)
        {
            if (model == null) return null;
            var json = new SysMenuJsonViewModel()
            {
                icon = model.MenuIcon,
                index = model.Id,
                route = model.RouteUrl,
                title = model.MenuName,
                items = null
            };
            if (model.Children != null && model.Children.Any())
            {
                json.items = new List<SysMenuJsonViewModel>();
                foreach (var child in model.Children)
                {
                    json.items.Add(ToJsonViewModel(child));
                }
            }

            return json;
        }

        [AjaxRequestOnly, HttpGet, ActionDescription(Name = "获取菜单树", Description = "Ajax获取菜单树")]
        public Task<IActionResult> GetTreeMenus(string parentId = null)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var nodes = this.GetService<ISysMenuRepository>().GetMenusByTreeView(m => m.Active && string.IsNullOrEmpty(m.ParentId))
                    .OrderBy(m => m.SortIndex).Select(m => GetTreeMenus(m, parentId)).ToList();
                var rows = new[]
                {
                    new
                    {
                        text = " 根节点",
                        icon = "fas fa-boxes",
                        tags = "0",
                        nodes,
                        state = new
                        {
                            selected = string.IsNullOrEmpty(parentId)
                        }
                    }
                };
                return Json(ExcutedResult.SuccessResult(rows));
            });
        }

        private object GetTreeMenus(SysMenuViewModel viewModel, string parentId = null)
        {
            if (viewModel.Children.Any())
            {
                return new
                {
                    text = " "+viewModel.MenuName,
                    icon = viewModel.MenuIcon,
                    tags = viewModel.Id.ToString(),
                    nodes = viewModel.Children.Select(x=>GetTreeMenus(x, parentId)),
                    state = new
                    {
                        expanded = false,
                        selected = viewModel.Id == parentId
                    }
                };
            }
            return new 
            {
                text = " "+viewModel.MenuName,
                icon = viewModel.MenuIcon,
                tags = viewModel.Id.ToString(),
                state = new
                {
                    selected = viewModel.Id == parentId
                }
            };
        }

        [AjaxRequestOnly, HttpGet, ActionDescription(Name = "获取菜单列表", Description = "Ajax分页获取菜单列表")]
        public Task<IActionResult> GetMenusByPaged(int pageSize, int pageIndex, string keyword)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var menuRepository = this.GetService<ISysMenuRepository>();
                Expression<Func<SysMenu, bool>> filter = m=>true;
                if(!string.IsNullOrEmpty(keyword))
                    filter = filter.And(m=>m.Identity.Contains(keyword));
                var total = menuRepository.Count(filter);
                var rows = menuRepository.GetByPagination(filter, pageSize, pageIndex, true,
                    m => m.Id).ToList();
                return Json(PaginationResult.PagedResult(rows, total, pageSize, pageIndex));
            });
        }
        /// <summary>
        /// 新建
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        [AjaxRequestOnly,HttpPost,ValidateAntiForgeryToken, ActionDescription(Name = "新建菜单", Description = "Ajax新建菜单")]
        public Task<IActionResult> Add(SysMenu menu)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                if(!ModelState.IsValid)
                    return Json(ExcutedResult.FailedResult("数据验证失败"));
                var menuRepository = this.GetService<ISysMenuRepository>();
                menuRepository.AddAsync(menu);
                return Json(ExcutedResult.SuccessResult());
            });
        }
        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        [AjaxRequestOnly, HttpPost, ActionDescription(Name = "编辑菜单", Description = "Ajax编辑菜单")]
        public Task<IActionResult> Edit(SysMenu menu)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                if (!ModelState.IsValid)
                    return Json(ExcutedResult.FailedResult("数据验证失败"));
                var menuRepository = this.GetService<ISysMenuRepository>();
                menuRepository.Edit(menu);
                return Json(ExcutedResult.SuccessResult());
            });
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly, ActionDescription(Name = "删除菜单", Description = "Ajax删除菜单")]
        public Task<IActionResult> Delete(string id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var menuRepository = this.GetService<ISysMenuRepository>();
                menuRepository.Delete(id);
                return Json(ExcutedResult.SuccessResult("成功删除一条数据。"));
            });
        }

        /// <summary>
        /// 启停用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly, ActionDescription(Name = "启/停用菜单")]
        public Task<IActionResult> Active(string id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var menuRepository = this.GetService<ISysMenuRepository>();
                var entity = menuRepository.GetSingle(id);
                entity.Active = !entity.Active;
                menuRepository.Update(entity, "Active");
                return Json(ExcutedResult.SuccessResult(entity.Active?"OK，已成功启用。":"OK，已成功停用"));
            });
        }
        /// <summary>
        /// 是否在左侧菜单栏显示
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly, ActionDescription(Name = "显示/隐藏菜单")]
        public Task<IActionResult> Visualize(string id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var menuRepository = this.GetService<ISysMenuRepository>();
                var entity = menuRepository.GetSingle(id);
                entity.Visible = !entity.Visible;
                menuRepository.Update(entity, "Visible");
                return Json(ExcutedResult.SuccessResult("操作成功，请刷新当前网页或者重新进入系统。"));
            });
        }

        #endregion
	}
}