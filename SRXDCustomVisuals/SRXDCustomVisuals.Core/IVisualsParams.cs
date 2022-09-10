using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public interface IVisualsParams {
    bool GetBool(string key);
    bool GetBool(string key, bool defaultValue);
    
    int GetInt(string key);
    int GetInt(string key, int defaultValue);
    
    float GetFloat(string key);
    float GetFloat(string key, float defaultValue);
    
    Vector3 GetVector(string key);
    Vector3 GetVector(string key, Vector3 defaultValue);
    
    Color GetColor(string key);
    Color GetColor(string key, Color defaultValue);
}