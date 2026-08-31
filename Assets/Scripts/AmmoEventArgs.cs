using System;

// Class strictly for EventArgs (values of properties already exist)
public class AmmoEventArgs : EventArgs {
    // Can only be set once (in the constructor)
    public int CurrentAmmo { get; }
    public int ReserveAmmo { get; }
    public int TotalAmmo { get; }

    public AmmoEventArgs(int currAmmo, int reserveAmmo, int totalAmmo) {
        this.CurrentAmmo = currAmmo;
        this.ReserveAmmo = reserveAmmo;
        this.TotalAmmo = totalAmmo;
    }
}
