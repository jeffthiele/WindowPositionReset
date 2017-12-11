using System;
using System.IO;
using System.Text;
using log4net.Appender;
using log4net.Core;

namespace WindowPositionReset
{
    public class EventTriggerAppender : AppenderSkeleton
    {
        public event EventHandler<string> OnLogEvent;

        protected override void Append(LoggingEvent loggingEvent)
        {
            var writer = new StringWriter(new StringBuilder());

            this.Layout?.Format(writer, loggingEvent);

            if (string.IsNullOrEmpty(writer.ToString()))
            {
                writer.WriteLine(loggingEvent.RenderedMessage);
            }

            OnLogEvent?.Invoke(this, writer.ToString());
        }
    }
}