using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualElement : MonoBehaviour, ISerializationCallbackReceiver {
    [SerializeField] private VisualElementEvent[] events;
    [SerializeField] private VisualElementProperty[] properties;

    [SerializeField, HideInInspector] private List<VisualsController> events_visualsControllers = new();
    [SerializeField, HideInInspector] private string events_jData = string.Empty;
    
    [SerializeField, HideInInspector] private List<VisualsController> properties_visualsControllers = new();
    [SerializeField, HideInInspector] private string properties_jData = string.Empty;

    internal VisualElementEvent[] Events => events;

    internal VisualElementProperty[] Properties => properties;

    public void OnBeforeSerialize() {
        events ??= Array.Empty<VisualElementEvent>();
        properties ??= Array.Empty<VisualElementProperty>();
        
        events_visualsControllers.Clear();
        properties_visualsControllers.Clear();

        events_jData = SafeSerialize.Serialize(events, new UnityObjectConverter<VisualsController>(events_visualsControllers));
        properties_jData = SafeSerialize.Serialize(properties, new UnityObjectConverter<VisualsController>(properties_visualsControllers));
    }

    public void OnAfterDeserialize() {
        events ??= SafeSerialize.Deserialize<VisualElementEvent[]>(events_jData, new UnityObjectConverter<VisualsController>(events_visualsControllers));
        properties ??= SafeSerialize.Deserialize<VisualElementProperty[]>(properties_jData, new UnityObjectConverter<VisualsController>(properties_visualsControllers));
    }

    internal void InitControllers(IVisualsParams parameters, IVisualsResources resources) {
        foreach (var controller in GetComponentsInChildren<VisualsController>())
            controller.Init(parameters, resources);
    }
}