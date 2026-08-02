using System.Collections.Generic;
using UnityEngine;

public class PatrolHenchman : Henchman {
    protected override void HandleStateSwitch() {
        base.HandleStateSwitch();
        switch (this.currentState) {
            case HenchmanState.Patrol: PatrolState(); break;
        }
    }

    private void PatrolState() {
        if (!this.isMovingToDesiredPoint) {
            // Not patrolling, wait until it's time to patrol
            this.elapsedDesiredTime += Time.fixedDeltaTime;
            if (this.elapsedDesiredTime < this.waitTimeUntilDesired) return;

            // Decide next movePoint
            if (!this.currTargetPoint) {
                List<Transform> movePoints = new() { this.leftMovePoint, this.rightMovePoint };
                this.currTargetPoint = movePoints[Random.Range(0, movePoints.Count)];
            } else if (this.currTargetPoint == this.leftMovePoint) {
                this.currTargetPoint = this.rightMovePoint;
            } else this.currTargetPoint = this.leftMovePoint;

            this.elapsedDesiredTime = 0f; // Reset nextPatrolTime
            this.isMovingToDesiredPoint = true; // Starts patrolling next frame
            return;
        }
        MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
        // Stop moving if reached currTargetPoint
        if (Mathf.Abs(this.rb2d.position.x - this.currTargetPoint.position.x) < 0.1f)
            this.isMovingToDesiredPoint = false;
    }

    protected override void ResetDesiredState() {
        this.isMovingToDesiredPoint = false;
        this.elapsedDesiredTime = 0f;
    }
}
