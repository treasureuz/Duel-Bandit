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

        if (!this.hasSetCurrTargetPoint) { // Removes the redundancy of setting it every frame
            // Sets to Idle movePoint: MidMovePoint
            this.currTargetPoint = this._midMovePoint;
            this.hasSetCurrTargetPoint = true;
        }
        if (!this.isMovingToDesiredPoint) return;
        MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
        // Stop moving if reached currTargetPoint
        var hasReachedTargetPoint = HasReachedTargetPoint();
        if (!hasReachedTargetPoint) return;
        this.isMovingToDesiredPoint = false;
        this._hasReachedIdlePoint = true;
    }

    protected override void ResetDesiredState() {
        base.ResetDesiredState();
        this._hasReachedIdlePoint = false;
    }
}
