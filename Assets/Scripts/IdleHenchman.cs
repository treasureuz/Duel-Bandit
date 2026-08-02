using UnityEngine;

public class IdleHenchman : Henchman {
    [Header("IdleHenchman")]
    [SerializeField] private Transform _midMovePoint;

    private bool _hasReachedIdlePoint;

    protected override void HandleStateSwitch() {
        base.HandleStateSwitch();
        switch (this.currentState) {
            case HenchmanState.Idle: IdleState(); break;
        }
    }

    private void IdleState() {
        if (this._hasReachedIdlePoint) return;

        if (!this.isMovingToDesiredPoint) {
            this.elapsedDesiredTime += Time.fixedDeltaTime;
            if (this.elapsedDesiredTime < this.waitTimeUntilDesired) return;

            // Pick Idle movePoint: MidMovePoint
            this.currTargetPoint = this._midMovePoint;

            this.elapsedDesiredTime = 0f; // Reset nextIdleTime
            this.isMovingToDesiredPoint = true; // Starts moving next frame
            return;
        }
        MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
        // Stop moving if reached currTargetPoint
        if (!(Mathf.Abs(this.rb2d.position.x - this.currTargetPoint.
                position.x) < 0.1f)) return;
        this.isMovingToDesiredPoint = false;
        this._hasReachedIdlePoint = true;
    }

    protected override void ResetDesiredState() {
        this.isMovingToDesiredPoint = false;
        this._hasReachedIdlePoint = false;
        this.elapsedDesiredTime = 0f;
    }
}
