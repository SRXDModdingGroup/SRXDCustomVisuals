using System;
using SRXDCustomVisuals.Core;
using UnityEngine;

namespace SRXDCustomVisuals.Behaviors;

public class PulseScale : VisualsEventTarget {
    [SerializeField] private Transform targetTransform;
    [SerializeField] private float amount;
    [SerializeField] private float attack;
    [SerializeField] private float decay;

    private float animTime;
    private float currentAmount;

    private void Start() {
        animTime = attack + decay;
        currentAmount = 1f;
    }

    private void Update() {
        animTime += Time.deltaTime;
        
        if (animTime < attack)
            currentAmount = animTime / attack;
        else if (animTime < attack + decay) {
            currentAmount = 1f - ((animTime - attack) / decay);
            currentAmount *= currentAmount;
        }
        else
            currentAmount = 0f;
        
        targetTransform.localScale = (1f + amount * currentAmount) * Vector3.one;
    }

    public override Action<VisualsEventParams> GetAction(string key) {
        if (key == "Pulse")
            return Pulse;

        return null;
    }

    private void Pulse(VisualsEventParams parameters) => animTime = currentAmount * attack;
}