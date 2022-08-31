using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualElement : MonoBehaviour, ISerializationCallbackReceiver {
    [SerializeField] private VisualsEvent[] events;

    [SerializeField, HideInInspector] private List<VisualsEventTarget> targets;
    [SerializeField, HideInInspector] private string jData;

    internal Dictionary<string, VisualsEventMapping[]> Events { get; private set; }

    private void Awake() {
        Events = new Dictionary<string, VisualsEventMapping[]>();

        foreach (var eventBinding in events)
            Events.Add(eventBinding.name, eventBinding.mappings);
    }

    public void OnBeforeSerialize() {
        targets = new List<VisualsEventTarget>();
        events ??= Array.Empty<VisualsEvent>();

        foreach (var visualsEvent in events) {
            foreach (var mapping in visualsEvent.mappings) {
                if (!targets.Contains(mapping.target))
                    targets.Add(mapping.target);
            }
        }

        jData = JsonConvert.SerializeObject(events, new VisualsEventTargetConverter(targets));
    }

    public void OnAfterDeserialize() {
        targets ??= new List<VisualsEventTarget>();
        jData ??= string.Empty;
        events ??= JsonConvert.DeserializeObject<VisualsEvent[]>(jData, new VisualsEventTargetConverter(targets));
    }

    private class VisualsEventTargetConverter : JsonConverter<VisualsEventTarget> {
        private List<VisualsEventTarget> targets;
        
        public VisualsEventTargetConverter(List<VisualsEventTarget> targets) => this.targets = targets;

        public override void WriteJson(JsonWriter writer, VisualsEventTarget value, JsonSerializer serializer)
            => writer.WriteValue(targets.IndexOf(value));

        public override VisualsEventTarget ReadJson(JsonReader reader, Type objectType, VisualsEventTarget existingValue, bool hasExistingValue, JsonSerializer serializer) {
            if (reader.Value is not long asLong)
                return null;
            
            return targets[(int) asLong];
        }
    }
}