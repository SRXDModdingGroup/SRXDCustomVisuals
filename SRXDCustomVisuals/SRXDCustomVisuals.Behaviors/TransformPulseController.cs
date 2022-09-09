using System;
using SRXDCustomVisuals.Core;
using UnityEngine;

namespace SRXDCustomVisuals.Behaviors;

public class TransformPulseController : VisualsController {
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Vector3 positionVector = Vector3.zero;
    [SerializeField] private Vector3 scaleVector = Vector3.zero;
    [SerializeField] private float defaultAmount = 1f;
    [SerializeField] private float defaultAttack = 0f;
    [SerializeField] private float defaultDecay = 1f;
    [SerializeField] private float defaultSustain = 0f;

    private float currentAmount;
    private float amount;
    private float attack;
    private float decay;
    private float sustain;
    private bool attacking;

    private void Awake() {
        amount = defaultAmount;
        attack = defaultAttack;
        decay = defaultDecay;
        sustain = defaultSustain;
    }

    private void Update() {
        if (attacking) {
            currentAmount += amount * Time.deltaTime / attack;

            if (currentAmount >= amount) {
                currentAmount = amount;
                attacking = false;
            }
        }
        else if (decay == 0f)
            currentAmount = sustain;
        else
            currentAmount = Mathf.Lerp(sustain, currentAmount, Mathf.Exp(-Time.deltaTime / decay));

        targetTransform.localPosition = currentAmount * positionVector;
        targetTransform.localScale = currentAmount * scaleVector + Vector3.one;
    }

    public override Action<IVisualsParams> GetAction(string key) => key switch {
        "Pulse" => Pulse,
        "Release" => Release,
        _ => null
    };

    private void Pulse(IVisualsParams parameters) {
        amount = parameters.GetFloat("amount", defaultAmount);
        attack = parameters.GetFloat("attack", defaultAttack);
        decay = parameters.GetFloat("decay", defaultDecay);
        sustain = parameters.GetFloat("sustain", defaultSustain);
        attacking = attack > 0f && currentAmount < amount;
    }

    private void Release(IVisualsParams parameters) {
        decay = parameters.GetFloat("decay", defaultDecay);
        sustain = 0f;
        attacking = false;
    }
}