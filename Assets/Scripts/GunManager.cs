using System;
using UnityEngine;

public abstract class GunManager : MonoBehaviour {
    [SerializeField] protected GunData currentGunData;

    protected Rigidbody2D rb2d;
    protected SpriteRenderer spriteRenderer;
    protected Transform bulletSpawnPoint;

    protected float elapsedShootTime;
    protected const float rotationSpeed = 330f;

    private float _rawAngleToTarget;

    protected virtual void Awake() {
        this.rb2d = this.GetComponent<Rigidbody2D>();
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();
        this.bulletSpawnPoint = this.transform.GetChild(0);
        ApplyGunData(); // Sets current sprite
    }

    protected abstract void FixedUpdate();

    public void EquipGun(GunData newGunData) {
        this.currentGunData = newGunData;
        ApplyGunData(); // Sets Unity gameObj-specific components to newGunData's
    }

    private void ApplyGunData() {
       this.spriteRenderer.sprite = this.currentGunData.sprite;
       this.transform.localScale = this.currentGunData.localScale;
    }

    protected virtual void Shoot() {
        Quaternion spawnRot = this.bulletSpawnPoint.rotation * this.
            currentGunData.bulletPrefab.transform.rotation;
        BulletBehavior bullet = Instantiate(this.currentGunData.bulletPrefab,
            this.bulletSpawnPoint.position, spawnRot);
        bullet.Init(this.currentGunData.bulletDamage);
    }

    public void MeasureAngleToTargetPos() {
        // Check if direction is positive (target position is to the right)
        // or negative (target position is to the left)
        Vector2 direction = (GetTargetPos() - this.rb2d.position).normalized;
        this._rawAngleToTarget = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // -> Deg
    }
    public void ApplyRotation() {
        // Initial angle without respect to localScale (for Bullet to use)
        // Keeps its transform.right pointing towards the targetPos
        this.bulletSpawnPoint.rotation = Quaternion.Euler(0f, 0f, this._rawAngleToTarget);

        // Takes this gun's scale sign into account: + or - (even its parent)
        // Rotates the gun by 180 if facing left (-), otherwise no additional rotation (+)
        var angle = this._rawAngleToTarget;
        if (this.transform.lossyScale.x < 0f) angle += 180f;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, angle); // Needs angle in Degrees
        SmoothlyRotateTowards(targetRot);
    }
    protected void RotateTowardsTargetPos() {
        MeasureAngleToTargetPos();
        ApplyRotation();
    }

    protected void SmoothlyRotateTowards(Quaternion targetRot) {
        Quaternion smoothedRot = Quaternion.RotateTowards(this.transform.
            rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        this.rb2d.MoveRotation(smoothedRot);
    }

    public GunData GetCurrentGunData() => this.currentGunData;
    public abstract Vector2 GetTargetPos();
}
