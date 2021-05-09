using System;
using eToroApiScraper.Services;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using System.Collections.Generic;
using eToroApiScraper.Objects;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace eToroApiScraper
{
    class Program
    {
        private sealed class Config
        {
            private readonly IConfigurationRoot _configuration;
            public Config(string config) => _configuration = new ConfigurationBuilder()
                    .AddJsonFile(config)
                    .Build();

            private T Get<T>() where T : class, new()
            {
                var obj = new T();
                _configuration.GetSection(typeof(T).Name).Bind(obj);
                return obj;
            }

            public BaseConfig Base => Get<BaseConfig>();
            public ChromeDriverConfig ChromeDriver => Get<ChromeDriverConfig>();
            public EtoroConfig Etoro => Get<EtoroConfig>();
        }

        private static ILogger _logger;
        private static IServiceProvider _services;
        private static Config _config;

        static async Task Main(string[] args)
        {
            _config = new Config("Config.json");

            _services = new ServiceCollection()
                .AddOptions()
                .Configure<eToroServiceOptions>(options => {
                    options.UserAgent = _config.ChromeDriver.UserAgent;
                    options.UserDataDir = Path.Combine(_config.Base.CacheDir, "ChromeCache");
                    options.Username = _config.Etoro.Username;
                    options.Password = _config.Etoro.Password;
                    options.Watchlists = _config.Etoro.Watchlists;
                })
                .AddSingleton<IeToroService, eToroService>()
                .AddLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddNLog(new NLogProviderOptions
                    {
                        CaptureMessageProperties = true,
                        CaptureMessageTemplates = true
                    });
                })
                .BuildServiceProvider();

            _logger = _services.GetService<ILoggerFactory>().CreateLogger<Program>();
            _logger.LogInformation("Starting up...");


            while (true)
            {
                await _services.GetService<IeToroService>().Login();

                Dictionary<string, List<eToroTrade>> tradeData = new Dictionary<string, List<eToroTrade>>();
                await _services.GetService<IeToroService>().GetAllWatchlistsTraderTrades(tradeData);

                foreach (var trader in tradeData)
                {
                    File.WriteAllText(Path.Combine(_config.Base.CacheDir, "TraderCache", trader.Key + ".json"), JsonSerializer.Serialize(trader.Value));
                }

                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }
    }
}
