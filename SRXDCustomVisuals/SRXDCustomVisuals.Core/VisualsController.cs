using System;
using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public abstract class VisualsController : MonoBehaviour {
    public virtual void Init(IVisualsParams parameters, IVisualsResources resources) { }
    
    public virtual Action<IVisualsParams> GetAction(string key) => null;
}