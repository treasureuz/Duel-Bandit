using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Henchman : Character<HRevolverManager> {
    [Header("References")]
    [SerializeField] protected HealthBarFollow _healthBarCanvasPrefab;
    [SerializeField] protected LayerMask raycastLayerMask;

    [Header("Transforms")]
    [SerializeField] protected Transform healthBarTransform;
    [SerializeField] protected Transform collectiblesSpawnPoint;
    [SerializeField] protected Transform leftMovePoint;
    [SerializeField] protected Transform rightMovePoint;

    [Header("Settings")]
    [SerializeField] protected int FOV = 180; // 90 degrees upward/downward this obj
    [SerializeField] protected float minMoveSpeed = 1.05f;
    [SerializeField] protected float timeUntilSearch = 1f;
    [SerializeField] protected float timeUntilSearchEnd = 3.16f;
    [SerializeField] protected float shotDuration = 3f; // how long Henchman stays "shot"
    [SerializeField] protected List<Collectible> itemsToDropOnDead;

    [Header("Raycast Settings")]
    [SerializeField] protected float baseViewDistance = 8.35f;
    [SerializeField] protected float maxViewDistance = 11.2f;
    [SerializeField] protected float viewDistIncrements = 0.107f;

    // Health Bar References
    private HealthBarFollow _healthBarCanvas;
    private Image _healthBarImage;

    protected Transform currTargetPoint;

    private Coroutine _onShotCoroutine;

    private float _elapsedTimeUntilSearchEnd;
    private float _elapsedTimeUntilSearch;

    protected bool hasSetCurrDesiredPoint;
    protected bool isMovingToDesiredPoint;

    private bool _hasSetCurrTargetPoint;
    private bool _hasReachedSearchPoint;

    private int _shotCounter;

    public float CurrentViewDistance {get; private set;}

    [Header("Desired HenchmanState (based on Henchman)")]
    [SerializeField] protected HenchmanState desiredState = HenchmanState.None;
    protected HenchmanState CurrentState { get; private set; } = HenchmanState.None;

    protected override void Awake() {
        base.Awake();
        this._healthBarCanvas = Instantiate(this._healthBarCanvasPrefab,
            this.healthBarTransform.position, Quaternion.identity);
        this._healthBarCanvas.Init(this.transform, this.healthBarTransform.localPosition);
        Transform healthBarBG = this._healthBarCanvas.transform.Find("HealthBarBG/HealthBarFill");
        this._healthBarImage = healthBarBG.GetComponent<Image>();
    }

    protected virtual void FixedUpdate() {
        if (!PlayerManager.instance.Player) {
            this.CurrentState = this.desiredState;
            HandleStateSwitch(); return;
        }
        // Henchman spawn in the air, so this stops any state functionality
        if (!this.isOnGround) return; // until its on the ground

        var isPlayerInLOS = IsPlayerInLOS();
        if (isPlayerInLOS) {
            // Resets prev state so if it becomes the CurrentState
            // again, its functionality starts with fresh values
            if (this.CurrentState == this.desiredState) { // Previous state check
                ResetDesiredState(); // Reset main state settings
            } else switch (this.CurrentState) { // Previous state check
                case HenchmanState.WaitingToSearch: // Repeated shots don't reset it
                    // Resets possibly accumulated time
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
            this.CurrentState = HenchmanState.Attack;
        } else {
            // Player not in LOS
            if (this.CurrentState == HenchmanState.Attack) {
                this.CurrentState = HenchmanState.WaitingToSearch;
            }
            /* NOTE: ALL STATES EXCEPT ATTACK ARE SELF MANAGING --
             they reset their settings and switch to their
             corresponding states within their functions */
            // CurrentState is switched to DesiredState automatically
            // within SearchState() after searching is done
        }
        HandleStateSwitch(); // Applies the CurrentState
    }

    #region State Handling
    protected virtual void HandleStateSwitch() {
        switch (this.CurrentState) {
            case HenchmanState.Attack: AttackState(); break;
            case HenchmanState.WaitingToSearch: WaitingToSearchState(); break;
            case HenchmanState.Search: SearchState(); break;
        }
    }
    // Only flips localScale to face the Player
    protected void AttackState() => HandleLocalScale(GetDirToPlayer());
    protected void WaitingToSearchState() {
        HandleLocalScale(GetDirToCurrTarget()); // Faces Henchman towards targetPoint
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
        this._elapsedTimeUntilSearchEnd = 0f;
    }
    
    protected virtual void ResetDesiredState() {
        // "if (!hasSetCDPoint) is true, it sets isMovingToDPoint to true, 
        // which instantly moves Henchman
        this.hasSetCurrDesiredPoint = false;
        this.isMovingToDesiredPoint = false;
    }
    #endregion

    protected void MoveTowardsTargetPoint() {
        HandleLocalScale(GetDirToCurrTarget()); // Flip towards targetPoint
        // Starts the actual moving
        var posX = Mathf.MoveTowards(this.rb2d.position.x,
            this.currTargetPoint.position.x, this.CurrentMoveSpeed * Time.fixedDeltaTime);
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
            this.CurrentViewDistance, this.raycastLayerMask);
        return hit && hit.collider.gameObject.CompareTag("Player");
    }

    private Vector2 GetDirToPlayer() {
        Player player = PlayerManager.instance.Player;
        return ((Vector2) player.transform.position - this.rb2d.position).normalized;
    }

    private Vector2 GetDirToCurrTarget() {
        return (Vector2)this.currTargetPoint.position - this.rb2d.position;
    }
    
    protected bool HasReachedCurrTargetPoint() {
        var distXToTargetPoint = Mathf.Abs(
            this.rb2d.position.x - this.currTargetPoint.position.x);
        return distXToTargetPoint < 0.08f;
    }

    #region On Shot Mechanics - View Distance & Local Scale
    private void HandleOnShotMechanics() {
        if (this.CurrentViewDistance != this.maxViewDistance){
            ++this._shotCounter; // Used by the method below
            UpdateViewDistanceOnShot();
        }
        // Only flips local scale if Player is not in LOS
        if (IsPlayerInLOS()) return;
        FlipLocalScaleOnShot();
    }

    private void StartOnShotCoroutine(){
        if (this._onShotCoroutine != null) {
            StopCoroutine(this._onShotCoroutine);
            this._onShotCoroutine = null;
        }
        this._onShotCoroutine = StartCoroutine(HandleAfterOnShot());
    }

    private IEnumerator HandleAfterOnShot(){
        yield return new WaitForSeconds(this.shotDuration);
        SetCurrentViewDistance(this.baseViewDistance);
        this._shotCounter = 0;
    }

    private void UpdateViewDistanceOnShot() {
        // The more Henchman gets shot the more the view distance increments by
        float newViewDistIncrements = this.viewDistIncrements * this._shotCounter;
        float newViewDist = this.CurrentViewDistance + newViewDistIncrements;
        SetCurrentViewDistance(newViewDist);
    }

    private void SetCurrentViewDistance(float viewDist) {
        this.CurrentViewDistance = Mathf.Clamp(viewDist, this.baseViewDistance, 
            this.maxViewDistance);
    }

    protected virtual void FlipLocalScaleOnShot() {
        // Flips Henchman's local scale to face the opposite movePoint
        Vector2 dirToOppMovePoint = this.transform.localScale.x < 0 ? 
            (Vector2)this.rightMovePoint.position - this.rb2d.position :
            (Vector2)this.leftMovePoint.position - this.rb2d.position;
        HandleLocalScale(dirToOppMovePoint);
    }
    #endregion

    protected override void TakeDamage(float amount) {
        base.TakeDamage(amount);
        this._healthBarImage.fillAmount = this.CurrentHealth / this.maxHealth;
        HenchmanHelper.instance.OnHenchmanDamaged?.Invoke(this,
            new BulletDamageEventArgs(amount));
        if (this.CurrentHealth != 0f) return;
        Destroy(this._healthBarCanvas.gameObject); // Destroys the health bar
        OnDead(); // Drops collectibles and destroys this obj
    }

    private void OnDead() {
        foreach (Collectible item in this.itemsToDropOnDead)
            Instantiate(item, this.collectiblesSpawnPoint.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    public void OnCollisionEnter2D(Collision2D collision) {
        GameObject colObj = collision.gameObject;
        if (colObj.CompareTag("PlayerBullet")) {
            StartOnShotCoroutine(); // Resets view dist and shot counter after shotDuration
            BulletBehavior bullet = colObj.GetComponent<BulletBehavior>();
            TakeDamage(bullet.Damage);
            HandleOnShotMechanics(); // Handles view distance & local scale
        } else if (colObj.layer == LayerMask.NameToLayer("Platform")) {
            this.isOnGround = true;
        }
    }

    public bool IsAttacking => this.CurrentState is HenchmanState.Attack;
    public bool IsInDesiredState => this.CurrentState == this.desiredState;

    public override void SetCurrentMoveSpeed(float speed) {
        float newMoveSpeed = Mathf.Clamp(speed, this.minMoveSpeed, this.baseMoveSpeed);
        base.SetCurrentMoveSpeed(newMoveSpeed);
    }
    public void SetCurrRevolverFireRate(float fireRate) {
        this.RevolverManager.SetCurrentFireRate(fireRate);
    }
    public float GetCurrRevolverFireRate() => this.RevolverManager.CurrentFireRate;

    public HenchmanState GetCurrentState() => this.CurrentState;
}
