﻿// Copyright (c) 2020 Yann Crumeyrolle. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

namespace JsonWebToken
{
    /// <summary>
    /// Represents a JSON object.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public sealed class JwtObject : IEnumerable<JwtProperty>
    {
        private readonly List<JwtProperty> _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtObject"/> class.
        /// </summary>
        public JwtObject()
        {
            _properties = new List<JwtProperty>(6);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtObject"/> class.
        /// </summary>
        public JwtObject(int capacity)
        {
            _properties = new List<JwtProperty>(capacity);
        }

        /// <summary>
        /// Gets the number of <see cref="JwtProperty"/>.
        /// </summary>
        public int Count => _properties.Count;

        /// <summary>
        /// Adds a <see cref="JwtProperty"/> to the end of the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="property"></param>
        public void Add(JwtProperty property) => _properties.Add(property);

        /// <summary>
        /// Adds a <see cref="string"/> property to the end of the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(ReadOnlySpan<byte> name, string value) => Add(new JwtProperty(name, value));

        /// <summary>
        /// Adds a <see cref="string"/> property to the end of the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, string value) => Add(new JwtProperty(name, value));

        /// <summary>
        /// Adds a <see cref="long"/> property to the end of the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(ReadOnlySpan<byte> name, long value) => Add(new JwtProperty(name, value));

        /// <summary>
        /// Adds a <see cref="long"/> property to the end of the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, long value) => Add(new JwtProperty(name, value));

        /// <summary>
        /// Adds a <see cref="double"/> property to the end of the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(ReadOnlySpan<byte> name, double value) => Add(new JwtProperty(name, value));

        /// <summary>
        /// Adds a <see cref="double"/> property to the end of the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, bool value) => Add(new JwtProperty(name, value));

        /// <summary>
        /// Adds a <see cref="double"/> property to the end of the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, double value) => Add(new JwtProperty(name, value));

        /// <summary>
        /// Adds a <see cref="bool"/> property to the end of the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(ReadOnlySpan<byte> name, bool value) => Add(new JwtProperty(name, value));

        /// <summary>
        /// Adds a <see cref="JwtObject"/> property to the end of the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(ReadOnlySpan<byte> name, JwtObject value) => Add(new JwtProperty(name, value));

        /// <summary>
        /// Adds a <see cref="JwtObject"/> property to the end of the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, JwtObject value) => Add(new JwtProperty(name, value));

        /// <summary>
        /// Adds a <see cref="JwtArray"/> property to the end of the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(ReadOnlySpan<byte> name, JwtArray value) => Add(new JwtProperty(name, value));

        /// <summary>
        /// Adds a <see cref="JwtArray"/> property to the end of the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, JwtArray value) => Add(new JwtProperty(name, value));

        /// <summary>
        /// Adds a <c>null</c> property to the end of the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="name"></param>
        public void Add(ReadOnlySpan<byte> name) => Add(new JwtProperty(name));

        /// <summary>
        /// Adds a <c>null</c> property to the end of the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="name"></param>
        public void Add(string name) => Add(new JwtProperty(name));

        /// <summary>
        /// Gets the <see cref="JwtProperty"/> at the specified index;
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public JwtProperty this[int index] => _properties[index];

        /// <summary>
        /// Gets or sets the <see cref="JwtProperty"/> at the specified key;
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public JwtProperty this[string key] => this[Utf8.GetBytes(key).AsSpan()];

        /// <summary>
        /// Gets or sets the <see cref="JwtProperty"/> at the specified key;
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public JwtProperty this[ReadOnlyMemory<byte> key]
        {
            get
            {
                var span = key.Span;
                var properties = _properties;
                for (int i = 0; i < properties.Count; i++)
                {
                    var current = properties[i];
                    if (current.Utf8Name.SequenceEqual(span))
                    {
                        return current;
                    }
                }

                return default;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="JwtProperty"/> at the specified key;
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public JwtProperty this[ReadOnlySpan<byte> key]
        {
            get
            {
                var properties = _properties;
                for (int i = 0; i < properties.Count; i++)
                {
                    var current = properties[i];
                    if (current.Utf8Name.SequenceEqual(key))
                    {
                        return current;
                    }
                }

                return default;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="JwtProperty"/> at the specified key;
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public JwtProperty this[WellKnownProperty key]
        {
            get
            {
                var properties = _properties;
                for (int i = 0; i < properties.Count; i++)
                {
                    var current = properties[i];
                    if (current.WellKnownName == key)
                    {
                        return current;
                    }
                }

                return default;
            }
        }

        /// <summary>
        /// Gets the <see cref="JwtProperty"/> associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out JwtProperty value) => TryGetValue(Utf8.GetBytes(key).AsSpan(), out value);

        /// <summary>
        /// Gets the <see cref="JwtProperty"/> associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(byte[] key, out JwtProperty value) => TryGetValue(key.AsSpan(), out value);

        /// <summary>
        /// Gets the <see cref="JwtProperty"/> associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(ReadOnlySpan<byte> key, out JwtProperty value)
        {
            var properties = _properties;
            int count = properties.Count;
            for (int i = 0; i < count; i++)
            {
                var current = properties[i];
                if (current.Utf8Name.SequenceEqual(key))
                {
                    value = current;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Gets the <see cref="JwtProperty"/> associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(WellKnownProperty key, out JwtProperty value)
        {
            var properties = _properties;
            int count = properties.Count;
            for (int i = 0; i < count; i++)
            {
                var current = properties[i];
                if (current.WellKnownName == key)
                {
                    value = current;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Determines whether a <see cref="JwtProperty"/> is in the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(WellKnownProperty key)
        {
            var properties = _properties;
            int count = properties.Count;
            for (int i = 0; i < count; i++)
            {
                var current = properties[i];
                if (current.WellKnownName == key)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether a <see cref="JwtProperty"/> is in the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(ReadOnlySpan<byte> key)
        {
            var properties = _properties;
            int count = properties.Count;
            for (int i = 0; i < count; i++)
            {
                var current = properties[i];
                if (current.Utf8Name.SequenceEqual(key))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether a <see cref="JwtProperty"/> is in the <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key) => ContainsKey(Utf8.GetBytes(key));

        /// <summary>
        /// Replaces a <see cref="JwtProperty"/> 
        /// </summary>
        /// <param name="property"></param>
        public void Replace(JwtProperty property)
        {
            var span = property.Utf8Name;
            var properties = _properties;
            int count = properties.Count;
            for (int i = 0; i < count; i++)
            {
                var current = properties[i];
                if (current.Utf8Name.SequenceEqual(span))
                {
                    properties[i] = property;
                    return;
                }
            }

            _properties.Add(property);
        }

        /// <summary>
        /// Serializes the <see cref="JwtObject"/> into it JSON representation.
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            using var bufferWriter = new PooledByteBufferWriter();
            Serialize(bufferWriter);
            return bufferWriter.WrittenSpan.ToArray();
        }

        /// <summary>
        /// Serializes the <see cref="JwtObject"/> into it JSON representation.
        /// </summary>
        /// <param name="bufferWriter"></param>
        public void Serialize(IBufferWriter<byte> bufferWriter)
        {
            using var writer = new Utf8JsonWriter(bufferWriter, Constants.NoJsonValidation);
            WriteObjectTo(writer);
            writer.Flush();
        }

        /// <summary>
        /// Serializes the <see cref="JwtObject"/> into it JSON representation.
        /// </summary>
        /// <param name="writer"></param>
        public void Serialize(Utf8JsonWriter writer)
        {
            WriteObjectTo(writer);
            writer.Flush();
        }

        internal void WriteObjectTo(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            var properties = _properties;
            for (int i = 0; i < properties.Count; i++)
            {
                properties[i].WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        internal void WriteTo(Utf8JsonWriter writer)
        {
            var properties = _properties;
            for (int i = 0; i < properties.Count; i++)
            {
                properties[i].WriteTo(writer);
            }
        }

        internal void WriteTo(Utf8JsonWriter writer, ReadOnlySpan<byte> utf8Name)
        {
            writer.WriteStartObject(utf8Name);
            var properties = _properties;
            for (int i = 0; i < properties.Count; i++)
            {
                properties[i].WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        private string DebuggerDisplay()
        {
            return ToString();
        }

        /// <inheritsdoc />
        public override string ToString()
        {
            using var bufferWriter = new PooledByteBufferWriter();
            using (var writer = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions { Indented = true }))
            {
                WriteObjectTo(writer);
            }

            var input = bufferWriter.WrittenSpan;
            return Utf8.GetString(input);
        }

        /// <summary>
        /// Merge the <paramref name="other"/> into this <see cref="JwtObject"/>.
        /// </summary>
        /// <param name="other"></param>
        public void Merge(JwtObject other)
        {
            var srcProperties = other._properties;
            var dstProperties = _properties;
            for (int i = 0; i < srcProperties.Count; i++)
            {
                dstProperties.Add(srcProperties[i]);
            }
        }

        /// <inheritsdoc />
        public IEnumerator<JwtProperty> GetEnumerator()
        {
            return ((IEnumerable<JwtProperty>)_properties).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<JwtProperty>)_properties).GetEnumerator();
        }
    }
}