using System.Collections.Generic;

namespace SRXDCustomVisuals.Core; 

public class VisualsResources : IVisualsResources {
    private Dictionary<string, object> resources = new();

    public void SetResource(string key, object resource) => resources[key] = resource;

    public void Clear() => resources.Clear();

    public T GetResource<T>(string key) => GetResource(key, default(T));
    
    public T GetResource<T>(string key, T defaultValue) {
        if (resources.TryGetValue(key, out object obj) && obj is T asT)
            return asT;

        return defaultValue;
    }
}