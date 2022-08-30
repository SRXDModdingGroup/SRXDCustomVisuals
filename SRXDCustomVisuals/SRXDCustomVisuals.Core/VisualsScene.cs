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

        loaded = false;
        visualElements.Clear();

        foreach (var instance in instances)
            Object.Destroy(instance);
        
        instances.Clear();
    }
    
    public void InvokeEvent(string key) => InvokeEvent(key, VisualsEventParams.Empty);

    public void InvokeEvent(string key, VisualsEventParams eventParams) {
        var mappedEventParams = new VisualsEventParams();
        
        foreach (var visualElement in visualElements) {
            if (!visualElement.EventBindings.TryGetValue(key, out var mappings))
                continue;

            foreach (var eventMapping in mappings) {
                mappedEventParams.Clear();

                foreach (var parameterMapping in eventMapping.parameterMappings) {
                    switch (parameterMapping.type) {
                        case VisualsEventParamType.Int:
                        default:
                            mappedEventParams.SetInt(parameterMapping.to, eventParams.GetInt(parameterMapping.from));
                            
                            break;
                        case VisualsEventParamType.Float:
                            mappedEventParams.SetFloat(parameterMapping.to, eventParams.GetFloat(parameterMapping.from));
                            
                            break;
                        case VisualsEventParamType.Vector:
                            mappedEventParams.SetVector(parameterMapping.to, eventParams.GetVector(parameterMapping.from));
                            
                            break;
                        case VisualsEventParamType.Color:
                            mappedEventParams.SetColor(parameterMapping.to, eventParams.GetColor(parameterMapping.from));
                            
                            break;
                    }
                }
                
                eventMapping.target.Invoke(mappedEventParams);
            }
        }
    }
}