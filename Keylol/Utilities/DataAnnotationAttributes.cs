using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Utilities
{
    /// <summary>
    ///     表示属性是必须的
    /// </summary>
    public class RequiredAttribute : System.ComponentModel.DataAnnotations.RequiredAttribute
    {
        /// <summary>
        ///     创建 <see cref="RequiredAttribute" />
        /// </summary>
        public RequiredAttribute()
        {
            ErrorMessage = Errors.Required;
        }
    }

    /// <summary>
    /// 表示属性有最大长度
    /// </summary>
    public class MaxLengthAttribute : System.ComponentModel.DataAnnotations.MaxLengthAttribute
    {
        /// <summary>
        /// 创建 <see cref="MaxLengthAttribute"/>
        /// </summary>
        /// <param name="length">最大长度</param>
        public MaxLengthAttribute(int length) : base(length)
        {
            ErrorMessage = Errors.TooMany;
        }

        /// <summary>Determines whether a specified object is valid.</summary>
        /// <returns>true if the value is null, or if the value is less than or equal to the specified maximum length; otherwise, false.</returns>
        /// <param name="value">The object to validate.</param>
        public override bool IsValid(object value)
        {
            if (value == null) return true;
            var length = ((value as string)?.Length ?? ((value as IList)?.Count ?? (value as Array)?.Length)) ?? -1;
            return -1 == Length || length <= Length;
        }
    }

    /// <summary>
    /// 表示属性有取值范围
    /// </summary>
    public class RangeAttribute : System.ComponentModel.DataAnnotations.RangeAttribute
    {
        /// <summary>
        /// 创建 <see cref="RangeAttribute"/>
        /// </summary>
        /// <param name="minimum">最小值，包含</param>
        /// <param name="maximum">最大值，包含</param>
        public RangeAttribute(int minimum, int maximum) : base(minimum, maximum)
        {
            ErrorMessage = Errors.Invalid;
        }

        /// <summary>
        /// 创建 <see cref="RangeAttribute"/>
        /// </summary>
        /// <param name="minimum">最小值，包含</param>
        /// <param name="maximum">最大值，包含</param>
        public RangeAttribute(double minimum, double maximum) : base(minimum, maximum)
        {
            ErrorMessage = Errors.Invalid;
        }
    }

    /// <summary>
    /// 表示属性需要满足正则表达式
    /// </summary>
    public class RegularExpressionAttribute : System.ComponentModel.DataAnnotations.RegularExpressionAttribute
    {
        /// <summary>
        /// 创建 <see cref="RegularExpressionAttribute"/>
        /// </summary>
        /// <param name="pattern"></param>
        public RegularExpressionAttribute(string pattern) : base(pattern)
        {
            ErrorMessage = Errors.Invalid;
        }
    }
}