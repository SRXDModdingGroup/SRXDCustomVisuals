using System;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public abstract class VisualsEventTarget : MonoBehaviour {
    public abstract Action<VisualsEventParams> GetAction(string key);
}