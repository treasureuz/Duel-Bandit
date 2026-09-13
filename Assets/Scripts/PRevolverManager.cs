using UnityEngine;
using UnityEngine.InputSystem;
using RevolverEventArgs;
using System.Collections;
using System;
using System.Collections.Generic;

public class PRevolverManager : TRevolverManager<PRevolverConfig> {
    private Camera _cam;

    private List<PRevolverData> _revolvers = new();
    private PRevolverData _currRevolverData;

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

    public override void SetCurrentRevolverConfig(PRevolverConfig rConfig) {
        SetCurrentRevolverData(rConfig);
        base.SetCurrentRevolverConfig(rConfig);
    }

    private void SetCurrentRevolverData(PRevolverConfig rConfig) {
        PRevolverData newRData = this._revolvers.Find(r => r.RevolverConfig == rConfig);
        if (newRData == null){
            newRData = new (rConfig, rConfig.startingTotalAmmo);
            this._revolvers.Add(newRData);
        }
        this._currRevolverData = newRData;
    }

    protected override void ApplyRevolverConfig() {
        base.ApplyRevolverConfig();
        RevolverDisplayInfoEventArgs revDisplayInfoArgs = new (
            this.currRevolverConfig.revolverName, this.currRevolverConfig.revolverColor);
        PlayerAmmoEventArgs ammoEventArgs = new (this._currRevolverData);
        PlayerManager.instance.OnPlayerRevolverEquipped?.Invoke(this,
            new (revDisplayInfoArgs, ammoEventArgs));
    }

    protected override void HandleShoot() {
        base.HandleShoot();
        this.elapsedShootTime = Time.time + this.currRevolverConfig.timeBetweenShots;
        // Decrement current mag count on every gun shot
        var currentMagCount = this._currRevolverData.CurrentMagCount;
        if (!HasInfiniteAmmo) this._currRevolverData.SetCurrentMagCount(--currentMagCount);
        // Applies ammo change to UI
        PlayerAmmoEventArgs ammoEventArgs = new (this._currRevolverData);
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
            (this, new (this.currRevolverConfig.reloadDuration));
        yield return new WaitForSeconds(this.currRevolverConfig.reloadDuration);
        var reserveAmmo = this._currRevolverData.CurrentReserveAmmo;
        this._currRevolverData.SetAmmoCounts(reserveAmmo);
        PlayerManager.instance.OnPlayerRevolverReloaded?.Invoke(this, new (this._currRevolverData));

        this._isReloading = false;
    }

      public void SwitchToNextRevolver() {
        if (this._revolvers.Count == 1) return;

        var currentIndex = this._revolvers.IndexOf(this._currRevolverData);
        var maxIndex = this._revolvers.Count - 1;
        int nextIndex;

        if (currentIndex == maxIndex) nextIndex = 0;
        else nextIndex = currentIndex + 1;

        PRevolverConfig revolver = this._revolvers[nextIndex].RevolverConfig;
        SetCurrentRevolverConfig(revolver);
    }

    public void SwitchToPreviousRevolver() {
        if (this._revolvers.Count == 1) return;

        var currentIndex = this._revolvers.IndexOf(this._currRevolverData);
        var minIndex = 0;
        int prevIndex;

        if (currentIndex == minIndex) prevIndex = this._revolvers.Count - 1;
        else prevIndex = currentIndex - 1;

        PRevolverConfig revolver = this._revolvers[prevIndex].RevolverConfig;
        SetCurrentRevolverConfig(revolver);
    }

    public void AddToReserveAmmo(int amount) {
        var newReserve = this._currRevolverData.CurrentReserveAmmo + amount;
        this._currRevolverData.SetCurrentReserveAmmo(newReserve);
    }

    private bool HasBulletsInCurrentMag => this._currRevolverData.CurrentMagCount > 0;
    private bool HasInfiniteAmmo => this.currRevolverConfig.startingTotalAmmo == int.MaxValue;
    // Can only reload is current mag is 0
    private bool CanReload =>
        !HasInfiniteAmmo && !this._isReloading && (this._currRevolverData.
        CurrentReserveAmmo > 0 && this._currRevolverData.CurrentMagCount == 0);

    protected override Vector2 GetTargetPos() {
        return this._cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    public string GetName() => this.currRevolverConfig.revolverName;
    public Color GetColor() => this.currRevolverConfig.revolverColor;
}
