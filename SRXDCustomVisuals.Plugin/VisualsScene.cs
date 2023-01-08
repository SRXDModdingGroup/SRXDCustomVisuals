using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SRXDCustomVisuals.Core; 

public class VisualsScene {
    private List<VisualsElementReference> elements;
    private List<GameObject> instances = new();
    private bool loaded;

    public VisualsScene(IEnumerable<VisualsElementReference> elements) => this.elements = elements.ToList();

    public void Load(IList<Transform> roots) {
        if (loaded)
            return;
        
        foreach (var element in elements) {
            if (element.Root < 0 || element.Root >= roots.Count)
                continue;
            
            var instance = Object.Instantiate(element.Prefab, roots[element.Root]);

            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            instances.Add(instance);
        }

        loaded = true;
    }

    public void Unload() {
        if (!loaded)
            return;
        
        foreach (var instance in instances)
            Object.Destroy(instance);

        instances.Clear();
        loaded = false;
    }
}