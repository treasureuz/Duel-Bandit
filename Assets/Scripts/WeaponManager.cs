using System;
using UnityEngine;

public abstract class WeaponManager : MonoBehaviour {
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected float timeBetweenShots;
    [SerializeField] protected float bulletMagazineCount = Mathf.Infinity;
    [SerializeField] protected float bulletDamage = 13.5f;

    protected Rigidbody2D rb2d;
    protected Transform bulletSpawnPoint;
    protected float nextShootTime;
    protected Camera cam;

    protected virtual void Awake() {
        this.cam = Camera.main;
        this.rb2d = this.GetComponent<Rigidbody2D>();
        this.bulletSpawnPoint = this.transform.GetChild(0);
    }

    protected abstract void FixedUpdate();

    protected virtual void Shoot() {
        Quaternion spawnRotation = this.transform.rotation * this.bulletPrefab.transform.rotation;
        Instantiate(this.bulletPrefab, this.bulletSpawnPoint.position, spawnRotation);
    }

    protected void RotateTowardsTarget(Vector3 targetPos) {
        // Check if direction is positive (target position is to the right)
        // or negative (target position is to the left)
        Vector2 direction = (targetPos - this.transform.position).normalized;
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // -> Deg
        this.rb2d.MoveRotation(angle); // Needs angle in Degrees
    }
    protected abstract Vector3 GetTargetPosition();

    public float GetBulletDamage() => this.bulletDamage;

}
