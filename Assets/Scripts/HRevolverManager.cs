using UnityEngine;

public class HRevolverManager : TRevolverManager<HRevolverData> {
    private Henchman _henchman;

    protected override void Awake() {
        base.Awake();
        this._henchman = this.GetComponentInParent<Henchman>();
    }

    protected override void FixedUpdate() {
        if (!PlayerManager.instance.Player) return;
        if (this._henchman.IsInDesiredState) {
            SmoothlyRotateTowards(Quaternion.identity);
            return;
        }
        RotateTowardsTargetPos(); // Handles gun rotation to Player
        if (Time.time < this.elapsedShootTime || !IsAimAlignedToPlayer()) return;
        HandleShoot(); // Shoots gun
    }

    // protected void HandleShoot() {
    //     // *Note: This is world space rotation (not localRotation)*
    //     // Takes the bulletPrefab's original rotation (180)
    //     // and adds it by the rotationAngle towards the targetPos.
    //     // ((0,0,180) * (0,0,40) = (0, 0, 220)
    //     Quaternion spawnRot = this.bulletSpawnPoint.rotation * this.
    //         currentRevolverData.bulletPrefab.transform.rotation;
    //     base.HandleShoot(spawnRot);
    // }

    private bool IsAimAlignedToPlayer() {
        var angleDiff = Mathf.Abs(Mathf.DeltaAngle(this.transform.
            eulerAngles.z, this.localScaleAngle));
        return angleDiff < 0.5f;
    }
    protected override Vector2 GetTargetPos() {
        Player player = PlayerManager.instance.Player;
        return player.transform.position;
    }

    public override void SetCurrentTimeBetweenShots(float tbs) {
        var newTimeBetweenShots = Mathf.Clamp(tbs,
            this.currentRevolverData.standardTimeBetweenShots,
            this.currentRevolverData.maxTimeBetweenShots);
            base.SetCurrentTimeBetweenShots(newTimeBetweenShots);
    }
}
