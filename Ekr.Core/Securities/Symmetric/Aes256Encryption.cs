using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Ekr.Core.Securities.Symmetric
{
    public static class Aes256Encryption
    {
        //nanti dipindah
        public const string AESKEY = "9IO5BHL8G3NJRQ35L5B3NI67LJ25DVES";
        public const string AESIV = "a4AUpovM1WxGNrXqKae9qw==";
        public static CryptoStream EncryptStream(Stream responseStream)
        {
            Aes aes = GetEncryptionAlgorithm();

            CryptoStream base64EncodedStream = new CryptoStream(responseStream, new ToBase64Transform(), CryptoStreamMode.Write);
            CryptoStream cryptoStream = new CryptoStream(base64EncodedStream, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write);            

            return cryptoStream;
        }


        private static Aes GetEncryptionAlgorithm()
        {
            Aes aes = Aes.Create();
            aes.Key = Convert.FromBase64String(AESKEY);
            aes.IV = Convert.FromBase64String(AESIV);

            return aes;
        }

        //public static Stream DecryptStream(Stream cipherStream)
        //{
        //    Aes aes = GetEncryptionAlgorithm();

        //    FromBase64Transform base64Transform = new FromBase64Transform(FromBase64TransformMode.IgnoreWhiteSpaces);
        //    CryptoStream base64DecodedStream = new CryptoStream(cipherStream, base64Transform, CryptoStreamMode.Read);
        //    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        //    CryptoStream decryptedStream = new CryptoStream(base64DecodedStream, decryptor, CryptoStreamMode.Read);
        //    return decryptedStream;
        //}

        public static string DecryptString(string cipherText)
        {
            Aes aes = GetEncryptionAlgorithm();
            byte[] buffer = Convert.FromBase64String(cipherText);

            using MemoryStream memoryStream = new MemoryStream(buffer);
            using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        }


        public static string ModifiedEncrypt(this string text, string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key must have valid value.", nameof(key));
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("The text must have valid value.", nameof(text));

            var buffer = Encoding.UTF8.GetBytes(text);
            var hash = new SHA512CryptoServiceProvider();
            var aesKey = new byte[32]; // 256 bits
            Buffer.BlockCopy(hash.ComputeHash(Encoding.UTF8.GetBytes(key)), 0, aesKey, 0, 32);

            using (var aes = Aes.Create())
            {
                if (aes == null)
                    throw new ArgumentException("Parameter must not be null.", nameof(aes));

                aes.Key = aesKey;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var resultStream = new MemoryStream())
                {
                    using (var aesStream = new CryptoStream(resultStream, encryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(buffer))
                    {
                        plainStream.CopyTo(aesStream);
                    }

                    var result = resultStream.ToArray();
                    var combined = new byte[aes.IV.Length + result.Length];
                    Array.ConstrainedCopy(aes.IV, 0, combined, 0, aes.IV.Length);
                    Array.ConstrainedCopy(result, 0, combined, aes.IV.Length, result.Length);

                    return Convert.ToBase64String(combined);
                }
            }
        }

        public static string ModifiedDecrypt(this string encryptedText, string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key must have valid value.", nameof(key));
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentException("The encrypted text must have valid value.", nameof(encryptedText));

            var combined = Convert.FromBase64String(encryptedText);
            var buffer = new byte[combined.Length];
            var hash = new SHA512CryptoServiceProvider();
            var aesKey = new byte[32];
            Buffer.BlockCopy(hash.ComputeHash(Encoding.UTF8.GetBytes(key)), 0, aesKey, 0, 32);

            using (var aes = Aes.Create())
            {
                if (aes == null)
                    throw new ArgumentException("Parameter must not be null.", nameof(aes));

                aes.Key = aesKey;

                var iv = new byte[aes.IV.Length];
                var ciphertext = new byte[buffer.Length - iv.Length];

                Array.ConstrainedCopy(combined, 0, iv, 0, iv.Length);
                Array.ConstrainedCopy(combined, iv.Length, ciphertext, 0, ciphertext.Length);

                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var resultStream = new MemoryStream())
                {
                    using (var aesStream = new CryptoStream(resultStream, decryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(ciphertext))
                    {
                        plainStream.CopyTo(aesStream);
                    }

                    return Encoding.UTF8.GetString(resultStream.ToArray());
                }
            }
        }

        public static string Encrypt(this string text, string key)
        {
            #region Microsoft Code
            // Check arguments.
            //if (plainText == null || plainText.Length <= 0)
            //    throw new ArgumentNullException("plainText");
            //if (key == null || key.Length <= 0)
            //    throw new ArgumentNullException("Key");
            //byte[] encrypted;
            //var hash = new SHA512CryptoServiceProvider();
            //var aesKey = new byte[32];
            //Buffer.BlockCopy(hash.ComputeHash(Encoding.UTF8.GetBytes(key)), 0, aesKey, 0, 32);

            //// Create an Aes object
            //// with the specified key and IV.
            //using (Aes aesAlg = Aes.Create())
            //{
            //    aesAlg.Key = aesKey;

            //    // Create an encryptor to perform the stream transform.
            //    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            //    // Create the streams used for encryption.
            //    using (MemoryStream msEncrypt = new MemoryStream())
            //    {
            //        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            //        {
            //            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
            //            {
            //                //Write all data to the stream.
            //                swEncrypt.Write(plainText);
            //            }
            //            encrypted = msEncrypt.ToArray();

            //            // Return the encrypted bytes from the memory stream.
            //            return Convert.ToBase64String(encrypted);
            //        }
            //    }
            //}
            #endregion

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key must have valid value.", nameof(key));
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("The text must have valid value.", nameof(text));

            var buffer = Encoding.UTF8.GetBytes(text);
            var hash = new SHA512CryptoServiceProvider();
            var aesKey = new byte[32]; // 256 bits
            Buffer.BlockCopy(hash.ComputeHash(Encoding.UTF8.GetBytes(key)), 0, aesKey, 0, 32);

            using (var aes = Aes.Create())
            {
                if (aes == null)
                    throw new ArgumentException("Parameter must not be null.", nameof(aes));

                aes.Key = aesKey;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var resultStream = new MemoryStream())
                {
                    using (var aesStream = new CryptoStream(resultStream, encryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(buffer))
                    {
                        plainStream.CopyTo(aesStream);
                    }

                    var result = resultStream.ToArray();
                    var combined = new byte[aes.IV.Length + result.Length];
                    Array.ConstrainedCopy(aes.IV, 0, combined, 0, aes.IV.Length);
                    Array.ConstrainedCopy(result, 0, combined, aes.IV.Length, result.Length);

                    return Convert.ToBase64String(combined);
                }
            }
        }

        public static string Decrypt(this string encryptedText, string key)
        {
            #region Microsoft Code
            // Check arguments.
            //if (cipherText == null || cipherText.Length <= 0)
            //    throw new ArgumentNullException("cipherText");
            //if (key == null || key.Length <= 0)
            //    throw new ArgumentNullException("Key");

            //var textByte = Convert.FromBase64String(cipherText);

            //var hash = new SHA512CryptoServiceProvider();
            //var aesKey = new byte[32]; // 256 bits
            //Buffer.BlockCopy(hash.ComputeHash(Encoding.UTF8.GetBytes(key)), 0, aesKey, 0, 32);

            //// Create an Aes object
            //// with the specified key and IV.
            //using (Aes aesAlg = Aes.Create())
            //{
            //    aesAlg.Key = aesKey;

            //    // Create a decryptor to perform the stream transform.
            //    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            //    // Create the streams used for decryption.
            //    using (MemoryStream msDecrypt = new MemoryStream(textByte))
            //    {
            //        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            //        {
            //            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
            //            {

            //                // Read the decrypted bytes from the decrypting stream
            //                // and place them in a string.
            //                var a = srDecrypt.ReadToEnd();
            //                return a;
            //            }
            //        }
            //    }
            //}
            #endregion

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key must have valid value.", nameof(key));
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentException("The encrypted text must have valid value.", nameof(encryptedText));

            var combined = Convert.FromBase64String(encryptedText);
            var buffer = new byte[combined.Length];
            var hash = new SHA512CryptoServiceProvider();
            var aesKey = new byte[32];
            Buffer.BlockCopy(hash.ComputeHash(Encoding.UTF8.GetBytes(key)), 0, aesKey, 0, 32);

            using (var aes = Aes.Create())
            {
                if (aes == null)
                    throw new ArgumentException("Parameter must not be null.", nameof(aes));

                aes.Key = aesKey;

                var iv = new byte[aes.IV.Length];
                var ciphertext = new byte[buffer.Length - iv.Length];

                Array.ConstrainedCopy(combined, 0, iv, 0, iv.Length);
                Array.ConstrainedCopy(combined, iv.Length, ciphertext, 0, ciphertext.Length);

                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var resultStream = new MemoryStream())
                {
                    using (var aesStream = new CryptoStream(resultStream, decryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(ciphertext))
                    {
                        plainStream.CopyTo(aesStream);
                    }

                    return Encoding.UTF8.GetString(resultStream.ToArray());
                }
            }
        }
    }
}
