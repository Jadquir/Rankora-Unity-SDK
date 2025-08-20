using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rankora_API.Scripts.Utils.Request
{
    public interface IJsonParser    
    {
        public string FromObjectToJson(object obj);
        public object FromJsonToObject(string json, Type targetType);
    }
}
