using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RecruitmentApp.Core.Helpers;

public static class ObjectHelper
{
    public static T? ShallowCopy<T>(T? source) where T : class
    {
        return source
            ?.GetType()
            ?.GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(source, null) as T;
    }

    public static T? DeepCopy<T>(T? source) where T : class
    {
        var jsonOptions = new JsonSerializerOptions();
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        var sourceSerialized = JsonSerializer.Serialize(source, jsonOptions);

        if (source is not null)
        {
            return JsonSerializer.Deserialize(sourceSerialized, source.GetType(), jsonOptions) as T;
        }

        return null;
    }
}
