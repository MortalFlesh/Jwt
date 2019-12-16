﻿using System;
#if NETCOREAPP3_0
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace JsonWebToken
{
    /// <summary>
    /// Computes a Hash-based Message Authentication Code (HMAC) using the SHA2-512 hash function.
    /// </summary>
    public class HmacSha512 : HmacSha2
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HmacSha512"/> class.
        /// </summary>
        /// <param name="key"></param>
        public HmacSha512(ReadOnlySpan<byte> key)
            : base(new Sha512(), key)
        {
        }

        /// <inheritsdoc />
        public override int BlockSize => 128;

        /// <inheritsdoc />
        public override void ComputeHash(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            Span<ulong> w = stackalloc ulong[80];
            Sha2.ComputeHash(source, destination, _innerPadKey.Span, w);
            Sha2.ComputeHash(destination, destination, _outerPadKey.Span, w);
        }

        /// <inheritsdoc />
        protected override void ComputeKeyHash(ReadOnlySpan<byte> key, Span<byte> keyPrime)
        {
            Sha2.ComputeHash(key, keyPrime, default, default(Span<ulong>));
        }
    }
}