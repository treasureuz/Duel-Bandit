using UnityEngine;
using UnityEngine.Serialization;

public abstract class RevolverManager : MonoBehaviour {
    [Header("References")]
    [SerializeField] protected BulletBehavior bulletPrefab;

    [Header("Settings")]
    [SerializeField] protected float bulletDamage;
    [FormerlySerializedAs("timeBetweenShots")]
    [SerializeField] protected float fireRate = 1.46f;

    protected Rigidbody2D rb2d;
    protected SpriteRenderer spriteRenderer;
    private Transform _bulletSpawnPoint;

    protected float elapsedShootTime;
    protected float rotationAngle;
    protected const float rotationSpeed = 380f;

    protected virtual void Awake() {
        this.rb2d = this.GetComponent<Rigidbody2D>();
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();
        this._bulletSpawnPoint = this.transform.GetChild(0);
    }

    protected abstract void FixedUpdate();

    protected virtual void HandleRevolverShoot() {
        // Takes the bulletPrefab's original rotation (0: Player, 180: Henchman)
        // and adjusts it by the angle towards the targetPos.
        Quaternion spawnRotation = this._bulletSpawnPoint.rotation * this.
            bulletPrefab.transform.rotation; // ((0,0,180) * (0,0,40) = (0, 0, 220)
        BulletBehavior bullet = Instantiate(this.bulletPrefab, this.
            _bulletSpawnPoint.position, spawnRotation);
        bullet.Init(this.bulletDamage);
        this.elapsedShootTime = Time.time + this.fireRate;
    }

    protected void RotateTowardsTargetPos() {
        // Check if direction is positive (target position is to the right)
        // or negative (target position is to the left)
        Vector2 direction = (GetTargetPos() - this.rb2d.position).normalized;
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // -> Deg

        // Initial angle without respect to localScale (for Bullet to use)
        // Keeps its transform.right pointing towards the targetPos
        this._bulletSpawnPoint.rotation = Quaternion.Euler(0f, 0f, angle);

        // Takes this gun's scale sign into account: + or - (even its parent)
        // Rotates the gun by 180 if facing left (-), otherwise no additional rotation (+)
        this.rotationAngle = angle;
        if (this.transform.lossyScale.x < 0f) rotationAngle += 180f;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, rotationAngle); // Needs angle in Degrees
        SmoothlyRotateTowards(targetRot);
    }

    protected void SmoothlyRotateTowards(Quaternion targetRot) {
        Quaternion smoothedRot = Quaternion.RotateTowards(this.transform.
            rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        this.rb2d.MoveRotation(smoothedRot);
    }

    protected abstract Vector2 GetTargetPos();
}
