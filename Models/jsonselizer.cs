using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CarMate.Models
{
    // فئة لتسلسل وفك تسلسل البيانات
    public class jsonselizer
    {
        // تسلسل كائن إلى JSON
        public static string SerializeToJson<T>(T obj)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true, // لجعل JSON منسقًا بشكل جيد
                ReferenceHandler = ReferenceHandler.Preserve // التعامل مع الحلقات المرجعية
            };

            return JsonSerializer.Serialize(obj, options);
        }

        // فك تسلسل JSON إلى كائن من النوع T
        public static T DeserializeFromJson<T>(string jsonString)
        {
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve // التعامل مع الحلقات المرجعية عند فك التسلسل
            };

            return JsonSerializer.Deserialize<T>(jsonString, options);
        }
    }
}
