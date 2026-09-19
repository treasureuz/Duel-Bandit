using UnityEngine;

public class HRevolverManager : TRevolverManager<HRevolverConfig> {
    [Header("References")]
    [SerializeField] protected HRevolverConfig startingRevolver;

    private Henchman _henchman;

    public float CurrentTimeBetweenShots {get; private set;}

    protected override void Awake() {
        base.Awake();
        this._henchman = this.GetComponentInParent<Henchman>();
    }

    private void Start() {
        SetCurrentRevolverConfig(this.startingRevolver);
    }

    protected override void FixedUpdate() {
        if (!PlayerManager.instance.Player) return;
        if (this._henchman.IsInDesiredState) {
            SmoothlyRotateTowards(Quaternion.identity);
            return;
        }
        if (!this._henchman.IsAttacking) return;
        RotateTowardsTargetPos(); // Handles revolver rotation to Player
        if (Time.time < this.elapsedShootTime || !IsAimAlignedToPlayer()) return;
        HandleShoot(); // Shoots revolver only if above conditions are false
    }

    protected override void ApplyRevolverConfig() {
        base.ApplyRevolverConfig();
        SetCurrentTimeBetweenShots(this.currRevolverConfig.baseTimeBetweenShots);
    }

    protected override void HandleShoot() {
        base.HandleShoot();
        this.elapsedShootTime = Time.time + this.CurrentTimeBetweenShots;
    }

    private bool IsAimAlignedToPlayer() {
        var angleDiff = Mathf.Abs(Mathf.DeltaAngle(this.transform.
            eulerAngles.z, this.rotationAngle));
        return angleDiff < 0.5f;
    }
    protected override Vector2 GetTargetPos() {
        Player player = PlayerManager.instance.Player;
        return player.transform.position;
    }

    public void SetCurrentTimeBetweenShots(float tbs) {
        this.CurrentTimeBetweenShots = Mathf.Clamp(tbs,
            this.currRevolverConfig.baseTimeBetweenShots,
            this.currRevolverConfig.maxTimeBetweenShots);
    }

    public float GetBaseTimeBetweenShots() {
        return this.currRevolverConfig.baseTimeBetweenShots;
    }

    public float CurrentRotationAngle => this.rb2d.rotation;
}
