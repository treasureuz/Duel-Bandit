using System;
using UnityEngine;

public abstract class GunManager : MonoBehaviour {
    [Header("References")]
    [SerializeField] protected GameObject bulletPrefab;

    [Header("Settings")]
    [SerializeField] protected float rotationSpeed = 325f;
    [SerializeField] protected float timeBetweenShots = 1.46f;

    protected Rigidbody2D rb2d;
    protected Transform bulletSpawnPoint;
    protected float elapsedShootTime;

    private float _rawAngleToTarget;

    protected virtual void Awake() {
        this.rb2d = this.GetComponent<Rigidbody2D>();
        this.bulletSpawnPoint = this.transform.GetChild(0);
    }

    protected abstract void FixedUpdate();

    protected virtual void Shoot() {
        Quaternion spawnRot = this.bulletSpawnPoint.rotation * this.bulletPrefab.transform.rotation;
        Instantiate(this.bulletPrefab, this.bulletSpawnPoint.position, spawnRot);
    }

    public void MeasureAngleToTargetPos() {
        // Check if direction is positive (target position is to the right)
        // or negative (target position is to the left)
        Vector2 direction = (GetTargetPosition() - this.rb2d.position).normalized;
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
        Quaternion smoothedRot = Quaternion.RotateTowards(this.transform.
            rotation, targetRot, this.rotationSpeed * Time.fixedDeltaTime);
        this.rb2d.MoveRotation(smoothedRot);
    }

    protected void RotateTowardsTargetPos() {
        MeasureAngleToTargetPos();
        ApplyRotation();
    }
    public abstract Vector2 GetTargetPosition();
}
