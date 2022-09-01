using System.Collections.Generic;

namespace SRXDCustomVisuals.Core; 

public abstract class VisualsSceneBase {
    protected abstract IEnumerable<VisualElement> VisualElements { get; }
    
    public void InvokeEvent(string key) => InvokeEvent(key, VisualsEventParams.Empty);

    public void InvokeEvent(string key, VisualsEventParams eventParams) {
        var actionParams = new VisualsEventParams();
        
        foreach (var visualElement in VisualElements) {
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