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
            this.RevolverName = name;
            this.RevolverColor = color;
        }
    }

    public class RevolverEquippedEventArgs : EventArgs {
        public RevolverDisplayInfoEventArgs revolverDisplayInfoArgs {get; }
        public AmmoEventArgs ammoEventArgs {get; }

        public RevolverEquippedEventArgs (RevolverDisplayInfoEventArgs
            revDisplayInfoArgs, AmmoEventArgs ammoArgs) {
            this.revolverDisplayInfoArgs = revDisplayInfoArgs;
            this.ammoEventArgs = ammoArgs;
        }
    }

    public class RevolverReloadingEventArgs : EventArgs {
        // {get; } - can only be set once (in the constructor)
        public float ReloadDuration {get; }

        public RevolverReloadingEventArgs(float reloadDur) {
            this.ReloadDuration = reloadDur;
        }
    }

    public class DamageTakenEventArgs : EventArgs {
        public float Damage {get; } // {get;} - can only be set once (in the constructor)

        public DamageTakenEventArgs(float damage) {
            this.Damage = damage;
        }
    }
}
