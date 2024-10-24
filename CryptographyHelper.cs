using System.Security.Cryptography;
using System.Text;

namespace sftp_client
{
    public static class CryptographyHelper
    {
        private static readonly byte[] _rgbIV = { 168, 135, 175, 106, 14, 6, 181, 20 };

        private static readonly byte[] _rgbKey = {
                                                     243, 79, 18, 135, 100, 247, 196, 101, 198, 159, 13, 41, 66, 179, 126,
                                                     147, 128, 8, 43, 39, 235, 63, 25, 119
                                                 };
        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

        public static string EncryptToBase64(string value, Encoding encoding)
        {
            return Convert.ToBase64String(Encrypt(encoding.GetBytes(value)));
        }

        public static string DecryptFromBase64(string value, Encoding encoding)
        {
            return encoding.GetString(Decrypt(Convert.FromBase64String(value)));
        }

        public static byte[] Encrypt(byte[] bytes)
        {
            var des = new TripleDESCryptoServiceProvider();
            byte[] result;
            using (var encryptor = des.CreateEncryptor(_rgbKey, _rgbIV))
            {
                result = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            }
            des.Clear();
            return result;
        }

        public static byte[] Decrypt(byte[] bytes)
        {
            var des = new TripleDESCryptoServiceProvider();
            byte[] result;
            using (var decryptor = des.CreateDecryptor(_rgbKey, _rgbIV))
            {
                result = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            }
            des.Clear();
            return result;
        }
    }
}
