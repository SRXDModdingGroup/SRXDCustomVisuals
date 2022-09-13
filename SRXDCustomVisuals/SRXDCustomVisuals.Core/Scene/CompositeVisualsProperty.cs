using System;
using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

internal class CompositeVisualsProperty : IVisualsProperty {
    private List<MappingInvoker> invokers = new();
    private Vector4 currentValue;

    public void AddMapping(VisualsPropertyMapping mapping) {
        var invoker = new MappingInvoker(mapping);
        
        if (!invoker.Empty)
            invokers.Add(invoker);
    }
    
    public void SetBool(bool value) => SetFloat(value ? 1f : 0f);

    public void SetInt(int value) => SetFloat(value);

    public void SetFloat(float value) => SetValue(new Vector4(value, value, value, 1f));

    public void SetVector(Vector3 value) => SetValue(new Vector4(value.x, value.y, value.z, 1f));

    public void SetColor(Color value) => SetValue(value);

    public void Clear() => invokers.Clear();

    private void SetValue(Vector4 value) {
        if (value == currentValue)
            return;

        currentValue = value;

        foreach (var invoker in invokers)
            invoker.Invoke(value);
    }

    private class MappingInvoker {
        public bool Empty { get; }

        private IVisualsProperty visualsProperty;
        private VisualsParamType paramType;

        public MappingInvoker(VisualsPropertyMapping mapping) {
            visualsProperty = mapping.target.GetProperty(mapping.name);

            if (visualsProperty == null) {
                Empty = true;
                
                return;
            }
            
            paramType = mapping.type;
        }

        public void Invoke(Vector4 value) {
            switch (paramType) {
                case VisualsParamType.Bool:
                    visualsProperty.SetBool(value.x > 0f);
                    
                    break;
                case VisualsParamType.Int:
                    visualsProperty.SetInt(Mathf.RoundToInt(value.x));
                    
                    break;
                case VisualsParamType.Float:
                    visualsProperty.SetFloat(value.x);
                    
                    break;
                case VisualsParamType.Vector:
                    visualsProperty.SetVector(value);
                    
                    break;
                case VisualsParamType.Color:
                    visualsProperty.SetColor(value);
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}