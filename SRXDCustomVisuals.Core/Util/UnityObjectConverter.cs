using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Object = UnityEngine.Object;

namespace SRXDCustomVisuals.Core; 

public class UnityObjectConverter<T> : JsonConverter<T> where T : Object {
    private List<T> objects;
    
    public UnityObjectConverter(List<T> objects) => this.objects = objects;

    public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer) {
        writer.WriteValue(objects.Count);
        objects.Add(value);
    }

    public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.Value is not long asLong)
            return null;
            
        return objects[(int) asLong];
    }
}