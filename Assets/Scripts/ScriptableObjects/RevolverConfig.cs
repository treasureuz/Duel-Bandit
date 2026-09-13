using UnityEngine;

public class RevolverConfig : ScriptableObject {
    [Header("References")]
    public Sprite sprite;
    public BulletBehavior bulletPrefab;

    [Header("Settings")]
    public float bulletDamage;
}
