﻿// Copyright (c) 2018 Yann Crumeyrolle. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for more information.
using System;

namespace JsonWebToken.Internal
{
    internal sealed class DirectKeyWrapper : KeyWrapper
    {
        public DirectKeyWrapper(Jwk key, EncryptionAlgorithm encryptionAlgorithm, KeyManagementAlgorithm algorithm)
            : base(key, encryptionAlgorithm, algorithm)
        {
        }

        public override int GetKeyUnwrapSize(int wrappedKeySize)
        {
            return wrappedKeySize;
        }

        public override int GetKeyWrapSize()
        {
            return 0;
        }

        public override bool TryUnwrapKey(ReadOnlySpan<byte> keyBytes, Span<byte> destination, JwtHeader header, out int bytesWritten)
        {
            keyBytes.CopyTo(destination);
            bytesWritten = keyBytes.Length;
            return true;
        }

        public override Jwk WrapKey(Jwk? staticKey, JwtObject header, Span<byte> destination)
        {
            if (staticKey != null)
            {
                ThrowHelper.ThrowArgumentException_StaticKeyNotSupported();
            }

            ReadOnlySpan<byte> bytes = Key.AsSpan();
            return SymmetricJwk.FromSpan(bytes, false);
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}
