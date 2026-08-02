using UnityEngine;
using UnityEngine.InputSystem;

public class PWeaponManager : WeaponManager {
    private Camera cam;

    private int _currentMagazineCount;

    protected override void Awake() {
        this.cam = Camera.main;
        base.Awake();
    }
    protected override void FixedUpdate() {
        // Handles shooting
        RotateTowardsTarget(GetTargetPosition());
        if (!Mouse.current.leftButton.isPressed ||
            this.elapsedShootTime > Time.time) return;
        Shoot(); // Shoots gun
        this.elapsedShootTime = Time.time + this.timeBetweenShots;
    }

    protected override void Shoot() {
        base.Shoot();
        //--this.currentMagazineCount; // Decrement mag count
    }

    protected override Vector3 GetTargetPosition() {
        Vector3 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        return mousePos;
    }

    protected bool HasBullets() => this._currentMagazineCount > 0;
    // public void SetCurrentMagazineCount(int num) {
    //     this._currentMagazineCount = Mathf.Clamp(num, 0, this.bulletMagazineCount);
    // }
    public int GetCurrentMagazineCount() => this._currentMagazineCount;
    //public int GetMaxMagazineCount() => this.bulletMagazineCount;
}
