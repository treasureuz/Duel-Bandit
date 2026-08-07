using UnityEngine;

[CreateAssetMenu(fileName = "NewGunData", menuName = "Scriptable Objects/GunData")]
public class GunData : ScriptableObject {
    [Header("References")]
    public Sprite sprite;
    public BulletBehavior bulletPrefab;

    [Header("Settings")]
    public string gunName;
    public Vector3 localScale;
    public Vector3 bulletSpawnPoint;
    public float bulletDamage;
    public float timeBetweenShots = 1.46f;
    public int maxMagCount = int.MaxValue;
}
