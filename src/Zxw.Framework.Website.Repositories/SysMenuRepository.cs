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
    public class SysMenuRepository : BaseRepository<SysMenu, string>, ISysMenuRepository
    {
        static SysMenuRepository()
        {
            TinyMapper.Bind<SysMenu, SysMenuViewModel>();
            //����ɹ��󴥷�
            Triggers<SysMenu>.Inserted += AfterInserted;
            //�޸�ʱ����
            Triggers<SysMenu>.Updating += WhileUpdating;
        }
        public SysMenuRepository(IDbContextCore dbContext) : base(dbContext)
        {
        }

        public static void AfterInserted(IInsertedEntry<SysMenu, DbContext> entry)
        {
            using (var service = AspectCoreContainer.Resolve<ISysMenuRepository>())
            {
                if (string.IsNullOrEmpty(entry.Entity.ParentId))
                {
                    entry.Entity.MenuPath = entry.Entity.Id;
                }
                else
                {
                    var parentMenu = service.GetSingle(entry.Entity.ParentId);
                    if (string.IsNullOrEmpty(parentMenu?.MenuPath))
                    {
                        entry.Entity.MenuPath = entry.Entity.Id;
                    }
                    else
                    {
                        entry.Entity.MenuPath = parentMenu.MenuPath + "," + entry.Entity.Id;
                    }
                }
                service.Update(entry.Entity, false, "MenuPath");
                DistributedCacheManager.Remove("Redis_Cache_SysMenu");//����ɹ�����������Ը���
            }
        }

        public static void WhileUpdating(IUpdatingEntry<SysMenu, DbContext> entry)
        {
            using (var service = AspectCoreContainer.Resolve<ISysMenuRepository>())
            {
                var parentMenu = service.GetSingle(entry.Entity.ParentId);
                if (string.IsNullOrEmpty(parentMenu?.MenuPath))
                {
                    entry.Entity.MenuPath = entry.Entity.Id;
                }
                else
                {
                    entry.Entity.MenuPath = parentMenu.MenuPath + "," + entry.Entity.Id;
                }
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
        /// ��ʼ��ϵͳģ��
        /// </summary>
        public void InitSysMenus(string controllerAssemblyName)
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
                        Add(new SysMenu()
                        {
                            MenuName = parentIdentity,
                            Activable = true,
                            Visiable = true,
                            Identity = parentIdentity,
                            RouteUrl = "",
                            ParentId = String.Empty
                        }, true);
                    }

                    foreach (var method in methods)
                    {
                        var identity = $"{controllerName}/{method.Name}";
                        if (Count(m => m.Identity.Equals(identity, StringComparison.OrdinalIgnoreCase)) == 0)
                        {
                            Add(new SysMenu()
                            {
                                MenuName = method.Name,
                                Activable = true,
                                Visiable = method.GetCustomAttribute<AjaxRequestOnlyAttribute>() == null,
                                Identity = identity,
                                RouteUrl = identity,
                                ParentId = identity.Equals(parentIdentity, StringComparison.OrdinalIgnoreCase)
                                    ? String.Empty
                                    : GetSingleOrDefault(x => x.Identity == parentIdentity)?.Id
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