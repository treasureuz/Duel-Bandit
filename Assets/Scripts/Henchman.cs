using UnityEngine;

public class Henchman : Character<HWeaponManager> {
    [Header("References")]
    [SerializeField] protected LayerMask raycastLayerMask;
    [SerializeField] protected Transform leftMovePoint;
    [SerializeField] protected Transform rightMovePoint;

    [Header("Settings")]
    [SerializeField] protected float timeUntilDesired = 2f;
    [SerializeField] protected float timeUntilSearch = 1f;
    [SerializeField] protected float timeUntilSearchEnd = 3.16f;
    [SerializeField] protected float viewDistance = 6.35f;

    protected Transform currTargetPoint;

    protected float elapsedTimeUntilDesired;
    private float _elapsedTimeUntilSearchEnd;
    private float _elapsedTimeUntilSearch;

    private bool _isShot;
    protected bool isMovingToDesiredPoint;
    private bool _isMovingToAggroPoint;
    private bool _hasReachedAggroPoint;

    [Header("Desired HenchmanState (based on Henchman)")]
    [SerializeField] protected HenchmanState desiredState = HenchmanState.None;
    protected HenchmanState currentState = HenchmanState.None;

    protected virtual void FixedUpdate() {
        var playerInLOS = IsPlayerInLOS();
        if (playerInLOS || this._isShot) {
            if (this.currentState == this.desiredState) { // Previous state check
                // Could've accumulated waitTime before activating the to be AttackState
                ResetDesiredState(); // Therefore, reset main state
            } else if (this.currentState is HenchmanState.Search && playerInLOS) {
                this._elapsedTimeUntilSearch = 0f;
            }
            this.currentState = playerInLOS ? HenchmanState.Attack : HenchmanState.Search;
        } else {
            if (IsInAggroState) { // Previous state check -- In Attack/SearchState
                // Set state to Search if Henchman hadn't reached targetPoint or
                // still in SearchState (elapsedTimeUntilSearchEnd hasn't finished)
                if (!this._hasReachedAggroPoint || IsInSearchState()) {
                    this.currentState = HenchmanState.Search;
                    HandleStateSwitch();
                    return;
                }
                ResetAggroState(); // Reset if previous state was Attack or Search
            }
            this.currentState = this.desiredState;
        }
        HandleStateSwitch();
    }

    protected override void HandleLocalScale() {
        if (!IsAttacking) {
            // If negative, the currTargetPoint is to the left, otherwise right
            // Therefore, flip this Henchman to that dirX
            var dirXToTargetPoint = this.currTargetPoint.position.x - this.rb2d.position.x;
            Vector3 localScale = this.transform.localScale;
            localScale.x = dirXToTargetPoint <= 0f ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
            this.transform.localScale = localScale;
        } else FlipScaleWithDir(GetDirToPlayer()); // Uses dirToPlayer to flip localScale
    }

    protected virtual void HandleStateSwitch() {
        switch (this.currentState) {
            case HenchmanState.Search: SearchState(); break;
            case HenchmanState.Attack: AttackState(); break;
        }
    }
    protected void SearchState() {
        Debug.Log("In Search");
        if (this._hasReachedAggroPoint) {
            if (this._elapsedTimeUntilSearchEnd < this.timeUntilSearchEnd) {
                this._elapsedTimeUntilSearchEnd += Time.fixedDeltaTime;
                return;
            }
            if (this._isShot) this._isShot = false;
        } else {
            if (this._elapsedTimeUntilSearch < this.timeUntilSearch) {
                this._elapsedTimeUntilSearch += Time.fixedDeltaTime;
                return;
            }
            MoveToClosestPointToPlayer(); // Sets flags to true/false once reached
        }
    }
    protected void AttackState() {
        HandleLocalScale(); // Specifically flips localScale to face the Player
    }

    protected void ResetAggroState() {
        this._isMovingToAggroPoint = false;
        this._hasReachedAggroPoint = false;
        this._elapsedTimeUntilSearchEnd = 0f;
        this._elapsedTimeUntilSearch = 0f;
        this._isShot = false;
    }
    // Same for PatrolHenchman
    protected virtual void ResetDesiredState() {
        this.isMovingToDesiredPoint = true; // Instantly moves when desiredState activated
        this.elapsedTimeUntilDesired = 0f;
    }

    private void MoveToClosestPointToPlayer() {
        if (!this._isMovingToAggroPoint) {
            // Pick closest movePoint to the Player to get a good view for shooting
            var playerToLeftMP = Vector2.Distance(this.leftMovePoint.position,
                Player.instance.transform.position);
            var playerToRightMP = Vector2.Distance(this.rightMovePoint.position,
                Player.instance.transform.position);
            this.currTargetPoint = playerToLeftMP < playerToRightMP ?
                this.leftMovePoint : this.rightMovePoint;

            this._isMovingToAggroPoint = true; // Starts moving next frame
        } else {
            MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
            // Stop moving if reached currTargetPoint
            if (!(Mathf.Abs(this.rb2d.position.x - this.currTargetPoint.
                    position.x) < 0.1f)) return;
            this._isMovingToAggroPoint = false;
            this._hasReachedAggroPoint = true;
        }
    }
    protected void MoveTowardsTargetPoint() {
        HandleLocalScale(); // Flip towards targetPoint
        // Starts the actual moving
        var posX = Mathf.MoveTowards(this.rb2d.position.x,
            this.currTargetPoint.position.x, this.moveSpeed * Time.fixedDeltaTime);
        Vector2 position = new(posX, this.rb2d.position.y);
        this.rb2d.MovePosition(position);
    }

    protected bool IsPlayerInLOS() {
        Vector2 dirToPlayer = GetDirToPlayer();
        Vector2 facingDir = this.transform.lossyScale.x < 0f ? Vector2.left : Vector2.right;
        // Calculates the angle from where this obj is facing to the direction to the player
        // "Vector2.Angle" = 0 to 180 (no negatives) 60 degs above == 60 degs below
        var angleToPlayer = Vector2.Angle(facingDir, dirToPlayer);

        // If the rotation towards the player calculated is within FOV
        var isPlayerWithinFOV = angleToPlayer <= (FOV / 2f);
        if (!isPlayerWithinFOV) return false;

        RaycastHit2D hit = Physics2D.Raycast(this.rb2d.position, dirToPlayer,
            viewDistance, this.raycastLayerMask);
        return hit && hit.collider.gameObject.CompareTag("Player");
    }

    private Vector2 GetDirToPlayer() {
        return ((Vector2)Player.instance.transform.
            position - this.rb2d.position).normalized;
    }

    public void OnCollisionEnter2D(Collision2D col) {
        if (col.collider.CompareTag("PlayerBullet")) {
            this._isShot = true;
        }
    }

    public bool IsAttacking => this.currentState is HenchmanState.Attack;
    private bool IsInAggroState =>
        this.currentState is HenchmanState.Attack or HenchmanState.Search;
    private bool IsInSearchState() {
        return this.currentState is HenchmanState.Search &&
            this._elapsedTimeUntilSearchEnd < this.timeUntilSearchEnd;
    }
    public bool IsWaitingUntilDesired() => this.elapsedTimeUntilDesired < this.timeUntilDesired;
    public HenchmanState GetCurrentState() => this.currentState;
}
