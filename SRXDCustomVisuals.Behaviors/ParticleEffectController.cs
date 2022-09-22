using SRXDCustomVisuals.Core;
using UnityEngine;

namespace SRXDCustomVisuals.Behaviors; 

public class ParticleEffectController : VisualsController {
    [SerializeField] private ParticleSystem[] particleSystems;

    public override IVisualsEvent GetEvent(string key) => key switch {
        "Play" => new VisualsEvent(Play),
        "Stop" => new VisualsEvent(Stop),
        _ => null
    };

    public override IVisualsProperty GetProperty(string key) => key switch {
        "EnableEmission" => new VisualsProperty(EnableEmissionChanged),
        _ => null
    };

    private void Play(IVisualsParams parameters) {
        foreach (var particleSystem in particleSystems)
            particleSystem.Play();
    }

    private void Stop(IVisualsParams parameters) {
        foreach (var particleSystem in particleSystems)
            particleSystem.Stop();
    }

    private void EnableEmissionChanged(VisualsValue value) {
        bool enable = value.Bool;
        
        foreach (var particleSystem in particleSystems) {
            var emission = particleSystem.emission;

            emission.enabled = enable;
        }
    }
}