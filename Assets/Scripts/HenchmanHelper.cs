using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HenchmanHelper : MonoBehaviour {
    public static HenchmanHelper instance;

    private List<Henchman> _henchmen;

    public EventHandler<BulletDamageEventArgs> OnHenchmanDamaged;

    void Awake() {
        if (!instance) instance = this;
        this._henchmen = FindObjectsByType<Henchman>(FindObjectsSortMode.None).ToList();
    }
}
