using System;
using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core;

[CreateAssetMenu(menuName = "Visuals Module", fileName = "Module")]
public class VisualsModule : ScriptableObject, ISerializationCallbackReceiver {
    [SerializeField] private VisualElementReference[] elements;
    [SerializeField, HideInInspector] private GameObject[] prefabs;
    [SerializeField, HideInInspector] private int[] roots;
    
    internal IReadOnlyList<VisualElementReference> Elements => elements;
    
    public void OnBeforeSerialize() {
        elements ??= Array.Empty<VisualElementReference>();
        prefabs = new GameObject[elements.Length];
        roots = new int[elements.Length];

        for (int i = 0; i < elements.Length; i++) {
            prefabs[i] = elements[i].prefab;
            roots[i] = elements[i].root;
        }
    }

    public void OnAfterDeserialize() {
        if (elements != null)
            return;

        if (prefabs == null) {
            elements = Array.Empty<VisualElementReference>();
            
            return;
        }
        
        elements = new VisualElementReference[prefabs.Length];

        for (int i = 0; i < elements.Length; i++)
            elements[i] = new VisualElementReference() { prefab = prefabs[i], root = roots[i] };
    }
}