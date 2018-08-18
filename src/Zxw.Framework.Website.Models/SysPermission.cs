using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.Website.Models
{
    [Table("SysPermission")]
    public class SysPermission:BaseModel<string>
    {
        [Key]
        [Column("SysPermissionId")]
        public override string Id { get; set; }


    }
}
