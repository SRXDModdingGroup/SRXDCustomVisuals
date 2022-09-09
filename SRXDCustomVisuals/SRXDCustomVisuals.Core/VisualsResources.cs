using System.Collections.Generic;

namespace SRXDCustomVisuals.Core; 

public class VisualsResources : IVisualsResources {
    private Dictionary<string, object> resources = new();

    public void AddResource(string key, object resource) => resources.Add(key, resource);

    public T GetResource<T>(string key) {
        if (resources.TryGetValue(key, out object obj) && obj is T asT)
            return asT;

        return default;
    }
}