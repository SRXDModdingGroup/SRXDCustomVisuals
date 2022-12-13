using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsElementReference {
    public GameObject Prefab { get; }
    
    public int Root { get; }

    public VisualsElementReference(GameObject prefab, int root) {
        Prefab = prefab;
        Root = root;
    }
}