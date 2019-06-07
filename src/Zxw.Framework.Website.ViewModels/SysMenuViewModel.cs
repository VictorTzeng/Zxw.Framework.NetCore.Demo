using System.Collections.Generic;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.ViewModels
{
    public class SysMenuViewModel : SysMenu
    {
        public IList<SysMenuViewModel> Children { get; set; } = new List<SysMenuViewModel>();
    }

    public class SysMenuJsonViewModel
    {
        public string index { get; set; }
        public string route { get; set; }
        public string icon { get; set; }
        public string title { get; set; }
        public List<SysMenuJsonViewModel> items { get; set; } = null;
    }
}
