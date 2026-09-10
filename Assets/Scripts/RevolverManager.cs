using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class RevolverManager : MonoBehaviour {
    protected Rigidbody2D rb2d;
    protected SpriteRenderer spriteRenderer;
    public Transform bulletSpawnPoint {get; private set;}

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
        Vector2 dirToTarget = (GetTargetPos() - this.rb2d.position).normalized;
        var angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg; // -> Deg

        // Takes this gun's scale sign into account: + or - (even its parent)
        // Rotates the gun by 180 if facing left (-), otherwise no additional rotation (+)
        this.rotationAngle = angle;
        if (this.transform.lossyScale.x < 0f) rotationAngle += 180f;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, rotationAngle);
        SmoothlyRotateTowards(targetRot);

        // *Unity reads BSP's parent (Revolver) world rotation and sets BSP's local rotation
        // to a value so that BSP world rotation matches the target "angle"*
        // BSP World Rotation = Revolver world rotation + BSP local rotation
        // Ex: Assuming dirTotarget is bottom left (-1, -1), BSP Local Rotation =
        // Target Angle (-135) - Revolver World Rotation (45) = -180
        // Since parent revolver is at 45 (Angle (-135) + 180 flip (if Character facing left),
        // Unity then sets BSP local rotation to -180 so BSP world rotation equals target angle (-135).
        this.bulletSpawnPoint.rotation = Quaternion.Euler(0f, 0f, angle);
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
