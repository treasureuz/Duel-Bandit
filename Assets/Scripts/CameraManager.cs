using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    [SerializeField] private CinemachineCamera _spawnPosCam;
    [SerializeField] private CinemachineCamera _followCam;

    void Awake() {
        SwitchToSpawnPosCam(this, EventArgs.Empty); // Starts at playerSpawnPoint
    }

    void OnEnable() {
        PlayerManager.instance.OnPlayerSpawned += SwitchToFollowCam;
        PlayerManager.instance.OnPlayerDead += SwitchToSpawnPosCam;
    }

    void OnDisable() {
        PlayerManager.instance.OnPlayerSpawned -= SwitchToFollowCam;
        PlayerManager.instance.OnPlayerDead -= SwitchToSpawnPosCam;
    }

    private void SwitchToSpawnPosCam(object sender, EventArgs e) {
        Transform spawnPos = PlayerManager.instance.GetPlayerSpawnPoint();
        this._spawnPosCam.transform.position =
            new Vector3(spawnPos.position.x, spawnPos.position.y, -10);
        this._spawnPosCam.enabled = true;
        this._followCam.enabled = false;
    }

    public void SwitchToFollowCam(object sender, EventArgs e) {
        Transform follow = PlayerManager.instance.GetPlayer().transform;
        this._followCam.Follow = follow;
        this._followCam.enabled = true;
        this._spawnPosCam.enabled = false;
    }
}
