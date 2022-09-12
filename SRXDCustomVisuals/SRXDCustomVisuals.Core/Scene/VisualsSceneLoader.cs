using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SRXDCustomVisuals.Core; 

public class VisualsSceneLoader {
    private IList<VisualsModule> modules;
    private VisualsScene scene;
    private List<GameObject> instances = new();
    private bool loaded;

    public VisualsSceneLoader(IList<VisualsModule> modules) => this.modules = modules;

    public VisualsScene Load(IList<Transform> roots) {
        if (loaded)
            return scene;

        var visualElements = new List<VisualElement>();
        
        foreach (var module in modules) {
            foreach (var element in module.Elements) {
                var instance = Object.Instantiate(element.prefab, roots[element.root]);

                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                instances.Add(instance);

                if (instance.TryGetComponent<VisualElement>(out var visualElement))
                    visualElements.Add(visualElement);
            }
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