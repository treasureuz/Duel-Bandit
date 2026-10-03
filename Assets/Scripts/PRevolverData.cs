using UnityEngine;

public class PRevolverData {
    public PRevolverConfig RevolverConfig {get;}
    public int CurrentMagCount { get; private set; }
    public int CurrentReserveAmmo { get; private set; }
    public int CurrentTotalAmmo => this.CurrentMagCount + this.CurrentReserveAmmo;

    public PRevolverData(PRevolverConfig revolverConfig, int startingTotalAmmo) {
        this.RevolverConfig = revolverConfig;
        SetAmmoCounts(startingTotalAmmo);
    }

    public void SetAmmoCounts(int ammoToUse) {
        var prevMagCount = this.CurrentMagCount;
        SetCurrentMagCount(ammoToUse); // Gets clamped to maxMagCount, if "ammoToUse" is high
        // calculates ammo added to current mag count
        var reserveAmmo = ammoToUse - (this.CurrentMagCount - prevMagCount);
        SetCurrentReserveAmmo(reserveAmmo);
    }

    public void SetCurrentMagCount(int count) {
        this.CurrentMagCount = Mathf.Clamp(count, 0, this.RevolverConfig.maxMagCount);
    }
    public void SetCurrentReserveAmmo(int count) {
        // If maxTotalAmmo = 100 and maxMagCount = 30, maxReserveAmmo can be only between
        // (70: assuming currentMagCount is filled up to maxMagCount,
        // 100: assuming no bullets in currentMagCount)
        var maxReserveAmmo = this.RevolverConfig.maxTotalAmmo - this.CurrentMagCount;
        this.CurrentReserveAmmo = Mathf.Clamp(count, 0, maxReserveAmmo);
    }
}
