using Rankora_API.Scripts.Utils.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Rankora_API.Scripts.Utils.Json.ObjectExtensions;

namespace Rankora_API.Scripts.Rankora.Types
{
#nullable enable
    /// <summary>
    /// Represents a key-value pair of metadata, supporting basic primitive types.
    /// </summary>
    [Serializable]
    public class MetadataItem
    {
        /// <summary>Metadata key</summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>Metadata value (primitive types supported)</summary>
        public object? Value { get; set; }

        public MetadataItem() { }

        public MetadataItem(string key, object? value)
        {
            Key = key;
            Value = value;
        }
    }

    /// <summary>
    /// Stores a collection of MetadataItems, provides typed getters and setters.
    /// Handles serialization via custom converters.
    /// </summary>
    [Serializable]
    [Caster(typeof(MetadataConverter))]
    public class Metadata
    {
        // Internal list to preserve order of metadata items
        List<MetadataItem> Data { get; set; } = new();

        /// <summary>Returns the list of metadata items.</summary>
        public List<MetadataItem> GetData() => Data;

        /// <summary>Number of metadata items.</summary>
        public int Count => Data.Count;

        /// <summary>
        /// Gets the value of the metadata item with the given key, cast to T if possible.
        /// Returns default(T) if key not found or type mismatch.
        /// </summary>
        public T? Get<T>(string key)
        {
            var item = Data.Find(i => i.Key == key);
            if (item != null && item.Value is T typedValue)
            {
                return typedValue;
            }
            return default;
        }

        // Internal helper to set or update a key-value pair
        private void _set(string key, object value)
        {
            var item = Data.Find(i => i.Key == key);
            if (item != null)
            {
                item.Value = value;
            }
            else
            {
                Data.Add(new MetadataItem(key, value));
            }
        }

        // Public overloads for supported primitive types
        public void Set(string key, string value) => _set(key, value);
        public void Set(string key, float value) => _set(key, value);
        public void Set(string key, double value) => _set(key, value);
        public void Set(string key, int value) => _set(key, value);
        public void Set(string key, bool value) => _set(key, value);

        /// <summary>
        /// Generic setter which only accepts supported primitive types, warns on unsupported.
        /// </summary>
        public void Set(string key, object value)
        {
            if (value is string || value is float || value is double || value is int || value is bool)
            {
                _set(key, value);
            }
            else
            {
                Debug.LogWarning($"Unsupported type for Metadata: {value.GetType()}. Only string, float, double, int, and bool are supported.");
            }
        }

        /// <summary>
        /// Overrides equality check by comparing keys and values of all metadata items.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is not Metadata other || other.Count != this.Count)
                return false;

            foreach (var item in this.GetData())
            {
                var otherValue = other.Get<object>(item.Key);
                if (!Equals(item.Value, otherValue))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Generates a hash code based on all metadata key-value pairs.
        /// </summary>
        public override int GetHashCode()
        {
            int hash = 17;
            foreach (var item in GetData())
            {
                hash = hash * 31 + (item.Key?.GetHashCode() ?? 0);
                hash = hash * 31 + (item.Value?.GetHashCode() ?? 0);
            }
            return hash;
        }

    }

    /// <summary>
    /// Custom caster to convert from Dictionary<string, object> to Metadata object.
    /// Used during deserialization to cast JSON objects into Metadata.
    /// </summary>
    public class MetadataConverter : ICustomCaster
    {
        public object Cast(object source, Type targetType)
        {
            if (source is Dictionary<string, object> dict)
            {
                var meta = new Metadata();
                foreach (var item in dict)
                {
                    meta.Set(item.Key, item.Value);
                }
                return meta;
            }
            throw new InvalidCastException("Invalid source for MetadataConverter");
        }
    }

    /// <summary>
    /// JSON converter for the Metadata class to handle serialization and deserialization.
    /// Supports converting between Metadata and dictionary-like JSON representation.
    /// </summary>
    public class MetadataJsonConverter : IJsonConverter
    {
        /// <summary>Checks if the given type can be converted by this converter.</summary>
        public bool CanConvert(Type objectType)
        {
            return typeof(Metadata).IsAssignableFrom(objectType);
        }

        /// <summary>
        /// Reads JSON and converts it to a Metadata object.
        /// Supports JSON string or dictionary representation.
        /// </summary>
        public object? ReadJson(object value, Type objectType, IJsonSerializer serializer)
        {
            if (value == null)
                return null;

            IDictionary<string, object>? dict = null;

            if (value is string s)
            {
                // Parse JSON string to dictionary
                var parsed = serializer.Deserialize(s);
                dict = parsed as IDictionary<string, object>;
            }
            else if (value is IDictionary<string, object> idict)
            {
                dict = idict;
            }
            else
            {
                throw new ArgumentException("Unsupported value type for Metadata deserialization");
            }

            var metadata = new Metadata();
            if (dict != null)
            {
                foreach (var kvp in dict)
                {
                    metadata.Set(kvp.Key, kvp.Value);
                }
            }

            return metadata;
        }

        /// <summary>
        /// Serializes a Metadata object into JSON representation.
        /// </summary>
        public void WriteJson(object value, Type objectType, StringBuilder builder, IJsonSerializer serializer)
        {
            if (value == null)
            {
                builder.Append("null");
                return;
            }

            if (!(value is Metadata metadata))
                throw new ArgumentException("Expected Metadata object.", nameof(value));

            // Convert Metadata list to dictionary for serialization
            var dict = new Dictionary<string, object?>();
            foreach (var item in metadata.GetData())
            {
                dict[item.Key] = item.Value;
            }

            // Serialize dictionary directly to JSON string builder
            serializer.SerializeValue(dict, builder);
        }
    }

#nullable disable
}
