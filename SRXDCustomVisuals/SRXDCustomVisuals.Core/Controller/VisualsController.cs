using UnityEngine;

namespace SRXDCustomVisuals.Core; 

public abstract class VisualsController : MonoBehaviour {
    public virtual void Init(IVisualsParams parameters, IVisualsResources resources) { }
    
    public virtual IVisualsEvent GetEvent(string key) => null;

    public virtual IVisualsProperty GetProperty(string key) => null;
}