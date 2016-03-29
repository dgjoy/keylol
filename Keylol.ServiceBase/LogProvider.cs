using log4net;

namespace Keylol.ServiceBase
{
    /// <summary>
    /// log4net 提供者
    /// </summary>
    public interface ILogProvider
    {
        /// <summary>
        /// 获取 Logger
        /// </summary>
        ILog Logger { get; }
    }

    /// <summary>
    /// log4net 提供者
    /// </summary>
    /// <typeparam name="T">创建特定类型的 Logger</typeparam>
    public class LogProvider<T> : ILogProvider
    {
        /// <summary>
        /// 由 LogManager.GetLogger 方法获取的 Logger
        /// </summary>
        public ILog Logger { get; } = LogManager.GetLogger(typeof (T));
    }
}