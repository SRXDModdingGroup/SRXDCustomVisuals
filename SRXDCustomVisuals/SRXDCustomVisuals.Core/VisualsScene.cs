using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SRXDCustomVisuals.Core; 

public class VisualsScene : VisualsSceneBase {
    private IList<VisualsModule> modules;
    private List<GameObject> instances = new();
    private List<VisualElement> visualElements = new();
    private bool loaded;

    protected override IEnumerable<VisualElement> VisualElements => visualElements;

    public VisualsScene(IList<VisualsModule> modules) => this.modules = modules;

    public void Load(IList<Transform> roots) {
        if (loaded)
            return;

        loaded = true;

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
    }

    public void Unload() {
        if (!loaded)
            return;

        visualElements.Clear();

        foreach (var instance in instances)
            Object.Destroy(instance);
        
        instances.Clear();
        loaded = false;
    }
}