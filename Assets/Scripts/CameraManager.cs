using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour {
    [SerializeField] private CinemachineCamera _followCam;

    public static CameraManager instance;

    void Awake() {
        if (!instance) instance = this;
    }

    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode) {
        PlayerManager.instance.OnPlayerSpawned += SwitchToFollowCam;
    }

    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerManager.instance.OnPlayerSpawned -= SwitchToFollowCam;
    }

    public void SwitchToFollowCam(object sender, EventArgs e) {
        Transform followPos = PlayerManager.instance.Player.transform;
        this._followCam.Follow = followPos;
        this._followCam.enabled = true;
    }
}
