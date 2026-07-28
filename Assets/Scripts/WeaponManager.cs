using System;
using UnityEngine;

public abstract class WeaponManager : MonoBehaviour {
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected float timeBetweenShots;
    [SerializeField] protected float bulletMagazineCount = Mathf.Infinity;
    [SerializeField] protected float bulletDamage = 13.5f;

    protected Transform bulletSpawnPoint;
    protected float nextShootTime;
    protected Camera cam;

    protected virtual void Awake() {
        this.cam = Camera.main;
        this.bulletSpawnPoint = this.transform.GetChild(0);
    }

    protected virtual void Update() {
        RotateTowardsTarget(GetTargetPosition());
        HandleShoot();
    }

    protected abstract void HandleShoot();
    protected void Shoot() {
        Quaternion spawnRotation = this.transform.rotation * this.bulletPrefab.transform.rotation;
        Instantiate(this.bulletPrefab, this.bulletSpawnPoint.position, spawnRotation);
        //--this.currentMagazineCount; // Decrement mag count
    }

    protected void RotateTowardsTarget(Vector3 targetPos) {
        // Check if direction is positive (target position is to the right)
        // or negative (target position is to the left)
        Vector2 direction = (targetPos - this.transform.position).normalized;
        var angle = Vector3.SignedAngle(this.transform.right, direction, Vector3.forward);
        this.transform.Rotate(Vector3.forward, angle);
    }
    protected abstract Vector3 GetTargetPosition();

    public float GetBulletDamage() => this.bulletDamage;

}
