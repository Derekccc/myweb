
namespace Logger.Logging
{
    public interface ILogger
    {
        void WebLog(string type, string logMessage);
        void WebApiLog(string type, string logMessage);
    }
}
