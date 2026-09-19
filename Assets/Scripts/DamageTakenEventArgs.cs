using System;
using UnityEngine;

public class DamageTakenEventArgs : EventArgs {
    public float Damage {get; } // {get;} - can only be set once (in the constructor)

    public DamageTakenEventArgs(float damage) {
        this.Damage = damage;
    }
}

