using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Henchman : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Transform _leftMovePoint;
    [SerializeField] private Transform _midMovePoint;
    [SerializeField] private Transform _rightMovePoint;
    [SerializeField] private LayerMask _playerLayerMask;

    [Header("Settings")]
    [SerializeField] private float _moveSpeed = 2.67f;
    [SerializeField] private float _waitTimeUntilPatrol = 3.46f;
    [SerializeField] private float _waitTimeUntilIdle = 3f;
    [SerializeField] private float _raycastDist = 2.5f;
    [SerializeField] private float _raycastHeight = 3.21f;

    private Rigidbody2D _rb2d;
    private Transform _currTargetPoint;

    private float _nextIdleTime;
    private float _nextPatrolTime;
    private bool _isPatrolling;
    private bool _isMovingToIdlePoint;
    private bool _hasReachedIdlePoint;
    private bool _isMovingToAttackPoint;
    private bool _hasReachedAttackPoint;

    [Header("Desired HenchmanState (based on HenchmanType)")]
    [SerializeField] private HenchmanState _desiredState = HenchmanState.Idle;
    private HenchmanState _currentState;

    void Awake() {
        this._rb2d = this.GetComponent<Rigidbody2D>();
    }

    void FixedUpdate() {
        RaycastHit2D hit = Physics2D.BoxCast(this.transform.position,
            new Vector2(0.1f, this._raycastHeight), 0f, Vector2.left,
                this._raycastDist, this._playerLayerMask);
        this._currentState = hit ? HenchmanState.Attack : this._desiredState;
        HandleStateSwitch();
    }

    private void HandleStateSwitch() {
        switch (this._currentState) {
            case HenchmanState.Idle: IdleState(); break;
            case HenchmanState.Patrol: PatrolState(); break;
            case HenchmanState.Attack: AttackState(); break;
        }
    }

    #region StateSwitch Methods/Helpers
    private void IdleState() {
        if (this._hasReachedIdlePoint) return;

        if (this._isMovingToIdlePoint) {
            MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
            // Stop moving if reached currTargetPoint
            if (!(Vector2.Distance(this._rb2d.position,
                    this._currTargetPoint.position) < 0.1f)) return;
            this._isMovingToIdlePoint = false;
            this._hasReachedIdlePoint = true;
            return;
        }
        this._nextIdleTime += Time.fixedDeltaTime;
        if (this._nextIdleTime < this._waitTimeUntilIdle) return;

        // Pick Idle movePoint: MidMovePoint
        this._currTargetPoint = this._midMovePoint;

        this._nextIdleTime = 0f; // Reset nextIdleTime
        this._isMovingToIdlePoint = true; // Starts moving next frame
    }

    private void PatrolState() {
        if (this._isPatrolling) {
            MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
            // Stop moving if reached currTargetPoint
            if (Vector2.Distance(this._rb2d.position,
                    this._currTargetPoint.position) < 0.1f)
                this._isPatrolling = false;
            return;
        }
        // Not patrolling, wait until it's time to patrol
        this._nextPatrolTime += Time.fixedDeltaTime;
        if (this._nextPatrolTime < this._waitTimeUntilPatrol) return;

        // Decide next movePoint
        if (!this._currTargetPoint) {
            List<Transform> movePoints = new() { this._leftMovePoint, this._rightMovePoint };
            this._currTargetPoint = movePoints[Random.Range(0, movePoints.Count)];
        } else if (this._currTargetPoint == this._leftMovePoint) {
            this._currTargetPoint = this._rightMovePoint;
        } else this._currTargetPoint = this._leftMovePoint;

        this._nextPatrolTime = 0f; // Reset nextPatrolTime
        this._isPatrolling = true; // Starts patrolling next frame
    }

    private void AttackState() {
        if (this._hasReachedAttackPoint) return;
        // Could've accumulated wait time before this state was activated
        DisablePatrolState(); DisableIdleState();

        if (this._isMovingToAttackPoint) {
            MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
            // Stop moving if reached currTargetPoint
            if (!(Vector2.Distance(this._rb2d.position,
                    this._currTargetPoint.position) < 0.1f)) return;
            this._isMovingToAttackPoint = false;
            this._hasReachedAttackPoint = true;
            return;
        }
        // Pick closest movePoint to the Player to get a good view for shooting
        var playerToLeftMP = Vector2.Distance(this._leftMovePoint.position,
            Player.instance.transform.position);
        var playerToRightMP = Vector2.Distance(this._rightMovePoint.position,
            Player.instance.transform.position);
        this._currTargetPoint = playerToLeftMP < playerToRightMP ?
            this._leftMovePoint : this._rightMovePoint;

        this._isMovingToAttackPoint = true; // Starts moving next frame
    }

    //TODO: Add DisableDesiredState method in replacement of below
    private void DisablePatrolState() {
        this._isPatrolling = false;
        this._nextPatrolTime = 0f;
    }

    private void DisableIdleState() {
        this._isMovingToIdlePoint = false;
        this._hasReachedIdlePoint = false;
        this._nextIdleTime = 0f;
    }

    private void DisableAttackState() {
        this._isMovingToAttackPoint = false;
        this._hasReachedAttackPoint = false;
    }

    private void MoveTowardsTargetPoint() {
        Vector2 position = Vector2.MoveTowards(this._rb2d.position,
            this._currTargetPoint.position, this._moveSpeed * Time.fixedDeltaTime);
        this._rb2d.MovePosition(position);
    }
    #endregion

    public HenchmanState GetCurrentState() => this._currentState;
}
