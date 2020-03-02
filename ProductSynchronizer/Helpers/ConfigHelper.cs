﻿using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ProductSynchronizer.Helpers
{
    public static class ConfigHelper
    {
        public static Configuration Config = new Configuration();

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
    }

    public class PriceConfig
    {
        public int PriceThreshold { get; set; }
        public int BelowThresholdIncreaseUsd { get; set; }
        public int OverThresholdIncreaseUsd { get; set; }
        public int OverThresholdIncreasePercentage { get; set; }
    }
}
