using System.Collections.Generic;
using UnityEngine;

public class PatrolHenchman : Henchman {
    [Header("PatrolHenchman")]
    [SerializeField] private float _waitTimeUntilPatrol = 3.46f;

    private float _nextPatrolTime;
    private bool _isPatrolling;

    protected override void HandleStateSwitch() {
        base.HandleStateSwitch();
        switch (this.currentState) {
            case HenchmanState.Patrol: PatrolState(); break;
        }
    }

    private void PatrolState() {
        if (this._isPatrolling) {
            MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
            // Stop moving if reached currTargetPoint
            if (Mathf.Abs(this.rb2d.position.x - this.currTargetPoint.position.x) < 0.1f)
                this._isPatrolling = false;
            return;
        }
        // Not patrolling, wait until it's time to patrol
        this._nextPatrolTime += Time.fixedDeltaTime;
        if (this._nextPatrolTime < this._waitTimeUntilPatrol) return;

        // Decide next movePoint
        if (!this.currTargetPoint) {
            List<Transform> movePoints = new() { this.leftMovePoint, this.rightMovePoint };
            this.currTargetPoint = movePoints[Random.Range(0, movePoints.Count)];
        } else if (this.currTargetPoint == this.leftMovePoint) {
            this.currTargetPoint = this.rightMovePoint;
        } else this.currTargetPoint = this.leftMovePoint;

        this._nextPatrolTime = 0f; // Reset nextPatrolTime
        this._isPatrolling = true; // Starts patrolling next frame
    }

    protected override void ResetDesiredState() {
        this._isPatrolling = false;
        this._nextPatrolTime = 0f;
    }
}
