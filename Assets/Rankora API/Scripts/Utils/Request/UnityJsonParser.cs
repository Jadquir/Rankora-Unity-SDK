using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rankora_API.Scripts.Utils.Request
{
    public class UnityJsonParser : IJsonParser
    {
        public object FromJsonToObject(string json, Type targetType)
        {
            return UnityEngine.JsonUtility.FromJson(json, targetType);
        }

        public string FromObjectToJson(object obj)
        {
            return UnityEngine.JsonUtility.ToJson(obj, true);
        }
    }
}
