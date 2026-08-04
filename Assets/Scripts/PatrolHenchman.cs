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
            // Wait until it's time to patrol
            if (this.elapsedTimeUntilDesired < this.timeUntilDesired) {
                this.elapsedTimeUntilDesired += Time.fixedDeltaTime;
                return;
            }

            // Decide next movePoint
            if (!this.currTargetPoint) {
                List<Transform> movePoints = new() { this.leftMovePoint, this.rightMovePoint };
                this.currTargetPoint = movePoints[Random.Range(0, movePoints.Count)];
            } else if (this.currTargetPoint == this.leftMovePoint) {
                this.currTargetPoint = this.rightMovePoint;
            } else this.currTargetPoint = this.leftMovePoint;

            this.elapsedTimeUntilDesired = 0f; // Reset nextPatrolTime
            this.isMovingToDesiredPoint = true; // Starts patrolling next frame
        } else {
            MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
            // Stop moving if reached currTargetPoint
            if (Mathf.Abs(this.rb2d.position.x - this.currTargetPoint.position.x) < 0.1f)
                this.isMovingToDesiredPoint = false;
        }

    }
}
