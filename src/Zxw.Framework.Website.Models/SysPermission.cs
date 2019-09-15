using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.Website.Models
{
    [Serializable]
    [DbContext(typeof(SqlServerDbContext))]
    [Table("SysPermission")]
    public class SysPermission:BaseModel<string>
    {
        [Key]
        [Column("SysPermissionId")]
        public override string Id { get; set; }


    }
}
