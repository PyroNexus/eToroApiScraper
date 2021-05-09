using eToroApiScraper.Objects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eToroApiScraper.Services
{
    public class eToroServiceOptions
    {
        public string UserAgent { get; set; }
        public string UserDataDir { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public List<string> Watchlists { get; set; }
    }

    public sealed class eToroService : IeToroService, IDisposable
    {
        readonly ILogger _logger;
        readonly ChromeOptions _driverOptions;

        static string username;
        static string password;

        private List<string> watchlists;

        static readonly Uri watchlistsUri = new Uri("https://www.etoro.com/watchlists/");
        static readonly string loginUrl = "https://www.etoro.com/login";
        static readonly Uri apiUri = new Uri("https://www.etoro.com/api/streams/v2/streams/user-trades/");

        public eToroService(IOptions<eToroServiceOptions> options, ILogger<eToroService> logger)
        {
            _logger = logger;

            username = options.Value.Username;
            password = options.Value.Password;
            watchlists = options.Value.Watchlists;

            _driverOptions = new ChromeOptions();
            _driverOptions.AddExcludedArgument("enable-automation");
            _driverOptions.AddAdditionalCapability("useAutomationExtension", false);
            _driverOptions.AddArgument(string.Format("--user-agent={0}", options.Value.UserAgent));
            _driverOptions.AddArgument("--start-maximized");
            _driverOptions.AddArgument("--disable-blink-features=AutomationControlled");
            _driverOptions.AddArgument(string.Format("user-data-dir={0}", options.Value.UserDataDir));
        }

        IWebDriver _driver;
        IWebDriver driver
        {
            get
            {
                if (_driver == null)
                {
                    _driver = new ChromeDriver(_driverOptions);
                    _driver.Url = watchlistsUri.ToString();
                }
                return _driver;
            }
        }

        public void Dispose()
        {
            if (_driver != null)
            {
                _driver.Close();
                _driver.Quit();
                _driver.Dispose();
                _driver = null;
            }
        }

        bool IsLoggedIn() => GetMyProfileLink() != null;
        IReadOnlyCollection<IWebElement> GetMyProfileLink() =>
            WaitUntilElementsAreVisible(By.CssSelector(string.Format("a[automation-id='menu-user-page-link'][href='/people/{0}']", username)));
        IReadOnlyCollection<IWebElement> GetWatchlistProfile(string trader = null) =>
            WaitUntilElementsAreVisible(By.CssSelector(string.Format("a.card-avatar-wrap[href^='/people/{0}']", trader)));

        public async Task GetAllWatchlistsTraderTrades(Dictionary<string, List<eToroTrade>> tradeData)
        {
            foreach (string watchlist in watchlists)
            {
                await GetWatchlistTraderTrades(tradeData, new Uri(watchlistsUri, watchlist));
            }
        }

        public async Task GetWatchlistTraderTrades(Dictionary<string, List<eToroTrade>> tradeData, Uri watchlistUri)
        {
            driver.Navigate().GoToUrl(watchlistsUri);
            await Task.Delay(TimeSpan.FromSeconds(2.5));
            driver.Navigate().GoToUrl(watchlistUri);

            var traders = new List<string>();

            foreach (var trader in GetWatchlistProfile())
            {
                traders.Add(trader.Text.ToLower());
            }

            foreach (var trader in traders)
            {
                var profile = GetWatchlistProfile(trader);
                if (profile == null)
                {
                    _logger.LogError("Unable to get trades for trader {0}", trader);
                }
                await GetTradesForProfileLink(profile.Single(), tradeData);
                await Task.Delay(TimeSpan.FromSeconds(2.5));
                driver.Navigate().GoToUrl(watchlistsUri);
                await Task.Delay(TimeSpan.FromSeconds(2.5));
                driver.Navigate().GoToUrl(watchlistUri);
            }
        }

        public class Trader
        {
            public string Name { get; set; }
            public string Selector { get; set; }
        }

        async Task GetTradesForProfileLink(IWebElement profileLink, Dictionary<string, List<eToroTrade>> tradeData)
        {
            try
            {
                var trader = profileLink.Text;
                profileLink.Click();
                var profileNav = WaitUntilElementsAreVisible(By.CssSelector("a[automation-id='user-head-navigation-wrapp-portfolio']")).Single();
                profileNav.Click();
                await Task.Delay(TimeSpan.FromSeconds(2.5));
                driver.Navigate().GoToUrl(new Uri(apiUri, trader));
                await Task.Delay(TimeSpan.FromSeconds(2.5));

                var data = WaitUntilElementsAreVisible(By.CssSelector("body")).Single().Text;

                tradeData.Add(trader, JsonSerializer.Deserialize<List<eToroTrade>>(data));
            }
            catch
            {
                _logger.LogError("Unable to get trades");
            }
        }

        public async Task Login()
        {
            var tries = 0;
            var maxTries = 2;

            startlogin:
            if (driver.Url == loginUrl)
            {
                var wait = new WebDriverWait(driver, new TimeSpan(0, 1, 0));

                var byUsername = By.CssSelector("input[automation-id='login-sts-username-input']");
                var byPassword = By.CssSelector("input[automation-id='login-sts-password-input']");
                var bySignInButton = By.CssSelector("button[automation-id='login-sts-btn-sign-in']");

                wait.Until(e => e.FindElement(byUsername));

                var usernameField = driver.FindElement(byUsername);
                var passwordField = driver.FindElement(byPassword);
                var signInButton = driver.FindElement(bySignInButton);

                usernameField.Click();
                usernameField.SendKeys(username);
                await Task.Delay(TimeSpan.FromSeconds(1));
                passwordField.Click();
                passwordField.SendKeys(password);
                await Task.Delay(TimeSpan.FromSeconds(1));
                signInButton.Click();
            }

            if (!IsLoggedIn())
            {
                _logger.LogError("I am not logged in");

                if (tries >= maxTries)
                {
                    throw new Exception("Unable to login");
                }

                driver.Navigate().GoToUrl(watchlistsUri);
                await Task.Delay(TimeSpan.FromSeconds(60));
                tries += 1;
                goto startlogin;
            }
        }

        IReadOnlyCollection<IWebElement> WaitUntilElementsAreVisible(By by)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromMinutes(1));
            try
            {
                var elements = wait.Until(e => {
                    var el = e.FindElements(by);
                    if (el.Any())
                        return el;
                    else
                        return null;
                });
                return elements;
            }
            catch (WebDriverTimeoutException exception)
            {
                if (exception.InnerException.GetType() == typeof(NoSuchElementException))
                {
                    _logger.LogError(exception, "Could not find element {0}", by.ToString());
                    return null;
                }
                throw exception;
            }
        }

    }
}
