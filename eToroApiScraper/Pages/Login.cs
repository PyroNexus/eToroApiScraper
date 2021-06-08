using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace eToroApiScraper.Pages
{
    public class Login
    {
        public static By input_Username = By.CssSelector("input[automation-id='login-sts-username-input']");
        public static By input_Password = By.CssSelector("input[automation-id='login-sts-password-input']");
        public static By button_SignIn = By.CssSelector("button[automation-id='login-sts-btn-sign-in']");
    }
}
