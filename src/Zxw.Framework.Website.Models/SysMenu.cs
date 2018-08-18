using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.Website.Models
{
    [Table("SysMenu")]
    public class SysMenu:BaseModel<string>
    {
        [Key]
        [Column("SysMenuId")]
        public override string Id { get; set; }

        public string ParentId { get; set; } = String.Empty;

        [MaxLength(2000)]
        public string MenuPath { get; set; }

        [Required]
        [MaxLength(20)]
        public string MenuName { get; set; }

        [MaxLength(50)]
        public string MenuIcon { get; set; }

        [Required]
        [MaxLength(100)]
        public string Identity { get; set; }

        [Required]
        [MaxLength(200)]
        public string RouteUrl { get; set; }

        public bool Visiable { get; set; } = true;

        public bool Activable { get; set; } = true;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SortIndex { get; set; }
    }
}
