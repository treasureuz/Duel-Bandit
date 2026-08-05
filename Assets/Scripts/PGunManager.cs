using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PGunManager : GunManager {
    [SerializeField] private int _maxMagCount = 30;

    private Camera _cam;
    private int _currentMagCount;

    public EventHandler OnShot;

    protected override void Awake() {
        this._cam = Camera.main;
        base.Awake();
        this._currentMagCount = this._maxMagCount;
    }
    protected override void FixedUpdate() {
        // Handles shooting
        RotateTowardsTargetPos();
        if (!HasBullets() || !Mouse.current.leftButton.isPressed
            || this.elapsedShootTime > Time.time) return;
        Shoot(); // Shoots gun
        this.elapsedShootTime = Time.time + this.timeBetweenShots;
    }

    protected override void Shoot() {
        base.Shoot();
        --this._currentMagCount; // Decrement mag count
        OnShot?.Invoke(this, EventArgs.Empty);
    }

    public override Vector2 GetTargetPosition() {
        Vector2 mousePos = this._cam.ScreenToWorldPoint(
            Mouse.current.position.ReadValue());
        return mousePos;
    }

    protected bool HasBullets() => this._currentMagCount > 0;
    public void SetCurrentMagCount(int count) {
        this._currentMagCount = Mathf.Clamp(count, 0, this._maxMagCount);
    }
    public void SetMaxMagCount(int count) => this._maxMagCount = count;
    public int GetCurrentMagCount() => this._currentMagCount;
    public int GetMaxMagCount() => this._maxMagCount;
}
