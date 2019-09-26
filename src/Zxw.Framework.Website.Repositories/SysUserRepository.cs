using System;
using System.Collections.Generic;
using System.Linq;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Repositories
{
    public class SysUserRepository : BaseRepository<SysUser, string>, ISysUserRepository
    {
        public SysUserRepository(IDbContextCore dbContext) : base(dbContext)
        {
        }

        public (bool, SysUser) Login(string account, string password, string ip)
        {
            var result = false;
            var model = GetSingleOrDefault(m =>
                (m.SysUserName.Equals(account)
                 || m.Telephone.Equals(account)
                 || m.EMail.Equals(account, StringComparison.OrdinalIgnoreCase))
                && password.Equals(m.SysPassword));
            if (model != null && model.Active)
            {
                model.LatestLoginDateTime = DateTime.Now;
                model.LatestLoginIP = ip;

                Update(model, "LatestLoginDateTime", "LatestLoginIP");
                result = true;
            }

            return (result, model);
        }

        public bool SignUp(string telephone, string userName, string password, string email)
        {
            var model = new SysUser()
            {
                Telephone = telephone,
                SysUserName = userName,
                SysPassword = password,
                CreatedDateTime = DateTime.Now,
                EMail = email,
                Active = true                
            };
            return Add(model) > 0;
        }

        public (bool, string) EditProfile(string userId, string telephone, string userName, string email)
        {
            var entity = GetSingle(userId);
            if (entity == null)
                return (false,"不存在的账户");
            if (!entity.Active)
                return (false,"该账户已被停用");
            var updateColumns = new List<string>();
            if (!string.IsNullOrEmpty(telephone))
            {
                entity.Telephone = telephone;
                updateColumns.Add("Telephone");
            }

            if (!string.IsNullOrEmpty(userName))
            {
                entity.SysUserName = userName;
                updateColumns.Add("SysUserName");
            }
            if (!string.IsNullOrEmpty(email))
            {
                entity.EMail = email;
                updateColumns.Add("EMail");
            }

            if (updateColumns.Any())
            {
                return Update(entity, updateColumns.ToArray()) > 0 ?  (true, "操作成功") : (false, "操作失败");
            }

            return (false, "未做任何更改");
        }

        public (bool, string) ChangePassword(string userId, string oldPwd, string newPwd)
        {
            var entity = GetSingle(userId);
            if (entity == null)
                return (false, "不存在的账户");
            if (!entity.Active)
                return (false, "该账户已被停用");
            if (!entity.SysPassword.Equals(oldPwd))
                return (false, "密码错误");
            entity.SysPassword = newPwd;
            return Update(entity, "SysPassword") > 0 ? (true, "操作成功") : (false, "操作失败");
        }

        public (bool, string) Active(string userId, bool activable)
        {
            var entity = GetSingle(userId);
            if (entity == null)
                return (false, "不存在的账户");
            entity.Active = activable;
            return Update(entity, "SysPassword") > 0 ? (true, "操作成功") : (false, "操作失败");
        }
    }
}