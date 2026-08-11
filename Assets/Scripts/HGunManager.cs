using UnityEngine;

public class HGunManager : GunManager {
    private Henchman _henchman;

    protected override void Awake() {
        base.Awake();
        this._henchman = this.GetComponentInParent<Henchman>();
    }

    protected override void FixedUpdate() {
        if (!this._henchman.IsAttacking) {
            // If Henchman isn't in desiredState
            if (!this._henchman.IsInDesiredState) return;
            SmoothlyRotateTowards(Quaternion.identity);
            return;
        }
        RotateTowardsTargetPos(); // Handles gun rotation to Player
        // Handles shooting
        if (this.elapsedShootTime > Time.time || !IsAimAlignedToPlayer()) return;
        Shoot(); // Shoots gun
        this.elapsedShootTime = Time.time + this.timeBetweenShots;
    }

    private bool IsAimAlignedToPlayer() {
        var angleDiff = Mathf.Abs(Mathf.DeltaAngle(this.transform.
            eulerAngles.z, this.rotationAngle));
        return angleDiff < 0.5f;
    }
    public override Vector2 GetTargetPos() {
        Player player = PlayerManager.instance.Player;
        return player.transform.position;
    }
}
