using log4net;
using log4net.Config;

namespace ProductSynchronizer.Logger
{
    public static class Logger
    {
        public static ILog Log { get; } = LogManager.GetLogger("LOGGER");

        public static void InitLogger()
        {
            XmlConfigurator.Configure();
        }

        public static void WriteLog(string msg)
        {
            Log.Debug(msg);
        }
    }
}
