using UnityEngine;

public class HGunManager : GunManager {
    private Henchman _henchman;

    protected override void Awake() {
        base.Awake();
        this._henchman = this.GetComponentInParent<Henchman>();
    }

    protected override void FixedUpdate() {
        if (!this._henchman.IsAttacking) {
            // If Henchman isnt in desiredState
            if (!this._henchman.IsInDesiredState) return;
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
    public override Vector2 GetTargetPos() {
        Player player = PlayerManager.instance.GetPlayer();
        return player.transform.position;
    }
}
