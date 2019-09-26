using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.Website.Models
{
    [DbContext(typeof(SqlServerDbContext))]
    [Table("SysUser"), Serializable]
    public class SysUser:BaseModel<string>
    {
        [Key]
        [Column("SysUserId")]
        [MaxLength(36)]
        public override string Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string SysUserName { get; set; }

        [MaxLength(100)]
        public string EMail{get;set;}

        [MaxLength(20)]
        public string Telephone{get;set;}

        [Required]
        [MaxLength(100)]
        public string SysPassword { get; set; }

        [Required]
        public bool Active{get;set;}=true;

        public DateTime CreatedDateTime{get;set;}=DateTime.Now;

        public DateTime? LatestLoginDateTime{get;set;}

        [MaxLength(100)]
        public string LatestLoginIP{get;set;}
    }
}
