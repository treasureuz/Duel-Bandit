using System;
using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Player _playerPrefab;
    [SerializeField] private Transform _playerSpawnPoint;

    [Header("Player Settings")]
    [SerializeField] private float _timeBeforePlayerRespawn = 2f;

    public static PlayerManager instance;

    private Player _player;

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
        SpawnPlayer(); // Spawns start Player
    }

    void OnDisable() {
        OnPlayerDead -= RespawnPlayer;
    }

    private void SpawnPlayer() {
        if (this._player) return; // If Player isnt null
        this._player = Instantiate(this._playerPrefab,
            this._playerSpawnPoint.position, Quaternion.identity);
        OnPlayerSpawned?.Invoke(this, EventArgs.Empty);
    }
    // Respawns Player after time
    private void RespawnPlayer(object sender, EventArgs e) {
        StartCoroutine(HandleRespawnPlayer());
    }
    private IEnumerator HandleRespawnPlayer() {
        yield return new WaitForSeconds(this._timeBeforePlayerRespawn);
        SpawnPlayer(); // Spawns a new Player at its spawnPoint
    }

    public Player GetPlayer() => this._player;
    public Transform GetPlayerSpawnPoint() => this._playerSpawnPoint;
}
