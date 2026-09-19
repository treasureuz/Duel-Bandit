using System;
using UnityEngine;

public class HenchmanHelper : MonoBehaviour {
    public static HenchmanHelper instance;

    public EventHandler<DamageTakenEventArgs> OnHenchmanDamaged;

    void Awake() {
        if (!instance) instance = this;
    }
}
