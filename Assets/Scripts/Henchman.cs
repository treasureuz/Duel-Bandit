using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Henchman : Character<HGunManager> {
    [Header("References")]
    [SerializeField] protected LayerMask raycastLayerMask;
    [SerializeField] protected Transform leftMovePoint;
    [SerializeField] protected Transform rightMovePoint;

    [Header("Settings")]
    [SerializeField] protected int FOV = 180; // 90 degrees upward/downward this obj
    [SerializeField] protected float timeUntilSearch = 1f;
    [SerializeField] protected float timeUntilSearchEnd = 3.16f;

    [Header("Raycast Settings")]
    [FormerlySerializedAs("viewDistance")]
    [SerializeField] protected float baseViewDistance = 8.35f;
    [SerializeField] protected float onShotViewDistance = 10.74f;
    [SerializeField] protected float raycastOriginOffset = 0.5f;

    protected Transform currTargetPoint;

    private float _elapsedTimeUntilSearchEnd;
    private float _elapsedTimeUntilSearch;

    protected bool hasSetCurrDesiredPoint;
    protected bool isMovingToDesiredPoint;

    protected float currViewDistance;

    private bool _isShot;
    private bool _hasSetCurrTargetPoint;
    private bool _isMovingToSearchPoint;
    private bool _hasReachedSearchPoint;

    [Header("Desired HenchmanState (based on Henchman)")]
    [SerializeField] protected HenchmanState desiredState = HenchmanState.None;
    protected HenchmanState CurrentState { get; private set; } = HenchmanState.None;

    protected override void Awake() {
        base.Awake();
        this.currViewDistance = this.baseViewDistance;
    }

    protected void Start() {
        PlayerManager.instance.OnPlayerDead += SetCurrentStateToDesired;
    }

    void OnDisable() {
        PlayerManager.instance.OnPlayerDead -= SetCurrentStateToDesired;
    }

    protected virtual void FixedUpdate() {
        if (!PlayerManager.instance.Player) return;
        var playerInLOS = IsPlayerInLOS();
        if (playerInLOS || this._isShot) {
            if (this.CurrentState == this.desiredState) { // Previous state check
                // Could've accumulated waitTime before activating the to be Attack/SearchState
                ResetDesiredState(); // Therefore, reset main state
            } else if (this.CurrentState == HenchmanState.Search && playerInLOS) {
                // If Henchman was in Search and now Player is in LOS:
                // If Henchman had not reached searchPoint, reset the time it would need to
                // wait before moving towards the searchPoint. Otherwise,
                // If Henchman had reached its searchPoint, reset the time it would need to
                // accumulate until SearchState deactivates.
                if (!this._hasReachedSearchPoint) this._elapsedTimeUntilSearch = 0f;
                else this._elapsedTimeUntilSearchEnd = 0f;
            }
            this.CurrentState = playerInLOS ? HenchmanState.Attack : HenchmanState.Search;
        } else {
            // Player not in LOS
            if (IsInAggroState) { // Previous state check -- In Attack/SearchState
                // Switches state to Search if previous state was Attack
                // Or if Henchman is still in SearchState (previous/current state
                // was Search and elapsedTimeUntilSearchEnd hasn't finished)
                if (this.CurrentState == HenchmanState.Attack || IsInSearchState) {
                    this.CurrentState = HenchmanState.Search;
                    HandleStateSwitch(); // Continues the moving/timer until SearchEnd
                    return;
                }
                ResetSearchState(); // Reset if previous state was Search
            }
            this.CurrentState = this.desiredState;
        }
        HandleStateSwitch(); // Applies the CurrentState
    }

    protected override void HandleLocalScale() {
        Vector3 localScale = this.transform.localScale;
        var dirX = ShouldFacePlayer ? GetDirToPlayer().x :
            this.currTargetPoint.position.x - this.rb2d.position.x;
        // If negative, the targetPos is to the left, otherwise right
        // Therefore, flip this Henchman to that dirX
        localScale.x = dirX <= 0f ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
        this.transform.localScale = localScale;
    }

    protected virtual void HandleStateSwitch() {
        switch (this.CurrentState) {
            case HenchmanState.Search: SearchState(); break;
            case HenchmanState.Attack: AttackState(); break;
        }
    }
    protected void SearchState() {
        HandleLocalScale();
        if (this._hasReachedSearchPoint) {
            if (this._elapsedTimeUntilSearchEnd < this.timeUntilSearchEnd) {
                this._elapsedTimeUntilSearchEnd += Time.fixedDeltaTime;
                return;
            }
            if (!this._isShot) return;
            this._isShot = false;
            this.currViewDistance = this.onShotViewDistance;
        } else {
            if (this._elapsedTimeUntilSearch < this.timeUntilSearch) {
                this._elapsedTimeUntilSearch += Time.fixedDeltaTime;
                return;
            }
            if (!this._hasSetCurrTargetPoint) {
                // Moves to the closest movePoint to the Player
                Player player = PlayerManager.instance.Player;
                // Pick closest movePoint to the Player to get a good view for shooting
                var playerToLeftMP = Vector2.Distance(this.leftMovePoint.position,
                    player.transform.position);
                var playerToRightMP = Vector2.Distance(this.rightMovePoint.position,
                    player.transform.position);
                this.currTargetPoint = playerToLeftMP < playerToRightMP ?
                    this.leftMovePoint : this.rightMovePoint;
                // Starts moving to currTargetPoint this frame
                this._hasSetCurrTargetPoint = true;
                this._isMovingToSearchPoint = true;
            }
            if (!this._isMovingToSearchPoint) return;
            MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
            // Stop moving if reached currTargetPoint
            var hasReachedTargetPoint = HasReachedCurrTargetPoint();
            if (!hasReachedTargetPoint) return;
            this._isMovingToSearchPoint = false;
            this._hasReachedSearchPoint = true;
        }
    }
    protected void AttackState() {
        HandleLocalScale(); // Specifically flips localScale to face the Player
    }

    protected void ResetSearchState() {
        // "if (!hasSetCTPoint) is true, it sets isMovingToDPoint to true, instantly moving Henchman
        this._hasSetCurrTargetPoint = false;
        this._isMovingToSearchPoint = false;
        this._hasReachedSearchPoint = false;
        this._elapsedTimeUntilSearchEnd = 0f;
        this._elapsedTimeUntilSearch = 0f;
        this._isShot = false;
        this.currViewDistance = this.baseViewDistance;
    }
    // Same for PatrolHenchman
    protected virtual void ResetDesiredState() {
        // "if (!hasSetCDPoint) is true, it sets isMovingToDPoint to true, instantly moving Henchman
        this.hasSetCurrDesiredPoint = false;
        this.isMovingToDesiredPoint = false;
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

        Vector2 originPos = this.rb2d.position + new Vector2(raycastOriginOffset, 0f);
        RaycastHit2D hit = Physics2D.Raycast(originPos, dirToPlayer,
            this.currViewDistance, this.raycastLayerMask);
        return hit && hit.collider.gameObject.CompareTag("Player");
    }

    private Vector2 GetDirToPlayer() {
        Player player = PlayerManager.instance.Player;
        return ((Vector2) player.transform.position - this.rb2d.position).normalized;
    }

    protected override void TakeDamage(float amount) {
        base.TakeDamage(amount);
        if (this.CurrentHealth == 0f) Destroy(this.gameObject);
    }

    private void SetCurrentStateToDesired(object sender, EventArgs e) {
        this.CurrentState = this.desiredState;
    }

    protected bool HasReachedCurrTargetPoint() {
        var distXToTargetPoint = Mathf.Abs(
            this.rb2d.position.x - this.currTargetPoint.position.x);
        return distXToTargetPoint < 0.08f;
    }

    public void OnCollisionEnter2D(Collision2D collision) {
        GameObject colObj = collision.gameObject;
        if (colObj.CompareTag("PlayerBullet")) {
            this._isShot = true;
            this.currViewDistance = this.onShotViewDistance;
            BulletBehavior bullet = colObj.GetComponent<BulletBehavior>();
            TakeDamage(bullet.Damage);
        }
    }

    // Or if in SearchState but is waiting before it starts moving
    private bool ShouldFacePlayer =>
        IsAttacking || (this.CurrentState == HenchmanState.Search && !this._isMovingToSearchPoint);
    private bool IsInAggroState =>
        this.CurrentState is HenchmanState.Attack or HenchmanState.Search;
    private bool IsInSearchState =>
        this.CurrentState is HenchmanState.Search &&
            this._elapsedTimeUntilSearchEnd < this.timeUntilSearchEnd;

    public bool IsAttacking => this.CurrentState is HenchmanState.Attack;
    public bool IsInDesiredState => this.CurrentState == this.desiredState;
    public HenchmanState GetCurrentState() => this.CurrentState;
}
