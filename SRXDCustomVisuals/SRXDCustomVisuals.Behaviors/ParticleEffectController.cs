using System;
using SRXDCustomVisuals.Core;
using UnityEngine;

namespace SRXDCustomVisuals.Behaviors; 

public class ParticleEffectController : VisualsController {
    [SerializeField] private ParticleSystem particleSystem;
    
    public override Action<IVisualsParams> GetAction(string key) => key switch {
        "Play" => Play,
        "Stop" => Stop,
        _ => null
    };

    private void Play(IVisualsParams parameters) => particleSystem.Play();
    
    private void Stop(IVisualsParams parameters) => particleSystem.Stop();
}