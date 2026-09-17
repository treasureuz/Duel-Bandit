using UnityEngine;

public abstract class TRevolverManager<TRevolverConfig> : RevolverManager
    where TRevolverConfig : RevolverConfig {

    protected TRevolverConfig currRevolverConfig;

    public virtual void SetCurrentRevolverConfig(TRevolverConfig rConfig) {
        this.currRevolverConfig = rConfig;
        ApplyRevolverConfig();
    }

    protected virtual void ApplyRevolverConfig() {
        this.spriteRenderer.sprite = this.currRevolverConfig.sprite;
    }

    protected virtual void HandleShoot() {
        // Both Player and Henchman's bulletPrefab face right (0 rotation),
        // Therefore, its spawn rotation is set to BSP World Rotation (Revolver world rot + BSP local rot)
        // Ex: assuming BSP World Rotation = (45) + (-180) = -135, the bullet prefab's rotation is -135 aswell
        BulletBehavior bullet = Instantiate(this.currRevolverConfig.bulletPrefab, this.
            bulletSpawnPoint.position, this.bulletSpawnPoint.rotation);
        bullet.Init(this.currRevolverConfig.bulletDamage);
    }
}
