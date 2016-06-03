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
    }
}