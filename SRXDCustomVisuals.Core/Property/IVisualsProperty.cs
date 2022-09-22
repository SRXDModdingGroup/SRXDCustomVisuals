using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public interface IVisualsProperty {
    void SetValue(VisualsValue value);
}

public static class VisualsPropertyExtensions {
    public static void SetBool(this IVisualsProperty property, bool value) => property.SetValue(new VisualsValue(value));
    
    public static void SetInt(this IVisualsProperty property, int value) => property.SetValue(new VisualsValue(value));
    
    public static void SetFloat(this IVisualsProperty property, float value) => property.SetValue(new VisualsValue(value));
    
    public static void SetVector(this IVisualsProperty property, Vector3 value) => property.SetValue(new VisualsValue(value));
    
    public static void SetColor(this IVisualsProperty property, Color value) => property.SetValue(new VisualsValue(value));
}