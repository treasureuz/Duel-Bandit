using UnityEngine;

public class HWeaponManager : WeaponManager {
    private Henchman _henchman;

    protected override void Awake() {
        base.Awake();
        this._henchman = this.GetComponentInParent<Henchman>();
    }

    protected override void FixedUpdate() {
        var canShoot = this._henchman.GetCurrentState() == HenchmanState.Attack;
        if (!canShoot) {
            // If Henchman isnt waiting before moving to its desired point
            if (!this._henchman.IsWaitingUntilDesired()) return;
            Quaternion smoothedRotation = Quaternion.RotateTowards(this.transform.
                rotation, Quaternion.identity, this.rotationSpeed);
            this.rb2d.MoveRotation(smoothedRotation);
            return;
        }
        RotateTowardsTarget(GetTargetPosition());
        // Handles shooting
        if (this.elapsedShootTime > Time.time) return;
        Shoot(); // Shoots gun
        this.elapsedShootTime = Time.time + this.timeBetweenShots;
    }

    protected override Vector3 GetTargetPosition() {
        return Player.instance.transform.position;
    }
}
