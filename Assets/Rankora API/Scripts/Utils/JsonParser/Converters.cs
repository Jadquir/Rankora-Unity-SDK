using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rankora_API.Scripts.Utils.Json { 
    public class DateTimeConverter : IJsonConverter
    {
        public bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
        }

        public object ReadJson(object value, Type objectType, IJsonSerializer serializer)
        {
            if (value is string str && DateTime.TryParse(str, null, System.Globalization.DateTimeStyles.RoundtripKind, out var result))
            {
                return result;
            }
            return default(DateTime);
        }

        public void WriteJson(object value, Type objectType, StringBuilder builder, IJsonSerializer serializer)
        {
            var dt = (DateTime)value;
            builder.Append('"');
            builder.Append(dt.ToString("o")); // ISO 8601
            builder.Append('"');
        }
    }
    public class DateTimeOffsetConverter : IJsonConverter
    {
        public bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?);
        }

        public object ReadJson(object value, Type objectType, IJsonSerializer serializer)
        {
            if (value is string str && DateTimeOffset.TryParse(str, null, System.Globalization.DateTimeStyles.RoundtripKind, out var result))
            {
                return result;
            }
            return default(DateTimeOffset);
        }

        public void WriteJson(object value, Type objectType, StringBuilder builder, IJsonSerializer serializer)
        {
            var dto = (DateTimeOffset)value;
            builder.Append('"');
            builder.Append(dto.ToString("o")); // ISO 8601
            builder.Append('"');
        }
    }
    public class TimeSpanConverter : IJsonConverter
    {
        public bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?);
        }

        public object ReadJson(object value, Type objectType, IJsonSerializer serializer)
        {
            if (value is string str && TimeSpan.TryParse(str, out var result))
            {
                return result;
            }
            return default(TimeSpan);
        }

        public void WriteJson(object value, Type objectType, StringBuilder builder, IJsonSerializer serializer)
        {
            var ts = (TimeSpan)value;
            builder.Append('"');
            builder.Append(ts.ToString("c")); // "c" = constant format (hh:mm:ss)
            builder.Append('"');
        }
    }



}
