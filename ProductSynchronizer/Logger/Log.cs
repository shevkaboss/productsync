using log4net;
using log4net.Config;
using ProductSynchronizer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ProductSynchronizer.Logger
{
    public static class Log
    {
        private static ILog Logger { get; set; }

        private static ILog ResultLogger { get; set; }
        
        static Log()
        {
            XmlConfigurator.Configure();
            Logger = LogManager.GetLogger("BaseLogger");
            ResultLogger = LogManager.GetLogger("ResultLogger");
        }

        public static void WriteLog(string msg)
        {
            Logger.Debug(msg);
        }

        public static void ResultLog(string timeConsumed, List<Error> errors)
        {
            ResultLogger.Info($"SYNC JOB HAS ENDED. TIME CONSUMED: {timeConsumed}");

            if (errors.Count > 0)
                ResultLogger.Info($"UNSUCCESSFUL PRODUCTS:{Environment.NewLine}{string.Join($",{Environment.NewLine}", errors.Select(x => $"Product ID: {x.ProductId} - Message: {x.Message}" ))}");
            else
                ResultLogger.Info($"ALL PRODUCTS SUCCESSFULY UPDATED.");

            _ = EmailSender.SendResultLog();
        }
    }
}
