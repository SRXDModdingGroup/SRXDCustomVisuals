using SRXDCustomVisuals.Core.Value;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

internal class EmptyVisualsParams : IVisualsParams {
    public static EmptyVisualsParams Instance { get; } = new();

    public VisualsValue GetValue(string key, VisualsValue defaultValue) => defaultValue;
}