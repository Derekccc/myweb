namespace Logger.Logging
{
    public class LoggerFactory
    {
        public static ILogger Logger
        {
            get
            {
                return new TextFileLogger();
            }
        }
    }
}