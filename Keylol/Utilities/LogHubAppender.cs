using Keylol.Hubs;
using log4net.Appender;
using log4net.Core;
using Microsoft.AspNet.SignalR;

namespace Keylol.Utilities
{
    /// <summary>
    /// 利用 <see cref="LogHub"/> 实现的一个 log4net Appender
    /// </summary>
    public class LogHubAppender : AppenderSkeleton
    {
        /// <summary>
        /// Subclasses of <see cref="T:log4net.Appender.AppenderSkeleton" /> should implement this method
        /// to perform actual logging.
        /// </summary>
        /// <param name="loggingEvent">The event to append.</param>
        /// <remarks>
        /// <para>
        /// A subclass must implement this method to perform
        /// logging of the <paramref name="loggingEvent" />.
        /// </para>
        /// <para>This method will be called by <see cref="M:DoAppend(LoggingEvent)" />
        /// if all the conditions listed for that method are met.
        /// </para>
        /// <para>
        /// To restrict the logging of events in the appender
        /// override the <see cref="M:PreAppendCheck()" /> method.
        /// </para>
        /// </remarks>
        protected override void Append(LoggingEvent loggingEvent)
        {
            GlobalHost.ConnectionManager
                .GetHubContext<LogHub, ILogHubClient>()
                .Clients.All.OnWrite(
                    $"[{loggingEvent.Level}] {loggingEvent.TimeStamp} {loggingEvent.LoggerName} - {loggingEvent.RenderedMessage}");
        }
    }
}