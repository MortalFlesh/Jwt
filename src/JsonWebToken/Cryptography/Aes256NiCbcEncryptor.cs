﻿// Copyright (c) 2020 Yann Crumeyrolle. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

#if !NETSTANDARD2_0 && !NET461 && !NETCOREAPP2_1
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace JsonWebToken.Internal
{
    internal sealed class Aes256NiCbcEncryptor : AesEncryptor
    {
        private const int BlockSize = 16;

        private readonly Aes256EncryptionKeys _keys;

        public Aes256NiCbcEncryptor(ReadOnlySpan<byte> key)
        {
            _keys = new Aes256EncryptionKeys(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void KeyGenAssist1(ref Vector128<byte> tmp1, Vector128<byte> tmp3, byte control)
        {
            var keyGened = Aes.KeygenAssist(tmp3, control);
            keyGened = Sse2.Shuffle(keyGened.AsInt32(), 0xFF).AsByte();
            tmp1 = Sse2.Xor(tmp1, Sse2.ShiftLeftLogical128BitLane(tmp1, 4));
            tmp1 = Sse2.Xor(tmp1, Sse2.ShiftLeftLogical128BitLane(tmp1, 4));
            tmp1 = Sse2.Xor(tmp1, Sse2.ShiftLeftLogical128BitLane(tmp1, 4));
            tmp1 = Sse2.Xor(tmp1, keyGened);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void KeyGenAssist2(Vector128<byte> tmp1, ref Vector128<byte> tmp3)
        {
            var keyGened = Aes.KeygenAssist(tmp1, 0);
            var tmp2 = Sse2.Shuffle(keyGened.AsInt32(), 0xAA).AsByte();
            tmp3 = Sse2.Xor(tmp3, Sse2.ShiftLeftLogical128BitLane(tmp3, 4));
            tmp3 = Sse2.Xor(tmp3, Sse2.ShiftLeftLogical128BitLane(tmp3, 4));
            tmp3 = Sse2.Xor(tmp3, Sse2.ShiftLeftLogical128BitLane(tmp3, 4));
            tmp3 = Sse2.Xor(tmp3, tmp2);
        }

        /// <inheritsdoc />
        public override void Dispose()
        {
            // Clear the keys
            _keys.Clear();
        }

        /// <inheritsdoc />
        public override void Encrypt(ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> nonce, Span<byte> ciphertext)
        {
            ref var inputRef = ref MemoryMarshal.GetReference(plaintext);
            ref var outputRef = ref MemoryMarshal.GetReference(ciphertext);

            var state = nonce.AsVector128<byte>();
            ref var inputEndRef = ref Unsafe.AddByteOffset(ref inputRef, (IntPtr)plaintext.Length - BlockSize + 1);

            while (Unsafe.IsAddressLessThan(ref inputRef, ref inputEndRef))
            {
                var src = Unsafe.ReadUnaligned<Vector128<byte>>(ref inputRef);
                src = Sse2.Xor(src, state);

                state = Sse2.Xor(src, _keys.Key0);
                state = Aes.Encrypt(state, _keys.Key1);
                state = Aes.Encrypt(state, _keys.Key2);
                state = Aes.Encrypt(state, _keys.Key3);
                state = Aes.Encrypt(state, _keys.Key4);
                state = Aes.Encrypt(state, _keys.Key5);
                state = Aes.Encrypt(state, _keys.Key6);
                state = Aes.Encrypt(state, _keys.Key7);
                state = Aes.Encrypt(state, _keys.Key8);
                state = Aes.Encrypt(state, _keys.Key9);
                state = Aes.Encrypt(state, _keys.Key10);
                state = Aes.Encrypt(state, _keys.Key11);
                state = Aes.Encrypt(state, _keys.Key12);
                state = Aes.Encrypt(state, _keys.Key13);
                state = Aes.EncryptLast(state, _keys.Key14);
                Unsafe.WriteUnaligned(ref outputRef, state);

                inputRef = ref Unsafe.AddByteOffset(ref inputRef, (IntPtr)BlockSize);
                outputRef = ref Unsafe.AddByteOffset(ref outputRef, (IntPtr)BlockSize);
            }

            int left = plaintext.Length & BlockSize - 1;

            // Reuse the destination buffer as last block buffer
            Unsafe.CopyBlockUnaligned(ref outputRef, ref inputRef, (uint)left);
            byte padding = (byte)(BlockSize - left);
            Unsafe.InitBlockUnaligned(ref Unsafe.AddByteOffset(ref outputRef, (IntPtr)left), padding, padding);
            var srcLast = Unsafe.ReadUnaligned<Vector128<byte>>(ref outputRef);
            srcLast = Sse2.Xor(srcLast, state);

            state = Sse2.Xor(srcLast, _keys.Key0);
            state = Aes.Encrypt(state, _keys.Key1);
            state = Aes.Encrypt(state, _keys.Key2);
            state = Aes.Encrypt(state, _keys.Key3);
            state = Aes.Encrypt(state, _keys.Key4);
            state = Aes.Encrypt(state, _keys.Key5);
            state = Aes.Encrypt(state, _keys.Key6);
            state = Aes.Encrypt(state, _keys.Key7);
            state = Aes.Encrypt(state, _keys.Key8);
            state = Aes.Encrypt(state, _keys.Key9);
            state = Aes.Encrypt(state, _keys.Key10);
            state = Aes.Encrypt(state, _keys.Key11);
            state = Aes.Encrypt(state, _keys.Key12);
            state = Aes.Encrypt(state, _keys.Key13);
            state = Aes.EncryptLast(state, _keys.Key14);
            Unsafe.WriteUnaligned(ref outputRef, state);
        }

        public override void EncryptBlock(ref byte plaintext, ref byte ciphertext)
        {
            var block = Unsafe.ReadUnaligned<Vector128<byte>>(ref plaintext);

            block = Sse2.Xor(block, _keys.Key0);
            block = Aes.Encrypt(block, _keys.Key1);
            block = Aes.Encrypt(block, _keys.Key2);
            block = Aes.Encrypt(block, _keys.Key3);
            block = Aes.Encrypt(block, _keys.Key4);
            block = Aes.Encrypt(block, _keys.Key5);
            block = Aes.Encrypt(block, _keys.Key6);
            block = Aes.Encrypt(block, _keys.Key7);
            block = Aes.Encrypt(block, _keys.Key8);
            block = Aes.Encrypt(block, _keys.Key9);
            block = Aes.Encrypt(block, _keys.Key10);
            block = Aes.Encrypt(block, _keys.Key11);
            block = Aes.Encrypt(block, _keys.Key12);
            block = Aes.Encrypt(block, _keys.Key13);
            block = Aes.EncryptLast(block, _keys.Key14);
            Unsafe.WriteUnaligned(ref ciphertext, block);
        }

        private struct Aes256EncryptionKeys
        {
            private const int Count = 15;

            public Vector128<byte> Key0;
            public Vector128<byte> Key1;
            public Vector128<byte> Key2;
            public Vector128<byte> Key3;
            public Vector128<byte> Key4;
            public Vector128<byte> Key5;
            public Vector128<byte> Key6;
            public Vector128<byte> Key7;
            public Vector128<byte> Key8;
            public Vector128<byte> Key9;
            public Vector128<byte> Key10;
            public Vector128<byte> Key11;
            public Vector128<byte> Key12;
            public Vector128<byte> Key13;
            public Vector128<byte> Key14;

            public Aes256EncryptionKeys(ReadOnlySpan<byte> key)
            {
                if (key.Length < 32)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException_EncryptionKeyTooSmall(EncryptionAlgorithm.Aes256CbcHmacSha512, 256, key.Length * 8);
                }

                ref var keyRef = ref MemoryMarshal.GetReference(key);
                var tmp1 = Unsafe.ReadUnaligned<Vector128<byte>>(ref keyRef);
                var tmp3 = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref keyRef, 16));
                Key0 = tmp1;
                Key1 = tmp3;
                KeyGenAssist1(ref tmp1, tmp3, 0x01);
                Key2 = tmp1;
                KeyGenAssist2(tmp1, ref tmp3);
                Key3 = tmp3;
                KeyGenAssist1(ref tmp1, tmp3, 0x02);
                Key4 = tmp1;
                KeyGenAssist2(tmp1, ref tmp3);
                Key5 = tmp3;
                KeyGenAssist1(ref tmp1, tmp3, 0x04);
                Key6 = tmp1;
                KeyGenAssist2(tmp1, ref tmp3);
                Key7 = tmp3;
                KeyGenAssist1(ref tmp1, tmp3, 0x08);
                Key8 = tmp1;
                KeyGenAssist2(tmp1, ref tmp3);
                Key9 = tmp3;
                KeyGenAssist1(ref tmp1, tmp3, 0x10);
                Key10 = tmp1;
                KeyGenAssist2(tmp1, ref tmp3);
                Key11 = tmp3;
                KeyGenAssist1(ref tmp1, tmp3, 0x20);
                Key12 = tmp1;
                KeyGenAssist2(tmp1, ref tmp3);
                Key13 = tmp3;
                KeyGenAssist1(ref tmp1, tmp3, 0x40);
                Key14 = tmp1;
            }

            public void Clear()
            {
                ref byte that = ref Unsafe.As<Aes256EncryptionKeys, byte>(ref Unsafe.AsRef(this));
                Unsafe.InitBlock(ref that, 0, Count * 16);
            }
        }
    }
}
#endif