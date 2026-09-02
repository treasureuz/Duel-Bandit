using UnityEngine;

public class HRevolverManager : RevolverManager {
    [Header("HRevolver Settings")]
    [SerializeField] private float _minFireRate;

    private Henchman _henchman;

    protected override void Awake() {
        base.Awake();
        this._henchman = this.GetComponentInParent<Henchman>();
    }

    protected override void FixedUpdate() {
        if (!this._henchman.IsAttacking) {
            SmoothlyRotateTowards(Quaternion.identity);
            return;
        }
        RotateTowardsTargetPos(); // Handles gun rotation to Player
        if (Time.time < this.elapsedShootTime || !IsAimAlignedToPlayer()) return;
        HandleRevolverShoot(); // Shoots gun
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
    
    public override void SetCurrentFireRate(float fireRate) {
        float newFireRate = Mathf.Clamp(fireRate, this._minFireRate, this.baseFireRate);
        SetCurrentFireRate(newFireRate);
    }
}
