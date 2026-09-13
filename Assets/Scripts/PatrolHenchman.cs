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
        if (isSoraEffectEnabled) DisableSoraSlowEffect();
        if (!this.hasSetCurrDesiredPoint) {
            if (!this.currTargetPoint) {
                List<Transform> movePoints = new() { this.leftMovePoint, this.rightMovePoint };
                this.currTargetPoint = movePoints[Random.Range(0, movePoints.Count)]; 
            } else if (HasReachedCurrTargetPoint()) { // If false, it moves to the currTargetPoint
                // *Fixes the bug that changes the currTargetPoint again when Henchman kills Player, 
                // meaning Player was in LOS, and its prev state was desired (PlayerInLOS resets prev state)* 
                // Decide next movePoint
                if (this.currTargetPoint == this.leftMovePoint) {
                    this.currTargetPoint = this.rightMovePoint;
                } else this.currTargetPoint = this.leftMovePoint;
            }
            this.hasSetCurrDesiredPoint = true;
            this.isMovingToDesiredPoint = true;
        }

        if (this.isMovingToDesiredPoint) {
            MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
            // Stop moving if reached currTargetPoint
            if (HasReachedCurrTargetPoint()) this.isMovingToDesiredPoint = false;
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
