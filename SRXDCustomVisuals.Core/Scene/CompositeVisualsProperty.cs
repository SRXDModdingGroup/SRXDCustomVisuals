using System;
using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

internal class CompositeVisualsProperty : IVisualsProperty {
    private List<MappingInvoker> invokers = new();
    private VisualsValue currentValue;

    public void AddMapping(VisualsPropertyMapping mapping) {
        var invoker = new MappingInvoker(mapping);
        
        if (!invoker.Empty)
            invokers.Add(invoker);
    }

    public void SetValue(VisualsValue value) {
        if (value == currentValue)
            return;

        currentValue = value;

        foreach (var invoker in invokers)
            invoker.Invoke(value);
    }

    public void Clear() => invokers.Clear();

    private class MappingInvoker {
        public bool Empty => visualsProperty == null;

        private IVisualsProperty visualsProperty;
        private VisualsPropertyMapping mapping;

        public MappingInvoker(VisualsPropertyMapping mapping) {
            visualsProperty = mapping.target.GetProperty(mapping.name);

            if (visualsProperty == null)
                return;

            this.mapping = mapping;
        }

        public void Invoke(VisualsValue value) {
            var scale = mapping.scale;
            var bias = mapping.bias;
            
            switch (mapping.type) {
                case VisualsParamType.Bool:
                    visualsProperty.SetBool(value.Bool == scale.x >= 0f);
                    
                    break;
                case VisualsParamType.Int:
                    visualsProperty.SetInt(value.Int * Mathf.RoundToInt(scale.x) + Mathf.RoundToInt(bias.x));
                    
                    break;
                case VisualsParamType.Float:
                    visualsProperty.SetFloat(value.Float * scale.x + bias.x);
                    
                    break;
                case VisualsParamType.Vector:
                    visualsProperty.SetVector(Vector3.Scale(value.Vector, scale) + (Vector3) bias);
                    
                    break;
                case VisualsParamType.Color:
                    visualsProperty.SetColor(value.Color * scale + (Color) bias);
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}