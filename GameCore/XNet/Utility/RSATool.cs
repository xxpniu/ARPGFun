using System;
using System.Security.Cryptography;

namespace XNet.Libs.Utility
{
    public static class RSATool
    {
        public static byte[] Decryption(byte[] Data, RSAParameters RSAKey, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData;
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(RSAKey);
                    decryptedData = rsa.Decrypt(Data, DoOAEPPadding);
                    
                }
                return decryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public static byte[] Encryption(byte[] Data, RSAParameters RSAKey, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(RSAKey);
                    encryptedData = rsa.Encrypt(Data, DoOAEPPadding);
                }
                return encryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
