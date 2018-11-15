﻿// Copyright (c) 2018 Yann Crumeyrolle. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace JsonWebToken
{
    /// <summary>
    /// Represents a provider of <see cref="JsonWebKey"/>.
    /// </summary>
    public interface IKeyProvider
    {
        /// <summary>
        /// Gets a list of <see cref="JsonWebKey"/>.
        /// </summary>
        IReadOnlyList<JsonWebKey> GetKeys(JwtHeader header);
    }
}