using System;
using System.Collections.Generic;
using UnityEngine;

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
            visualsEvent = mapping.target.GetEvent(mapping.name);

            if (visualsEvent == null) {
                Empty = true;
                
                return;
            }
            
            cachedParameters = new VisualsParams();
            dynamicParamMappings = new List<VisualsParamMapping>();

            foreach (var parameterMapping in mapping.parameters) {
                string name = parameterMapping.name;
                string parameter = parameterMapping.parameter;

                if (!string.IsNullOrWhiteSpace(parameter)) {
                    dynamicParamMappings.Add(parameterMapping);
                    
                    continue;
                }
                
                var bias = parameterMapping.bias;

                switch (parameterMapping.type) {
                    case VisualsParamType.Bool:
                        cachedParameters.SetBool(name, parameterMapping.scale.x > 0f);

                        continue;
                    case VisualsParamType.Int:
                        cachedParameters.SetInt(name, Mathf.RoundToInt(bias.x));

                        continue;
                    case VisualsParamType.Float:
                        cachedParameters.SetFloat(name, bias.x);

                        continue;
                    case VisualsParamType.Vector:
                        cachedParameters.SetVector(name, bias);

                        continue;
                    case VisualsParamType.Color:
                        cachedParameters.SetColor(name, bias);

                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void Invoke(IVisualsParams parameters) {
            foreach (var parameterMapping in dynamicParamMappings) {
                string name = parameterMapping.name;
                string parameter = parameterMapping.parameter;
                var scale = parameterMapping.scale;
                var bias = parameterMapping.bias;

                switch (parameterMapping.type) {
                    case VisualsParamType.Bool:
                        cachedParameters.SetBool(name, parameters.GetBool(parameter) == scale.x > 0f);
                            
                        continue;
                    case VisualsParamType.Int:
                        cachedParameters.SetInt(name, parameters.GetInt(parameter) * Mathf.RoundToInt(scale.x) + Mathf.RoundToInt(bias.x));
                            
                        continue;
                    case VisualsParamType.Float:
                        cachedParameters.SetFloat(name, parameters.GetFloat(parameter) * scale.x + bias.x);
                            
                        continue;
                    case VisualsParamType.Vector:
                        cachedParameters.SetVector(name, Vector3.Scale(parameters.GetVector(parameter), scale) + (Vector3) bias);
                            
                        continue;
                    case VisualsParamType.Color:
                        cachedParameters.SetColor(name, parameters.GetColor(parameter) * scale + (Color) bias);
                            
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            visualsEvent.Invoke(cachedParameters);
        }
    }
}