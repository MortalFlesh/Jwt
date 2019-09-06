﻿// Copyright (c) 2018 Yann Crumeyrolle. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace JsonWebToken.Internal
{
    /// <summary>
    /// Provides Wrap key and Unwrap key services.
    /// </summary>
    internal sealed class AesKeyWrapper : KeyWrapper
    {
        private const int BlockSizeInBytes = 8;

        private static readonly ulong _defaultIV = 0XA6A6A6A6A6A6A6A6;

        private readonly ObjectPool<ICryptoTransform> _encryptorPool;
        private readonly ObjectPool<ICryptoTransform> _decryptorPool;

        private readonly Aes _aes;
        private bool _disposed;

        public AesKeyWrapper(SymmetricJwk key, EncryptionAlgorithm encryptionAlgorithm, KeyManagementAlgorithm algorithm)
            : base(key, encryptionAlgorithm, algorithm)
        {
            if (algorithm.Category != AlgorithmCategory.Aes)
            {
                ThrowHelper.ThrowNotSupportedException_AlgorithmForKeyWrap(algorithm);
            }

            _aes = GetSymmetricAlgorithm(key, algorithm);
            _encryptorPool = new ObjectPool<ICryptoTransform>(new PooledEncryptorPolicy(_aes));
            _decryptorPool = new ObjectPool<ICryptoTransform>(new PooledDecryptorPolicy(_aes));
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _encryptorPool.Dispose();
                    _decryptorPool.Dispose();
                    _aes.Dispose();
                }

                _disposed = true;
            }
        }

        private static Aes GetSymmetricAlgorithm(SymmetricJwk key, KeyManagementAlgorithm algorithm)
        {
            if (algorithm.RequiredKeySizeInBits != key.KeySizeInBits)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_KeyWrapKeySizeIncorrect(algorithm, algorithm.RequiredKeySizeInBits >> 3, key, key.KeySizeInBits);
            }

            byte[] keyBytes = key.ToArray();
            Aes? aes = null;
            try
            {
                aes = Aes.Create();
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.None;
                aes.KeySize = keyBytes.Length << 3;
                aes.Key = keyBytes;

                // Set the AES IV to Zeroes
                var iv = new byte[aes.BlockSize >> 3];
                Array.Clear(iv, 0, iv.Length);
                aes.IV = iv;

                return aes;
            }
            catch (Exception ex)
            {
                if (aes != null)
                {
                    aes.Dispose();
                }

                ThrowHelper.ThrowCryptographicException_CreateSymmetricAlgorithmFailed(key, algorithm, ex);
                throw;
            }
        }

        public override bool TryUnwrapKey(ReadOnlySpan<byte> key, Span<byte> destination, JwtHeader header, out int bytesWritten)
        {
            if (key.IsEmpty)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }

            if (key.Length % 8 != 0)
            {
                ThrowHelper.ThrowArgumentException_KeySizeMustBeMultipleOf64(key);
            }

            if (_disposed)
            {
                ThrowHelper.ThrowObjectDisposedException(GetType());
            }

            return TryUnwrapKeyPrivate(key, destination, out bytesWritten);
        }

        private unsafe bool TryUnwrapKeyPrivate(ReadOnlySpan<byte> inputBuffer, Span<byte> destination, out int bytesWritten)
        {
            var decryptor = _decryptorPool.Get();
            try
            {
                fixed (byte* input = inputBuffer)
                {
                    ulong a = *(ulong*)input;

                    // The number of input blocks
                    var n = (inputBuffer.Length - BlockSizeInBytes) >> 3;

                    // The set of input blocks
                    byte* r = stackalloc byte[n << 3];
                    for (var i = 0; i < n; i++)
                    {
                        *(ulong*)(r + (i << 3)) = *(ulong*)(input + ((i + 1) << 3));
                    }

                    byte[] block = new byte[16];
                    fixed (byte* pBlock = block)
                    {
                        byte* t = stackalloc byte[8];
                        *(ulong*)t = 0L;
                        for (var j = 5; j >= 0; j--)
                        {
                            for (var i = n; i > 0; i--)
                            {
                                *(t + 7) = (byte)((n * j) + i);
                                a ^= *(ulong*)t;
                                *(ulong*)pBlock = a;
                                ulong* rCurrent = (ulong*)(r + ((i - 1) << 3));
                                *(ulong*)(pBlock + 8) = *rCurrent;
                                fixed (byte* b = decryptor.TransformFinalBlock(block, 0, 16))
                                {
                                    ulong* bLong = (ulong*)b;
                                    a = *bLong;
                                    *rCurrent = *(bLong + 1);
                                }
                            }
                        }

                        if (a == _defaultIV)
                        {
                            fixed (byte* pDestination = destination)
                            {
                                ulong* keyBytes = (ulong*)pDestination;
                                for (var i = 0; i < n; i++)
                                {
                                    *(keyBytes + i) = *(ulong*)(r + (i << 3));
                                }
                            }

                            bytesWritten = n << 3;
                            return true;
                        }

                        return ThrowHelper.TryWriteError(out bytesWritten);
                    }
                }
            }
            finally
            {
                _decryptorPool.Return(decryptor);
            }
        }

        /// <summary>
        /// Wrap a key using AES encryption.
        /// </summary>
        /// <param name="staticKey">the key to be wrapped. If <c>null</c>, a new <see cref="SymmetricJwk"/> will be generated.</param>
        /// <param name="header"></param>
        /// <param name="destination"></param>
        public override Jwk WrapKey(Jwk? staticKey, JwtObject header, Span<byte> destination)
        {
            if (_disposed)
            {
                ThrowHelper.ThrowObjectDisposedException(GetType());
            }

            if (destination.Length < GetKeyWrapSize())
            {
                ThrowHelper.ThrowArgumentException_DestinationTooSmall(destination.Length, GetKeyWrapSize());
            }

            var contentEncryptionKey = CreateSymmetricKey(EncryptionAlgorithm, staticKey);
            WrapKeyPrivate(contentEncryptionKey.AsSpan(), destination);
            return contentEncryptionKey;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void WrapKeyPrivate(ReadOnlySpan<byte> inputBuffer, Span<byte> destination)
        {
            var encryptor = _encryptorPool.Get();
            try
            {
                // The default initialization vector from RFC3394
                ulong a = _defaultIV;
                var n = inputBuffer.Length >> 3;
                byte* r = stackalloc byte[n << 3];
                fixed (byte* input = inputBuffer)
                {
                    for (var i = 0; i < n; i++)
                    {
                        *(ulong*)(r + (i << 3)) = *(ulong*)(input + (i << 3));
                    }
                }

                byte[] block = new byte[16];
                fixed (byte* pBlock = block)
                {
                    byte* t = stackalloc byte[8];
                    *(ulong*)t = 0L;

                    for (var j = 0; j < 6; j++)
                    {
                        for (var i = 0; i < n; i++)
                        {
                            *(ulong*)pBlock = a;
                            *(ulong*)(pBlock + 8) = *(ulong*)(r + (i << 3));
                            fixed (byte* b = encryptor.TransformFinalBlock(block, 0, 16))
                            {
                                ulong* bLong = (ulong*)b;
                                a = *bLong;
                                *(t + 7) = (byte)((n * j) + i + 1);
                                a ^= *(ulong*)t;
                                *(ulong*)(r + (i << 3)) = *(bLong + 1);
                            }
                        }
                    }
                }

                fixed (byte* keyBytes = destination)
                {
                    *(ulong*)keyBytes = a;
                    for (var i = 0; i < n; i++)
                    {
                        *(ulong*)(keyBytes + ((i + 1) << 3)) = *(ulong*)(r + (i << 3));
                    }
                }

                if(destination.Length != (n + 1) << 3)
                {
                    ThrowHelper.ThrowCryptographicException_KeyWrapFailed();
                }
            }
            finally
            {
                _encryptorPool.Return(encryptor);
            }
        }

        public override int GetKeyUnwrapSize(int wrappedKeySize)
        {
            return GetKeyUnwrappedSize(wrappedKeySize);
        }

        public override int GetKeyWrapSize()
        {
            return GetKeyWrappedSize(EncryptionAlgorithm);
        }

        public static int GetKeyUnwrappedSize(int wrappedKeySize)
        {
            return wrappedKeySize - BlockSizeInBytes;
        }

        public static int GetKeyWrappedSize(EncryptionAlgorithm encryptionAlgorithm)
        {
            return encryptionAlgorithm.KeyWrappedSizeInBytes;
        }

        private sealed class PooledEncryptorPolicy : PooledObjectFactory<ICryptoTransform>
        {
            private readonly Aes _aes;

            public PooledEncryptorPolicy(Aes aes)
            {
                _aes = aes;
            }

            public override ICryptoTransform Create()
            {
                return _aes.CreateEncryptor();
            }
        }

        private sealed class PooledDecryptorPolicy : PooledObjectFactory<ICryptoTransform>
        {
            private readonly Aes _aes;

            public PooledDecryptorPolicy(Aes aes)
            {
                _aes = aes;
            }

            public override ICryptoTransform Create()
            {
                return _aes.CreateDecryptor();
            }
        }
    }
}
