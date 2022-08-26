using System;
using System.Text;
using System.Security.Cryptography;

namespace ARIS_Net
{
    public class Tools
    {
        public static String Decrypt(String encryptedText, String key, String iv)
        {
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var aes = GetAES(key);
            aes.IV = Convert.FromBase64String(iv);
            return Encoding.UTF8.GetString(Decrypt(encryptedBytes, aes));
        }

        private static byte[] Decrypt(byte[] encryptedData, Aes aes)
        {
            return aes.CreateDecryptor()
                .TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        }

        public static Aes GetAES(String secretKey)
        {
            int keysize = 256;

            var keyBytes = new byte[keysize / 8];
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
            Array.Copy(secretKeyBytes, keyBytes, Math.Min(keyBytes.Length, secretKeyBytes.Length));

            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = keysize;
            aes.BlockSize = 128;//AES es siempre 128 el blocksize
            aes.Key = keyBytes;
            aes.GenerateIV();

            return aes;
        }
    }
}
