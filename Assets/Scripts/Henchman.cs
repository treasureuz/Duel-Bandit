using System;
using UnityEngine;

public class Henchman : Character<HGunManager> {
    [Header("References")]
    [SerializeField] protected LayerMask raycastLayerMask;
    [SerializeField] protected Transform leftMovePoint;
    [SerializeField] protected Transform rightMovePoint;

    [Header("Settings")]
    [SerializeField] protected float timeUntilSearch = 1f;
    [SerializeField] protected float timeUntilSearchEnd = 3.16f;
    [SerializeField] protected float viewDistance = 6.35f;

    protected Transform currTargetPoint;

    private float _elapsedTimeUntilSearchEnd;
    private float _elapsedTimeUntilSearch;

    protected bool hasSetCurrTargetPoint;
    protected bool isMovingToDesiredPoint;
    private bool _isShot;
    private bool _isMovingToSearchPoint;
    private bool _hasReachedSearchPoint;

    [Header("Desired HenchmanState (based on Henchman)")]
    [SerializeField] protected HenchmanState desiredState = HenchmanState.None;
    protected HenchmanState currentState = HenchmanState.None;

    void OnEnable() {
        PlayerManager.instance.OnPlayerDead += SetCurrentStateToNone;
    }

    void OnDisable() {
        PlayerManager.instance.OnPlayerDead -= SetCurrentStateToNone;
    }

    protected virtual void FixedUpdate() {
        if (!PlayerManager.instance.GetPlayer()) return;
        var playerInLOS = IsPlayerInLOS();
        if (playerInLOS || ShouldSwitchToSearchOnShot()) {
            if (this.currentState == this.desiredState) { // Previous state check
                // Could've accumulated waitTime before activating the to be AttackState
                ResetDesiredState(); // Therefore, reset main state
            } else if (this.currentState == HenchmanState.Search && playerInLOS) {
                // If Henchman was in Search and now Player is in LOS:
                // If Henchman had not reached targetPoint, reset the time would need to
                // wait before moving towards the targetPoint. Otherwise,
                // If Henchman had reached its targetPoint, reset the time it would need to
                // accumulate until SearchState deactivates.
                if (!this._hasReachedSearchPoint) this._elapsedTimeUntilSearch = 0f;
                else this._elapsedTimeUntilSearchEnd = 0f;
            }
            this.currentState = playerInLOS ? HenchmanState.Attack : HenchmanState.Search;
        } else {
            // Player not in LOS
            if (IsInAggroState) { // Previous state check -- In Attack/SearchState
                // Switches state to Search if previous state was Attack
                // Or if Henchman is still in SearchState (previous/current state
                // was Search and elapsedTimeUntilSearchEnd hasn't finished)
                if (this.currentState == HenchmanState.Attack || IsInSearchState()) {
                    this.currentState = HenchmanState.Search;
                    HandleStateSwitch(); // Continues the moving/timer until SearchEnd
                    return;
                }
                ResetSearchState(); // Reset if previous state was Search
            }
            this.currentState = this.desiredState;
        }
        HandleStateSwitch(); // Applies the CurrentState
    }

    protected override void HandleLocalScale() {
        Vector3 localScale = this.transform.localScale;
        // If negative, the targetPos is to the left, otherwise right
        // Therefore, flip this Henchman to that dirX
        if (!IsAttacking) {
            var dirXToTargetPoint = this.currTargetPoint.position.x - this.rb2d.position.x;
            localScale.x = dirXToTargetPoint <= 0f ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
        } else {
            var dirXToPlayer = GetDirToPlayer().x;
            localScale.x = dirXToPlayer <= 0f ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
        }
        this.transform.localScale = localScale;
    }

    protected virtual void HandleStateSwitch() {
        switch (this.currentState) {
            case HenchmanState.Search: SearchState(); break;
            case HenchmanState.Attack: AttackState(); break;
        }
    }
    protected void SearchState() {
        Debug.Log("In Search");
        if (this._hasReachedSearchPoint) {
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
            // Moves to the closest movePoint to the Player
            Player player = PlayerManager.instance.GetPlayer();
            if (!this._isMovingToSearchPoint) {
                // Pick closest movePoint to the Player to get a good view for shooting
                var playerToLeftMP = Vector2.Distance(this.leftMovePoint.position,
                    player.transform.position);
                var playerToRightMP = Vector2.Distance(this.rightMovePoint.position,
                    player.transform.position);
                this.currTargetPoint = playerToLeftMP < playerToRightMP ?
                    this.leftMovePoint : this.rightMovePoint;
                // Starts moving to currTargetPoint next frame
                this._isMovingToSearchPoint = true;
            } else {
                MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
                // Stop moving if reached currTargetPoint
                var hasReachedTargetPoint = HasReachedTargetPoint();
                if (!hasReachedTargetPoint) return;
                this._isMovingToSearchPoint = false;
                this._hasReachedSearchPoint = true;
            }
        }
    }
    protected void AttackState() {
        HandleLocalScale(); // Specifically flips localScale to face the Player
    }

    protected void ResetSearchState() {
        this._isMovingToSearchPoint = false;
        this._hasReachedSearchPoint = false;
        this._elapsedTimeUntilSearchEnd = 0f;
        this._elapsedTimeUntilSearch = 0f;
        this._isShot = false;
    }
    // Same for PatrolHenchman
    protected virtual void ResetDesiredState() {
        this.isMovingToDesiredPoint = true; // Instantly moves when desiredState activated
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
        var isPlayerWithinFOV = angleToPlayer <= (this.FOV / 2f);
        if (!isPlayerWithinFOV) return false;

        RaycastHit2D hit = Physics2D.Raycast(this.rb2d.position, dirToPlayer,
            viewDistance, this.raycastLayerMask);
        return hit && hit.collider.gameObject.CompareTag("Player");
    }

    private Vector2 GetDirToPlayer() {
        Player player = PlayerManager.instance.GetPlayer();
        return ((Vector2) player.transform.position - this.rb2d.position).normalized;
    }

    protected override void TakeDamage(float amount) {
        base.TakeDamage(amount);
        if (this.currentHealth == 0f) Destroy(this.gameObject);
    }

    private void SetCurrentStateToNone(object sender, EventArgs e) {
        this.currentState = HenchmanState.None;
    }

    private bool ShouldSwitchToSearchOnShot() {
        // If Henchman is/was shot at and isn't at the targetPoint
        return this._isShot && !HasReachedTargetPoint();
    }

    protected bool HasReachedTargetPoint() {
        var distXToTargetPoint = Mathf.Abs(
            this.rb2d.position.x - this.currTargetPoint.position.x);
        return distXToTargetPoint < 0.1f;
    }

    public void OnCollisionEnter2D(Collision2D collision) {
        GameObject colObj = collision.gameObject;
        if (colObj.CompareTag("PlayerBullet")) {
            this._isShot = true;
            BulletBehavior bullet = colObj.GetComponent<BulletBehavior>();
            TakeDamage(bullet.GetDamage());
        }
    }

    public bool IsAttacking => this.currentState is HenchmanState.Attack;
    public bool IsInDesiredState => this.currentState == this.desiredState;
    private bool IsInAggroState =>
        this.currentState is HenchmanState.Attack or HenchmanState.Search;
    private bool IsInSearchState() {
        return this.currentState is HenchmanState.Search &&
            this._elapsedTimeUntilSearchEnd < this.timeUntilSearchEnd;
    }
    public HenchmanState GetCurrentState() => this.currentState;
}
