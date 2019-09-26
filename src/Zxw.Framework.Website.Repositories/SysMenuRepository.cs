using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Nelibur.ObjectMapper;
using Zxw.Framework.NetCore.Helpers;
using Zxw.Framework.NetCore.IDbContext;
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
        }
        public SysMenuRepository(IDbContextCore dbContext) : base(dbContext)
        {
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

        public Task<IList<SysMenu>> GetMenusByCacheAsync(Expression<Func<SysMenu, bool>> @where)
        {
            return Task.Factory.StartNew<IList<SysMenu>>(
                () => AspectCoreContainer.Resolve<IDbContextCore>().Get(where).ToList()
            );
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
                tmp.Children = GetHomeTreeMenu(m => m.ParentId == tmp.Id && m.Active && m.Visible);
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
                tmp.Children = GetTreeMenu(m => m.ParentId == tmp.Id && m.Active);
                reslut.Add(tmp);
            }
            return reslut;
        }
    }
}