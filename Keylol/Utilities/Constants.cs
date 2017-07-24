using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Keylol.Utilities
{
    /// <summary>
    /// 常用常量
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// 用户、据点识别码正则表达式约束
        /// </summary>
        public const string IdCodeConstraint = @"^[A-Za-z0-9]{5}$";

        /// <summary>
        /// 昵称（用户名）正则表达式约束
        /// </summary>
        public const string UserNameConstraint = @"^[0-9A-Za-z\u4E00-\u9FCC]+$";

        /// <summary>
        /// 中国地区手机号码正则表达式约束
        /// </summary>
        public const string ChinesePhoneNumberConstraint = @"^(0|86|17951)?(13[0-9]|15[012356789]|17[678]|18[0-9]|14[57])[0-9]{8}$";
    }
}