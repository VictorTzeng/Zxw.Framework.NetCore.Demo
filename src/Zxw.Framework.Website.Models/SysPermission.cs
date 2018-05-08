using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.Website.Models
{
    [Table("SysPermission")]
    public class SysPermission:BaseModel<int>
    {
        [Key]
        [Column("SysPermissionId")]
        public override int Id { get; set; }


    }
}
