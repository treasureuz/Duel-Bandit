using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class RevolverManager : MonoBehaviour {
    protected Rigidbody2D rb2d;
    protected SpriteRenderer spriteRenderer;
    protected Transform bulletSpawnPoint;

    protected float elapsedShootTime;
    protected float rotationAngle;
    protected const float rotationSpeed = 380f;

    public float CurrentTimeBetweenShots {get; private set;}

    protected virtual void Awake() {
        // Getting components
        this.rb2d = this.GetComponent<Rigidbody2D>();
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();
        this.bulletSpawnPoint = this.transform.GetChild(0);
    }

    protected abstract void FixedUpdate();

    protected void RotateTowardsTargetPos() {
        // Check if direction is positive (target position is to the right)
        // or negative (target position is to the left)
        Vector2 direction = (GetTargetPos() - this.rb2d.position).normalized;
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // -> Deg

        // Initial angle without respect to localScale (for Bullet to use)
        // Keeps its transform.right pointing towards the targetPos
        this.bulletSpawnPoint.rotation = Quaternion.Euler(0f, 0f, angle);

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

    public virtual void SetCurrentTimeBetweenShots(float tbs) {
        this.CurrentTimeBetweenShots = tbs;
    }
}
