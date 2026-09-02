using UnityEngine;
using UnityEngine.Serialization;

public abstract class Character<TRevolverManager> : MonoBehaviour
    where TRevolverManager : RevolverManager {
    [Header("References")]
    [SerializeField] protected TRevolverManager baseRevolver;

    [Header("Settings")]
    [SerializeField] protected float maxHealth = 200f;
    [FormerlySerializedAs("moveSpeed")]
    [SerializeField] protected float baseMoveSpeed = 2.67f;

    protected Rigidbody2D rb2d;
    public TRevolverManager RevolverManager { get; private set; }

    public float CurrentHealth { get; private set; }
    public float CurrentMoveSpeed { get; private set; }

    protected bool isOnGround;

    protected virtual void Awake() {
        this.rb2d = this.GetComponent<Rigidbody2D>();
        this.RevolverManager = this.GetComponentInChildren<TRevolverManager>();

        SetCurrentHealth(this.maxHealth);
        SetCurrentMoveSpeed(this.baseMoveSpeed);
        EquipGun(this.baseRevolver); // If Get<TGunManager> returned null, it equips baseGun
    }

    protected void HandleLocalScale(Vector2 dirToTarget) {
        Vector2 localScale = this.transform.localScale;
        localScale.x = dirToTarget.x > 0f ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        this.transform.localScale = localScale;
    }

    public void EquipGun(TRevolverManager gunPrefab) {
        if (this.RevolverManager) Destroy(this.RevolverManager.gameObject);
        this.RevolverManager = Instantiate(gunPrefab, this.transform);
    }

    protected virtual void TakeDamage(float amount) {
        SetCurrentHealth(this.CurrentHealth - amount);
    }

    public void SetCurrentHealth(float health) {
        this.CurrentHealth = Mathf.Clamp(health, 0f, this.maxHealth);
    }
    public virtual void SetCurrentMoveSpeed(float speed) => this.CurrentMoveSpeed = speed;
    public void SetMaxHealth(float health) => this.maxHealth = health;
    public float GetMaxHealth() => this.maxHealth;
}
