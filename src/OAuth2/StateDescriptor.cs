﻿// Copyright (c) 2018 Yann Crumeyrolle. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for more information.

using JsonWebToken.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JsonWebToken
{
    public class StateDescriptor : JwsDescriptor
    {
        private static readonly ReadOnlyDictionary<ReadOnlyMemory<byte>, JwtTokenType[]> StateRequiredClaims
            = new ReadOnlyDictionary<ReadOnlyMemory<byte>, JwtTokenType[]>(new Dictionary<ReadOnlyMemory<byte>, JwtTokenType[]>
              {
                { OAuth2Claims.RfpUtf8.ToArray(), new [] { JwtTokenType.String} }
              });

        public StateDescriptor()
        {
        }

        public StateDescriptor(JwtObject header, JwtObject payload)
            : base(header, payload)
        {
        }

        protected override ReadOnlyDictionary<ReadOnlyMemory<byte>, JwtTokenType[]> RequiredClaims => StateRequiredClaims;

        /// <summary>
        /// Gets or sets the value of the 'rfp' claim.
        /// </summary>
        public string RequestForgeryProtection
        {
            get { return GetStringClaim(OAuth2Claims.RfpUtf8); }
            set { AddClaim(OAuth2Claims.RfpUtf8, value); }
        }

        /// <summary>
        /// Gets or sets the value of the 'target_link_uri' claim.
        /// </summary>
        public string TargetLinkUri
        {
            get { return GetStringClaim(OAuth2Claims.TargetLinkUriUtf8); }
            set { AddClaim(OAuth2Claims.TargetLinkUriUtf8, value); }
        }

        /// <summary>
        /// Gets or sets the value of the 'rfp' claim.
        /// </summary>
        public string AuthorizationServer
        {
            get { return GetStringClaim(OAuth2Claims.AsUtf8); }
            set { AddClaim(OAuth2Claims.AsUtf8, value); }
        }

        /// <summary>
        /// Gets or sets the value of the 'at_hash' claim.
        /// </summary>
        public string AccessTokenHash
        {
            get => GetStringClaim(OAuth2Claims.AtHashUtf8);
            set => AddClaim(OAuth2Claims.AtHashUtf8, value);
        }

        /// <summary>     
        /// Gets or sets the value of the 'c_hash' claim.
        /// </summary>
        public string CodeHash
        {
            get => GetStringClaim(OAuth2Claims.CHashUtf8);
            set => AddClaim(OAuth2Claims.CHashUtf8, value);
        }
    }
}
