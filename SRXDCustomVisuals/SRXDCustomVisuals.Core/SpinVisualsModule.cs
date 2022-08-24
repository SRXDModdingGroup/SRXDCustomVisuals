using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core;

public class SpinVisualsModule : ScriptableObject {
    [SerializeField] private VisualElement[] elements;

    internal IReadOnlyList<VisualElement> Elements => elements;
}