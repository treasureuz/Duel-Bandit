using System;
using UnityEngine;

public abstract class Character<TGunManager> : MonoBehaviour
    where TGunManager : GunManager {
    [Header("Settings")]
    [SerializeField] protected int FOV = 180; // 90 degrees upward/downward this obj
    [SerializeField] protected float maxHealth = 200f;
    [SerializeField] protected float moveSpeed = 2.67f;

    protected Rigidbody2D rb2d;
    protected TGunManager gunManager;
    public TGunManager GunManager => this.gunManager;

    public float CurrentHealth { get; private set; }

    protected virtual void Awake() {
        this.rb2d = this.GetComponent<Rigidbody2D>();
        this.gunManager = this.GetComponentInChildren<TGunManager>();
        SetCurrentHealth(this.maxHealth);
    }

    protected abstract void HandleLocalScale();

    protected virtual void TakeDamage(float amount) {
        SetCurrentHealth(this.CurrentHealth - amount);
    }

    public void SetCurrentHealth(float health) {
        this.CurrentHealth = Mathf.Clamp(health, 0f, this.maxHealth);
    }
    public void SetMaxHealth(float health) => this.maxHealth = health;
    public float GetMaxHealth() => this.maxHealth;
}
