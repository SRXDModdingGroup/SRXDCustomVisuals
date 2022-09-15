using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public interface IVisualsProperty {
    void SetBool(bool value);

    void SetInt(int value);

    void SetFloat(float value);

    void SetVector(Vector3 value);

    void SetColor(Color value);
}