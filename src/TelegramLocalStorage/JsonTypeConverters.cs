using Newtonsoft.Json;
using System;
using MihaZupan.TelegramLocalStorage.Types;

namespace MihaZupan.TelegramLocalStorage
{
    public class FileKeyConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((FileKey)value).Key);
        }
        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer) => null;
        public override bool CanRead => false;
        public override bool CanConvert(Type type) => type == typeof(DateTime);
    }
    public class PeerIdConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((PeerId)value).Id);
        }
        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer) => null;
        public override bool CanRead => false;
        public override bool CanConvert(Type type) => type == typeof(PeerId);
    }
}
