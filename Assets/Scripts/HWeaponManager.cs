using UnityEngine;

public class HWeaponManager : WeaponManager {
    private Henchman _henchman;

    protected override void Awake() {
        base.Awake();
        this._henchman = this.GetComponentInParent<Henchman>();
    }

    protected override void FixedUpdate() {
        var playerLayerMask = 1 << Player.instance.gameObject.layer;
        if (this._henchman.GetCurrentState() != HenchmanState.Attack) return;
        RotateTowardsTarget(GetTargetPosition());
        // RaycastHit2D hit = Physics2D.Raycast(this.rb2d.position, Vector2.left,
        //     this._henchman.GetRaycastDistance(), playerLayerMask);
        // if (!hit) return;
        // Handles shooting
        if (this.nextShootTime > Time.time) return;
        Shoot(); // Shoots gun
        this.nextShootTime = Time.time + this.timeBetweenShots;
    }

    protected override Vector3 GetTargetPosition() {
        return Player.instance.transform.position;
    }
}
