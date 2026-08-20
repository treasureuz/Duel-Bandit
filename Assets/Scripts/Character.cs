using UnityEngine;

public abstract class Character<TGunManager> : MonoBehaviour
    where TGunManager : GunManager {
    [Header("References")]
    [SerializeField] protected TGunManager baseGun;

    [Header("Settings")]
    [SerializeField] protected float maxHealth = 200f;
    [SerializeField] protected float moveSpeed = 2.67f;

    protected Rigidbody2D rb2d;
    public TGunManager GunManager { get; private set; }

    public float CurrentHealth { get; private set; }

    protected virtual void Awake() {
        this.rb2d = this.GetComponent<Rigidbody2D>();
        this.GunManager = this.GetComponentInChildren<TGunManager>();
        SetCurrentHealth(this.maxHealth);
        EquipGun(this.baseGun); // If Get<TGunManager> returned null, it equips baseGun
    }

    protected abstract void HandleLocalScale();

    public void EquipGun(TGunManager gunPrefab) {
        if (this.GunManager) Destroy(this.GunManager.gameObject);
        this.GunManager = Instantiate(gunPrefab, this.transform);
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
