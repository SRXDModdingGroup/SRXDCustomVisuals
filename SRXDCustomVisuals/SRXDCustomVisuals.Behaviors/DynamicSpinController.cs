using System;
using SRXDCustomVisuals.Core;
using UnityEngine;

namespace SRXDCustomVisuals.Behaviors; 

public class DynamicSpinController : VisualsController {
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Vector3 spinAxis;
    [SerializeField] private float defaultSpeed = 1f;
    [SerializeField] private float defaultDecay = 1f;
    [SerializeField] private float defaultSustain = 0f;

    private float spin;
    private float speed;
    private float decay;
    private float sustain;

    private void Awake() {
        decay = defaultDecay;
        sustain = defaultSustain;
    }

    private void Update() {
        if (decay == 0f)
            speed = sustain;
        else
            speed = Mathf.Lerp(sustain, speed, Mathf.Exp(-Time.deltaTime / decay));
        
        spin = Mathf.Repeat(spin + Time.deltaTime * speed, 360f);
        targetTransform.localRotation = Quaternion.AngleAxis(spin, spinAxis);
    }

    public override Action<VisualsEventParams> GetAction(string key) => key switch {
        "Spin" => Spin,
        "Release" => Release,
        _ => null
    };

    private void Spin(VisualsEventParams parameters) {
        speed = parameters.GetFloat("speed", defaultSpeed);
        decay = parameters.GetFloat("decay", defaultDecay);
        sustain = parameters.GetFloat("sustain", defaultSustain);
    }

    private void Release(VisualsEventParams parameters) {
        decay = parameters.GetFloat("decay", defaultDecay);
        sustain = 0f;
    }
}