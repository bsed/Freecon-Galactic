using System;
using System.Security.Cryptography;

namespace Core.Cryptography
{
    public class CryptographyManager
    {
        TripleDESCryptoServiceProvider _cryptoService;

        public RandomNumberGenerator RNG { get; protected set; }

        public CryptographyManager()
        {
            _cryptoService = new TripleDESCryptoServiceProvider();
            RNG = new RNGCryptoServiceProvider();
        }
        
        public IVKey GenerateIVKey()
        {
            _cryptoService.GenerateIV();
            _cryptoService.GenerateKey();
            return new IVKey(_cryptoService.Key, _cryptoService.IV);
        }
        public string GetRandomToken(int length = 32)
        {
            byte[] tokenData = new byte[32];
            RNG.GetBytes(tokenData);

            return Convert.ToBase64String(tokenData);
        }

    }

    public class IVKey
    {
        public byte[] Key {get;set;}
        public byte[] IV {get;set;}
        
        public IVKey(byte[] key, byte[] iv)
        {
            Key = key;
            IV = iv;
        }

    }
}
