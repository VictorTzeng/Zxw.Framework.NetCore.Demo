using System;
using System.Collections.Generic;
using System.Text;
using Zxw.Framework.NetCore.Attributes;

namespace Zxw.Framework.Website.IRepositories
{
    [FromDbContextFactoryInterceptor]
    public interface ITestRepository
    {
        void Run();
    }
}
