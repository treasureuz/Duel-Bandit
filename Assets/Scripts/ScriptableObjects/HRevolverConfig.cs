using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewHRevolverData", menuName = "Revolvers/HRevolverData")]
public class HRevolverConfig : RevolverConfig {
    [Header("HRevolverData Settings")]
     [FormerlySerializedAs("standardTimeBetweenShots")]
    public float baseTimeBetweenShots; // maxTBS
    public float maxTimeBetweenShots;
}
