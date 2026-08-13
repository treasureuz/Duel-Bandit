using UnityEngine;
using UnityEngine.Serialization;

public class Henchman : Character<HGunManager> {
    [Header("References")]
    [SerializeField] protected LayerMask raycastLayerMask;
    [SerializeField] protected Transform leftMovePoint;
    [SerializeField] protected Transform rightMovePoint;

    [Header("Settings")]
    [SerializeField] private Transform _healthBarPos;
    [SerializeField] protected int FOV = 180; // 90 degrees upward/downward this obj
    [SerializeField] protected float shotStateDur = 1f;
    [SerializeField] protected float timeUntilSearch = 1f;
    [SerializeField] protected float timeUntilSearchEnd = 3.16f;

    [Header("Raycast Settings")]
    [FormerlySerializedAs("viewDistance")]
    [SerializeField] protected float baseViewDistance = 8.35f;
    [SerializeField] protected float onShotViewDistance = 10.74f;

    protected Transform currTargetPoint;

    private float _elapsedShotStateDur;
    private float _elapsedTimeUntilSearchEnd;
    private float _elapsedTimeUntilSearch;

    protected bool hasSetCurrDesiredPoint;
    protected bool isMovingToDesiredPoint;
    protected bool isOnGround;

    private bool _isShot;
    private bool _hasSetCurrTargetPoint;
    private bool _hasReachedSearchPoint;

    [Header("Desired HenchmanState (based on Henchman)")]
    [SerializeField] protected HenchmanState desiredState = HenchmanState.None;
    protected HenchmanState CurrentState { get; private set; } = HenchmanState.None;

    protected virtual void FixedUpdate() {
        if (!PlayerManager.instance.Player) {
            this.CurrentState = this.desiredState;
            HandleStateSwitch();
            return;
        }
        if (!this.isOnGround) return;
        var isPlayerInLOS = IsPlayerInLOS();
        if (isPlayerInLOS || this._isShot) {
            // Resets prev state so if it becomes the CurrentState
            // again, its functionality starts with fresh values
            if (this.CurrentState == this.desiredState) { // Previous state check
                ResetDesiredState(); // Reset main state settings
            } else switch (this.CurrentState) { // Previous state check
                case HenchmanState.WaitingToSearch when isPlayerInLOS: // Repeated shots don't reset it
                    // Reset possibly accumulated time
                    this._elapsedTimeUntilSearch = 0f; break;
                case HenchmanState.Search: {
                    // Doesn't call ResetSearchSettings() because it resets hasReachedSearchPoint
                    // but if Henchman already reached searchPoint, it shouldn't perform a redundant move
                    this._hasSetCurrTargetPoint = false; // Recalculates closestPoint when in Search again
                    if (this._hasReachedSearchPoint) // Time was accumulated if true
                        this._elapsedTimeUntilSearchEnd = 0f; // Therefore, reset it
                    break;
                }
            }
            if (isPlayerInLOS) {
                this._isShot = false;
                this.CurrentState = HenchmanState.Attack;
            } else this.CurrentState = HenchmanState.WaitingToSearch;
        } else {
            // Player not in LOS
            if (this.CurrentState == HenchmanState.Attack) {
                this.CurrentState = HenchmanState.WaitingToSearch;
            } else if (!IsInSelfManagedState) { // Shot/WaitingToSearch/Search - all reset themselves
                this.CurrentState = this.desiredState;
            }
        }
        HandleStateSwitch(); // Applies the CurrentState
    }

    protected override void HandleLocalScale() {
        Vector3 localScale = this.transform.localScale;
        // ShouldFacePlayer if Henchman is in Attack/WaitingToSearch/ShotState
        var dirX = ShouldFacePlayer ? GetDirToPlayer().x :
            this.currTargetPoint.position.x - this.rb2d.position.x;
        // If negative, the targetPos is to the left, otherwise right
        // Therefore, flip this Henchman to that dirX
        localScale.x = dirX <= 0f ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
        this.transform.localScale = localScale;
    }

    protected virtual void HandleStateSwitch() {
        switch (this.CurrentState) {
            case HenchmanState.Attack: AttackState(); break;
            case HenchmanState.WaitingToSearch: WaitingToSearchState(); break;
            case HenchmanState.Search: SearchState(); break;
        }
    }
    // Only flips localScale to face the Player
    protected void AttackState() => HandleLocalScale();

    protected void WaitingToSearchState() {
        HandleLocalScale();
        if (this._elapsedTimeUntilSearch < this.timeUntilSearch) {
            this._elapsedTimeUntilSearch += Time.fixedDeltaTime;
            return;
        }
        this._elapsedTimeUntilSearch = 0f;
        this.CurrentState = HenchmanState.Search;
    }
    protected void SearchState() {
        if (this._hasReachedSearchPoint) {
            if (this._elapsedTimeUntilSearchEnd < this.timeUntilSearchEnd) {
                this._elapsedTimeUntilSearchEnd += Time.fixedDeltaTime;
                return;
            }
            ResetSearchSettings(); // Resets all Search related settings
            this.CurrentState = this.desiredState;
        } else {
            if (!this._hasSetCurrTargetPoint) {
                // Set currTargetPoint to the closest movePoint to the Player
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
            }
            MoveTowardsTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
            // Stop moving if reached currTargetPoint
            var hasReachedTargetPoint = HasReachedCurrTargetPoint();
            if (!hasReachedTargetPoint) return;
            this._hasReachedSearchPoint = true;
        }
    }

    private void ResetSearchSettings() {
        this._hasSetCurrTargetPoint = false;
        this._hasReachedSearchPoint = false;
        this._elapsedTimeUntilSearch = 0f;
        this._elapsedTimeUntilSearchEnd = 0f;
        if (this._isShot) this._isShot = false;
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

        Vector2 originPos = this.rb2d.position;
        RaycastHit2D hit = Physics2D.Raycast(originPos, dirToPlayer,
            this.CurrViewDistance, this.raycastLayerMask);
        return hit && hit.collider.gameObject.CompareTag("Player");
    }

    private Vector2 GetDirToPlayer() {
        Player player = PlayerManager.instance.Player;
        return ((Vector2) player.transform.position - this.rb2d.position).normalized;
    }

    protected override void OnDamaged(float amount) {
        base.OnDamaged(amount);
        HenchmanManager.instance.OnHenchmanDamaged?.Invoke(this,
            new BulletDamageEventArgs(amount));
        if (this.CurrentHealth == 0f) Destroy(this.gameObject);
    }

    protected bool HasReachedCurrTargetPoint() {
        var distXToTargetPoint = Mathf.Abs(
            this.rb2d.position.x - this.currTargetPoint.position.x);
        return distXToTargetPoint < 0.08f;
    }

    public void OnCollisionEnter2D(Collision2D collision) {
        GameObject colObj = collision.gameObject;
        if (colObj.CompareTag("PlayerBullet")) {
            if (!IsPlayerInLOS()) this._isShot = true; // Only set to true if Player not in LOS
            BulletBehavior bullet = colObj.GetComponent<BulletBehavior>();
            OnDamaged(bullet.Damage);
        } else if (colObj.layer == LayerMask.NameToLayer("Platform")) {
            this.isOnGround = true;
        }
    }

    private float CurrViewDistance =>
        this._isShot ? this.onShotViewDistance : this.baseViewDistance;
    private bool ShouldFacePlayer =>
        IsAttacking || this.CurrentState is HenchmanState.WaitingToSearch;
    private bool IsInSelfManagedState =>
        this.CurrentState is HenchmanState.WaitingToSearch or HenchmanState.Search;

    public bool IsAttacking => this.CurrentState is HenchmanState.Attack;
    public bool IsInDesiredState => this.CurrentState == this.desiredState;
    public HenchmanState GetCurrentState() => this.CurrentState;

    public Transform GetHealthBarPos() => this._healthBarPos;
}
