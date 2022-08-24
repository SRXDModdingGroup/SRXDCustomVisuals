using System;
using UnityEngine;

namespace SRXDCustomVisuals.Core;

[Serializable]
internal struct VisualElement {
    public GameObject prefab;

    public VisualElementRoot root;
}