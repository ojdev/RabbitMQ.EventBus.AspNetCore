using System;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// 
    /// </summary>
    static class ILoggerConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public static LogLevel Level { get; set; } = LogLevel.Information;
    }
    /// <summary>
    /// 
    /// </summary>
    public static class LoggerExtensions
    {
        private static Action<ILogger, string, Exception> _informationRequested = null;
        static LoggerExtensions()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="LogContent"></param>
        public static void Information(this ILogger logger, string LogContent)
        {
            if (_informationRequested == null)
            {
                LoggerMessage.Define<string>(ILoggerConfig.Level, new EventId(1, nameof(Information)), "{LogContent}");
            }
            _informationRequested(logger, LogContent, null);
        }
    }
}
