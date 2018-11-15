﻿// Copyright (c) 2018 Yann Crumeyrolle. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for more information.

using System;

namespace JsonWebToken.Internal
{
    public class DefaultKeyWrapperFactory : IKeyWrapperFactory
    {
        private readonly CryptographicStore< KeyWrapper> _keyWrappers = new CryptographicStore<KeyWrapper>();
        private bool _disposed;

        /// <summary>
        /// Creates a <see cref="KeyWrapper"/>.
        /// </summary>
        /// <param name="key">the key used for key wrapping.</param>
        /// <param name="encryptionAlgorithm">The encryption algorithm.</param>
        /// <param name="contentEncryptionAlgorithm">The content encryption algorithm.</param>
        public virtual KeyWrapper Create(JsonWebKey key, EncryptionAlgorithm encryptionAlgorithm, KeyManagementAlgorithm contentEncryptionAlgorithm)
        {
            if (_disposed)
            {
                Errors.ThrowObjectDisposed(GetType());
            }

            if (encryptionAlgorithm == null || contentEncryptionAlgorithm == null)
            {
                return null;
            }

            var algorithmKey = (encryptionAlgorithm.Id << 8) | (byte)contentEncryptionAlgorithm.Id;
            var factoryKey = new CryptographicFactoryKey(key, algorithmKey);
            if (_keyWrappers.TryGetValue(factoryKey, out var cachedKeyWrapper))
            {
                return cachedKeyWrapper;
            }

            if (key.IsSupported(contentEncryptionAlgorithm))
            {
                var keyWrapper = key.CreateKeyWrapper(encryptionAlgorithm, contentEncryptionAlgorithm);
                return _keyWrappers.AddValue(factoryKey, keyWrapper);
            }

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _keyWrappers.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }
    }
}