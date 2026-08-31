using UnityEngine;
using UnityEngine.InputSystem;

public class PRevolverManager : RevolverManager {
    [Header("PRevolver Settings")]
    [SerializeField] private int _maxTotalAmmo;
    // the extra ammo/bullets that move into the current mag when reloading
    [SerializeField] private int _startingTotalAmmo;
    [SerializeField] private int _maxMagCount;

    private Camera _cam;

    public int CurrentMagCount { get; private set; }
    public int CurrentReserveAmmo { get; private set; }
    public int CurrentTotalAmmo => this.CurrentMagCount + this.CurrentReserveAmmo;

    protected override void Awake() {
        base.Awake();
        this._cam = Camera.main;
        SetAmmoCounts(this._startingTotalAmmo);
    }

    protected override void FixedUpdate() {
        RotateTowardsTargetPos(); // Handles gun rotation
        if (!Mouse.current.leftButton.isPressed || !HasBulletsInCurrentMag
            || Time.time < this.elapsedShootTime) return;
        HandleRevolverShoot(); // Shoots gun
    }

    protected override void HandleRevolverShoot() {
        base.HandleRevolverShoot();
        // Decrement current mag count on every gun shot
        if (!HasInfiniteAmmo) SetCurrentMagCount(--this.CurrentMagCount);
        // Applies ammo change to UI
        AmmoEventArgs ammoEventArgs = new (this.CurrentMagCount,
            this.CurrentReserveAmmo, this.CurrentTotalAmmo);
        PlayerManager.instance.OnPlayerRevolverShot?.Invoke(this, ammoEventArgs);
    }

    public bool Reload() {
        if (!CanReload) return false;
        SetAmmoCounts(this.CurrentReserveAmmo);
        return true;
    }

    protected override Vector2 GetTargetPos() {
        return this._cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    private bool HasBulletsInCurrentMag => this.CurrentMagCount > 0;
    private bool HasInfiniteAmmo => this._startingTotalAmmo == int.MaxValue;
    private bool CanReload =>
        !HasInfiniteAmmo && (this.CurrentReserveAmmo > 0 || this.CurrentMagCount < this._maxMagCount);

    private void SetAmmoCounts(int ammoToUse) {
        //if (HasInfiniteAmmo) return;
        SetCurrentMagCount(ammoToUse); // Gets clamped to maximum: maxMagCount, if "ammoToUse" is high
        var reserveAmmo = ammoToUse - this.CurrentMagCount; // remaining ammo from what was put into the current mag
        SetCurrentReserveAmmo(reserveAmmo);
    }

    private void SetCurrentMagCount(int count) {
        this.CurrentMagCount = Mathf.Clamp(count, 0, this._maxMagCount);
    }
    public void SetCurrentReserveAmmo(int count) {
        // If maxTotalAmmo = 100 and maxMagCount = 30, maxReserveAmmo can be only between
        // (70: assuming currentMagCount is filled up to maxMagCount, 100: assuming no bullets in currentMagCount)
        var maxReserveAmmo = this._maxTotalAmmo - this.CurrentMagCount;
        this.CurrentReserveAmmo = Mathf.Clamp(count, 0, maxReserveAmmo);
    }
}
