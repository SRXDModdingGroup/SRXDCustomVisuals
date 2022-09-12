using System;
using System.Collections.Generic;

namespace SRXDCustomVisuals.Core; 

internal class CompositeVisualsEvent : IVisualsEvent {
    private List<MappingInvoker> invokers = new();

    public void AddMapping(VisualsEventMapping mapping) {
        var invoker = new MappingInvoker(mapping);
        
        if (!invoker.Empty)
            invokers.Add(invoker);
    }

    public void Invoke(IVisualsParams parameters) {
        foreach (var visualsEvent in invokers)
            visualsEvent.Invoke(parameters);
    }

    public void Clear() => invokers.Clear();

    private class MappingInvoker {
        public bool Empty { get; }
        
        private IVisualsEvent visualsEvent;
        private VisualsParams cachedParameters;
        private List<VisualsParamMapping> dynamicParamMappings;

        public MappingInvoker(VisualsEventMapping mapping) {
            visualsEvent = mapping.target.GetEvent(mapping.action);

            if (visualsEvent == null) {
                Empty = true;
                
                return;
            }
            
            cachedParameters = new VisualsParams();
            dynamicParamMappings = new List<VisualsParamMapping>();

            foreach (var parameterMapping in mapping.parameters) {
                string name = parameterMapping.name;
                string value = parameterMapping.value;

                switch (parameterMapping.type) {
                    case VisualsParamType.Bool when bool.TryParse(value, out bool boolVal):
                        cachedParameters.SetBool(name, boolVal);
                            
                        break;
                    case VisualsParamType.Int when Util.TryParseInt(value, out int intVal):
                        cachedParameters.SetInt(name, intVal);
                            
                        break;
                    case VisualsParamType.Float when Util.TryParseFloat(value, out float floatVal):
                        cachedParameters.SetFloat(name, floatVal);
                            
                        break;
                    case VisualsParamType.Vector when Util.TryParseVector(value, out var vectorVal):
                        cachedParameters.SetVector(name, vectorVal);
                            
                        break;
                    case VisualsParamType.Color when Util.TryParseColor(value, out var colorVal):
                        cachedParameters.SetColor(name, colorVal);
                            
                        break;
                    default:
                        dynamicParamMappings.Add(parameterMapping);
                        
                        break;
                }
            }
        }

        public void Invoke(IVisualsParams parameters) {
            foreach (var parameterMapping in dynamicParamMappings) {
                string name = parameterMapping.name;
                string value = parameterMapping.value;

                switch (parameterMapping.type) {
                    case VisualsParamType.Bool:
                        cachedParameters.SetBool(name, parameters.GetBool(value));
                            
                        break;
                    case VisualsParamType.Int:
                        cachedParameters.SetInt(name, parameters.GetInt(value));
                            
                        break;
                    case VisualsParamType.Float:
                        cachedParameters.SetFloat(name, parameters.GetFloat(value));
                            
                        break;
                    case VisualsParamType.Vector:
                        cachedParameters.SetVector(name, parameters.GetVector(value));
                            
                        break;
                    case VisualsParamType.Color:
                        cachedParameters.SetColor(name, parameters.GetColor(value));
                            
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            visualsEvent.Invoke(cachedParameters);
        }
    }
}