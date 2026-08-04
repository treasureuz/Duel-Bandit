using UnityEngine;

public class HWeaponManager : WeaponManager {
    private Henchman _henchman;

    protected override void Awake() {
        base.Awake();
        this._henchman = this.GetComponentInParent<Henchman>();
    }

    protected override void FixedUpdate() {
        if (!this._henchman.IsAttacking) {
            // If Henchman isnt waiting before moving to its desired point
            if (!this._henchman.IsWaitingUntilDesired()) return;
            RotateToIdentity();
            return;
        }
        RotateTowardsTargetPos();
        // Handles shooting
        if (this.elapsedShootTime > Time.time) return;
        Shoot(); // Shoots gun
        this.elapsedShootTime = Time.time + this.timeBetweenShots;
    }

    public void RotateToIdentity() {
        Quaternion smoothedRot = Quaternion.RotateTowards(this.transform.
            rotation, Quaternion.identity, this.rotationSpeed * Time.fixedDeltaTime);
        this.rb2d.MoveRotation(smoothedRot);
    }
    protected override Vector2 GetTargetPosition() {
        return Player.instance.transform.position;
    }
}
