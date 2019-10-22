using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AspectCore.Extensions.Cache;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.Models;
using Zxw.Framework.Website.ViewModels;

namespace Zxw.Framework.Website.IRepositories
{
    public interface ISysMenuRepository:IRepository<SysMenu, string>
    {
        IList<SysMenuViewModel> GetHomeMenusByTreeView(Expression<Func<SysMenu, bool>> where);
        IList<SysMenuViewModel> GetMenusByTreeView(Expression<Func<SysMenu, bool>> where);

        [Cached(CacheKey = "Memory_Cache_SysMenu")]
        //[MemoryCache(CacheKey = "Memory_Cache_SysMenu", Expiration = 5)]
        //[RedisCache(CacheKey = "Redis_Cache_SysMenu", Expiration = 5)]
        IList<SysMenu> GetMenusByCache(Expression<Func<SysMenu, bool>> where);

        [Cached(CacheKey = "Memory_Cache_SysMenu")]
        //[MemoryCache(CacheKey = "Memory_Cache_SysMenuAsync", Expiration = 5)]
        //[RedisCache(CacheKey = "Redis_Cache_SysMenuAsync", Expiration = 5)]
        Task<IList<SysMenu>> GetMenusByCacheAsync(Expression<Func<SysMenu, bool>> where);
    }
}