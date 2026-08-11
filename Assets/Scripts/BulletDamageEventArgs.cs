using System;

// Class strictly for EventArgs (values of properties already exist)
public class BulletDamageEventArgs : EventArgs {
    public float BulletDamage {get; } // Can only be set once (in the constructor)

    public BulletDamageEventArgs(float bulletDamage) {
        BulletDamage = bulletDamage;
    }
}
