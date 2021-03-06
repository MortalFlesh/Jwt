﻿// Copyright (c) 2020 Yann Crumeyrolle. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace JsonWebToken.Internal
{
    /// <summary>
    /// Represent a store of cryptographics elements. 
    /// It is a specialized implementation of the <see cref="Dictionary{TKey, TValue}" />.
    /// </summary>
    /// <remarks>Inspired from https://github.com/dotnet/coreclr/pull/8216. </remarks>
    /// <typeparam name="TValue"></typeparam>
    public sealed class CryptographicStore<TValue> : IDisposable where TValue : class, IDisposable
    {
        private Map<TValue> _map = Map<TValue>.Empty;

        /// <summary>
        /// Gets the count of elements.
        /// </summary>
        public int Count => _map.Count;

        /// <summary>
        /// Tries to add the <paramref name="value"/> with <paramref name="key"/> as key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryAdd(int key, TValue value)
        {
            _map = _map.TryAdd(key, value, out bool success);
            return success;
        }

        /// <summary>
        /// Tries to get the <paramref name="value"/> withe the <paramref name="key"/> as key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(int key, [NotNullWhen(true)] out TValue? value) => _map.TryGetValue(key, out value);

        /// <inheritsdoc />
        public void Dispose()
        {
            _map.Dispose();
        }

        private abstract partial class Map<TMapValue> : IDisposable where TMapValue : class, IDisposable
        {
            public static Map<TMapValue> Empty { get; } = new EmptyMap();

            public abstract int Count { get; }

            public abstract Map<TMapValue> TryAdd(int key, TMapValue value, out bool success);

            public abstract bool TryGetValue(int key, [NotNullWhen(true)] out TMapValue? value);

            public abstract void Dispose();

            // Instance without any key/value pairs. Used as a singleton.
            private sealed class EmptyMap : Map<TMapValue>
            {
                public override int Count => 0;

                public override void Dispose()
                {
                }

                public override Map<TMapValue> TryAdd(int key, TMapValue value, out bool success)
                {
                    // Create a new one-element map to store the key/value pair
                    success = true;
                    return new OneElementMap(key, value);
                }

                public override bool TryGetValue(int key, out TMapValue? value)
                {
                    // Nothing here
                    value = null;
                    return false;
                }
            }

            private sealed class OneElementMap : Map<TMapValue>
            {
                private readonly int _key1;
                private readonly TMapValue _value1;

                public OneElementMap(int key, TMapValue value)
                {
                    _key1 = key;
                    _value1 = value;
                }

                public override int Count => 1;

                public override void Dispose()
                {
                    _value1.Dispose();
                }

                public override Map<TMapValue> TryAdd(int key, TMapValue value, out bool success)
                {
                    if (key == _key1)
                    {
                        success = false;
                        return this;
                    }
                    else
                    {
                        success = true;
                        return new TwoElementMap(_key1, _value1, key, value);
                    }
                }

                public override bool TryGetValue(int key, out TMapValue? value)
                {
                    if (key == _key1)
                    {
                        value = _value1;
                        return true;
                    }
                    else
                    {
                        value = null;
                        return false;
                    }
                }
            }

            private sealed class TwoElementMap : Map<TMapValue>
            {
                private readonly int _key1;
                private readonly TMapValue _value1;
                private readonly int _key2;
                private readonly TMapValue _value2;

                public TwoElementMap(int key1, TMapValue value1, int key2, TMapValue value2)
                {
                    _key1 = key1;
                    _value1 = value1;
                    _key2 = key2;
                    _value2 = value2;
                }

                public override int Count => 2;

                public override void Dispose()
                {
                    _value1.Dispose();
                    _value2.Dispose();
                }

                public override Map<TMapValue> TryAdd(int key, TMapValue value, out bool success)
                {
                    if (key == _key1)
                    {
                        success = false;
                        return this;
                    }
                    else if (key == _key2)
                    {
                        success = false;
                        return this;
                    }
                    else
                    {
                        success = true;
                        return new ThreeElementMap(_key1, _value1, _key2, _value2, key, value);
                    }
                }

                public override bool TryGetValue(int key, out TMapValue? value)
                {
                    if (key == _key1)
                    {
                        value = _value1;
                        return true;
                    }
                    if (key == _key2)
                    {
                        value = _value2;
                        return true;
                    }
                    else
                    {
                        value = null;
                        return false;
                    }
                }
            }

            private sealed class ThreeElementMap : Map<TMapValue>
            {
                private readonly int _key1;
                private readonly TMapValue _value1;
                private readonly int _key2;
                private readonly TMapValue _value2;
                private readonly int _key3;
                private readonly TMapValue _value3;

                public ThreeElementMap(int key1, TMapValue value1, int key2, TMapValue value2, int key3, TMapValue value3)
                {
                    _key1 = key1;
                    _value1 = value1;
                    _key2 = key2;
                    _value2 = value2;
                    _key3 = key3;
                    _value3 = value3;
                }

                public override int Count => 3;

                public override void Dispose()
                {
                    _value1.Dispose();
                    _value2.Dispose();
                    _value3.Dispose();
                }

                public override Map<TMapValue> TryAdd(int key, TMapValue value, out bool success)
                {
                    if (key == _key1)
                    {
                        success = false;
                        return this;
                    }
                    else if (key == _key2)
                    {
                        success = false;
                        return this;
                    }
                    else if (key == _key3)
                    {
                        success = false;
                        return this;
                    }
                    else
                    {
                        success = true;
                        var multi = new MultiElementMap(4);
                        multi.UnsafeStore(0, _key1, _value1);
                        multi.UnsafeStore(1, _key2, _value2);
                        multi.UnsafeStore(2, _key3, _value3);
                        multi.UnsafeStore(3, key, value);
                        return multi;
                    }
                }

                public override bool TryGetValue(int key, out TMapValue? value)
                {
                    if (key == _key1)
                    {
                        value = _value1;
                        return true;
                    }
                    if (key == _key2)
                    {
                        value = _value2;
                        return true;
                    }
                    else if (key == _key3)
                    {
                        value = _value3;
                        return true;
                    }
                    else
                    {
                        value = null;
                        return false;
                    }
                }
            }

            private sealed class MultiElementMap : Map<TMapValue>
            {
                private const int MaxMultiElements = 16;
                private readonly KeyValuePair<int, TMapValue>[] _keyValues;

                public override int Count => _keyValues.Length;

                public MultiElementMap(int count)
                {
                    Debug.Assert(count <= MaxMultiElements);
                    _keyValues = new KeyValuePair<int, TMapValue>[count];
                }

                public void UnsafeStore(int index, int key, TMapValue value)
                {
                    Debug.Assert(index < _keyValues.Length);
                    _keyValues[index] = new KeyValuePair<int, TMapValue>(key, value);
                }

                public override Map<TMapValue> TryAdd(int key, TMapValue value, out bool success)
                {
                    for (int i = 0; i < _keyValues.Length; i++)
                    {
                        if (key == _keyValues[i].Key)
                        {
                            // The key is in the map. 
                            success = false;
                            return this;
                        }
                    }

                    // The key does not already exist in this map.
                    // We need to create a new map that has the additional key/value pair.
                    // If with the addition we can still fit in a multi map, create one.
                    if (_keyValues.Length < MaxMultiElements)
                    {
                        var multi = new MultiElementMap(_keyValues.Length + 1);
                        Array.Copy(_keyValues, 0, multi._keyValues, 0, _keyValues.Length);
                        multi._keyValues[_keyValues.Length] = new KeyValuePair<int, TMapValue>(key, value);
                        success = true;
                        return multi;
                    }

                    // Otherwise, upgrade to a many map.
                    var many = new ManyElementMap(MaxMultiElements + 1);
                    foreach (KeyValuePair<int, TMapValue> pair in _keyValues)
                    {
                        many[pair.Key] = pair.Value;
                    }

                    many[key] = value;
                    success = true;
                    return many;
                }

                public override bool TryGetValue(int key, out TMapValue? value)
                {
                    foreach (KeyValuePair<int, TMapValue> pair in _keyValues)
                    {
                        if (key == pair.Key)
                        {
                            value = pair.Value;
                            return true;
                        }
                    }

                    value = null;
                    return false;
                }

                public override void Dispose()
                {
                    foreach (KeyValuePair<int, TMapValue> pair in _keyValues)
                    {
                        pair.Value.Dispose();
                    }
                }
            }

            private sealed class ManyElementMap : Map<TMapValue>
            {
                private readonly Dictionary<int, TMapValue> _dictionary;

                public override int Count => _dictionary.Count;

                public ManyElementMap(int capacity)
                {
                    _dictionary = new Dictionary<int, TMapValue>(capacity);
                }

                public override Map<TMapValue> TryAdd(int key, TMapValue value, out bool success)
                {
#if NETCOREAPP
                    success = _dictionary.TryAdd(key, value);
#else
                success = !_dictionary.ContainsKey(key);
                if (_dictionary.ContainsKey(key))
                {
                    success = false;
                }
                else
                {
                    _dictionary[key] = value;
                    success = true;
                }
#endif

                    return this;
                }

                public override bool TryGetValue(int key, out TMapValue? value)
                {
                    return _dictionary.TryGetValue(key, out value);
                }

                public override void Dispose()
                {
                    foreach (var value in _dictionary.Values)
                    {
                        value.Dispose();
                    }
                }

                public TMapValue this[int key]
                {
                    get => _dictionary[key];
                    set => _dictionary[key] = value;
                }
            }
        }
    }
}