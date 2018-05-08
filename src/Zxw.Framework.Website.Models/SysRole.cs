using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.Website.Models
{
    [Table("SysRole")]
    public class SysRole : BaseModel<int>
    {
        [Key]
        [Column("SysRoleId")]
        public override int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string SysRoleName{get;set;}

        public int ParentId{get;set;}

        public string NodePath{get;set;}

        public bool Activable{get;set;}=true;

        public DateTime CreatedDateTime{get;set;}
    }
}
