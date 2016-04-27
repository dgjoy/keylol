namespace Keylol.Utilities
{
    /// <summary>
    /// 表示属性是必须的
    /// </summary>
    public class RequiredAttribute : System.ComponentModel.DataAnnotations.RequiredAttribute
    {
        /// <summary>
        /// 创建 <see cref="RequiredAttribute"/>
        /// </summary>
        public RequiredAttribute()
        {
            ErrorMessage = Errors.Required;
        }
    }
}