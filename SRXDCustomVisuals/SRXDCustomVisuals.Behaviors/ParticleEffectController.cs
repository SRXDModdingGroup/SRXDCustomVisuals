using SRXDCustomVisuals.Core;
using UnityEngine;

namespace SRXDCustomVisuals.Behaviors; 

public class ParticleEffectController : VisualsController {
    [SerializeField] private ParticleSystem particleSystem;
    
    public override IVisualsEvent GetEvent(string key) => key switch {
        "Play" => new VisualsEvent(Play),
        "Stop" => new VisualsEvent(Stop),
        _ => null
    };

    private void Play(IVisualsParams parameters) => particleSystem.Play();
    
    private void Stop(IVisualsParams parameters) => particleSystem.Stop();
}