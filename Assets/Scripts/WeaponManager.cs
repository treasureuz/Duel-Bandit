using System;
using UnityEngine;

public abstract class WeaponManager : MonoBehaviour {
    [Header("References")]
    [SerializeField] protected GameObject bulletPrefab;

    [Header("Settings")]
    [SerializeField] protected float rotationSpeed = 100f;
    [SerializeField] protected float timeBetweenShots = 1.46f;
    [SerializeField] protected float bulletMagazineCount = Mathf.Infinity;
    [SerializeField] protected float bulletDamage = 13.5f;

    protected Rigidbody2D rb2d;
    protected Transform bulletSpawnPoint;
    protected float elapsedShootTime;

    protected virtual void Awake() {
        this.rb2d = this.GetComponent<Rigidbody2D>();
        this.bulletSpawnPoint = this.transform.GetChild(0);
    }

    protected abstract void FixedUpdate();

    protected virtual void Shoot() {
        Quaternion spawnRot = this.bulletSpawnPoint.rotation * this.bulletPrefab.transform.rotation;
        Instantiate(this.bulletPrefab, this.bulletSpawnPoint.position, spawnRot);
    }

    protected void RotateTowardsTarget(Vector2 targetPos) {
        // Check if direction is positive (target position is to the right)
        // or negative (target position is to the left)
        Vector2 direction = (targetPos - this.rb2d.position).normalized;
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // -> Deg

        // Initial angle without respect to localScale (for Bullet to use)
        this.bulletSpawnPoint.rotation = Quaternion.Euler(0f, 0f, angle);

        // Takes this gun's scale sign into account: + or - (even its parent)
        // Flips the gun's rotation according by 180 if facing left (-), otherwise no flip (+)
        if (this.transform.lossyScale.x < 0f) angle += 180f;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, angle); // Needs angle in Degrees
        Quaternion smoothedRot = Quaternion.RotateTowards(this.transform.
            rotation, targetRot, this.rotationSpeed);
        this.rb2d.MoveRotation(smoothedRot);
    }
    protected abstract Vector3 GetTargetPosition();

    public float GetBulletDamage() => this.bulletDamage;

}
