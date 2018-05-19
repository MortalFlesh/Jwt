﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsonWebToken
{
    public class JsonWebTokenReader
    {
        private static readonly HashSet<char> JwsCharacters = new HashSet<char>(JwtConstants.JwsCompactSerializationCharacters);
        private readonly IList<IKeyProvider> _encryptionKeyProviders;

        public JsonWebTokenReader(IEnumerable<IKeyProvider> encryptionKeyProviders)
        {
            _encryptionKeyProviders = encryptionKeyProviders.ToArray();
        }

        public JsonWebTokenReader(IKeyProvider encryptionKeyProvider)
            : this(new[] { encryptionKeyProvider })
        {
        }

        public JsonWebTokenReader(JsonWebKeySet encryptionKeys)
            : this(new StaticKeyProvider(encryptionKeys))
        {
        }

        public JsonWebTokenReader(JsonWebKey encryptionKey)
            : this(new JsonWebKeySet(encryptionKey))
        {
        }

        /// <summary>
        /// Reads and validates a 'JSON Web Token' (JWT) encoded as a JWS or JWE in Compact Serialized Format.
        /// </summary>
        /// <param name="token">the JWT encoded as JWE or JWS</param>
        /// <param name="validationParameters">Contains validation parameters for the <see cref="JsonWebToken"/>.</param>
        /// <param name="validatedToken">The <see cref="JsonWebToken"/> that was validated.</param>
        public TokenValidationResult TryReadToken(string token, ValidationParameters validationParameters)
        {
            if (validationParameters == null)
            {
                throw new ArgumentNullException(nameof(validationParameters));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return TokenValidationResult.MalformedToken();
            }

            if (token.Length > validationParameters.MaximumTokenSizeInBytes)
            {
                return TokenValidationResult.MalformedToken();
            }

            var separators = GetSegmentCount(token);
            if (separators.Count + 1 == JwtConstants.JwsSegmentCount || separators.Count + 1 == JwtConstants.JweSegmentCount)
            {
                if (!CanReadToken(token, validationParameters))
                {
                    return TokenValidationResult.MalformedToken();
                }

                JsonWebToken jwt;
                if (separators.Count + 1 == JwtConstants.JwsSegmentCount)
                {
                    jwt = new JsonWebToken(token, separators.ToArray());
                }
                else if (separators.Count + 1 == JwtConstants.JweSegmentCount)
                {
                    jwt = new JsonWebToken(token, separators.ToArray());

                    if (string.IsNullOrEmpty(jwt.Header.Enc))
                    {
                        return TokenValidationResult.MissingEncryptionAlgorithm(jwt);
                    }

                    var keys = GetContentEncryptionKeys(jwt);

                    string decryptedToken = null;
                    for (int i = 0; i < keys.Count; i++)
                    {
                        decryptedToken = DecryptToken(jwt, keys[i]);
                        if (decryptedToken != null)
                        {
                            break;
                        }
                    }

                    if (decryptedToken == null)
                    {
                        return TokenValidationResult.DecryptionFailed(jwt);
                    }

                    var innerSeparators = GetSegmentCount(decryptedToken);
                    if (!CanReadToken(decryptedToken, validationParameters))
                    {
                        // The decrypted payload is not a JWT
                        jwt.PlainText = decryptedToken;
                        return TokenValidationResult.Success(jwt);
                    }

                    var decryptionResult = ReadJwtToken(decryptedToken, innerSeparators, validationParameters);
                    if (!decryptionResult.Succedeed)
                    {
                        return decryptionResult;
                    }

                    var decryptedJwt = decryptionResult.Token;
                    jwt.InnerToken = decryptedJwt;
                    return validationParameters.TryValidate(decryptedJwt);
                }
                else
                {
                    return TokenValidationResult.MalformedToken();
                }

                return validationParameters.TryValidate(jwt);
            }
            
            return TokenValidationResult.MalformedToken();
        }

        private static IList<int> GetSegmentCount(string input)
        {
            int next = 0;
            int current = 0;
            int i = 0;
            int segmentCount = 1;
            List<int> separators = new List<int>();

            while ((next = input.IndexOf('.', next+1)) != -1)
            {
                separators.Add(next - current);
                if (segmentCount++ > JwtConstants.MaxJwtSegmentCount)
                {
                    break;
                }

                i++;
                current = next;
            }

            return separators;
        }

        private bool CanReadToken(string token, ValidationParameters validationParameters)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            if (token.Length * 2 > validationParameters.MaximumTokenSizeInBytes)
            {
                return false;
            }

            for (int i = 0; i < token.Length; i++)
            {
                if (!JwsCharacters.Contains(token[i]))
                {
                    return false;
                }
            }

            return true;
        }
        
        private TokenValidationResult ReadJwtToken(string token, IList<int> separators, ValidationParameters validationParameters)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (!CanReadToken(token, validationParameters))
            {
                return TokenValidationResult.MalformedToken();
            }

            JsonWebToken jwt;
            if (separators.Count + 1 == JwtConstants.JwsSegmentCount)
            {
                jwt = new JsonWebToken(token, separators.ToArray());
            }
            else
            {
                jwt = new JsonWebToken(token, separators.ToArray());
            }

            return TokenValidationResult.Success(jwt);
        }

        private string DecryptToken(JsonWebToken jwtToken, JsonWebKey key)
        {
            var decryptionProvider = key.CreateAuthenticatedEncryptionProvider(jwtToken.Header.Enc);
            if (decryptionProvider == null)
            {
                return null;
            }

            try
            {
                var decryptedToken = decryptionProvider.Decrypt(
                    Base64Url.DecodeBytes(jwtToken.RawCiphertext.ToString()),
                    Encoding.ASCII.GetBytes(jwtToken.RawHeader.ToString()),
                    Base64Url.DecodeBytes(jwtToken.RawInitializationVector.ToString()),
                    Base64Url.DecodeBytes(jwtToken.RawAuthenticationTag.ToString()));
                if (decryptedToken == null)
                {
                    return null;
                }

                return Encoding.UTF8.GetString(decryptedToken);
            }
            finally
            {
                key.ReleaseAuthenticatedEncryptionProvider(decryptionProvider);
            }
        }

        private IList<JsonWebKey> GetContentEncryptionKeys(JsonWebToken jwtToken)
        {
            var keys = ResolveDecryptionKey(jwtToken);
            if (string.Equals(jwtToken.Header.Alg, SecurityAlgorithms.Direct, StringComparison.Ordinal))
            {
                return keys;
            }

            var unwrappedKeys = new List<JsonWebKey>();
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                KeyWrapProvider kwp = key.CreateKeyWrapProvider(jwtToken.Header.Alg);
                try
                {
                    if (kwp != null)
                    {
                        var unwrappedKey = kwp.UnwrapKey(Base64Url.DecodeBytes(jwtToken.RawEncryptedKey.ToString()));
                        unwrappedKeys.Add(SymmetricJwk.FromByteArray(unwrappedKey));
                    }
                }
                finally
                {
                    key.ReleaseKeyWrapProvider(kwp);
                }
            }

            return unwrappedKeys;
        }
        
        private IList<JsonWebKey> ResolveDecryptionKey(JsonWebToken jwtToken)
        {
            var keys = new List<JsonWebKey>();
            for (int i = 0; i < _encryptionKeyProviders.Count; i++)
            {
                var keySet = _encryptionKeyProviders[i].GetKeys(jwtToken);

                for (int j = 0; j < keySet.Keys.Count; j++)
                {
                    var key = keySet.Keys[j];
                    if ((string.IsNullOrWhiteSpace(key.Use) || string.Equals(key.Use, JsonWebKeyUseNames.Enc, StringComparison.Ordinal)) &&
                         (string.Equals(key.Kid, jwtToken.Header.Kid, StringComparison.Ordinal)))
                    {
                        keys.Add(key);
                    }
                }
            }

            return keys;
        }
    }
}
