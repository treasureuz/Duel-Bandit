using UnityEngine;

public class IdleHenchman : Henchman {
    [Header("IdleHenchman")]
    [SerializeField] private Transform _midMovePoint;

    private bool _hasReachedIdlePoint;

    protected override void HandleStateSwitch() {
        base.HandleStateSwitch();
        switch (this.CurrentState) {
            case HenchmanState.Idle: IdleState(); break;
        }
    }

    private void IdleState() {
        if (this._hasReachedIdlePoint) return;
        if (isSoraEffectEnabled) DisableSoraSlowEffect();

        if (!this.hasSetCurrDesiredPoint) { // Removes the redundancy of setting it every frame
            // Sets to Idle movePoint: MidMovePoint
            this.currTargetPoint = this._midMovePoint;
            this.hasSetCurrDesiredPoint = true;
            this.isMovingToDesiredPoint = true;
        }
        if (!this.isMovingToDesiredPoint) return;
        MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
        // Stop moving if reached currTargetPoint
        var hasReachedTargetPoint = HasReachedCurrTargetPoint();
        if (!hasReachedTargetPoint) return;
        this.isMovingToDesiredPoint = false;
        this._hasReachedIdlePoint = true;
    }

    protected override void ResetDesiredState() {
        base.ResetDesiredState();
        this._hasReachedIdlePoint = false;
    }
}
