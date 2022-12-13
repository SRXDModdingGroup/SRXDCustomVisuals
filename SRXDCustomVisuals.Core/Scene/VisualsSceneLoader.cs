using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SRXDCustomVisuals.Core; 

public class VisualsSceneLoader {
    private IList<VisualsElementReference> elements;
    private VisualsScene scene;
    private List<GameObject> instances = new();
    private bool loaded;

    public VisualsSceneLoader(IList<VisualsElementReference> elements) => this.elements = elements;

    public VisualsScene Load(IList<Transform> roots) {
        if (loaded)
            return scene;

        var visualElements = new List<VisualsElement>();
        
        foreach (var element in elements) {
            if (element.Root < 0 || element.Root >= roots.Count)
                continue;
            
            var instance = Object.Instantiate(element.Prefab, roots[element.Root]);

            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            instances.Add(instance);

            if (instance.TryGetComponent<VisualsElement>(out var visualElement))
                visualElements.Add(visualElement);
        }

        scene = new VisualsScene(visualElements);
        loaded = true;

        return scene;
    }

    public void Unload() {
        if (!loaded)
            return;
        
        foreach (var instance in instances)
            Object.Destroy(instance);

        scene = null;
        instances.Clear();
        loaded = false;
    }
}