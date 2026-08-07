using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerManager : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Player _playerPrefab;
    [SerializeField] private Transform _playerSpawnPoint;

    [Header("Player Settings")]
    [SerializeField] private List<PGunManager> _gunsList;
    [SerializeField] private float _timeBeforePlayerRespawn = 2f;

    public static PlayerManager instance;

    public Player Player { get; private set; }

    public EventHandler OnPlayerSpawned;
    public EventHandler OnPlayerDead;
    public EventHandler OnPlayerDamaged;
    public EventHandler OnPlayerGunShot;

    void Awake() {
        if (!instance) instance = this;
    }

    void OnEnable() {
        OnPlayerDead += RespawnPlayer;
    }

    void Start() {
        CameraManager.instance.SwitchToSpawnPosCam(this._playerSpawnPoint);
        SpawnPlayer(); // Spawns start Player
    }

    void OnDisable() {
        OnPlayerDead -= RespawnPlayer;
    }

    private void SpawnPlayer() {
        if (this.Player) return; // If Player isnt null
        this.Player = Instantiate(this._playerPrefab,
            this._playerSpawnPoint.position, Quaternion.identity, this.transform);
        CameraManager.instance.SwitchToFollowCam(this.Player.transform);
    }
    // Respawns Player after time
    private void RespawnPlayer(object sender, EventArgs e) {
        StartCoroutine(HandleRespawnPlayer());
    }
    private IEnumerator HandleRespawnPlayer() {
        yield return new WaitForSeconds(this._timeBeforePlayerRespawn);
        SpawnPlayer(); // Spawns a new Player at its spawnPoint
        PGunManager newGun = this._gunsList[Random.Range(0, this._gunsList.Count)];
        this.Player.EquipGun(newGun);
    }
}
