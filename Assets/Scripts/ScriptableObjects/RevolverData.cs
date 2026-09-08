using UnityEngine;
using UnityEngine.Serialization;

public class RevolverData : ScriptableObject {
    [Header("References")]
    public Sprite sprite;
    public BulletBehavior bulletPrefab;

    [Header("Settings")]
    public float bulletDamage;
    [FormerlySerializedAs("standardFireRate")]
    public float standardTimeBetweenShots;
}
