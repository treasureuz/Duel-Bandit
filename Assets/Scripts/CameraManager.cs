using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    [SerializeField] private CinemachineCamera _followCam;

    public static CameraManager instance;

    void Awake() {
        if (!instance) instance = this;
    }

    void Start() {
        PlayerManager.instance.OnPlayerSpawned += SwitchToFollowCam;
    }
    
    void OnDisable() {
        PlayerManager.instance.OnPlayerSpawned -= SwitchToFollowCam;
    }

    public void SwitchToFollowCam(object sender, EventArgs e) {
        Transform followPos = PlayerManager.instance.Player.transform;
        this._followCam.Follow = followPos;
        this._followCam.enabled = true;
    }
}
