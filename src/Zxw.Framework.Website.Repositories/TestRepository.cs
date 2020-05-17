using System;
using System.Collections.Generic;
using System.Text;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Repositories
{
    /// <summary>
    /// 此仓储用于测试FromDbContextFactory
    /// </summary>
    public class TestRepository : BaseRepository<SysMenu, string>, ITestRepository
    {
        [FromDbContextFactory(tagName: "db1")]
        public ISqlServerDbContext dbFromFactory1 { get; set; }
        [FromDbContextFactory(tagName: "db2")]
        public ISqlServerDbContext dbFromFactory2 { get; set; }
        [FromDbContextFactory(tagName: "db3")]
        public ISqlServerDbContext dbFromFactory3 { get; set; }
        public TestRepository(IDbContextCore dbContext) : base(dbContext)
        {
        }

        public void Run()
        {
            dbFromFactory1.Get<SysMenu>();
            dbFromFactory2.Get<SysMenu>();
            dbFromFactory3.Get<SysMenu>();
        }
    }
}
