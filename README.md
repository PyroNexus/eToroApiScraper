# eToro Api Scraper
Uses Selenium WebDriver to login to eToro and simulate user actions to retrieve the api stream from https://www.etoro.com/api/streams/v2/streams/user-trades/username

This is because eToro make it impossible to obtain an API key and now they are using Cloudflare to try to prevent scraping this...

## How to use
1. Sign up for eToro with a new account

2. Create a environment variable for PyroNexusConfigKey and PyroNexusConfigIV containing an aes key and IV for Config.json

3. Create some watchlists with the desired traders

4. Encrypt the watchlist id (the bit at the end of http://www.etoro.com/watchlists/) with the key and IV and place into Config.json

5. Encrypt the username and password with the key and IV and place into Config.json

6. Set a desired dir for the program to store files

7. The first run of the program, you will want to enable saving cookies in the Chrome settings - subsequent launches will retain the setting (not figured out how to automate this yet)

8. Run the program for real... it should then save the json files into TraderCache folder if all is successful...