using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsElement : MonoBehaviour, ISerializationCallbackReceiver {
    [SerializeField] private VisualsElementEvent[] events;
    [SerializeField] private VisualsElementProperty[] properties;

    [SerializeField, HideInInspector] private List<VisualsController> events_visualsControllers = new();
    [SerializeField, HideInInspector] private string events_jData = string.Empty;
    
    [SerializeField, HideInInspector] private List<VisualsController> properties_visualsControllers = new();
    [SerializeField, HideInInspector] private string properties_jData = string.Empty;

    internal VisualsElementEvent[] Events => events;

    internal VisualsElementProperty[] Properties => properties;

    public void OnBeforeSerialize() {
        events ??= Array.Empty<VisualsElementEvent>();
        properties ??= Array.Empty<VisualsElementProperty>();
        
        events_visualsControllers.Clear();
        properties_visualsControllers.Clear();

        events_jData = SafeSerialize.Serialize(events, new UnityObjectConverter<VisualsController>(events_visualsControllers));
        properties_jData = SafeSerialize.Serialize(properties, new UnityObjectConverter<VisualsController>(properties_visualsControllers));
    }

    public void OnAfterDeserialize() {
        events ??= SafeSerialize.Deserialize<VisualsElementEvent[]>(events_jData, new UnityObjectConverter<VisualsController>(events_visualsControllers));
        properties ??= SafeSerialize.Deserialize<VisualsElementProperty[]>(properties_jData, new UnityObjectConverter<VisualsController>(properties_visualsControllers));
    }

    internal void InitControllers(IVisualsParams parameters, IVisualsResources resources) {
        foreach (var controller in GetComponentsInChildren<VisualsController>())
            controller.Init(parameters, resources);
    }
}