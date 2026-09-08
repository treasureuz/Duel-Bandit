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
        // Takes the bulletPrefab's original rotation (0: Player, 180: Henchman)
        // and adjusts it by the angle towards the targetPos.
        Quaternion spawnRotation = this.bulletSpawnPoint.rotation * this.
            currentRevolverData.bulletPrefab.transform.rotation; // ((0,0,180) * (0,0,40) = (0, 0, 220)
        BulletBehavior bullet = Instantiate(this.currentRevolverData.bulletPrefab, this.
            bulletSpawnPoint.position, spawnRotation);
        bullet.Init(this.currentRevolverData.bulletDamage);
        this.elapsedShootTime = Time.time + this.CurrentTimeBetweenShots;
    }

    public float GetStandardTimeBetweenShots() {
        return this.currentRevolverData.standardTimeBetweenShots;
    }
}
