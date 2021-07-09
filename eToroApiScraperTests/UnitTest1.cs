using eToroApiScraper.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace eToroScraperTests
{
    [TestClass]
    public class CryptoHelperTests
    {
        [TestMethod]
        public void Test_EncryptDecryptNewKey()
        {
            var plaintext = "hello";

            var helper = new CryptoHelper(CryptoHelper.NewKey());

            var encrypted = helper.EncryptString(plaintext);

            var decrypted = helper.DecryptString(encrypted);

            Assert.IsTrue(plaintext == decrypted);
        }

        [TestMethod]
        public void Test_Decrypt()
        {
            var helper = new CryptoHelper("s/XmBX61n7hqVgx1tzxrCMqysnAXKKafKpOOrpcvi8E=");

            var decrypted = helper.DecryptString("jqV0yxsliCJ/LeZRXjGJdjhn00b+o/FgSGJ8jwD60oE=");

            Assert.IsTrue("hello" == decrypted);
        }

        [TestMethod]
        public void Test_Encrypt()
        {
            var helper = new CryptoHelper("s/XmBX61n7hqVgx1tzxrCMqysnAXKKafKpOOrpcvi8E=");

            var encrypted = helper.EncryptString("hello");

            Assert.IsTrue(helper.DecryptString("jyErNIk+tYYV5mwRMHSrNN//SJF1wxc6yrJU4Uclpeo=") == helper.DecryptString(encrypted));
        }

        //[TestMethod]
        //public void Test_Encrypt2()
        //{
        //    var key = CryptoHelper.NewKey();
        //    var helper = new CryptoHelper(key);

        //    var encrypted = helper.EncryptString("");
        //    var encrypted2 = helper.EncryptString("");
        //    var encrypted3 = helper.EncryptString("");
        //}
    }
}
