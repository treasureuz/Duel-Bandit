using UnityEngine;

public abstract class Henchman : Character {
    [Header("References")]
    [SerializeField] protected LayerMask raycastLayerMask;
    [SerializeField] protected Transform leftMovePoint;
    [SerializeField] protected Transform rightMovePoint;

    [Header("Settings")]
    [SerializeField] protected float waitTimeUntilDesired = 2f;
    [SerializeField] protected float searchStateDuration;
    [SerializeField] protected float minDistFromPlayer = 4.843f;
    [SerializeField] protected float raycastDistance = 7.35f;

    protected Rigidbody2D rb2d;
    protected HWeaponManager weaponManager;
    protected Transform currTargetPoint;

    protected float elapsedDesiredTime;
    private float _elapsedSearchDur;

    private bool _isShot;
    protected bool isMovingToDesiredPoint;
    private bool _isMovingToAggroPoint;
    private bool _hasReachedAggroPoint;

    [Header("Desired HenchmanState (based on Henchman)")]
    [SerializeField] protected HenchmanState desiredState = HenchmanState.None;
    protected HenchmanState currentState = HenchmanState.None;

    protected virtual void Awake() {
        this.rb2d = this.GetComponent<Rigidbody2D>();
        this.weaponManager = this.GetComponentInChildren<HWeaponManager>();
    }

    protected virtual void FixedUpdate() {
        if (IsPlayerInLOS() || this._isShot) {
            if (this.currentState == this.desiredState) { // Previous state check
                // Could've accumulated waitTime before activating the to be AttackState
                ResetDesiredState(); // Therefore, reset main state
            }
            this.currentState = IsPlayerInLOS() ? HenchmanState.Attack : HenchmanState.Search;
        } else {
            // Previous state check
            if (this.currentState is HenchmanState.Attack or HenchmanState.Search)
                ResetAggroState(); // Reset if previous state was AttackState or SearchState
            this.currentState = this.desiredState;
        }
        HandleStateSwitch();
    }

    protected virtual void HandleStateSwitch() {
        switch (this.currentState) {
            case HenchmanState.Search: SearchState(); break;
            case HenchmanState.Attack: AttackState(); break;
        }
    }
    // Henchman in SearchState should not know/care of its distance to player
    protected void SearchState() {
        if (this._hasReachedAggroPoint) {
            this._elapsedSearchDur += Time.fixedDeltaTime;
            if (this._elapsedSearchDur < this.searchStateDuration) return;
            this._isShot = false;
            return;
        }
        MoveToClosestPointToPlayer(); // Sets flags to true/false once reached
    }
    // Henchman in AttackState knows/cares of its distance to player
    protected void AttackState() {
        var distFromPlayer = Mathf.Abs(this.rb2d.position.x - Player.instance.transform.position.x);
        var isPlayerWithinRange = distFromPlayer <= this.minDistFromPlayer;
        // Stops moving if Henchman is at an appropriate distance away from Player
        if (this._hasReachedAggroPoint || isPlayerWithinRange) return;
        MoveToClosestPointToPlayer(); // Sets flags to true/false once reached
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
            return;
        }
        MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
        // Stop moving if reached currTargetPoint
        if (!(Mathf.Abs(this.rb2d.position.x - this.currTargetPoint.
                position.x) < 0.1f)) return;
        this._isMovingToAggroPoint = false;
        this._hasReachedAggroPoint = true;
    }

    protected void ResetAggroState() {
        this._isMovingToAggroPoint = false;
        this._hasReachedAggroPoint = false;
    }
    protected abstract void ResetDesiredState();

    protected void MoveTowardsTargetPoint() {
        // If negative, the currTargetPoint is to the left, otherwise right
        // Therefore, flip this Henchman to that dirX
        var dirXToTargetPoint =  this.currTargetPoint.position.x - this.rb2d.position.x;
        Vector3 localScale = this.transform.localScale;
        localScale.x = dirXToTargetPoint < 0f ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
        this.transform.localScale = localScale;

        // Starts the actual moving
        var posX = Mathf.MoveTowards(this.rb2d.position.x,
            this.currTargetPoint.position.x, this.moveSpeed * Time.fixedDeltaTime);
        Vector2 position = new(posX, this.rb2d.position.y);
        this.rb2d.MovePosition(position);
    }

    protected bool IsPlayerInLOS() {
        Vector2 weaponPos = this.weaponManager.transform.position;
        Vector2 dirToPlayer = ((Vector2)Player.instance.transform.position - weaponPos).normalized;
        Vector2 facingDir = this.transform.lossyScale.x < 0f ? Vector2.left : Vector2.right;
        // Calculates the angle from where this obj is facing to the direction to the player
        // "Vector2.Angle" = 0 to 180 (no negatives) 60 degs above == 60 degs below
        var angleToPlayer = Vector2.Angle(facingDir, dirToPlayer);
        // If the rotation towards the player calculated is within "FOV" deg
        var isPlayerWithinFOV = angleToPlayer <= (FOV / 2f);
        if (!isPlayerWithinFOV) return false;
        RaycastHit2D hit = Physics2D.Raycast(weaponPos, dirToPlayer,
            raycastDistance, this.raycastLayerMask);
        return hit && hit.collider.gameObject.CompareTag("Player");
    }

    public void OnCollisionEnter2D(Collision2D col) {
        if (col.collider.CompareTag("PlayerBullet")) {
            this._isShot = true;
        }
    }

    public bool IsWaitingUntilDesired() => this.elapsedDesiredTime < this.waitTimeUntilDesired;
    public HenchmanState GetCurrentState() => this.currentState;
}
