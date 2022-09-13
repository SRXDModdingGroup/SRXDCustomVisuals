using System.Collections.Generic;

namespace SRXDCustomVisuals.Core; 

public class VisualsSceneManager {
    public bool HasScene => scene != null;
    
    private VisualsScene scene;
    private Dictionary<string, CompositeVisualsEvent> events = new();
    private Dictionary<string, CompositeVisualsProperty> properties = new();

    public void SetScene(VisualsScene scene) {
        Clear();
        this.scene = scene;

        foreach (var element in scene.Elements) {
            foreach (var elementEvent in element.Events) {
                if (!events.TryGetValue(elementEvent.name, out var composite)) {
                    composite = new CompositeVisualsEvent();
                    events.Add(elementEvent.name, composite);
                }
                
                foreach (var mapping in elementEvent.mappings)
                    composite.AddMapping(mapping);
            }

            foreach (var elementProperty in element.Properties) {
                if (!properties.TryGetValue(elementProperty.name, out var composite)) {
                    composite = new CompositeVisualsProperty();
                    properties.Add(elementProperty.name, composite);
                }

                foreach (var mapping in elementProperty.mappings)
                    composite.AddMapping(mapping);
            }
        }
    }

    public void InitControllers(IVisualsParams parameters, IVisualsResources resources) {
        if (scene == null)
            return;
        
        foreach (var element in scene.Elements)
            element.InitControllers(parameters, resources);
    }

    public void Clear() {
        foreach (var compositeEvent in events)
            compositeEvent.Value.Clear();

        foreach (var compositeProperty in properties)
            compositeProperty.Value.Clear();

        events.Clear();
        properties.Clear();
        scene = null;
    }

    public IVisualsEvent GetEvent(string name) {
        if (events.TryGetValue(name, out var compositeEvent))
            return compositeEvent;

        return EmptyVisualsEvent.Instance;
    }

    public IVisualsProperty GetProperty(string name) {
        if (properties.TryGetValue(name, out var compositeProperty))
            return compositeProperty;
        
        return EmptyVisualsProperty.Instance;
    }
}