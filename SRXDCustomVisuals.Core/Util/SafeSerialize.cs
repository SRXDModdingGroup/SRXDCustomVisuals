using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public static class SafeSerialize {
    public static T Deserialize<T>(string jData, params JsonConverter[] converters)
        => JsonConvert.DeserializeObject<T>(jData ?? string.Empty, new JsonSerializerSettings {
            Converters = converters,
            ContractResolver = UnitySerializationContractResolver.Instance,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
    
    public static string Serialize(object obj, params JsonConverter[] converters)
        => JsonConvert.SerializeObject(obj, new JsonSerializerSettings {
            Converters = converters,
            ContractResolver = UnitySerializationContractResolver.Instance,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

    private class UnitySerializationContractResolver : DefaultContractResolver {
        public static UnitySerializationContractResolver Instance { get; } = new();
        
        protected override List<MemberInfo> GetSerializableMembers(Type objectType) {
            var memberInfo = new List<MemberInfo>();

            foreach (var fieldInfo in objectType.GetFields()) {
                if (IsSerializable(fieldInfo))
                    memberInfo.Add(fieldInfo);
            }

            return memberInfo;
        }

        private static bool IsSerializable(FieldInfo fieldInfo) {
            if (fieldInfo.IsStatic || fieldInfo.IsLiteral || fieldInfo.IsInitOnly || fieldInfo.IsNotSerialized)
                return false;

            bool hasSerializeField = false;

            foreach (var attribute in fieldInfo.GetCustomAttributes()) {
                switch (attribute) {
                    case SerializeField:
                        hasSerializeField = true;
                        continue;
                    case NonSerializedAttribute:
                        return false;
                }
            }

            return (hasSerializeField || fieldInfo.IsPublic) && IsSerializableType(fieldInfo.FieldType);
        }

        private static bool IsSerializableType(Type type) {
            while (true) {
                if (type == null)
                    return false;
                
                if (type == typeof(Array)) {
                    type = type.GetElementType();
                    continue;
                }

                if (!type.IsGenericType)
                    return true;
                
                if (type.GetGenericTypeDefinition() != typeof(List<>))
                    return false;
                    
                type = type.GenericTypeArguments[0];
            }
        }
    }
}