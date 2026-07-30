using UnityEngine;

public class IdleHenchman : Henchman {
    [Header("IdleHenchman")]
    [SerializeField] private Transform _midMovePoint;
    [SerializeField] private float _waitTimeUntilIdle = 3f;

    private float _nextIdleTime;

    private bool _isMovingToIdlePoint;
    private bool _hasReachedIdlePoint;

    protected override void HandleStateSwitch() {
        base.HandleStateSwitch();
        switch (this.currentState) {
            case HenchmanState.Idle: IdleState(); break;
        }
    }

    private void IdleState() {
        if (this._hasReachedIdlePoint) return;

        if (this._isMovingToIdlePoint) {
            MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
            // Stop moving if reached currTargetPoint
            if (!(Mathf.Abs(this.rb2d.position.x - this.currTargetPoint.
                    position.x) < 0.1f)) return;
            this._isMovingToIdlePoint = false;
            this._hasReachedIdlePoint = true;
            return;
        }
        this._nextIdleTime += Time.fixedDeltaTime;
        if (this._nextIdleTime < this._waitTimeUntilIdle) return;

        // Pick Idle movePoint: MidMovePoint
        this.currTargetPoint = this._midMovePoint;

        this._nextIdleTime = 0f; // Reset nextIdleTime
        this._isMovingToIdlePoint = true; // Starts moving next frame
    }

    protected override void ResetDesiredState() {
        this._isMovingToIdlePoint = false;
        this._hasReachedIdlePoint = false;
        this._nextIdleTime = 0f;
    }
}
