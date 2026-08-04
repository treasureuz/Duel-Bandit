using UnityEngine;

public abstract class Character<TWeaponManager> : MonoBehaviour
    where TWeaponManager : WeaponManager {
    [SerializeField] protected int fieldOfView = 180; // 90 degrees upward/downward this obj
    [SerializeField] protected float moveSpeed = 2.67f;

    protected Rigidbody2D rb2d;
    protected WeaponManager weaponManager;

    protected virtual void Awake() {
        this.rb2d = this.GetComponent<Rigidbody2D>();
        this.weaponManager = this.GetComponentInChildren<TWeaponManager>();
    }

    protected abstract void HandleLocalScale();
    protected void FlipScaleWithDir(Vector2 direction) {
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // Flip localScale if angle created by direction to mousePos "<" or ">" this FOV
        Vector3 localScale = this.transform.localScale;
        var isWithinFOV = Mathf.Abs(angle) <= (FOV / 2f);
        localScale.x = isWithinFOV ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        this.transform.localScale = localScale;
    }

    public int FOV => this.fieldOfView;
}
