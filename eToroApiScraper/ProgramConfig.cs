using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace eToroApiScraper
{
    public class BaseConfig
    {
        static string _defaultCacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "eToroScraper");
        string _cacheDir;
        public string CacheDir
        {
            get => string.IsNullOrWhiteSpace(_cacheDir) ? _defaultCacheDir : _cacheDir;
            set => _cacheDir = value;
        }
    }
    public class ChromeDriverConfig
    {
        static string _defaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.152 Safari/537.36";
        string _userAgent;
        public string UserAgent
        {
            get => string.IsNullOrWhiteSpace(_userAgent) ? _defaultUserAgent : _userAgent;
            set => _userAgent = value;
        }
    }
    public class EtoroConfig
    {
        string _username;
        public string Username
        {
            get => _username;
            set => _username = new Helpers.CryptoHelper().DecryptString(value);
        }
        string _password;
        public string Password
        {
            get => _password;
            set => _password = new Helpers.CryptoHelper().DecryptString(value);
        }

        List<string> _watchlists;
        public List<string> Watchlists
        {
            get => _watchlists;
            set
            {
                var watchlists = new List<string>();
                foreach (var item in value)
                    watchlists.Add(new Helpers.CryptoHelper().DecryptString(item));
                _watchlists = watchlists;
            }
        }
    }
}
