using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualElement : MonoBehaviour, ISerializationCallbackReceiver {
    [SerializeField] private VisualsEvent[] events;

    [SerializeField, HideInInspector] private List<VisualsController> targets;
    [SerializeField, HideInInspector] private string jData;

    internal Dictionary<string, VisualsEventMapping[]> Events { get; private set; }

    private void Awake() {
        Events = new Dictionary<string, VisualsEventMapping[]>();

        foreach (var eventBinding in events)
            Events.Add(eventBinding.name, eventBinding.mappings);
    }

    public void InitControllers(IVisualsParams parameters, IVisualsResources resources) {
        foreach (var controller in GetComponentsInChildren<VisualsController>())
            controller.Init(parameters, resources);
    }

    public void OnBeforeSerialize() {
        targets = new List<VisualsController>();
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
        targets ??= new List<VisualsController>();
        jData ??= string.Empty;
        events ??= JsonConvert.DeserializeObject<VisualsEvent[]>(jData, new VisualsEventTargetConverter(targets));
    }

    private class VisualsEventTargetConverter : JsonConverter<VisualsController> {
        private List<VisualsController> targets;
        
        public VisualsEventTargetConverter(List<VisualsController> targets) => this.targets = targets;

        public override void WriteJson(JsonWriter writer, VisualsController value, JsonSerializer serializer)
            => writer.WriteValue(targets.IndexOf(value));

        public override VisualsController ReadJson(JsonReader reader, Type objectType, VisualsController existingValue, bool hasExistingValue, JsonSerializer serializer) {
            if (reader.Value is not long asLong)
                return null;
            
            return targets[(int) asLong];
        }
    }
}