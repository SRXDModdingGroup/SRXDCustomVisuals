using System;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public abstract class VisualsController : MonoBehaviour {
    public abstract Action<VisualsEventParams> GetAction(string key);
}