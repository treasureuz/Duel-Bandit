using System;
using UnityEngine;

namespace RevolverEventArgs {
    // Classes strictly for EventArgs (values of properties already exist)
    public class AmmoEventArgs : EventArgs {
        // {get;} - can only be set once (in the constructor)
        public int CurrentMagCount { get; }
        public int ReserveAmmo { get; }
        public int TotalAmmo { get; }

        public AmmoEventArgs(int currMagCount, int reserveAmmo, int totalAmmo) {
            this.CurrentMagCount = currMagCount;
            this.ReserveAmmo = reserveAmmo;
            this.TotalAmmo = totalAmmo;
        }
    }

    public class RevolverDisplayInfoEventArgs : EventArgs {
        // {get;} - can only be set once (in the constructor)
        public string RevolverName {get; }
        public Color RevolverColor {get; }

        public RevolverDisplayInfoEventArgs(string name, Color color) {
            RevolverName = name;
            RevolverColor = color;
        }
    }

    public class DamageTakenEventArgs : EventArgs {
        public float Damage {get; } // {get;} - can only be set once (in the constructor)

        public DamageTakenEventArgs(float damage) {
            Damage = damage;
        }
    }
}
