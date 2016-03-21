using log4net;
using log4net.Core;

namespace Keylol.ServiceBase
{
    internal sealed class LogImpl<T> : LogImpl
    {
        public LogImpl() : base(LogManager.GetLogger(typeof (T)).Logger)
        {
        }
    }
}