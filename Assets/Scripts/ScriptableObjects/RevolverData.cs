using UnityEngine;

public class RevolverData : ScriptableObject {
    [Header("References")]
    public Sprite sprite;
    public BulletBehavior bulletPrefab;

    [Header("Settings")]
    public float bulletDamage;
    public float standardFireRate;
}
