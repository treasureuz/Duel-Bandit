using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PGunManager : GunManager {
    private Camera _cam;
    public int CurrentMagCount { get; private set; }

    protected override void Awake() {
        base.Awake();
        this._cam = Camera.main;
        SetCurrentMagCount(this.maxMagCount);
    }

    protected override void FixedUpdate() {
        // Handles shooting (Player handles the gun rotation towards mousePos)
        if (!HasBullets || !Mouse.current.leftButton.isPressed
            || this.elapsedShootTime > Time.time) return;
        Shoot(); // Shoots gun
        this.elapsedShootTime = Time.time + this.timeBetweenShots;
    }

    protected override void Shoot() {
        base.Shoot();
        SetCurrentMagCount(--this.CurrentMagCount); // Decrement mag count
        PlayerManager.instance.OnPlayerGunShot?.Invoke(this, EventArgs.Empty);
    }

    public override Vector2 GetTargetPos() {
        Vector2 mousePos = this._cam.ScreenToWorldPoint(
            Mouse.current.position.ReadValue());
        return mousePos;
    }

    protected bool HasBullets => this.CurrentMagCount > 0;
    public void SetCurrentMagCount(int count) {
        this.CurrentMagCount = Mathf.Clamp(count, 0, this.maxMagCount);
    }
    public void SetMaxMagCount(int count) => this.maxMagCount = count;
    public int GetMaxMagCount() => this.maxMagCount;
}
