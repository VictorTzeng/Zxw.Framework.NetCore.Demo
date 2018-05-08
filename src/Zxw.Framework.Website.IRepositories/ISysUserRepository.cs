using System;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.IRepositories
{
    public interface ISysUserRepository:IRepository<SysUser, Int32>
    {
        /// <summary>
        /// µÇÂ¼
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        (bool, SysUser) Login(string account, string password, string ip);
        /// <summary>
        /// ×¢²á
        /// </summary>
        /// <param name="telephone"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        bool SignUp(string telephone, string userName, string password, string email);
        /// <summary>
        /// ±à¼­×ÊÁÏ
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="telephone"></param>
        /// <param name="userName"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        (bool, string) EditProfile(int userId, string telephone, string userName, string email);
        /// <summary>
        /// ÐÞ¸ÄÃÜÂë
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="oldPwd"></param>
        /// <param name="newPwd"></param>
        /// <returns></returns>
        (bool, string) ChangePassword(int userId, string oldPwd, string newPwd);
        /// <summary>
        /// ÕËºÅÆôÍ£ÓÃ
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="activable"></param>
        /// <returns></returns>
        (bool, string) Active(int userId, bool activable);
    }
}