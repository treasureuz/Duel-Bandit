using UnityEngine;

[CreateAssetMenu(fileName = "NewPRevolverData", menuName = "Revolvers/PRevolverData")]
public class PRevolverData : RevolverData {
    [Header("PRevolver Settings")]
    public string revolverName;
    public Color revolverColor;
    public int maxTotalAmmo;
    // the extra ammo/bullets that move into the current mag when reloading
    public int startingTotalAmmo;
    public int maxMagCount;
    public float reloadDuration;
}
