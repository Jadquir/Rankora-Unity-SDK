using Rankora_API.Scripts.Rankora.Types;
using Rankora_API.Scripts.Utils.Json;
using Rankora_API.Scripts.Utils.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rankora_API.Scripts.Utils.Json
{
    public class CustomHttpJsonParser : IJsonParser
    {
        static JsonSettings JsonSettings = new JsonSettings()
        {
            Converters = new List<IJsonConverter> { new MetadataJsonConverter() }
        };
        public object FromJsonToObject(string json, Type targetType)
        {
            var jsonObject = Json.Deserialize(json, JsonSettings) as Dictionary<string, object>;
            return ObjectExtensions.CastRecursively(jsonObject, targetType);
        }
        public string FromObjectToJson(object obj)
        {
            return Json.Serialize(obj, JsonSettings);
        }
        public static CustomHttpJsonParser Instance { get; } = new CustomHttpJsonParser();
    }
}
