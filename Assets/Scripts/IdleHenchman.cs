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
            if (this.elapsedTimeUntilDesired < this.timeUntilDesired) {
                this.elapsedTimeUntilDesired += Time.fixedDeltaTime;
                return;
            };

            // Pick Idle movePoint: MidMovePoint
            this.currTargetPoint = this._midMovePoint;

            this.elapsedTimeUntilDesired = 0f; // Reset nextIdleTime
            this.isMovingToDesiredPoint = true; // Starts moving next frame
        } else {
            MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
            // Stop moving if reached currTargetPoint
            if (!(Mathf.Abs(this.rb2d.position.x - this.currTargetPoint.
                    position.x) < 0.1f)) return;
            this.isMovingToDesiredPoint = false;
            this._hasReachedIdlePoint = true;
        }
    }

    protected override void ResetDesiredState() {
        base.ResetDesiredState();
        this._hasReachedIdlePoint = false;
    }
}
