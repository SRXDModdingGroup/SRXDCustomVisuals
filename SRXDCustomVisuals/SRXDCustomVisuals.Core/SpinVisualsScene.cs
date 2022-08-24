using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SRXDCustomVisuals.Core; 

public class SpinVisualsScene {
    private SpinVisualsModule[] modules;
    private List<GameObject> instances;
    private bool loaded;

    public SpinVisualsScene(SpinVisualsModule[] modules) {
        this.modules = modules;
        instances = new List<GameObject>();
    }

    public void Load(Transform worldRoot, Transform cameraRoot) {
        if (loaded)
            return;

        loaded = true;

        foreach (var module in modules) {
            foreach (var element in module.Elements) {
                Transform root;
                
                switch (element.root) {
                    case VisualElementRoot.World:
                        root = worldRoot;
                        break;
                    case VisualElementRoot.Camera:
                    default:
                        root = cameraRoot;
                        break;
                }

                instances.Add(Object.Instantiate(element.prefab, Vector3.zero, Quaternion.identity, root));
            }
        }
    }

    public void Unload() {
        if (!loaded)
            return;

        loaded = false;

        foreach (var instance in instances)
            Object.Destroy(instance);
        
        instances.Clear();
    }
}