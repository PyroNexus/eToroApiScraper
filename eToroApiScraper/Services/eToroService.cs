using eToroApiScraper.Objects;
using eToroApiScraper.Pages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

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

        readonly string _username;
        readonly string _password;
        readonly List<string> _watchlists;

        static readonly Uri _watchlistsUri = new Uri("https://www.etoro.com/watchlists/");
        static readonly string _loginUrl = "https://www.etoro.com/login";
        static readonly Uri _apiUri = new Uri("https://www.etoro.com/api/streams/v2/streams/user-trades/");

        public eToroService(IOptions<eToroServiceOptions> options, ILogger<eToroService> logger)
        {
            _logger = logger;

            _username = options.Value.Username;
            _password = options.Value.Password;
            _watchlists = options.Value.Watchlists;

            _driverOptions = new ChromeOptions();
            _driverOptions.AddExcludedArgument("enable-automation");
            _driverOptions.AddArgument($"--user-agent={options.Value.UserAgent}");
            _driverOptions.AddArgument("--start-maximized");
            _driverOptions.AddArgument("--disable-blink-features=AutomationControlled");
            _driverOptions.AddArgument($"user-data-dir={options.Value.UserDataDir}");
        }

        IWebDriver _driver;
        IWebDriver driver
        {
            get
            {
                if (_driver == null)
                {
                    _driver = new ChromeDriver(_driverOptions);
                    _driver.Url = _watchlistsUri.ToString();
                }
                return _driver;
            }
        }

        public void Dispose() => Dispose(true);

        void Dispose(bool disposing)
        {
            if (_driver != null)
            {
                _driver.Close();
                if (disposing)
                {
                    _driver.Quit();
                    _driver.Dispose();
                }
                _driver = null;
            }
        }

        void HupDriver()
        {
            _logger.LogInformation("Attempting to restart driver...");
            Dispose(false);
        }

        bool IsLoggedIn() => GetMyPeopleLink() != null;
        IReadOnlyCollection<IWebElement> GetMyPeopleLink() =>
            WaitUntilElementsAreVisible(By.CssSelector(string.Format("a[automation-id='menu-user-page-link'][href='/people/{0}']", _username)));
        IReadOnlyCollection<IWebElement> GetWatchlistPeopleLink(string trader = null) =>
            WaitUntilElementsAreVisible(By.CssSelector(string.Format("a.card-avatar-wrap[href^='/people/{0}']", trader)));

        IWebElement LoginField => WaitUntilElementsAreVisible(Pages.Login.input_Username).Single();
        IWebElement PasswordField => WaitUntilElementsAreVisible(Pages.Login.input_Password).Single();
        IWebElement SignInButton => WaitUntilElementsAreVisible(Pages.Login.button_SignIn).Single();


        public async Task GetAllWatchlistsPeopleTrades(Dictionary<string, List<eToroTrade>> tradeData)
        {
            try
            {
                foreach (string watchlist in _watchlists)
                    await GetWatchlistPeopleTrades(tradeData, new Uri(_watchlistsUri, watchlist));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error obtaining all watchlist people trades.");
                HupDriver();
            }
        }

        async Task GetWatchlistPeopleTrades(Dictionary<string, List<eToroTrade>> tradeData, Uri watchlistUri)
        {
            await Login();

            driver.Navigate().GoToUrl(_watchlistsUri);
            await Task.Delay(TimeSpan.FromSeconds(2.5));
            driver.Navigate().GoToUrl(watchlistUri);

            var traders = new List<string>();

            foreach (var profileLinks in GetWatchlistPeopleLink())
                traders.Add(profileLinks.Text.ToLower());

            foreach (var trader in traders)
            {
                var peopleLink = GetWatchlistPeopleLink(trader);
                if (peopleLink == null)
                {
                    _logger.LogError("Unable to get profile link for trader {0}", trader);
                    continue;
                }
                await GetTradesForPeopleLink(peopleLink.Single(), tradeData);
                await Task.Delay(TimeSpan.FromSeconds(2.5));
                driver.Navigate().GoToUrl(_watchlistsUri);
                await Task.Delay(TimeSpan.FromSeconds(2.5));
                driver.Navigate().GoToUrl(watchlistUri);
            }
        }

        async Task GetTradesForPeopleLink(IWebElement peopleLink, Dictionary<string, List<eToroTrade>> tradeData)
        {
            string trader = peopleLink.Text.ToLower();
            try
            {
                peopleLink.Click();
                var portfolioLink = WaitUntilElementsAreVisible(People.PortfolioLink).Single();
                portfolioLink.Click();
                await Task.Delay(TimeSpan.FromSeconds(2.5));
                driver.Navigate().GoToUrl(new Uri(_apiUri, trader));
                await Task.Delay(TimeSpan.FromSeconds(2.5));

                var data = WaitUntilElementsAreVisible(By.CssSelector("body")).Single().Text;

                tradeData.Add(trader, JsonSerializer.Deserialize<List<eToroTrade>>(data));
            }
            catch
            {
                _logger.LogError("Unable to get trades for trader {0}", trader);
            }
        }

        public async Task Login()
        {
            if (IsLoggedIn())
                return;

            var tries = 0;
            var maxTries = 2;

            startlogin:
            try
            {
                if (driver.Url == _loginUrl)
                {
                    _logger.LogInformation("Attempting to login...");
                    LoginField.Click();
                    LoginField.Clear();
                    LoginField.SendKeys(_username);
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    PasswordField.Click();
                    PasswordField.Clear();
                    PasswordField.SendKeys(_password);
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    SignInButton.Click();
                }

                if (!IsLoggedIn())
                {
                    _logger.LogError("I am not logged in");

                    if (tries >= maxTries)
                    {
                        throw new Exception("Unable to login");
                    }

                    driver.Navigate().GoToUrl(_watchlistsUri);
                    await Task.Delay(TimeSpan.FromSeconds(60));
                    tries += 1;
                    goto startlogin;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error while logging in.");
                HupDriver();
                goto startlogin;
            }
        }

        IReadOnlyCollection<IWebElement> WaitUntilElementsAreVisible(By by)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromMinutes(1));
            try
            {
                return wait.Until(e => {
                    var el = e.FindElements(by);
                    if (el.Any())
                        return el;
                    else
                        return null;
                });
            }
            catch (WebDriverTimeoutException exception)
            {
                if (exception.InnerException.GetType() == typeof(NoSuchElementException))
                {
                    _logger.LogError(exception, "Could not find element {0}", by.ToString());
                    return null;
                }
                _logger.LogError(exception, "An unexpected error occurred processing {0}", by.ToString());
                throw exception;
            }
        }
    }
}
