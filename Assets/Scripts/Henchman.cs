using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Henchman : MonoBehaviour {
    [Header("References")]
    //[SerializeField] private List<Transform> _movePoints;
    [SerializeField] private Transform _leftMovePoint;
    [SerializeField] private Transform _midMovePoint;
    [SerializeField] private Transform _rightMovePoint;
    [SerializeField] private LayerMask _playerLayerMask;

    [Header("Settings")]
    [SerializeField] private float _moveSpeed = 2.67f;
    [SerializeField] private float _timeBetweenPatrol = 3f;
    [SerializeField] private float _raycastDist = 2.5f;
    [SerializeField] private float _raycastHeight = 3.21f;

    private Rigidbody2D _rb2d;
    private Transform _currTargetPoint;

    private float _nextPatrolTime;
    private bool _isMoving;

    public enum HenchmanState {
        Idle,
        Patrol,
        Attack
    }
    [SerializeField] private HenchmanState _state = HenchmanState.Patrol;

    void Awake() {
        this._rb2d = this.GetComponent<Rigidbody2D>();
    }

    void FixedUpdate() {
        RaycastHit2D hit = Physics2D.BoxCast(this.transform.position,
            new Vector2(0.1f, this._raycastHeight), 0f, Vector2.left,
                this._raycastDist, this._playerLayerMask);
        this._state = hit ? HenchmanState.Attack : HenchmanState.Patrol;
        HandleStateSwitch();
    }

    private void HandleStateSwitch() {
        switch (this._state) {
            case HenchmanState.Idle: break;
            case HenchmanState.Patrol: {
                if (this._isMoving) {
                    // Start patrolling towards currentTargetPoint
                    Vector2 position = Vector2.MoveTowards(this._rb2d.position,
                        this._currTargetPoint.position, this._moveSpeed * Time.fixedDeltaTime);
                    this._rb2d.MovePosition(position);
                    // Stop moving if reached currTargetPoint
                    if (Vector2.Distance(this._rb2d.position, this._currTargetPoint.position) < 0.05f)
                        this._isMoving = false;
                    break;
                }
                // Not patrolling, wait until time to patrol
                this._nextPatrolTime += Time.fixedDeltaTime;
                if (this._nextPatrolTime < this._timeBetweenPatrol) break;

                // Pick next movePoint
                if (this._currTargetPoint == this._leftMovePoint) {
                    this._currTargetPoint = this._rightMovePoint;
                } else if (this._currTargetPoint == this._rightMovePoint) {
                    this._currTargetPoint = this._leftMovePoint;
                } else {
                    List<Transform> movePoints = new() { this._leftMovePoint, this._rightMovePoint };
                    this._currTargetPoint = movePoints[Random.Range(0, movePoints.Count)];
                }
                this._isMoving = true;
                break;
            }
            case HenchmanState.Attack: {
                if (this._isMoving) {
                    Vector2 position = Vector2.MoveTowards(this._rb2d.position,
                        this._currTargetPoint.position, this._moveSpeed * Time.fixedDeltaTime);
                    this._rb2d.MovePosition(position);
                    // Stop moving if reached currTargetPoint
                    if (Vector2.Distance(this._rb2d.position, this._currTargetPoint.position) < 0.05f)
                        this._isMoving = false;
                    break;
                }
                // Not moving, pick to closest movePoint to the Player
                // to get a good view for shooting
                var playerToLeftMP = Vector2.Distance(this._leftMovePoint.position,
                    Player.instance.transform.position);
                var playerToRightMP = Vector2.Distance(this._rightMovePoint.position,
                    Player.instance.transform.position);
                this._currTargetPoint = playerToLeftMP < playerToRightMP ?
                    this._leftMovePoint : this._rightMovePoint;
                this._isMoving = true;
                break;
            }
        }
    }

    public HenchmanState GetCurrentState() => this._state;
}
