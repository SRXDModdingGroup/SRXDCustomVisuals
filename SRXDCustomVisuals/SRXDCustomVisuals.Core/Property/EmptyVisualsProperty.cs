using UnityEngine;

namespace SRXDCustomVisuals.Core; 

internal class EmptyVisualsProperty : IVisualsProperty {
    public static EmptyVisualsProperty Instance { get; } = new();
    
    public void SetBool(bool value) { }

    public void SetInt(int value) { }

    public void SetFloat(float value) { }

    public void SetVector(Vector3 value) { }

    public void SetColor(Color value) { }
}