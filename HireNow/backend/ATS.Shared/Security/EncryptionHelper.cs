using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ATS.Shared.Security
{
    public static class EncryptionHelper
    {
        private static readonly byte[] Key;

        static EncryptionHelper()
        {
            var keyStr = Environment.GetEnvironmentVariable("PII_ENCRYPTION_KEY") 
                         ?? "this_is_a_fallback_encryption_key_for_pii_security_32_bytes_long_!";
            
            var bytes = Encoding.UTF8.GetBytes(keyStr);
            Key = new byte[32];
            Array.Copy(bytes, Key, Math.Min(bytes.Length, 32));
        }

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.GenerateIV();
                var iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length);

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    var encrypted = ms.ToArray();
                    return "ENC_" + Convert.ToBase64String(encrypted);
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;
            if (!cipherText.StartsWith("ENC_")) return cipherText;

            try
            {
                var cleanCipher = cipherText.Substring(4);
                var fullCipher = Convert.FromBase64String(cleanCipher);

                using (var aes = Aes.Create())
                {
                    aes.Key = Key;
                    
                    var iv = new byte[16];
                    Array.Copy(fullCipher, iv, 16);
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
                    using (var ms = new MemoryStream(fullCipher, 16, fullCipher.Length - 16))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch
            {
                return cipherText;
            }
        }
    }
}
