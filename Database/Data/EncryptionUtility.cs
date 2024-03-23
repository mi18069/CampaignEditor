using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Database.Data
{
    public static class EncryptionUtility
    {
        /*public static string EncryptString(string input)
        {
            byte[] encryptedData = ProtectedData.Protect(
                Encoding.Unicode.GetBytes(input),
                null,
                DataProtectionScope.CurrentUser
            );

            return Convert.ToBase64String(encryptedData);
        }

        public static string DecryptString(string encryptedInput)
        {
            byte[] encryptedData = Convert.FromBase64String(encryptedInput);

            byte[] decryptedData = ProtectedData.Unprotect(
                encryptedData,
                null,
                DataProtectionScope.CurrentUser
            );

            return Encoding.Unicode.GetString(decryptedData);
        }*/

        public static string EncryptString(string plainText)
        {
            string key = "b!hs6apgjs77@oiab5uc8sVnTHanTHap";

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);

                // Generate a random IV for this encryption operation
                aesAlg.GenerateIV();
                byte[] iv = aesAlg.IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // Prepend the IV to the encrypted data
                    msEncrypt.Write(iv, 0, iv.Length);

                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }

                    byte[] encryptedBytes = msEncrypt.ToArray();
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        public static string DecryptString(string cipherText)
        {
            string key = "b!hs6apgjs77@oiab5uc8sVnTHanTHap";

            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            // Extract the IV from the beginning of the cipherText
            byte[] iv = new byte[16]; // IV size for AES
            Array.Copy(cipherBytes, 0, iv, 0, iv.Length);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes, iv.Length, cipherBytes.Length - iv.Length))
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
