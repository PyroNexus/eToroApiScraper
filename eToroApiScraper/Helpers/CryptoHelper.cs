using System;
using System.IO;
using System.Security.Cryptography;
using System.Linq;

namespace eToroApiScraper.Helpers
{
    public class CryptoHelper
    {
        private readonly byte[] Key;

        public CryptoHelper()
            : this(Environment.GetEnvironmentVariable("PyroNexusConfigKey"))
        { }

        public CryptoHelper(string key) => Key = Convert.FromBase64String(key);

        public static string NewKey()
        {
            using Aes aes = Aes.Create();
            aes.GenerateKey();
            return Convert.ToBase64String(aes.Key);
        }

        public string EncryptString(string plainText)
        {
            byte[] buffer;

            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.GenerateIV();

            using MemoryStream memoryStream = new MemoryStream();
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(plainText);
            }
            buffer = memoryStream.ToArray();

            return Convert.ToBase64String(Enumerable.Concat(aes.IV, buffer).ToArray());
        }

        public string DecryptString(string cipherText)
        {
            byte[] buffer = Convert.FromBase64String(cipherText);

            using Aes aes = Aes.Create();

            aes.Key = Key;
            aes.IV = buffer.Take(16).ToArray();
            var data = buffer.TakeLast(buffer.Length - 16).ToArray();

            using MemoryStream memoryStream = new MemoryStream(data);
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        }
    }
}
