using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    [SerializeField] private CinemachineCamera _spawnPosCam;
    [SerializeField] private CinemachineCamera _followCam;

    public static CameraManager instance;

    void Awake() {
        if (!instance) instance = this;
    }

    public void SwitchToSpawnPosCam(Transform spawnPos) {
        this._spawnPosCam.transform.position =
            new Vector3(spawnPos.position.x, spawnPos.position.y, -10);
        this._spawnPosCam.enabled = true;
        this._followCam.enabled = false;
    }

    public void SwitchToFollowCam(Transform followPos) {
        this._followCam.Follow = followPos;
        this._followCam.enabled = true;
        this._spawnPosCam.enabled = false;
    }
}
