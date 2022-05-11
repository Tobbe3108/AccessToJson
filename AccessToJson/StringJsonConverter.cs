using Newtonsoft.Json;

namespace AccessToJson;

internal class StringJsonConverter : JsonConverter
{
    public override bool CanRead => false;

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(string);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not string str) return;
        if (str.Contains('.'))
        {
            writer.WriteValue(str.Replace('.', ','));
            return;
        }

        writer.WriteValue(str);
    }
}