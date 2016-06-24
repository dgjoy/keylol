using System;

namespace Keylol.StateTreeManager
{
    /// <summary>
    /// 路径格式错误
    /// </summary>
    public class MalformedTreePathException : Exception
    {
        /// <summary>
        /// 创建 <see cref="MalformedTreePathException"/>
        /// </summary>
        public MalformedTreePathException() : base("Malformed tree path.")
        {
        }
    }

    /// <summary>
    /// 无效的属性
    /// </summary>
    public class InvalidPropertyException : Exception
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 创建 <see cref="InvalidPropertyException"/>
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        public InvalidPropertyException(string propertyName) : base($"Invalid property \"{propertyName}\".")
        {
            PropertyName = propertyName;
        }
    }

    /// <summary>
    /// 没有可用的 Get 方法
    /// </summary>
    public class NoGetMethodException : Exception
    {
        /// <summary>
        /// 创建 <see cref="NoGetMethodException"/>
        /// </summary>
        public NoGetMethodException() : base("No get method.")
        {
        }
    }

    /// <summary>
    /// 指定属性不支持 Locator
    /// </summary>
    public class LocatorNotSupportedException : Exception
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 创建 <see cref="LocatorNotSupportedException"/>
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        public LocatorNotSupportedException(string propertyName)
            : base($"Property \"{propertyName}\" doesn't support locator.")
        {
            PropertyName = propertyName;
        }
    }

    /// <summary>
    /// Locator 为空
    /// </summary>
    public class EmptyLocatorException : Exception
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 创建 <see cref="EmptyLocatorException"/>
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        public EmptyLocatorException(string propertyName)
            : base($"Locator of property \"{propertyName}\" cannot be empty.")
        {
            PropertyName = propertyName;
        }
    }

    /// <summary>
    /// 权限不足
    /// </summary>
    public class NotAuthorizedException : Exception
    {
        /// <summary>
        /// 创建 <see cref="NotAuthorizedException"/>
        /// </summary>
        public NotAuthorizedException() : base("Not authorized.")
        {
        }
    }
}