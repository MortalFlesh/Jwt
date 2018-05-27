﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;

namespace JsonWebToken
{
    public abstract class HttpKeyProvider : IKeyProvider
    {
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1);
        private readonly HttpDocumentRetriever _documentRetriever;
        private TimeSpan _refreshInterval = DefaultRefreshInterval;
        private DateTimeOffset _syncAfter;
        private JsonWebKeySet _currentKeys;

        /// <summary>
        /// 1 day is the default time interval that afterwards, <see cref="GetConfigurationAsync()"/> will obtain new configuration.
        /// </summary>
        public static readonly TimeSpan DefaultAutomaticRefreshInterval = new TimeSpan(1, 0, 0, 0);

        /// <summary>
        /// 30 seconds is the default time interval that must pass for <see cref="RequestRefresh"/> to obtain a new configuration.
        /// </summary>
        public static readonly TimeSpan DefaultRefreshInterval = new TimeSpan(0, 0, 0, 30);

        /// <summary>
        /// 5 minutes is the minimum value for automatic refresh. <see cref="AutomaticRefreshInterval"/> can not be set less than this value.
        /// </summary>
        public static readonly TimeSpan MinimumAutomaticRefreshInterval = new TimeSpan(0, 0, 5, 0);

        /// <summary>
        /// 1 second is the minimum time interval that must pass for <see cref="RequestRefresh"/> to obtain new configuration.
        /// </summary>
        public static readonly TimeSpan MinimumRefreshInterval = new TimeSpan(0, 0, 0, 1);

        private TimeSpan _automaticRefreshInterval = DefaultAutomaticRefreshInterval;

        public HttpKeyProvider(HttpDocumentRetriever documentRetriever)
        {
            _documentRetriever = documentRetriever;
        }

        public HttpKeyProvider()
            : this(new HttpDocumentRetriever())
        {
        }

        public abstract JsonWebKeySet GetKeys(JObject header);

        protected abstract JsonWebKeySet DeserializeKeySet(string value);

        protected JsonWebKeySet GetKeys(JObject header, string metadataAddress)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (_currentKeys != null && _syncAfter > now)
            {
                return _currentKeys;
            }

            _refreshLock.Wait();
            try
            {
                if (_syncAfter <= now)
                {
                    try
                    {
                        var value = _documentRetriever.GetDocument(metadataAddress, CancellationToken.None);

                        _currentKeys = JsonConvert.DeserializeObject<JsonWebKeySet>(value);

                        _syncAfter = DateTimeUtil.Add(now.UtcDateTime, _automaticRefreshInterval);
                    }
                    catch
                    {
                        _syncAfter = DateTimeUtil.Add(now.UtcDateTime, _automaticRefreshInterval < _refreshInterval ? _automaticRefreshInterval : _refreshInterval);
                        throw;
                    }
                }

                if (_currentKeys == null)
                {
                    throw new InvalidOperationException(ErrorMessages.FormatInvariant("Unable to obtain keys from: '{0}'", (metadataAddress ?? "null")));
                }

                // Stale metadata is better than no metadata
                return _currentKeys;
            }
            finally
            {
                _refreshLock.Release();
            }
        }
    }
}
