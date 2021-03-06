using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;

namespace EasyRSA
{
    public class CryptoRsa
    {
        private readonly RSACryptoServiceProvider provider;
        public bool IsPrivate { get; private set; }
        
        /// <summary>
        /// Create new CryptoRSA object from RsaKey
        /// </summary>
        /// <param name="key">The private or public RsaKey</param>
        public CryptoRsa(RsaKey key)
        {
            this.provider = new RSACryptoServiceProvider();
            IsPrivate = key.IsPrivate;
            provider.ImportParameters(key.Parameters);
        }
        
        /// <summary>
        /// Create new cryptoRSA object from CSPBlob
        /// </summary>
        /// <param name="cspBlob">The cspBlob containing the keys</param>
        public CryptoRsa(byte[] cspBlob) : this(new RSACryptoServiceProvider())
        {
            if (cspBlob == null) throw new ArgumentException("Invalid key, key is null.");
            this.provider.ImportCspBlob(cspBlob);
            this.IsPrivate = !provider.PublicOnly;
        }

        /// <summary>
        /// Create new CryptoRSA object using existing RSACryptoServiceProvider
        /// </summary>
        /// <param name="provider">The existing provider</param>
        public CryptoRsa(RSACryptoServiceProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.IsPrivate = !provider.PublicOnly;
        }

        /// <summary>
        /// Encrypt text with RSA
        /// </summary>
        /// <param name="text">The input string (UTF-8)</param>
        /// <returns>Encrypted string (BASE-64)</returns>
        public string Encrypt(string text) => Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(text)));
        public string Encrypt(string text, Encoding encoder)
           => Convert.ToBase64String(Encrypt(encoder.GetBytes(text)));

        /// <summary>
        /// Encrypt data
        /// </summary>
        /// <param name="data">The binary data</param>
        /// <returns>Encrypted data</returns>
        public byte[] Encrypt(byte[] data)
            => provider.Encrypt(data, false);

        /// <summary>
        /// Decrypt encrypted text
        /// </summary>
        /// <param name="text">The encrypted string (BASE-64)</param>
        /// <returns>Decrypted string (UTF-8)</returns>
        public string Decrypt(string text) => Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(text)));
        public string Decrypt(string text, Encoding encoder)
           => encoder.GetString(Decrypt(Convert.FromBase64String(text)));

        /// <summary>
        /// Decrypt encrypted data
        /// </summary>
        /// <param name="data">The encryped data</param>
        /// <returns>decrypted byte[]</returns>
        public byte[] Decrypt(byte[] data)
            => (IsPrivate) ? provider.Decrypt(data, false) : throw new ArgumentException("Can't decrypt data with a public key");

        /// <summary>
        /// Sign binary data with the private key using SHA256
        /// </summary>
        /// <param name="data">The data to be signed</param>
        /// <returns>byte[32]</returns>
        public byte[] Sign(byte[] data) => Sign(data, SHA256.Create());
        public byte[] Sign(byte[] data, HashAlgorithm algorithm)
            => (IsPrivate) ? provider.SignData(data, algorithm) : throw new ArgumentException("Can't sign data with a public key");

        /// <summary>
        /// Verify signed data with the public key
        /// </summary>
        /// <param name="data">The data to be validated with the signed data</param>
        /// <param name="signedData">byte[32] signed data</param>
        /// <returns>true or false</returns>
        public bool Verify(byte[] data, byte[] signedData) => Verify(data, SHA256.Create(), signedData);
        public bool Verify(byte[] data, HashAlgorithm algorithm, byte[] signedData)
            => provider.VerifyData(data, algorithm, signedData);
        
        /// <summary>
        /// Get private or public key (CSPBlob)
        /// </summary>
        /// <returns>byte[] key</returns>
        public byte[] GetBytes()
            => provider.ExportCspBlob(IsPrivate);
    }
}