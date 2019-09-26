using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.Website.Models
{
    [DbContext(typeof(SqlServerDbContext))]
    [ShardingTable("DailyLog")]
    public class DailyLog : BaseModel<long>
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id { get; set; }
    }
}
