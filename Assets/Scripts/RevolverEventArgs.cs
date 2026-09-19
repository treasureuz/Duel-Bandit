using System;
using UnityEngine;

namespace RevolverEventArgs {
    // Classes strictly for EventArgs (values of properties already exist)
    public class PlayerAmmoEventArgs : EventArgs {
        // {get;} - can only be set once (in the constructor)
        public PRevolverData RevolverData {get; }

        public PlayerAmmoEventArgs(PRevolverData revolverData) {
            this.RevolverData = revolverData;
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
        public RevolverDisplayInfoEventArgs revolverDisplayInfoEArgs {get; }
        public PlayerAmmoEventArgs ammoEventArgs {get; }

        public RevolverEquippedEventArgs (RevolverDisplayInfoEventArgs
            revDisplayInfoArgs, PlayerAmmoEventArgs ammoArgs) {
            this.revolverDisplayInfoEArgs = revDisplayInfoArgs;
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
}
