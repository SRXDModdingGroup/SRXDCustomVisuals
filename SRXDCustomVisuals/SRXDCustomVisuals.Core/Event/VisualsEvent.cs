using System;

namespace SRXDCustomVisuals.Core; 

public class VisualsEvent : IVisualsEvent {
    private Action<IVisualsParams> action;
    
    public VisualsEvent(Action<IVisualsParams> action) => this.action = action;

    public void Invoke(IVisualsParams parameters) => action.Invoke(parameters);
}