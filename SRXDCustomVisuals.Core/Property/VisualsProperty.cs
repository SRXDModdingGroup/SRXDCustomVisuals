using System;
using SRXDCustomVisuals.Core.Value;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public class VisualsProperty : IVisualsProperty {
    private VisualsValue value = new(float.PositiveInfinity);
    private Action<VisualsValue> changed;

    public VisualsProperty(Action<VisualsValue> changed = null) => this.changed = changed;

    public void SetValue(VisualsValue value) {
        if (value == this.value)
            return;

        this.value = value;
        changed?.Invoke(value);
    }
}