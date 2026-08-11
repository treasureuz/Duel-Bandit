using System;

// Class strictly for EventArgs (values of properties already exist)
public class AmmoEventArgs : EventArgs {
    // Can only be set once (in the constructor)
    public int CurrentAmmo { get; }
    public int MaxAmmo { get; }

    public AmmoEventArgs(int currAmmo, int maxAmmo) {
        this.CurrentAmmo = currAmmo;
        this.MaxAmmo = maxAmmo;
    }
}
