using UnityEngine;

public abstract class Character<TRevolverManager> : MonoBehaviour
    where TRevolverManager : RevolverManager {
    [Header("Settings")]
    [SerializeField] protected float maxHealth = 200f;

    protected Rigidbody2D rb2d;
    public TRevolverManager RevolverManager { get; private set; }

    public float CurrentHealth { get; private set; }

    protected bool isOnGround;

    protected virtual void Awake() {
        this.rb2d = this.GetComponent<Rigidbody2D>();
        this.RevolverManager = this.GetComponentInChildren<TRevolverManager>();

        SetCurrentHealth(this.maxHealth);
    }

    protected void HandleLocalScale(Vector2 dirToTarget) {
        Vector2 localScale = this.transform.localScale;
        if (dirToTarget.x > 0f) localScale.x = Mathf.Abs(localScale.x);
        else if (dirToTarget.x < 0f) localScale.x = -Mathf.Abs(localScale.x);
        this.transform.localScale = localScale;
    }

    protected virtual void TakeDamage(float amount) {
        SetCurrentHealth(this.CurrentHealth - amount);
    }

    public void SetCurrentHealth(float health) {
        this.CurrentHealth = Mathf.Clamp(health, 0f, this.maxHealth);
    }

    public void SetMaxHealth(float health) => this.maxHealth = health;
    public float GetMaxHealth() => this.maxHealth;
}
