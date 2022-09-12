using System;

namespace SRXDCustomVisuals.Core; 

public class VisualsEvent : IVisualsEvent {
    public static VisualsEvent Empty { get; } = new(_ => { });
    
    private Action<IVisualsParams> action;
    
    public VisualsEvent(Action<IVisualsParams> action) => this.action = action;

    public void Invoke(IVisualsParams parameters) => action.Invoke(parameters);
}