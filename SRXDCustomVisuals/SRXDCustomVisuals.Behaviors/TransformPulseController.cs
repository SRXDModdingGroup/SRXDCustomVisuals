using System;
using SRXDCustomVisuals.Core;
using UnityEngine;

namespace SRXDCustomVisuals.Behaviors;

public class TransformPulseController : VisualsEventTarget {
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Vector3 positionVector = Vector3.zero;
    [SerializeField] private Vector3 scaleVector = Vector3.one;

    private float currentAmount;
    private float amount = 1f;
    private float attack = 0f;
    private float decay = 1f;
    private float sustain = 0f;
    private bool attacking;

    private void Update() {
        if (attacking) {
            currentAmount += amount * Time.deltaTime / attack;

            if (currentAmount >= amount) {
                currentAmount = amount;
                attacking = false;
            }
        }
        else if (decay == 0f)
            currentAmount = 0f;
        else
            currentAmount = Mathf.Lerp(sustain, currentAmount, Mathf.Exp(-Time.deltaTime / decay));

        targetTransform.localPosition = currentAmount * positionVector;
        targetTransform.localScale = currentAmount * scaleVector + Vector3.one;
    }

    public override Action<VisualsEventParams> GetAction(string key) => key switch {
        "Pulse" => Pulse,
        "Release" => Release,
        _ => null
    };

    private void Pulse(VisualsEventParams parameters) {
        amount = parameters.GetFloat("amount", 1f);
        attack = parameters.GetFloat("attack", 0f);
        decay = parameters.GetFloat("decay", 1f);
        sustain = parameters.GetFloat("sustain", 0f);
        attacking = attack > 0f && currentAmount < amount;
    }

    private void Release(VisualsEventParams parameters) {
        decay = parameters.GetFloat("decay", 1f);
        sustain = 0;
        attacking = false;
    }
}