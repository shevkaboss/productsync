using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ProductSynchronizer.Helpers
{
    public static class ConfigHelper
    {
        public static Configuration Config = new Configuration();
        public static string SizesConfigFilePath = @"Config\sizes.json";
        public static string ResultLogFilePath = @"Logs\ResultLog.log";

        static ConfigHelper()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(@"Config\config.json", optional: true)
                .Build();
            configuration.GetSection("Configuration").Bind(Config);
        }
    }

    public class Configuration
    {
        public int ThreadsPerResource { get; set; }
        public string JobCronConfig { get; set; }
        public string ConnectionString { get; set; }
        public string CurrencyApiUrl { get; set; }
        public PriceConfig PriceConfig { get; set; }
        public EmailConfig EmailConfig { get; set; }
        public IEnumerable<ProxyConfig> ProxiesConfig { get; set; }
    }

    public class PriceConfig
    {
        public int PriceThreshold { get; set; }
        public double BelowThresholdIncreaseUsd { get; set; }
        public double OverThresholdIncreaseUsd { get; set; }
    }

    public class EmailConfig
    {
        public string ResultLogMailReciever { get; set; }
        public string ResultLogMailSenderMail { get; set; }
        public string ResultLogMailSenderPass { get; set; }
        public string ResultLogMailCC { get; set; }
    }

    public class ProxyConfig
    {
        public string ProxyIpPort { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string UserAgent { get; set; }
        public Resource? Resource { get; set; }
        public ProxyType Type { get; set; }
    }

    public enum ProxyType
    {
        Http,
        Socks5
    }
}
