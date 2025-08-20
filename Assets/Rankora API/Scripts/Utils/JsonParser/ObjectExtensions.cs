using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Linq;
using Unity.VisualScripting;

namespace Rankora_API.Scripts.Utils.Json
{
    public static class ObjectExtensions
    {
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
        public class CasterAttribute : Attribute
        {
            public Type ConverterType { get; }
            public CasterAttribute(Type converterType)
            {
                ConverterType = converterType;
            }
        }

        public interface ICustomCaster
        {
            object Cast(object source, Type targetType);
        }

        // Cache for reflection results to improve performance
        private static readonly ConcurrentDictionary<Type, ICustomCaster> _casterCache = new();
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();
        private static readonly ConcurrentDictionary<Type, FieldInfo[]> _fieldCache = new();
        private static readonly ConcurrentDictionary<string, Dictionary<string, string>> _keyMappingCache = new();

        private static ICustomCaster GetCustomCaster(Type targetType)
        {
            return _casterCache.GetOrAdd(targetType, type =>
            {
                var casterAttr = type.GetCustomAttribute<CasterAttribute>(inherit: true);
                if (casterAttr == null)
                    return null;

                if (Activator.CreateInstance(casterAttr.ConverterType) is not ICustomCaster caster)
                    throw new InvalidOperationException($"Caster {casterAttr.ConverterType.Name} must implement ICustomCaster.");

                return caster;
            });
        }

        private static PropertyInfo[] GetCachedProperties(Type type)
        {
            return _propertyCache.GetOrAdd(type, t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                 .Where(p => p.CanWrite)
                 .ToArray());
        }

        private static FieldInfo[] GetCachedFields(Type type)
        {
            return _fieldCache.GetOrAdd(type, t =>
                t.GetFields(BindingFlags.Public | BindingFlags.Instance));
        }

        public static T CastRecursively<T>(object source)
        {
            return (T)CastRecursively(source, typeof(T));
        }

        public static object CastRecursively(object source, Type targetType = null)
        {
            return CastRecursivelyInternal(source, targetType, new HashSet<object>());
        }

        private static object CastRecursivelyInternal(object source, Type targetType, HashSet<object> visited)
        {
            if (source == null)
                return GetDefaultValue(targetType);

            // Prevent infinite recursion with circular references
            if (IsReferenceType(source) && visited.Contains(source))
                return GetDefaultValue(targetType);

            // Handle direct type matches first (most common case)
            if (targetType != null && targetType.IsAssignableFrom(source.GetType()))
                return source;

            // Handle primitive types and strings early
            if (targetType != null && IsPrimitiveOrString(targetType))
                return ConvertPrimitive(source, targetType);

            // Add to visited set for reference types
            var wasAdded = IsReferenceType(source) && visited.Add(source);

            try
            {
                var result = CastRecursivelyCore(source, targetType, visited);
                return result;
            }
            finally
            {
                if (wasAdded)
                    visited.Remove(source);
            }
        }

        private static object CastRecursivelyCore(object source, Type targetType, HashSet<object> visited)
        {
            // Handle Dictionary<string, object> -> Complex Object
            if (source is Dictionary<string, object> dict)
            {
                return CastFromDictionary(dict, targetType, visited);
            }

            // Handle IList -> Array or List
            if (source is IList list)
            {
                return CastFromList(list, targetType, visited);
            }

            // Handle other IEnumerable types
            if (source is IEnumerable enumerable && !(source is string))
            {
                return CastFromEnumerable(enumerable, targetType, visited);
            }

            // For other types, try direct conversion
            return targetType != null ? ConvertValue(source, targetType) : source;
        }

        private static object CastFromDictionary(Dictionary<string, object> dict, Type targetType, HashSet<object> visited)
        {
            if (targetType == null)
            {
                // No targetType: recursively convert inner values
                var resultDict = new Dictionary<string, object>();
                foreach (var kvp in dict)
                {
                    resultDict[kvp.Key] = CastRecursivelyInternal(kvp.Value, null, visited);
                }
                return resultDict;
            }

            // Check for custom caster
            var customCaster = GetCustomCaster(targetType);
            if (customCaster != null)
                return customCaster.Cast(dict, targetType);

            // Create target instance
            var target = CreateInstance(targetType);
            if (target == null)
                return null;

            // Create key mapping for case-insensitive lookup
            var keyMapping = CreateKeyMapping(dict, targetType);

            // Set Properties
            var properties = GetCachedProperties(targetType);
            foreach (var prop in properties)
            {
                if (TryGetDictionaryValue(dict, prop.Name, keyMapping, out var value))
                {
                    try
                    {
                        var propValue = CastValue(value, prop.PropertyType, visited);
                        prop.SetValue(target, propValue);
                    }
                    catch (Exception ex)
                    {
                        // Log or handle property setting errors gracefully
                        Console.WriteLine($"Failed to set property {prop.Name}: {ex.Message}");
                    }
                }
            }

            // Set Fields
            var fields = GetCachedFields(targetType);
            foreach (var field in fields)
            {
                if (TryGetDictionaryValue(dict, field.Name, keyMapping, out var value))
                {
                    try
                    {
                        var fieldValue = CastValue(value, field.FieldType, visited);
                        field.SetValue(target, fieldValue);
                    }
                    catch (Exception ex)
                    {
                        // Log or handle field setting errors gracefully
                        Console.WriteLine($"Failed to set field {field.Name}: {ex.Message}");
                    }
                }
            }

            return target;
        }

        private static object CastFromList(IList list, Type targetType, HashSet<object> visited)
        {
            if (targetType == null)
            {
                var resultList = new List<object>();
                foreach (var item in list)
                {
                    resultList.Add(CastRecursivelyInternal(item, null, visited));
                }
                return resultList;
            }

            return CastToCollectionType(list, targetType, visited);
        }

        private static object CastFromEnumerable(IEnumerable enumerable, Type targetType, HashSet<object> visited)
        {
            var list = enumerable.Cast<object>().ToList();
            return CastFromList(list, targetType, visited);
        }

        private static object CastValue(object value, Type targetType, HashSet<object> visited)
        {
            if (value == null)
                return GetDefaultValue(targetType);

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Handle enums
            if (underlyingType.IsEnum)
            {
                if (value is string stringValue)
                    return Enum.Parse(underlyingType, stringValue, ignoreCase: true);
                return Enum.ToObject(underlyingType, value);
            }

            // Handle primitives and strings
            if (IsPrimitiveOrString(underlyingType))
                return ConvertPrimitive(value, underlyingType);

            // Handle collections
            if (IsCollectionType(underlyingType) && value is IEnumerable valueEnumerable)
                return CastToCollectionType(valueEnumerable, underlyingType, visited);

            // Handle complex objects
            if (IsComplexClass(underlyingType))
                return CastRecursivelyInternal(value, underlyingType, visited);

            // Fallback conversion
            return ConvertValue(value, underlyingType);
        }

        private static object CastToCollectionType(IEnumerable source, Type targetType, HashSet<object> visited)
        {
            var itemType = GetCollectionElementType(targetType);
            var sourceList = source.Cast<object>().ToList();

            if (targetType.IsArray)
            {
                var array = Array.CreateInstance(itemType, sourceList.Count);
                for (int i = 0; i < sourceList.Count; i++)
                {
                    array.SetValue(CastRecursivelyInternal(sourceList[i], itemType, visited), i);
                }
                return array;
            }

            // Handle List<T>, IList<T>, ICollection<T>, IEnumerable<T>
            if (targetType.IsGenericType)
            {
                var genericTypeDef = targetType.GetGenericTypeDefinition();
                Type listType;

                if (genericTypeDef == typeof(List<>) ||
                    genericTypeDef == typeof(IList<>) ||
                    genericTypeDef == typeof(ICollection<>) ||
                    genericTypeDef == typeof(IEnumerable<>))
                {
                    listType = typeof(List<>).MakeGenericType(itemType);
                }
                else
                {
                    // Try to create the specific generic type
                    listType = targetType;
                }

                var typedList = (IList)Activator.CreateInstance(listType);
                foreach (var item in sourceList)
                {
                    typedList.Add(CastRecursivelyInternal(item, itemType, visited));
                }
                return typedList;
            }

            // Fallback to List<object>
            var resultList = new List<object>();
            foreach (var item in sourceList)
            {
                resultList.Add(CastRecursivelyInternal(item, itemType, visited));
            }
            return resultList;
        }

        private static Dictionary<string, string> CreateKeyMapping(Dictionary<string, object> dict, Type targetType)
        {
            var cacheKey = $"{targetType.FullName}_{string.Join(",", dict.Keys.OrderBy(k => k))}";

            return _keyMappingCache.GetOrAdd(cacheKey, _ =>
            {
                var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var memberNames = new HashSet<string>(
                    GetCachedProperties(targetType).Select(p => p.Name)
                        .Concat(GetCachedFields(targetType).Select(f => f.Name)));

                foreach (var dictKey in dict.Keys)
                {
                    var matchingMember = memberNames.FirstOrDefault(m =>
                        string.Equals(m, dictKey, StringComparison.OrdinalIgnoreCase));

                    if (matchingMember != null)
                        mapping[matchingMember] = dictKey;
                }

                return mapping;
            });
        }

        private static bool TryGetDictionaryValue(Dictionary<string, object> dict, string key,
            Dictionary<string, string> keyMapping, out object value)
        {
            if (dict.TryGetValue(key, out value))
                return true;

            if (keyMapping.TryGetValue(key, out var mappedKey) && dict.TryGetValue(mappedKey, out value))
                return true;

            value = null;
            return false;
        }

        private static object CreateInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch
            {
                // Try to find a parameterless constructor or return null
                return type.IsValueType ? Activator.CreateInstance(type) : null;
            }
        }

        private static object ConvertPrimitive(object value, Type targetType)
        {
            try
            {
                if (targetType == typeof(string))
                    return value?.ToString();

                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return GetDefaultValue(targetType);
            }
        }

        private static object ConvertValue(object value, Type targetType)
        {
            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return GetDefaultValue(targetType);
            }
        }

        private static object GetDefaultValue(Type type)
        {
            return type?.IsValueType == true ? Activator.CreateInstance(type) : null;
        }

        private static bool IsPrimitiveOrString(Type type)
        {
            return type == typeof(string) || type.IsPrimitive || type == typeof(decimal) ||
                   type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan) ||
                   type == typeof(Guid);
        }

        private static bool IsComplexClass(Type type)
        {
            return type.IsClass && !IsPrimitiveOrString(type) && !IsCollectionType(type);
        }

        private static bool IsCollectionType(Type type)
        {
            return type.IsArray ||
                   (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type)) ||
                   typeof(IList).IsAssignableFrom(type) ||
                   typeof(IEnumerable).IsAssignableFrom(type);
        }

        private static Type GetCollectionElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();

            if (type.IsGenericType)
                return type.GetGenericArguments()[0];

            return typeof(object);
        }

        private static bool IsReferenceType(object obj)
        {
            return obj != null && !obj.GetType().IsValueType && obj.GetType() != typeof(string);
        }

        // Helper method for case-insensitive key lookup (keeping for backward compatibility)
        private static object TryGetValueIgnoreCase(Dictionary<string, object> dict, string key)
        {
            foreach (var kvp in dict)
            {
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                    return kvp.Value;
            }
            return null;
        }
    }
}