using System;
using UnityEngine;

namespace SRXDCustomVisuals.Core;

[Serializable]
internal struct VisualElementReference {
    public GameObject prefab;

    public int root;
}