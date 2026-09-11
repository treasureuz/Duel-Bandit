using UnityEngine;
using UnityEngine.InputSystem;
using RevolverEventArgs;
using System.Collections;
using System;

public class PRevolverManager : TRevolverManager<PRevolverData> {
    private Camera _cam;

    public int CurrentMagCount { get; private set; }
    public int CurrentReserveAmmo { get; private set; }
    public int CurrentTotalAmmo => this.CurrentMagCount + this.CurrentReserveAmmo;

    private bool _isReloading;

    protected override void Awake() {
        base.Awake();
        this._cam = Camera.main;
    }

    protected override void FixedUpdate() {
        RotateTowardsTargetPos(); // Handles gun rotation
        if (!Mouse.current.leftButton.isPressed || !HasBulletsInCurrentMag
            || Time.time < this.elapsedShootTime) return;
        HandleShoot(); // Shoots gun
    }

    protected override void ApplyRevolverData() {
        base.ApplyRevolverData();
        SetAmmoCounts(this.currentRevolverData.startingTotalAmmo);
        RevolverDisplayInfoEventArgs revDisplayInfoArgs = new (
            this.currentRevolverData.revolverName, this.currentRevolverData.revolverColor);
        AmmoEventArgs ammoEventArgs = new (this.CurrentMagCount,
            this.CurrentReserveAmmo, this.CurrentTotalAmmo);
        PlayerManager.instance.OnPlayerRevolverEquipped?.Invoke(this,
            new (revDisplayInfoArgs, ammoEventArgs));
    }

    // private void SaveRevolverData(int currentIndex) {
    //     this.revolvers[currentIndex] = this.currentRevolverData;
    // }

    protected override void HandleShoot() {
        base.HandleShoot();
        // Decrement current mag count on every gun shot
        if (!HasInfiniteAmmo) SetCurrentMagCount(--this.CurrentMagCount);
        // Applies ammo change to UI
        AmmoEventArgs ammoEventArgs = new (this.CurrentMagCount,
            this.CurrentReserveAmmo, this.CurrentTotalAmmo);
        PlayerManager.instance.OnPlayerRevolverShot?.Invoke(this, ammoEventArgs);
    }

    public bool Reload() {
        if (!CanReload) {
            PlayerManager.instance.OnPlayerRevolverUnableToReload?.Invoke(this, EventArgs.Empty);
            return false;
        }
        StartCoroutine(HandleReload());
        return true;
    }

    private IEnumerator HandleReload() {
        if (this._isReloading) yield break;

        this._isReloading = true;
        PlayerManager.instance.OnPlayerRevolverReloading?.Invoke
            (this, new (this.currentRevolverData.reloadDuration));
        yield return new WaitForSeconds(this.currentRevolverData.reloadDuration);
        SetAmmoCounts(this.CurrentReserveAmmo);

        this._isReloading = false;
    }

      public void SwitchToNextRevolver() {
        if (this.revolvers.Count == 1) return;

        var currentIndex = this.revolvers.IndexOf(this.currentRevolverData);
        var maxIndex = this.revolvers.Count - 1;
        int nextIndex;

        if (currentIndex == maxIndex) nextIndex = 0;
        else nextIndex = currentIndex + 1;

        //SaveRevolverData(currentIndex);
        SetCurrentRevolver(this.revolvers[nextIndex]);
    }

    public void SwitchToPreviousRevolver() {
        if (this.revolvers.Count == 1) return;

        var currentIndex = this.revolvers.IndexOf(this.currentRevolverData);
        var minIndex = 0;
        int prevIndex;

        if (currentIndex == minIndex) prevIndex = this.revolvers.Count - 1;
        else prevIndex = currentIndex - 1;

        //SaveRevolverData(currentIndex);
        SetCurrentRevolver(this.revolvers[prevIndex]);
    }

    private bool HasBulletsInCurrentMag => this.CurrentMagCount > 0;
    private bool HasInfiniteAmmo => this.currentRevolverData.startingTotalAmmo == int.MaxValue;
    // Can only reload is current mag is 0
    private bool CanReload =>
        !HasInfiniteAmmo && !this._isReloading &&
        (this.CurrentReserveAmmo > 0 && this.CurrentMagCount == 0);

    private void SetAmmoCounts(int ammoToUse) {
        SetCurrentMagCount(ammoToUse); // Gets clamped to maxMagCount, if "ammoToUse" is high
        // remaining ammo from what was put into the current mag
        var reserveAmmo = ammoToUse - this.CurrentMagCount;
        SetCurrentReserveAmmo(reserveAmmo);
    }

    private void SetCurrentMagCount(int count) {
        this.CurrentMagCount = Mathf.Clamp(count, 0, this.currentRevolverData.maxMagCount);
    }
    public void SetCurrentReserveAmmo(int count) {
        // If maxTotalAmmo = 100 and maxMagCount = 30, maxReserveAmmo can be only between
        // (70: assuming currentMagCount is filled up to maxMagCount,
        // 100: assuming no bullets in currentMagCount)
        var maxReserveAmmo = this.currentRevolverData.maxTotalAmmo - this.CurrentMagCount;
        this.CurrentReserveAmmo = Mathf.Clamp(count, 0, maxReserveAmmo);
    }

    protected override Vector2 GetTargetPos() {
        return this._cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    public string GetName() => this.currentRevolverData.revolverName;
    public Color GetColor() => this.currentRevolverData.revolverColor;
}
