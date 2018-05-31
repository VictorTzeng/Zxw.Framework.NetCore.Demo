using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using EntityFrameworkCore.Triggers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nelibur.ObjectMapper;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Cache;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.Helpers;
using Zxw.Framework.NetCore.IoC;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;
using Zxw.Framework.Website.ViewModels;

namespace Zxw.Framework.Website.Repositories
{
    public class SysMenuRepository : BaseRepository<SysMenu, Int32>, ISysMenuRepository
    {
        static SysMenuRepository()
        {
            TinyMapper.Bind<SysMenu, SysMenuViewModel>();
            //插入成功后触发
            Triggers<SysMenu>.Inserted += AfterInsertedAsync;
            //修改时触发
            Triggers<SysMenu>.Updating += WhileUpdatingAsync;
        }
        public SysMenuRepository(IDbContextCore dbContext) : base(dbContext)
        {
        }

        public static async void AfterInsertedAsync(IInsertedEntry<SysMenu, DbContext> entry)
        {
            using (var service = AspectCoreContainer.Resolve<ISysMenuRepository>())
            {
                var parentMenu = await service.GetSingleAsync(entry.Entity.ParentId);
                entry.Entity.MenuPath = (parentMenu?.MenuPath ?? "0") + "," + entry.Entity.Id;
                entry.Entity.SortIndex = entry.Entity.Id;
                service.Update(entry.Entity, false, "MenuPath", "SortIndex");
                await DistributedCacheManager.RemoveAsync("Redis_Cache_SysMenu");//插入成功后清除缓存以更新
            }
        }

        public static async void WhileUpdatingAsync(IUpdatingEntry<SysMenu, DbContext> entry)
        {
            using (var service = AspectCoreContainer.Resolve<ISysMenuRepository>())
            {
                var parentMenu = await service.GetSingleAsync(entry.Entity.ParentId);
                entry.Entity.SortIndex = entry.Entity.Id;
                entry.Entity.MenuPath = (parentMenu?.MenuPath ?? "0") + "," + entry.Entity.Id;
            }
        }
        public IList<SysMenuViewModel> GetHomeMenusByTreeView(Expression<Func<SysMenu, bool>> where)
        {
            return GetHomeTreeMenu(where);
        }
        public IList<SysMenuViewModel> GetMenusByTreeView(Expression<Func<SysMenu, bool>> @where)
        {
            return GetTreeMenu(where);
        }

        public IList<SysMenu> GetMenusByCache(Expression<Func<SysMenu, bool>> @where)
        {
            return DbContext.Get(where, true).ToList();
        }
        /// <summary>
        /// 初始化系统模块
        /// </summary>
        public async void InitSysMenus(string controllerAssemblyName)
        {
            var assembly = Assembly.Load(controllerAssemblyName);
            var types = assembly?.GetTypes();
            var list = types?.Where(t =>t.Name.Contains("Controller") && !t.IsAbstract).ToList();
            if (list != null)
            {
                foreach (var type in list)
                {
                    var controllerName = type.Name.Replace("Controller", "");
                    var methods = type.GetMethods().Where(m =>
                        m.IsPublic && (m.ReturnType == typeof(IActionResult) ||
                                       m.ReturnType == typeof(Task<IActionResult>)));
                    var parentIdentity = $"{controllerName}";
                    if (Count(m => m.Identity.Equals(parentIdentity, StringComparison.OrdinalIgnoreCase)) == 0)
                    {
                        await AddAsync(new SysMenu()
                        {
                            MenuName = parentIdentity,
                            Activable = true,
                            Visiable = true,
                            Identity = parentIdentity,
                            RouteUrl = "",
                            ParentId = 0
                        }, true);
                    }

                    foreach (var method in methods)
                    {
                        var identity = $"{controllerName}/{method.Name}";
                        if (Count(m => m.Identity.Equals(identity, StringComparison.OrdinalIgnoreCase)) == 0)
                        {
                            await AddAsync(new SysMenu()
                            {
                                MenuName = method.Name,
                                Activable = true,
                                Visiable = method.GetCustomAttribute<AjaxRequestOnlyAttribute>() == null,
                                Identity = identity,
                                RouteUrl = identity,
                                ParentId = identity.Equals(parentIdentity, StringComparison.OrdinalIgnoreCase)
                                    ? 0
                                    : GetSingleOrDefault(x => x.Identity == parentIdentity)?.Id ?? 0
                            }, true);
                        }
                    }
                }
            }
        }

        private IList<SysMenuViewModel> GetHomeTreeMenu(Expression<Func<SysMenu, bool>> where)
        {
            var reslut = new List<SysMenuViewModel>();
            var children = Get(where).OrderBy(m => m.SortIndex).ToList();
            foreach (var child in children)
            {
                var tmp = new SysMenuViewModel();
                tmp = TinyMapper.Map(child, tmp);
                tmp.RouteUrl = ConfigHelper.GetConfigurationValue("appSettings:AdminDomain") + tmp.RouteUrl;
                tmp.Children = GetHomeTreeMenu(m => m.ParentId == tmp.Id && m.Activable && m.Visiable);
                reslut.Add(tmp);
            }
            return reslut;
        }
        private IList<SysMenuViewModel> GetTreeMenu(Expression<Func<SysMenu, bool>> where)
        {
            var reslut = new List<SysMenuViewModel>();
            var children = Get(where).OrderBy(m => m.SortIndex).ToList();
            foreach (var child in children)
            {
                var tmp = new SysMenuViewModel();
                tmp = TinyMapper.Map(child, tmp);
                tmp.Children = GetTreeMenu(m => m.ParentId == tmp.Id && m.Activable);
                reslut.Add(tmp);
            }
            return reslut;
        }
    }
}