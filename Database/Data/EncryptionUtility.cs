using System;
using System.Security.Cryptography;
using System.Text;

namespace Database.Data
{
    public static class EncryptionUtility
    {
        public static string EncryptString(string input)
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
        }
    }
}
