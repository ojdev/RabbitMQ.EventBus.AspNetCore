using System;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// 
    /// </summary>
    public static class LoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception> _informationRequested;
        static LoggerExtensions()
        {
            _informationRequested = LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1, nameof(Information)), "{LogContent}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="LogContent"></param>
        public static void Information(this ILogger logger, string LogContent)
        {
            _informationRequested(logger, LogContent, null);
        }
    }
}
