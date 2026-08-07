using System.Collections.Generic;
using UnityEngine;

public class PatrolHenchman : Henchman {
    [Header("PatrolHenchman")]
    [SerializeField] private float _timeUntilPatrol = 2.62f;

    private float _elapsedTimeUntilPatrol;

    protected override void HandleStateSwitch() {
        base.HandleStateSwitch();
        switch (this.CurrentState) {
            case HenchmanState.Patrol: PatrolState(); break;
        }
    }

    private void PatrolState() {
        if (!hasSetCurrDesiredPoint) {
            // Decide next movePoint
            if (!this.currTargetPoint) {
                List<Transform> movePoints = new() { this.leftMovePoint, this.rightMovePoint };
                this.currTargetPoint = movePoints[Random.Range(0, movePoints.Count)];
            } else if (this.currTargetPoint == this.leftMovePoint) {
                this.currTargetPoint = this.rightMovePoint;
            } else this.currTargetPoint = this.leftMovePoint;
            hasSetCurrDesiredPoint = true;
            this.isMovingToDesiredPoint = true;
        }

        if (this.isMovingToDesiredPoint) {
            MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
            // Stop moving if reached currTargetPoint
            var hasReachedTargetPoint = HasReachedCurrTargetPoint();
            if (hasReachedTargetPoint) this.isMovingToDesiredPoint = false;
        } else {
            // Wait until it's time to patrol
            if (this._elapsedTimeUntilPatrol < this._timeUntilPatrol) {
                this._elapsedTimeUntilPatrol += Time.fixedDeltaTime;
                return;
            }
            this._elapsedTimeUntilPatrol = 0f; // Reset nextPatrolTime
            hasSetCurrDesiredPoint = false;
        }
    }

    protected override void ResetDesiredState() {
        base.ResetDesiredState();
        this._elapsedTimeUntilPatrol = 0f;
    }
}
