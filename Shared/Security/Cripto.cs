using System;
using System.Text;
using System.Security.Cryptography;

namespace iSportsRecruiting.Shared.Security
{
    public static class Cripto
    {
        private static readonly string _secret = "R.&Z?gQm6}nj8655";
        public static string EncryptPassword(string password)
        {
            try
            {
                byte[] data = new byte[password.Length];
                SHA512 shaM = new SHA512Managed();
                var result = shaM.ComputeHash(data);

                return Convert.ToBase64String(result);
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public static string DecryptPassword(string tag, string password)
        {

            ReadOnlySpan<byte> tagBytes = Convert.FromBase64String(tag);
            ReadOnlySpan<byte> cipherText = Convert.FromBase64String(password);
            Span<byte> plainText = stackalloc byte[cipherText.Length];
            ReadOnlySpan<byte> nonce = Encoding.UTF8.GetBytes(_secret);

            AesCcm aes = new AesCcm(nonce);
            aes.Decrypt(nonce[..12], cipherText, tagBytes, plainText);

            return Encoding.UTF8.GetString(plainText);
        }
    }
}
