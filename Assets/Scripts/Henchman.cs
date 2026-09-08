using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RevolverEventArgs;

public class Henchman : Character<HRevolverManager> {
    [Header("References")]
    [SerializeField] protected HealthBarFollow _healthBarCanvasPrefab;
    [SerializeField] protected LayerMask raycastLayerMask;
     // Health Bar References
    private HealthBarFollow _healthBarCanvas;
    private Image _healthBarFill;

    [Header("Transforms")]
    [SerializeField] protected Transform healthBarTransform;
    [SerializeField] protected Transform collectiblesSpawnPos;
    [SerializeField] protected Transform leftMovePoint;
    [SerializeField] protected Transform rightMovePoint;

    [Header("Main Settings")]
    [SerializeField] protected int FOV = 180; // 90 degrees upward/downward this obj
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
    //[SerializeField] private int _rayCount = 50;

    protected Transform currTargetPoint;

    private Coroutine _onShotCoroutine;

    private float _elapsedTimeUntilSearchEnd;
    private float _elapsedTimeUntilSearch;

    protected bool hasSetCurrDesiredPoint;
    protected bool isMovingToDesiredPoint;
    protected static bool isSoraEffectEnabled;

    private bool _hasSetCurrTargetPoint;
    private bool _hasReachedSearchPoint;

    private bool _isShot;
    private int _shotCounter;

    public float CurrentViewDistance {get; private set;}

    [Header("Desired HenchmanState (based on Henchman)")]
    [SerializeField] protected HenchmanState desiredState = HenchmanState.None;
    protected HenchmanState CurrentState { get; private set; }

    protected override void Awake() {
        base.Awake();
        // Spawns health bar UI above Henchman
        this._healthBarCanvas = Instantiate(this._healthBarCanvasPrefab,
            this.healthBarTransform.position, Quaternion.identity);
        this._healthBarCanvas.Init(this.transform, this.healthBarTransform.localPosition);
        Transform healthBarFillObj = this._healthBarCanvas.transform.Find("HealthBarBG/HealthBarFill");
        this._healthBarFill = healthBarFillObj.GetComponent<Image>();

        SetCurrentViewDistance(this.baseViewDistance);
        this.CurrentState = this.desiredState;
    }

    protected virtual void FixedUpdate() {
        if (!PlayerManager.instance.Player) {
            this.CurrentState = this.desiredState;
            HandleStateSwitch(); return;
        }
        // Henchman spawn in the air, so this stops any state functionality
        if (!this.isOnGround) return; // until its on the ground

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
                case HenchmanState.Search when isPlayerInLOS: { // If prev state was Search & now Player is in LOS
                    // Doesn't call ResetSearchSettings() because it resets hasReachedSearchPoint
                    // but if Henchman already reached searchPoint, it shouldn't perform a redundant move
                    this._hasSetCurrTargetPoint = false; // Recalculates closestPoint when in Search again
                    if (this._hasReachedSearchPoint) // SearchEndTime was accumulated if true
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

    // void LateUpdate() {
    //     if (!PlayerManager.instance.Player) return;
    //     Mesh mesh = new ();
    //     this._meshFilter.mesh = mesh;

    //     Vector3 originPos = this.rb2d.position;

    //     var isPlayerInLOS = IsPlayerInLOS();
    //     Vector3 dirToPlayer = GetDirToPlayer();
    //     var angle = isPlayerInLOS ? Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg : 0f;
    //     angle -= this.FOV / 2f;
    //       // angle to increase and beam a ray at from the origin to end
    //     var angleIncrease = this.FOV / this._rayCount;
    //       // + 1 for origin, + 1 for ray at startingAngle?
    //     Vector3[] vertices = new Vector3[this._rayCount + 1 + 1];
    //     Vector2[] uv = new Vector2[vertices.Length];
    //     int[] triangles = new int[this._rayCount * 3]; // 3 rayCounts/(vertices?) for 1 triangle (3 sides)

    //     vertices[0] = originPos;

    //     var vertexIndex = 1; // index 0 already accounted for above
    //     var triangleIndex = 0;
    //     for (var i = 0; i <= this._rayCount; ++i) {
    //         Vector3 vertex;
    //         RaycastHit2D raycastHit2D;

    //         Vector3 facingDir = this.transform.localScale.x < 0 ? Vector2.left : Vector2.right;

    //         raycastHit2D = isPlayerInLOS ? Physics2D.Raycast(
    //             originPos, dirToPlayer, this.CurrentViewDistance, LayerMask.NameToLayer("Platform")) :
    //             Physics2D.Raycast(originPos, facingDir, this.CurrentViewDistance, LayerMask.NameToLayer("Platform"));
    //         if (raycastHit2D.collider == null) {
    //             vertex = isPlayerInLOS ? originPos + dirToPlayer * this.CurrentViewDistance
    //                 : originPos + facingDir * this.CurrentViewDistance;
    //         } else {
    //             vertex = raycastHit2D.point;
    //         }
    //         vertices[vertexIndex] = vertex;

    //         if (i > 0) {
    //             // A triangle forms a connection from the origin to prev vertex to current vertex
    //             triangles[triangleIndex + 0] = 0;
    //             triangles[triangleIndex + 1] = vertexIndex - 1; // prev
    //             triangles[triangleIndex + 2] = vertexIndex; // curr

    //             triangleIndex += 3;
    //         }

    //         vertexIndex++;
    //         angle -= angleIncrease;
    //     }

    //     mesh.vertices = vertices;
    //     mesh.uv = uv;
    //     mesh.triangles = triangles;
    // }

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
        HandleLocalScale(GetDirToPlayer()); // Faces Henchman towards Player
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
        var isPlayerWithinFOV = angleToPlayer < (this.FOV / 2f);
        if (!isPlayerWithinFOV) return false;

        // TODO: Add some type of offset to this obj's position (below example works)
        Vector2 originPos = this.rb2d.position + new Vector2(0.3f, 0);
        RaycastHit2D hit = Physics2D.Raycast(originPos, dirToPlayer,
            this.CurrentViewDistance, this.raycastLayerMask);
        return hit && hit.collider.gameObject.CompareTag("Player");
    }

    private Vector2 GetDirToPlayer() {
        Player player = PlayerManager.instance.Player;
        return ((Vector2) player.transform.position - this.rb2d.position).normalized;
    }

    private Vector2 GetDirToCurrTarget() {
        return (Vector2) this.currTargetPoint.position - this.rb2d.position;
    }

    protected bool HasReachedCurrTargetPoint() {
        var distXToTargetPoint = Mathf.Abs(
            this.rb2d.position.x - this.currTargetPoint.position.x);
        return distXToTargetPoint < 0.08f;
    }

    protected void DisableSoraSlowEffect() {
        SetCurrentMoveSpeed(this.standardMoveSpeed);
        SetRevolverCurrentTBS(this.RevolverManager.GetStandardTimeBetweenShots());
        isSoraEffectEnabled = false;
    }

    #region On Shot Mechanics - Calls TakeDamage() & Handles View Distance
    private void HandleOnShot(float amount) {
        if (!IsPlayerInLOS()) this._isShot = true;
        StartOnShotCoroutine(); // Waits shotDuration before resetting view dist and shot counter
        TakeDamage(amount);
        if (this.CurrentViewDistance != this.maxViewDistance){
            ++this._shotCounter; // Used by the method below
            UpdateViewDistanceOnShot();
        }
    }

    private void StartOnShotCoroutine(){
        if (this._onShotCoroutine != null) {
            StopCoroutine(this._onShotCoroutine);
            this._onShotCoroutine = null;
        }
        this._onShotCoroutine = StartCoroutine(HandleAfterOnShotDuration());
    }

    private IEnumerator HandleAfterOnShotDuration(){
        yield return new WaitForSeconds(this.shotDuration);
        SetCurrentViewDistance(this.baseViewDistance);
        this._shotCounter = 0;
        this._isShot = false;
    }

    private void UpdateViewDistanceOnShot() {
        // The more Henchman gets shot the more the view distance increments by
        var newViewDistIncrements = this.viewDistIncrements * this._shotCounter;
        var newViewDist = this.CurrentViewDistance + newViewDistIncrements;
        SetCurrentViewDistance(newViewDist);
    }

    private void SetCurrentViewDistance(float viewDist) {
        this.CurrentViewDistance = Mathf.Clamp(viewDist, this.baseViewDistance,
            this.maxViewDistance);
    }
    #endregion

    protected override void TakeDamage(float amount) {
        base.TakeDamage(amount);
        this._healthBarFill.fillAmount = this.CurrentHealth / this.maxHealth;
        HenchmanHelper.instance.OnHenchmanDamaged?.Invoke(this,
            new DamageTakenEventArgs(amount));
        if (this.CurrentHealth != 0f) return;
        Destroy(this._healthBarCanvas.gameObject); // Destroys the health bar
        OnDead(); // Drops collectibles and destroys this obj
    }

    private void OnDead() {
        foreach (Collectible item in this.itemsToDropOnDead)
            Instantiate(item, this.collectiblesSpawnPos.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    public void OnCollisionEnter2D(Collision2D collision) {
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

    public override void SetCurrentMoveSpeed(float speed) {
        var newMoveSpeed = Mathf.Clamp(speed, this.minMoveSpeed, this.standardMoveSpeed);
        base.SetCurrentMoveSpeed(newMoveSpeed);
    }
    public void SetRevolverCurrentTBS(float tbs) {
        this.RevolverManager.SetCurrentTimeBetweenShots(tbs);
    }
    public float GetRevolverCurrentTBS() => this.RevolverManager.CurrentTimeBetweenShots;
    public void EnableSoraSlowEffect() => isSoraEffectEnabled = true;

    public HenchmanState GetCurrentState() => this.CurrentState;
}
