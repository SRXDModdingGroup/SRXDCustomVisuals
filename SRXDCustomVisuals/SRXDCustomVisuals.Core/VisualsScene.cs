using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SRXDCustomVisuals.Core; 

public class VisualsScene {
    private IList<VisualsModule> modules;
    private List<GameObject> instances = new();
    private List<VisualElement> visualElements = new();
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
                instances.Add(instance);
                
                if (instance.TryGetComponent<VisualElement>(out var visualElement))
                    visualElements.Add(visualElement);
            }
        }
    }

    public void Unload() {
        if (!loaded)
            return;

        visualElements.Clear();

        foreach (var instance in instances)
            Object.Destroy(instance);
        
        instances.Clear();
        loaded = false;
    }
    
    public void InvokeEvent(string key) => InvokeEvent(key, VisualsEventParams.Empty);

    public void InvokeEvent(string key, VisualsEventParams eventParams) {
        var actionParams = new VisualsEventParams();
        
        foreach (var visualElement in visualElements) {
            if (!visualElement.Events.TryGetValue(key, out var mappings))
                continue;

            foreach (var eventMapping in mappings) {
                actionParams.Clear();

                foreach (var parameterMapping in eventMapping.parameters) {
                    string name = parameterMapping.name;
                    string value = parameterMapping.value;

                    switch (parameterMapping.type) {
                        case VisualsEventParamType.Int:
                        default:
                            actionParams.SetInt(name, Util.TryParseInt(value, out int intVal) ? intVal : eventParams.GetInt(value));
                            
                            break;
                        case VisualsEventParamType.Float:
                            actionParams.SetFloat(name, Util.TryParseFloat(value, out float floatVal) ? floatVal : eventParams.GetFloat(value));
                            
                            break;
                        case VisualsEventParamType.Vector:
                            actionParams.SetVector(name, Util.TryParseVector(value, out var vectorVal) ? vectorVal : eventParams.GetVector(value));
                            
                            break;
                        case VisualsEventParamType.Color:
                            actionParams.SetColor(name, Util.TryParseColor(value, out var colorVal) ? colorVal : eventParams.GetColor(value));
                            
                            break;
                    }
                }

                eventMapping.target.GetAction(eventMapping.action)?.Invoke(actionParams);
            }
        }
    }
}