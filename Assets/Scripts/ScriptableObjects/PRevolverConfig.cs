using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewPRevolverData", menuName = "Revolvers/PRevolverData")]
public class PRevolverConfig : RevolverConfig {
    [Header("PRevolver Settings")]
    public string revolverName;
    public Color revolverColor;
    public int maxTotalAmmo;
    // the extra ammo/bullets that move into the current mag when reloading
    public int startingTotalAmmo;
    public int maxMagCount;
    [FormerlySerializedAs("standardTimeBetweenShots")]
    public float timeBetweenShots;
    public float reloadDuration;
}
