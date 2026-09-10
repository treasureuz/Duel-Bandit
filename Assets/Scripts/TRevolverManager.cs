using System.Collections.Generic;
using UnityEngine;

public abstract class TRevolverManager<TRevolverData> : RevolverManager
    where TRevolverData : RevolverData {
    [Header("References")]
    [SerializeField] protected TRevolverData startingRevolver;
    protected List<TRevolverData> revolvers = new();
    protected TRevolverData currentRevolverData;

    protected void Start() {
        SetCurrentRevolver(this.startingRevolver);
    }

    public void SetCurrentRevolver(TRevolverData revolverData) {
        this.currentRevolverData = revolverData;
        // Add revolver to list if its new
        if (!this.revolvers.Find(revolver => revolver == this.currentRevolverData))
            this.revolvers.Add(this.currentRevolverData);
        ApplyRevolverData();
    }

    protected virtual void ApplyRevolverData() {
        this.spriteRenderer.sprite = this.currentRevolverData.sprite;
        SetCurrentTimeBetweenShots(this.currentRevolverData.standardTimeBetweenShots);
    }

    protected virtual void HandleShoot() {
        // Both Player and Henchman's bulletPrefab face right (0 rotation),
        // Therefore, its spawn rotation is set to BSP World Rotation (Revolver world rot + BSP local rot)
        // Ex: assuming BSP World Rotation = (45) + (-180) = -135, the bullet prefab's rotation is -135 aswell
        BulletBehavior bullet = Instantiate(this.currentRevolverData.bulletPrefab, this.
            bulletSpawnPoint.position, this.bulletSpawnPoint.rotation);
        bullet.Init(this.currentRevolverData.bulletDamage);
        this.elapsedShootTime = Time.time + this.CurrentTimeBetweenShots;
    }

    public float GetStandardTimeBetweenShots() {
        return this.currentRevolverData.standardTimeBetweenShots;
    }
}
