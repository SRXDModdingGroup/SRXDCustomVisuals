using System.Collections.Generic;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualElement : MonoBehaviour {
    [SerializeField] private VisualsEventBinding[] eventBindings;

    internal Dictionary<string, VisualsEventMapping[]> EventBindings { get; private set; }

    private void Awake() {
        EventBindings = new Dictionary<string, VisualsEventMapping[]>();

        foreach (var eventBinding in eventBindings)
            EventBindings.Add(eventBinding.eventName, eventBinding.mappings);
    }
}