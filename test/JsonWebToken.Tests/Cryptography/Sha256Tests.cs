﻿using System;
using Xunit;

namespace JsonWebToken.Tests.Cryptography
{
    public class Sha256Tests : ShaAlgorithmTest
    {
        protected override void ComputeHash(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            var sha256 = new Sha256();
            sha256.ComputeHash(source, destination);
        }

        protected override void ComputeHash(ReadOnlySpan<byte> source, ReadOnlySpan<byte> preprend, Span<byte> destination)
        {
            var sha256 = new Sha256();
            sha256.ComputeHash(source, preprend, destination);
        }

        [Fact]
        public void Sha256_Empty()
        {
            Verify(
                Array.Empty<byte>(),
                "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855");
        }

        // These test cases are from http://csrc.nist.gov/publications/fips/fips180-2/fips180-2.pdf Appendix B
        [Fact]
        public void Sha256_Fips180_1()
        {
            Verify(
                "abc",
                "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");
        }

        [Fact]
        public void Sha256_Fips180_2()
        {
            Verify(
                "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq",
                "248d6a61d20638b8e5c026930c3e6039a33ce45964ff2167f6ecedd419db06c1");
        }

        [Fact]
        public void Sha256_Fips180_3()
        {
            Verify(
                'a',
                1000000,
                "cdc76e5c9914fb9281a1c7e284d73e67f1809a48a497200e046d39ccc7112cd0");
        }

        [Fact]
        public void Sha256_Fips180_3_Prepend1()
        {
            Verify(
                'a',
                1000000 - 64,
                'a',
                64,
                "cdc76e5c9914fb9281a1c7e284d73e67f1809a48a497200e046d39ccc7112cd0");
        }

        [Fact]
        public void Sha256_Fips180_3_Prepend2()
        {
            Verify(
                'a',
                1000000 - 1,
                'a',
                1,
                "cdc76e5c9914fb9281a1c7e284d73e67f1809a48a497200e046d39ccc7112cd0");
        }

        [Fact]
        public void Sha256_Fips180_3_Prepend3()
        {
            Verify(
                'a',
                1000000 - 63,
                'a',
                63,
                "cdc76e5c9914fb9281a1c7e284d73e67f1809a48a497200e046d39ccc7112cd0");
        }

        [Fact]
        public void Sha256_Fips180_1_Prepend1()
        {
            Verify(
                "bc",
                "a",
                "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");
        }
    }
}
