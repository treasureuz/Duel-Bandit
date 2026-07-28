using UnityEngine;
using UnityEngine.InputSystem;

public class PWeaponManager : WeaponManager {
    private int _currentMagazineCount;

    protected override void Update() {
        RotateTowardsTarget(GetTargetPosition());
        HandleShooting();
    }

    protected override void HandleShooting() {
        base.HandleShooting();
        this.nextShootTime += Time.deltaTime;
        if (!Mouse.current.leftButton.isPressed ||
            this.nextShootTime < this.timeBetweenShots) return;
        Shoot(); this.nextShootTime = 0f;
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
