using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RevolverEventArgs;
using UnityEngine.Serialization;

public class Henchman : Character<HRevolverManager> {
    [Header("References")]
    [SerializeField] protected VisionCone visionCone;
    [SerializeField] protected HealthBarFollow healthBarCanvas;
    [SerializeField] protected LayerMask raycastLayerMask;

    [Header("Transforms")]
    [SerializeField] protected Transform healthBarTransform;
    [SerializeField] protected Transform collectiblesSpawnPos;
    [SerializeField] protected Transform leftMovePoint;
    [SerializeField] protected Transform rightMovePoint;

    [Header("Main Settings")]
    [SerializeField] protected int FOV = 180; // 90 degrees upward/downward this obj
    [FormerlySerializedAs("standardMoveSpeed")]
    [SerializeField] protected float baseMoveSpeed = 2.67f; // maxMoveSpeed
    [SerializeField] protected float minMoveSpeed = 1.05f;
    [SerializeField] protected List<Collectible> itemsToDropOnDead;

    [Header("State Settings")]
    [SerializeField] protected float timeUntilSearch = 1f;
    [SerializeField] protected float timeUntilSearchEnd = 3.16f;
    [SerializeField] protected float shotDuration = 3f; // how long Henchman stays "shot"

    [Header("Raycast Settings")]
    [SerializeField] protected float baseViewDistance = 8.35f;
    [SerializeField] protected float maxViewDistance = 11.2f;
    [SerializeField] protected float viewDistIncrements = 0.107f;
    [SerializeField] protected float viewDistExpandSpeed = 4f;
    [SerializeField] protected float revolverBarrelRadius = 0.18f;

    protected Transform currTargetPoint;
    private Image _healthBarFill;

    private Coroutine _onShotCoroutine;

    private float _elapsedTimeUntilSearchEnd;
    private float _elapsedTimeUntilSearch;

    protected bool hasSetCurrDesiredPoint;
    protected bool isMovingToDesiredPoint;
    protected bool isSoraEffectEnabled;

    private bool _hasSetCurrTargetPoint;
    private bool _hasReachedSearchPoint;

    private bool _isShot;
    private int _shotCounter;

    private float _currentViewDistance;
    private float _targetViewDistance;

    public float CurrentMoveSpeed { get; private set; }
    private const float collectiblesSpacing = 0.75f;

    [Header("Desired HenchmanState (based on Henchman)")]
    [SerializeField] protected HenchmanState desiredState = HenchmanState.None;
    protected HenchmanState CurrentState { get; private set; }

    protected override void Awake() {
        base.Awake();
        // Sets up health bar UI above Henchman
        Transform healthBarFillObj = this.healthBarCanvas.transform.Find("HealthBarBG/HealthBarFill");
        this._healthBarFill = healthBarFillObj.GetComponent<Image>();

        // Initialize important variables
        this.CurrentState = this.desiredState;
        SetCurrentMoveSpeed(this.baseMoveSpeed);
        InitializeViewDistances();
    }

    void Update() {
        // Update current view distance smoothly to targetViewDistance
        SmoothlyUpdateCurrViewDistance();

        // Set visionCone's variables
        this.visionCone.SetOriginPos(this.transform.position);
        float angle;
        var facingAngle = this.transform.lossyScale.x < 0 ? 180f : 0f;
        if (PlayerManager.instance.Player) {
            // VisionCone rotation follows the Revolver's rotation (*this way makes the most sense*)
            var revolverZAngle = this.RevolverManager.transform.eulerAngles.z;
            angle = this.transform.lossyScale.x > 0 ? revolverZAngle : revolverZAngle - 180f;
        } else angle = facingAngle;
        this.visionCone.SetStartingAngle(angle);
    }

    protected virtual void FixedUpdate() {
        // Henchman spawn in the air, so this stops any state functionality
        if (!this.isOnGround) return; // until its on the ground

        if (!PlayerManager.instance.Player) {
            this.CurrentState = this.desiredState;
            HandleStateSwitch(); return;
        }
        var isPlayerInLOS = IsPlayerInLOS();
        if (isPlayerInLOS || this._isShot) {
            // **Resets prev state so if it becomes the CurrentState
            // again, its functionality starts with fresh values**
            if (this.CurrentState == this.desiredState) { // Previous state check
                ResetDesiredState(); // Reset main state settings
            } else switch (this.CurrentState) { // Previous state check
                case HenchmanState.WaitingToSearch:
                    // Resets possibly accumulated time
                    this._elapsedTimeUntilSearch = 0f; break;
                case HenchmanState.Search when isPlayerInLOS: { // If state was Search & now Player is in LOS
                    // Doesn't call ResetSearchSettings() because it resets hasReachedSearchPoint
                    // but if Henchman already reached searchPoint, it shouldn't perform a redundant move
                    this._hasSetCurrTargetPoint = false; // Recalculates closestPoint when in Search again
                    if (this._hasReachedSearchPoint) // SearchEndTime was accumulated if true,
                        this._elapsedTimeUntilSearchEnd = 0f; // Therefore, reset it
                    break;
                }
            }
            if (isPlayerInLOS) {
                this.CurrentState = HenchmanState.Attack;
                if (this._isShot) this._isShot = false;
            } else this.CurrentState = HenchmanState.Search;
        } else {
            // Player not in LOS
            if (this.CurrentState == HenchmanState.Attack) {
                this.CurrentState = HenchmanState.WaitingToSearch;
            }
            /* NOTE: ALL STATES EXCEPT ATTACK ARE SELF MANAGING --
             they reset their settings and switch to their
             corresponding states within their methods */
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
                // Pick closest movePoint to the Player to get a good view for shooting
                this.currTargetPoint = GetDirToPlayer().x < 0 ?
                    this.leftMovePoint : this.rightMovePoint;
                // Starts moving to currTargetPoint this frame
                this._hasSetCurrTargetPoint = true;
            }
            MoveTowardsCurrTargetPoint(); // Moves moveSpeed*fixedDeltaTime units/frame
            // Stop moving if reached currTargetPoint
            if (!HasReachedCurrTargetPoint()) return;
            this._hasReachedSearchPoint = true;
        }
    }

    private void ResetSearchSettings() {
        this._hasSetCurrTargetPoint = false;
        this._hasReachedSearchPoint = false;
        this._elapsedTimeUntilSearchEnd = 0f;
        if (this._isShot) this._isShot = false;
    }

    protected virtual void ResetDesiredState() {
        // "if (!hasSetCDPoint) is true, it sets isMovingToDPoint to true,
        // which instantly moves Henchman
        this.hasSetCurrDesiredPoint = false;
        this.isMovingToDesiredPoint = false;
    }
    #endregion

    protected void MoveTowardsCurrTargetPoint() {
        var posX = Mathf.MoveTowards(this.rb2d.position.x,
            this.currTargetPoint.position.x, this.CurrentMoveSpeed * Time.fixedDeltaTime);
        Vector2 position = new(posX, this.rb2d.position.y);
        this.rb2d.MovePosition(position);
        HandleLocalScale(GetDirToCurrTarget()); // Flip towards targetPoint
    }

    protected bool IsPlayerInLOS() {
        // Can't be from Revolver's position because when localScale flips,
        // the revolver flips, which causes jittering
        Vector2 dirToPlayer = GetDirToPlayer();
        Vector2 facingDir = GetFacingDirection();
        // Calculates the angle from where this obj is facing to the direction to the player
        // "Vector2.Angle" = 0 to 180 (no negatives) 60 degs above == 60 degs below
        var angleToPlayer = Vector2.Angle(facingDir, dirToPlayer);

        // If the rotation towards the player calculated is within FOV
        var isPlayerWithinFOV = angleToPlayer < (this.FOV / 2f);
        if (!isPlayerWithinFOV) return false;

        Vector2 originPos = this.rb2d.position;
        RaycastHit2D hit = Physics2D.CircleCast(originPos, revolverBarrelRadius, dirToPlayer,
            this._currentViewDistance, this.raycastLayerMask);
        return hit && hit.collider.gameObject.CompareTag("Player");
    }

    protected Vector2 GetDirToPlayer() {
        Player player = PlayerManager.instance.Player;
        return ((Vector2) player.transform.position - this.rb2d.position).normalized;
    }

    protected Vector2 GetDirToCurrTarget() {
        return ((Vector2) this.currTargetPoint.position - this.rb2d.position).normalized;
    }

    protected Vector2 GetFacingDirection() {
        return this.transform.lossyScale.x < 0f ? Vector2.left : Vector2.right;
    }

    protected bool HasReachedCurrTargetPoint() {
        // var distXToTargetPoint = Mathf.Abs(
        //     this.rb2d.position.x - this.currTargetPoint.position.x);
        // Approximately makes GetDirToCurrTarget = 0, making Henchman flip its localScale.x to -1
        return Mathf.Approximately(this.transform.position.x, this.currTargetPoint.position.x);
        //return distXToTargetPoint < 0.1f;
    }

    public void EnableSoraSlowEffect() => this.isSoraEffectEnabled = true;
    protected void DisableSoraSlowEffect() {
        SetCurrentMoveSpeed(this.baseMoveSpeed);
        SetRevolverCurrentTBS(this.RevolverManager.GetBaseTimeBetweenShots());
        this.isSoraEffectEnabled = false;
    }

    #region On Shot Mechanics - Calls TakeDamage() & Handles View Distance
    private void HandleOnShot(float amount) {
        if (PlayerManager.instance.Player && !IsPlayerInLOS() 
            && this.CurrentState is not HenchmanState.Search)
            this._isShot = true;

        // Waits shotDuration before resetting view dist and shot counter
        StartOnShotCoroutine(); // Also sets isShot to true
        TakeDamage(amount); // Sets current health -= amount

        if (this._currentViewDistance == this.maxViewDistance) return;
        ++this._shotCounter; // Used by the method below
        SetTargetViewDistanceOnShot();
    }

    private void StartOnShotCoroutine() {
        if (this._onShotCoroutine != null) {
            StopCoroutine(this._onShotCoroutine);
            this._onShotCoroutine = null;
        }
        this._onShotCoroutine = StartCoroutine(HandleShotDuration());
    }

    private IEnumerator HandleShotDuration() {
        yield return new WaitForSeconds(this.shotDuration);
        this._targetViewDistance = this.baseViewDistance;
        this._shotCounter = 0;
    }

    private void SetTargetViewDistanceOnShot() {
        // The more Henchman gets shot the more the view distance increments by
        var newViewDist = this._currentViewDistance +
            (this.viewDistIncrements * this._shotCounter);
        this._targetViewDistance = Mathf.Clamp(newViewDist,
            this.baseViewDistance, this.maxViewDistance);
    }

    private void SmoothlyUpdateCurrViewDistance() {
        // Stops infinite assignments of currentViewDistance
        if (Mathf.Approximately(this._currentViewDistance, this._targetViewDistance)) return;
        this._currentViewDistance = Mathf.MoveTowards
            (this._currentViewDistance, this._targetViewDistance,
            this.viewDistExpandSpeed * Time.smoothDeltaTime);
        this.visionCone.SetViewDistance(this._currentViewDistance);
    }

    private void InitializeViewDistances() {
        this._currentViewDistance = this.baseViewDistance;
        this._targetViewDistance = this._currentViewDistance;
        this.visionCone.SetViewDistance(this._currentViewDistance);
    }
    #endregion

    protected override void TakeDamage(float amount) {
        base.TakeDamage(amount);
        this._healthBarFill.fillAmount = this.CurrentHealth / this.maxHealth;
        HenchmanHelper.instance.OnHenchmanDamaged?.Invoke(this,
            new DamageTakenEventArgs(amount));
        if (this.CurrentHealth != 0f) return;
        HandleDead(); // Drops collectibles and destroys this obj
    }

    private void HandleDead() {
        var i = 0;
        Vector2 itemsSpawnPoint = this.collectiblesSpawnPos.position;
        foreach (Collectible item in this.itemsToDropOnDead) {
            // Adds 0.75i offset so collectibles don't stack on each other
            var itemOffsetX = i * collectiblesSpacing;
            itemsSpawnPoint += new Vector2(itemOffsetX, 0);
            Instantiate(item, itemsSpawnPoint, Quaternion.identity);
            i++;
        }
        DestroyAll(); // Destroys all relevant Henchman objects
    }

    private void DestroyAll() {
        // Destroys "MoveZone" obj (holds all movePoints)
        Destroy(this.leftMovePoint.parent.gameObject);
        Destroy(this.transform.parent.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        GameObject colObj = collision.gameObject;
        if (colObj.CompareTag("PlayerBullet")) {
            BulletBehavior bullet = colObj.GetComponent<BulletBehavior>();
            HandleOnShot(bullet.Damage); // Takes damage and handles view distance
        } else if (colObj.layer == LayerMask.NameToLayer("Platform")) {
            this.isOnGround = true;
        }
    }

    public bool IsAttacking => this.CurrentState is HenchmanState.Attack;
    public bool IsInDesiredState => this.CurrentState == this.desiredState;

    public void SetCurrentMoveSpeed(float speed) {
        this.CurrentMoveSpeed = Mathf.Clamp(speed, this.minMoveSpeed, this.baseMoveSpeed);
    }
    public void SetRevolverCurrentTBS(float tbs) {
        this.RevolverManager.SetCurrentTimeBetweenShots(tbs);
    }
    public float GetRevolverCurrentTBS() => this.RevolverManager.CurrentTimeBetweenShots;

    public HenchmanState GetCurrentState() => this.CurrentState;
}
