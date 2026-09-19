using UnityEngine;

public class IdleHenchman : Henchman {
    [Header("IdleHenchman")]
    [SerializeField] private Transform _midMovePoint;

    private bool _hasReachedIdlePoint;
    private Vector2 _prevFacingDir;

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
            this._prevFacingDir = this.currTargetPoint ?
                GetDirToCurrTarget() : GetFacingDirection();
            // Sets to Idle movePoint: MidMovePoint
            this.currTargetPoint = this._midMovePoint;
            this.hasSetCurrDesiredPoint = true;
            this.isMovingToDesiredPoint = true;
        }

        if (HasReachedCurrTargetPoint()) {
            this.isMovingToDesiredPoint = false;
            this._hasReachedIdlePoint = true;
            // Flips localScale to always face prevTargetPoint when reached idlePoint
            HandleLocalScale(this._prevFacingDir);
        } else if (this.isMovingToDesiredPoint) {
            MoveTowardsCurrTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
        }
    }

    protected override void ResetDesiredState() {
        base.ResetDesiredState();
        this._hasReachedIdlePoint = false;
    }
}
