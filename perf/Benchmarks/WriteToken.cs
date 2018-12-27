﻿using BenchmarkDotNet.Attributes;
using Jose;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JsonWebToken.Performance
{
    [Config(typeof(DefaultCoreConfig))]
    [BenchmarkCategory("CI-CD")]
    public class WriteSignedToken : WriteToken
    {
        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(GetPayloads))]
        public void Jwt(string payload)
        {
            var value = Writer.WriteToken(JwtPayloads[payload]);
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetPayloads))]
        public void Wilson(string payload)
        {
            var token = Handler.CreateEncodedJwt(WilsonPayloads[payload]);
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetNotEncryptedPayloads))]
        public void WilsonJwt(string payload)
        {
            var token = Handler2.CreateToken(Tokens.Payloads[payload.Substring(4)].ToString(), signingCredentials);
        }

        public IEnumerable<string> GetPayloads()
        {
            yield return "JWS-empty";
            yield return "JWS-small";
            yield return "JWS-medium";
            yield return "JWS-big";
        }
    }

    [Config(typeof(DefaultCoreConfig))]
    [BenchmarkCategory("CI-CD")]
    public class WriteUnsignedToken : WriteToken
    {
        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(GetPayloads))]
        public void Jwt(string payload)
        {
            var value = Writer.WriteToken(JwtPayloads[payload]);
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetPayloads))]
        public void Wilson(string payload)
        {
            var token = Handler.CreateEncodedJwt(WilsonPayloads[payload]);
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetNotEncryptedPayloads))]
        public void WilsonJwt(string payload)
        {
            var token = Handler2.CreateToken(Tokens.Payloads[payload.Substring(4)].ToString(), signingCredentials);
        }

        public IEnumerable<string> GetPayloads()
        {
            yield return "JWT-empty";
            yield return "JWT-small";
            yield return "JWT-medium";
            yield return "JWT-big";
        }
    }

    [Config(typeof(DefaultCoreConfig))]
    [BenchmarkCategory("CI-CD")]
    public class WriteEncryptedToken : WriteToken
    {
        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(GetPayloads))]
        public void Jwt(string payload)
        {
            var value = Writer.WriteToken(JwtPayloads[payload]);
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetPayloads))]
        public void Wilson(string payload)
        {
            var token = Handler.CreateEncodedJwt(WilsonPayloads[payload]);
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetNotEncryptedPayloads))]
        public void WilsonJwt(string payload)
        {
            var token = Handler2.CreateToken(Tokens.Payloads[payload.Substring(4)].ToString(), signingCredentials);
        }

        public IEnumerable<string> GetPayloads()
        {
            yield return "JWE-empty";
            yield return "JWE-small";
            yield return "JWE-medium";
            yield return "JWE-big";
        }
    }

    public abstract class WriteToken
    {
        private static readonly SymmetricJwk SigningKey = Tokens.SigningKey;

        private static readonly SymmetricJwk EncryptionKey = Tokens.EncryptionKey;

        private static readonly Microsoft.IdentityModel.Tokens.JsonWebKey WilsonSharedKey = Microsoft.IdentityModel.Tokens.JsonWebKey.Create(SigningKey.ToString());

        private static readonly IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
        private static readonly IJsonSerializer serializer = new JsonNetSerializer();
        private static readonly IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        private static readonly IDateTimeProvider dateTimeProvider = new UtcDateTimeProvider();
        public static readonly IJwtEncoder JwtDotNetEncoder = new JwtEncoder(algorithm, serializer, urlEncoder);
        public static readonly JwtDecoder JwtDotNetDecoder = new JwtDecoder(serializer, new JwtValidator(serializer, dateTimeProvider), urlEncoder);

        public static readonly JwtSecurityTokenHandler Handler = new JwtSecurityTokenHandler();
        public static readonly JsonWebTokenHandler Handler2 = new JsonWebTokenHandler();

        public static readonly SigningCredentials signingCredentials = new SigningCredentials(WilsonSharedKey, SigningKey.Alg);

        public static readonly JwtWriter Writer = new JwtWriter();

        protected static readonly Dictionary<string, JwtDescriptor> JwtPayloads = CreateJwtDescriptors();
        protected static readonly Dictionary<string, Dictionary<string, object>> DictionaryPayloads = CreateDictionaryDescriptors();
        protected static readonly Dictionary<string, SecurityTokenDescriptor> WilsonPayloads = CreateWilsonDescriptors();

        static WriteToken()
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
        }

        //[Benchmark]
        //[ArgumentsSource(nameof(GetPayloads))]
        public void JoseDotNet(string payload)
        {
            if (payload.StartsWith("JWE-"))
            {
                payload = payload.Substring(4, payload.Length - 4);
                var value = Jose.JWT.Encode(DictionaryPayloads[payload], SigningKey.RawK, JwsAlgorithm.HS256);
                value = Jose.JWT.Encode(value, EncryptionKey.RawK, JweAlgorithm.A128KW, JweEncryption.A128CBC_HS256);
            }
            else
            {
                var value = Jose.JWT.Encode(DictionaryPayloads[payload], SigningKey.RawK, JwsAlgorithm.HS256);
            }
        }

        //[Benchmark]
        [ArgumentsSource(nameof(GetNotEncryptedPayloads))]
        public void JwtDotNet(string payload)
        {
            var value = JwtDotNetEncoder.Encode(DictionaryPayloads[payload], SigningKey.RawK);
        }

        public IEnumerable<string> GetNotEncryptedPayloads()
        {
            yield return "JWS-empty";
            yield return "JWS-small";
            yield return "JWS-medium";
            yield return "JWS-big";
        }

        //public IEnumerable<object[]> GetJsonPayloads()
        //{
        //    yield return new[] { "empty" };
        //    yield return new[] { "small" };
        //    yield return new[] { "medium" };
        //    yield return new[] { "big" };
        //}
        private static Dictionary<string, JwtDescriptor> CreateJwtDescriptors()
        {
            var descriptors = new Dictionary<string, JwtDescriptor>();
            foreach (var payload in Tokens.Payloads)
            {
                var descriptor = new JwsDescriptor()
                {
                    Algorithm = SignatureAlgorithm.None
                };

                foreach (var property in payload.Value.Properties())
                {
                    switch (property.Name)
                    {
                        case "iat":
                        case "nbf":
                        case "exp":
                            descriptor.AddClaim(property.Name, Microsoft.IdentityModel.Tokens.EpochTime.DateTime((long)property.Value));
                            break;
                        default:
                            descriptor.AddClaim(property.Name, (string)property.Value);
                            break;
                    }
                }

                descriptors.Add("JWT-" + payload.Key, descriptor);
            }

            foreach (var payload in Tokens.Payloads)
            {
                var descriptor = new JwsDescriptor()
                {
                    Key = SigningKey
                };

                foreach (var property in payload.Value.Properties())
                {
                    switch (property.Name)
                    {
                        case "iat":
                        case "nbf":
                        case "exp":
                            descriptor.AddClaim(property.Name, Microsoft.IdentityModel.Tokens.EpochTime.DateTime((long)property.Value));
                            break;
                        default:
                            descriptor.AddClaim(property.Name, (string)property.Value);
                            break;
                    }
                }

                descriptors.Add("JWS-" + payload.Key, descriptor);
            }

            foreach (var payload in Tokens.Payloads)
            {
                var descriptor = new JwsDescriptor()
                {
                    Key = SigningKey
                };

                foreach (var property in payload.Value.Properties())
                {
                    switch (property.Name)
                    {
                        case "iat":
                        case "nbf":
                        case "exp":
                            descriptor.AddClaim(property.Name, Microsoft.IdentityModel.Tokens.EpochTime.DateTime((long)property.Value));
                            break;
                        default:
                            descriptor.AddClaim(property.Name, (string)property.Value);
                            break;
                    }
                }

                var jwe = new JweDescriptor
                {
                    Payload = descriptor,
                    Key = EncryptionKey,
                    EncryptionAlgorithm = EncryptionAlgorithm.Aes128CbcHmacSha256
                };

                descriptors.Add("JWE-" + payload.Key, jwe);
            }

            return descriptors;
        }

        private static Dictionary<string, SecurityTokenDescriptor> CreateWilsonDescriptors()
        {
            var descriptors = new Dictionary<string, SecurityTokenDescriptor>();
            foreach (var payload in Tokens.Payloads)
            {
                var descriptor = new SecurityTokenDescriptor()
                {
                    //SigningCredentials = new SigningCredentials(WilsonSharedKey, SigningKey.Alg),
                    Subject = new ClaimsIdentity(),
                    Expires = payload.Value.ContainsKey("exp") ? Microsoft.IdentityModel.Tokens.EpochTime.DateTime(payload.Value.Value<long>("exp")) : default(DateTime?),
                    IssuedAt = payload.Value.ContainsKey("iat") ? Microsoft.IdentityModel.Tokens.EpochTime.DateTime(payload.Value.Value<long>("iat")) : default(DateTime?),
                };

                foreach (var property in payload.Value.Properties())
                {
                    switch (property.Name)
                    {
                        case "iat":
                        case "nbf":
                        case "exp":
                            break;
                        default:
                            descriptor.Subject.AddClaim(new Claim(property.Name, (string)property.Value));
                            break;
                    }
                }

                descriptors.Add("JWT-" + payload.Key, descriptor);
            }

            foreach (var payload in Tokens.Payloads)
            {
                var descriptor = new SecurityTokenDescriptor()
                {
                    SigningCredentials = new SigningCredentials(WilsonSharedKey, SigningKey.Alg),
                    Subject = new ClaimsIdentity(),
                    Expires = payload.Value.ContainsKey("exp") ? Microsoft.IdentityModel.Tokens.EpochTime.DateTime(payload.Value.Value<long>("exp")) : default(DateTime?),
                    IssuedAt = payload.Value.ContainsKey("iat") ? Microsoft.IdentityModel.Tokens.EpochTime.DateTime(payload.Value.Value<long>("iat")) : default(DateTime?),
                };

                foreach (var property in payload.Value.Properties())
                {
                    switch (property.Name)
                    {
                        case "iat":
                        case "nbf":
                        case "exp":
                            break;
                        default:
                            descriptor.Subject.AddClaim(new Claim(property.Name, (string)property.Value));
                            break;
                    }
                }

                descriptors.Add("JWS-" + payload.Key, descriptor);
            }

            foreach (var payload in Tokens.Payloads)
            {
                var descriptor = new SecurityTokenDescriptor()
                {
                    SigningCredentials = new SigningCredentials(WilsonSharedKey, SigningKey.Alg),
                    EncryptingCredentials = new EncryptingCredentials(new SymmetricSecurityKey(EncryptionKey.RawK), KeyManagementAlgorithm.Aes128KW, EncryptionAlgorithm.Aes128CbcHmacSha256),
                    Subject = new ClaimsIdentity(),
                    Expires = payload.Value.ContainsKey("exp") ? Microsoft.IdentityModel.Tokens.EpochTime.DateTime(payload.Value.Value<long>("exp")) : default(DateTime?),
                    IssuedAt = payload.Value.ContainsKey("iat") ? Microsoft.IdentityModel.Tokens.EpochTime.DateTime(payload.Value.Value<long>("iat")) : default(DateTime?),
                };

                foreach (var property in payload.Value.Properties())
                {
                    switch (property.Name)
                    {
                        case "iat":
                        case "nbf":
                        case "exp":
                            break;
                        default:
                            descriptor.Subject.AddClaim(new Claim(property.Name, (string)property.Value));
                            break;
                    }
                }

                descriptors.Add("JWE-" + payload.Key, descriptor);
            }

            return descriptors;
        }

        private static Dictionary<string, Dictionary<string, object>> CreateDictionaryDescriptors()
        {
            var descriptors = new Dictionary<string, Dictionary<string, object>>();
            foreach (var payload in Tokens.Payloads)
            {
                var descriptor = new Dictionary<string, object>();

                foreach (var property in payload.Value.Properties())
                {
                    switch (property.Name)
                    {
                        case "iat":
                        case "nbf":
                        case "exp":
                            descriptor.Add(property.Name, (long)property.Value);
                            break;
                        default:
                            descriptor.Add(property.Name, (string)property.Value);
                            break;
                    }
                }

                descriptors.Add(payload.Key, descriptor);
            }

            return descriptors;
        }
    }
}