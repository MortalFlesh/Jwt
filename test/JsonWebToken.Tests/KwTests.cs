﻿using Xunit;
using System;
using JsonWebToken.Internal;
using System.Text;

namespace JsonWebToken.Tests
{
    public class KwTests
    {
        private readonly SymmetricJwk _keyToWrap = SymmetricJwk.FromBase64Url("U1oK6e4BAR4kKTdyA1OqEFYwX9pIrswuUMNt8qW4z-k");
        private readonly SymmetricJwk _key= SymmetricJwk.FromByteArray(Encoding.UTF8.GetBytes("gXoKEcss-xFuZceE"));

        [Fact]
        public void WrapUnwrap()
        {
            var kwp = new AesKeyWrapper(_key, EncryptionAlgorithm.Aes128CbcHmacSha256, KeyManagementAlgorithm.Aes128KW);
            byte[] wrappedKey = new byte[kwp.GetKeyWrapSize()];
            var cek = kwp.WrapKey(_keyToWrap, null, wrappedKey);

            var kuwp = new AesKeyUnwrapper(_key, EncryptionAlgorithm.Aes128CbcHmacSha256, KeyManagementAlgorithm.Aes128KW);
            var unwrappedKey = new byte[kuwp.GetKeyUnwrapSize(wrappedKey.Length)];
            var unwrapped = kuwp.TryUnwrapKey(wrappedKey, unwrappedKey, null, out int keyWrappedBytesWritten);
            Assert.True(unwrapped);
        }
    }
}
