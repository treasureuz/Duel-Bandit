using UnityEngine;

public class HRevolverManager : TRevolverManager<HRevolverData> {
    private Henchman _henchman;

    protected override void Awake() {
        base.Awake();
        this._henchman = this.GetComponentInParent<Henchman>();
    }

    protected override void FixedUpdate() {
        if (!PlayerManager.instance.Player) return;
        if (this._henchman.IsInDesiredState || this._henchman.GetCurrentState() is HenchmanState.Search) {
            SmoothlyRotateTowards(Quaternion.identity);
            return;
        }
        // Henchman keeps its revolver rotated towards the Player in "Attack" and "WaitingToSearch"
        // In WaitingToSearch, it suggests to the player that the Henchman is "on alert"
        // However, it keeps its rotation at its identity in when moving towards a point (Search and desired)
        // bc Henchman looks weird if rotated towards the last pos it seen Player at (could be rotated up/down)
        RotateTowardsTargetPos(); // Handles revolver rotation to Player
        if (this._henchman.GetCurrentState() is not HenchmanState.Attack
            || Time.time < this.elapsedShootTime || !IsAimAlignedToPlayer()) return;
        HandleShoot(); // Shoots revolver only if above conditions are false
    }

    private bool IsAimAlignedToPlayer() {
        var angleDiff = Mathf.Abs(Mathf.DeltaAngle(this.transform.
            eulerAngles.z, this.rotationAngle));
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
