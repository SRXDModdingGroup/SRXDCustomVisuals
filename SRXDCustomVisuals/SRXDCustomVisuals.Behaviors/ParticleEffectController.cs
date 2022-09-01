using System;
using SRXDCustomVisuals.Core;
using UnityEngine;

namespace SRXDCustomVisuals.Behaviors; 

public class ParticleEffectController : VisualsEventTarget {
    [SerializeField] private ParticleSystem particleSystem;
    
    public override Action<VisualsEventParams> GetAction(string key) => key switch {
        "Play" => Play,
        "Stop" => Stop,
        _ => null
    };

    private void Play(VisualsEventParams parameters) => particleSystem.Play();
    
    private void Stop(VisualsEventParams parameters) => particleSystem.Stop();
}