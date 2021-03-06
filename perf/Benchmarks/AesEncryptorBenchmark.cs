﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using JsonWebToken.Internal;

namespace JsonWebToken.Performance
{
    [MemoryDiagnoser]
    public class AesEncryptorBenchmark
    {
        private static AesCbcEncryptor _encryptor;
#if NETCOREAPP3_0
        private static Aes128NiCbcEncryptor _encryptorNi;
#endif
        private static byte[] ciphertext;
        private static byte[] nonce;

        static AesEncryptorBenchmark()
        {
            ciphertext = new byte[(2048 * 16 + 16) & ~15];
            var key = SymmetricJwk.GenerateKey(128);
            nonce = new byte[] { 0x1, 0x1, 0x1, 0x1, 0x1, 0x1, 0x1, 0x1, 0x1, 0x1, 0x1, 0x1, 0x1, 0x1, 0x1, 0x1 };
            _encryptor = new AesCbcEncryptor(key.K, EncryptionAlgorithm.Aes128CbcHmacSha256);
#if NETCOREAPP3_0
            _encryptorNi = new Aes128NiCbcEncryptor(key.K);
#endif  
        }

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(GetData))]
        public void Encrypt(Item data)
        {
            _encryptor.Encrypt(data.Plaintext, nonce, ciphertext);
        }

#if NETCOREAPP3_0
        [Benchmark(Baseline = false)]
        [ArgumentsSource(nameof(GetData))]
        public void Encrypt_Simd(Item data)
        {
            _encryptorNi.Encrypt(data.Plaintext, nonce, ciphertext);
        }
#endif

        public static IEnumerable<Item> GetData()
        {
            yield return new Item(Encoding.UTF8.GetBytes(Enumerable.Repeat('a', 1).ToArray()));
            yield return new Item(Encoding.UTF8.GetBytes(Enumerable.Repeat('a', 2048).ToArray()));
            yield return new Item(Encoding.UTF8.GetBytes(Enumerable.Repeat('a', 2048 * 16).ToArray()));
        }

        public class Item
        {
            public Item(byte[] plaintext)
            {
                Plaintext = plaintext;
            }

            public byte[] Plaintext { get; }

            public override string ToString()
            {
                return Plaintext.Length.ToString();
            }
        }
    }
}
