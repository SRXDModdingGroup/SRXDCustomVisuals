using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualElement : MonoBehaviour, ISerializationCallbackReceiver {
    [SerializeField] private VisualElementEvent[] events;
    [SerializeField] private VisualElementProperty[] properties;

    [SerializeField, HideInInspector] private List<VisualsController> events_visualsControllers;
    [SerializeField, HideInInspector] private string events_jData;
    
    [SerializeField, HideInInspector] private List<VisualsController> properties_visualsControllers;
    [SerializeField, HideInInspector] private string properties_jData;

    internal VisualElementEvent[] Events => events;

    internal VisualElementProperty[] Properties => properties;

    public void OnBeforeSerialize() {
        events ??= Array.Empty<VisualElementEvent>();
        events_visualsControllers = new List<VisualsController>();
        events_jData = JsonConvert.SerializeObject(events, new UnityObjectConverter<VisualsController>(events_visualsControllers));

        properties ??= Array.Empty<VisualElementProperty>();
        properties_visualsControllers = new List<VisualsController>();
        properties_jData = JsonConvert.SerializeObject(properties, new UnityObjectConverter<VisualsController>(properties_visualsControllers));
    }

    public void OnAfterDeserialize() {
        events_visualsControllers ??= new List<VisualsController>();
        events_jData ??= string.Empty;
        events ??= JsonConvert.DeserializeObject<VisualElementEvent[]>(events_jData, new UnityObjectConverter<VisualsController>(events_visualsControllers));

        properties_visualsControllers ??= new List<VisualsController>();
        properties_jData ??= string.Empty;
        properties ??= JsonConvert.DeserializeObject<VisualElementProperty[]>(properties_jData, new UnityObjectConverter<VisualsController>(properties_visualsControllers));
    }

    internal void InitControllers(IVisualsParams parameters, IVisualsResources resources) {
        foreach (var controller in GetComponentsInChildren<VisualsController>())
            controller.Init(parameters, resources);
    }
}