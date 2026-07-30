using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Henchman : MonoBehaviour {
    [Header("References")]
    [SerializeField] protected Transform leftMovePoint;
    [SerializeField] protected Transform rightMovePoint;

    [Header("Settings")]
    [SerializeField] protected float moveSpeed = 2.67f;
    [SerializeField] protected float maxDistFromPlayer = 4.843f;
    [SerializeField] protected float raycastDistance = 8.35f;
    [SerializeField] protected float raycastHeight = 3.21f;

    protected Rigidbody2D rb2d;
    protected Transform currTargetPoint;

    private bool _isMovingToAttackPoint;
    private bool _hasReachedAttackPoint;

    [Header("Desired HenchmanState (based on Henchman)")]
    [SerializeField] protected HenchmanState desiredState = HenchmanState.None;
    protected HenchmanState currentState;

    protected virtual void Awake() {
        this.rb2d = this.GetComponent<Rigidbody2D>();
    }

    protected virtual void FixedUpdate() {
        var playerLayerMask = 1 << Player.instance.gameObject.layer;
        RaycastHit2D hit = Physics2D.BoxCast(this.transform.position,
            new Vector2(0.1f, this.raycastHeight), 0f, Vector2.left,
                this.raycastDistance, playerLayerMask);
        if (hit) {
            if (this.currentState == this.desiredState) { // Previous state
                // Could've accumulated waitTime before activating to be AttackState
                ResetDesiredState(); // Therefore, reset main state
            }
            this.currentState = HenchmanState.Attack;
        } else {
            if (this.currentState == HenchmanState.Attack) // Previous state
                ResetAttackState(); // Reset if previous state was AttackState
            this.currentState = this.desiredState;
        }
        HandleStateSwitch();
    }

    protected virtual void HandleStateSwitch() {
        switch (this.currentState) {
            case HenchmanState.Attack: AttackState(); break;
        }
    }

    #region StateSwitch Methods/Helpers
    protected void AttackState() {
        var distFromPlayer = Mathf.Abs(this.rb2d.position.x - Player.instance.transform.position.x);
        // Stops moving if Henchman is at an appropriate distance away from Player
        if (this._hasReachedAttackPoint || distFromPlayer <= this.maxDistFromPlayer) return;

        if (this._isMovingToAttackPoint) {
            MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
            // Stop moving if reached currTargetPoint
            if (!(Mathf.Abs(this.rb2d.position.x - this.currTargetPoint.
                    position.x) < 0.1f)) return;
            this._isMovingToAttackPoint = false;
            this._hasReachedAttackPoint = true;
            return;
        }
        // Pick closest movePoint to the Player to get a good view for shooting
        var playerToLeftMP = Vector2.Distance(this.leftMovePoint.position,
            Player.instance.transform.position);
        var playerToRightMP = Vector2.Distance(this.rightMovePoint.position,
            Player.instance.transform.position);
        this.currTargetPoint = playerToLeftMP < playerToRightMP ?
            this.leftMovePoint : this.rightMovePoint;

        this._isMovingToAttackPoint = true; // Starts moving next frame
    }

    protected void ResetAttackState() {
        this._isMovingToAttackPoint = false;
    }
    protected abstract void ResetDesiredState();

    protected void MoveTowardsTargetPoint() {
        var posX = Mathf.MoveTowards(this.rb2d.position.x,
            this.currTargetPoint.position.x, this.moveSpeed * Time.fixedDeltaTime);
        Vector2 position = new(posX, this.rb2d.position.y);
        this.rb2d.MovePosition(position);
    }
    #endregion

    public HenchmanState GetCurrentState() => this.currentState;
    public float GetRaycastDistance() => this.raycastDistance;
}
