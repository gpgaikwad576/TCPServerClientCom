using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace TCPServerService.Helper
{
    public static class CryptoHelper
    {
        private static readonly string
            Key = "t6Xcz5kQ0aC7HwY9MDc5IDjGOyXsjDk9q4vRtD2Gb/4="; // Replace with your actual key

        private static readonly string Iv = "mJ1XfGq1u5hGb5MxNnL1mA=="; // Replace with your actual IV

        // AES Encryption
        public static string Encrypt(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(Key); // Ensure the key is 256 bits
                aesAlg.IV = Encoding.UTF8.GetBytes(Iv); // Ensure the IV is 128 bits

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        // AES Decryption
        public static string Decrypt(string cipherText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(Key); // Ensure the key is 256 bits
                aesAlg.IV = Encoding.UTF8.GetBytes(Iv); // Ensure the IV is 128 bits

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
