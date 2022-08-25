using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SRXDCustomVisuals.Core; 

public class VisualsScene {
    private IList<VisualsModule> modules;
    private List<GameObject> instances = new();
    private Dictionary<string, List<Action<VisualsEvent>>> eventResponses = new();
    private bool loaded;

    public VisualsScene(IList<VisualsModule> modules) => this.modules = modules;

    public void Load(IList<Transform> roots) {
        if (loaded)
            return;

        loaded = true;

        foreach (var module in modules) {
            foreach (var element in module.Elements) {
                var instance = Object.Instantiate(element.prefab, roots[element.root]);

                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;

                foreach (var visualElement in instance.GetComponents<IVisualElement>())
                    visualElement.Init(this);

                instances.Add(instance);
            }
        }
    }

    public void Unload() {
        if (!loaded)
            return;

        loaded = false;
        eventResponses.Clear();

        foreach (var instance in instances)
            Object.Destroy(instance);
        
        instances.Clear();
    }
    
    public void InvokeEvent(string key) {
        if (!eventResponses.TryGetValue(key, out var responses))
            return;
        
        var visualsEvent = VisualsEvent.Empty;

        foreach (var response in responses) {
            visualsEvent.Reset();
            response.Invoke(visualsEvent);
        }
    }

    public void InvokeEvent(string key, VisualsEventBuilder eventBuilder) {
        if (!eventResponses.TryGetValue(key, out var responses))
            return;
        
        var visualsEvent = eventBuilder.Build();

        foreach (var response in responses) {
            visualsEvent.Reset();
            response.Invoke(visualsEvent);
        }
    }

    public void AddEventResponse(string key, Action<VisualsEvent> response) {
        if (!eventResponses.TryGetValue(key, out var responses)) {
            responses = new List<Action<VisualsEvent>>();
            eventResponses.Add(key, responses);
        }
        
        responses.Add(response);
    }
}