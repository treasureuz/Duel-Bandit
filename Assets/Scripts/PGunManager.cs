using UnityEngine;
using UnityEngine.InputSystem;

public class PGunManager : GunManager {
    private Camera _cam;
    public int CurrentAmmo { get; private set; }

    protected override void Awake() {
        base.Awake();
        this._cam = Camera.main;
        SetCurrentAmmo(this.maxAmmo);
    }

    protected override void FixedUpdate() {
        RotateTowardsTargetPos(); // Handles gun rotation
        if (!HasAmmo || !Mouse.current.leftButton.isPressed
            || this.elapsedShootTime > Time.time) return;
        Shoot(); // Shoots gun
        this.elapsedShootTime = Time.time + this.timeBetweenShots;
    }

    protected override void Shoot() {
        base.Shoot();
        SetCurrentAmmo(--this.CurrentAmmo); // Decrement mag count
        AmmoEventArgs ammoEventArgs = new (this.CurrentAmmo, this.maxAmmo);
        PlayerManager.instance.OnPlayerGunShot?.Invoke(this, ammoEventArgs);
    }

    public override Vector2 GetTargetPos() {
        Vector2 mousePos = this._cam.ScreenToWorldPoint(
            Mouse.current.position.ReadValue());
        return mousePos;
    }

    protected bool HasAmmo => this.CurrentAmmo > 0;
    public void SetCurrentAmmo(int count) {
        this.CurrentAmmo = Mathf.Clamp(count, 0, this.maxAmmo);
    }
    public void SetMaxAmmo(int count) => this.maxAmmo = count;
    public int GetMaxAmmo() => this.maxAmmo;
}
