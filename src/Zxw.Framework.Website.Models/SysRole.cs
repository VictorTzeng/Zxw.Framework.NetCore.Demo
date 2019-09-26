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
    [Table("SysRole")]
    public class SysRole : BaseModel<string>
    {
        [Key]
        [Column("SysRoleId")]
        [MaxLength(36)]
        public override string Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string SysRoleName{get;set;}

        public int ParentId{get;set;}

        [MaxLength(255)]
        public string NodePath{get;set;}

        public bool Active{get;set;}=true;

        public DateTime CreatedDateTime{get;set;}
    }
}
