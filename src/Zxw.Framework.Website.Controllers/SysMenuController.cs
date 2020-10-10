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
    [ControllerDescription(Name = "�˵�����")]
    public class SysMenuController : BaseController
    {
        //private ISysMenuRepository menuRepository;
        
        public SysMenuController(IWebContext webContext):base(webContext)
        {
            //this.menuRepository = menuRepository ?? throw new ArgumentNullException(nameof(menuRepository));
        }

        #region Views
        [ActionDescription(Name = "�˵��б�")]
        public IActionResult Index()
        {
            //menuRepository.GetMenusByCache(m => true);
            //menuRepository.GetMenusByCacheAsync(m => true);
            return View();
        }
        [ActionDescription(Name = "�½��˵�")]
        public IActionResult Create()
        {
            return View();
        }
        [ActionDescription(Name = "�༭�˵�")]
        public Task<IActionResult> Edit(string id)
        {
            return Task.Factory.StartNew<IActionResult>(() => View(this.GetService<ISysMenuRepository>().GetSingle(id)));
        }

        #endregion

        #region Methods

        [AjaxRequestOnly, HttpGet, ActionDescription(Description = "Ajax��ȡ�˵��б�", Name = "��ȡ�˵��б�")]
        public Task<IActionResult> GetMenus()
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var service = this.GetService<ISysMenuRepository>();
                var list = this.GetService<ISysMenuRepository>().GetAsync(m => m.Active && m.Visible).Result;
                var rows = this.GetService<ISysMenuRepository>()
                    .GetHomeMenusByTreeView(m => m.Active && m.Visible && string.IsNullOrEmpty(m.ParentId))
                    .OrderBy(m => m.SortIndex).ToList();
                return Json(ExcutedResult.SuccessResult(rows));
            });
        }
        [AjaxRequestOnly, HttpGet, ActionDescription(Description = "Ajax��ȡ�˵��б�", Name = "��ȡ�˵��б�")]
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

        [AjaxRequestOnly, HttpGet, ActionDescription(Name = "��ȡ�˵���", Description = "Ajax��ȡ�˵���")]
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
                        text = " ���ڵ�",
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

        [AjaxRequestOnly, HttpGet, ActionDescription(Name = "��ȡ�˵��б�", Description = "Ajax��ҳ��ȡ�˵��б�")]
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
        /// �½�
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        [AjaxRequestOnly,HttpPost,ValidateAntiForgeryToken, ActionDescription(Name = "�½��˵�", Description = "Ajax�½��˵�")]
        public Task<IActionResult> Add(SysMenu menu)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                if(!ModelState.IsValid)
                    return Json(ExcutedResult.FailedResult("������֤ʧ��"));
                var menuRepository = this.GetService<ISysMenuRepository>();
                menuRepository.AddAsync(menu);
                return Json(ExcutedResult.SuccessResult());
            });
        }
        /// <summary>
        /// �༭
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        [AjaxRequestOnly, HttpPost, ActionDescription(Name = "�༭�˵�", Description = "Ajax�༭�˵�")]
        public Task<IActionResult> Edit(SysMenu menu)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                if (!ModelState.IsValid)
                    return Json(ExcutedResult.FailedResult("������֤ʧ��"));
                var menuRepository = this.GetService<ISysMenuRepository>();
                menuRepository.Edit(menu);
                return Json(ExcutedResult.SuccessResult());
            });
        }
        /// <summary>
        /// ɾ��
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly, ActionDescription(Name = "ɾ���˵�", Description = "Ajaxɾ���˵�")]
        public Task<IActionResult> Delete(string id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var menuRepository = this.GetService<ISysMenuRepository>();
                menuRepository.Delete(id);
                return Json(ExcutedResult.SuccessResult("�ɹ�ɾ��һ�����ݡ�"));
            });
        }

        /// <summary>
        /// ��ͣ��
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly, ActionDescription(Name = "��/ͣ�ò˵�")]
        public Task<IActionResult> Active(string id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var menuRepository = this.GetService<ISysMenuRepository>();
                var entity = menuRepository.GetSingle(id);
                entity.Active = !entity.Active;
                menuRepository.Update(entity, "Active");
                return Json(ExcutedResult.SuccessResult(entity.Active?"OK���ѳɹ����á�":"OK���ѳɹ�ͣ��"));
            });
        }
        /// <summary>
        /// �Ƿ������˵�����ʾ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly, ActionDescription(Name = "��ʾ/���ز˵�")]
        public Task<IActionResult> Visualize(string id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var menuRepository = this.GetService<ISysMenuRepository>();
                var entity = menuRepository.GetSingle(id);
                entity.Visible = !entity.Visible;
                menuRepository.Update(entity, "Visible");
                return Json(ExcutedResult.SuccessResult("�����ɹ�����ˢ�µ�ǰ��ҳ�������½���ϵͳ��"));
            });
        }

        #endregion
	}
}