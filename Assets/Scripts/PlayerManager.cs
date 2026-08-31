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
    [SerializeField] private int _maxPlayerLives = 3;
    [SerializeField] private float _timeBeforePlayerRespawn = 2f;
    [SerializeField] private List<PRevolverManager> _revolvers;

    public static PlayerManager instance;

    public Player Player { get; private set; }

    private int _currentPlayerLives;

    public EventHandler<AmmoEventArgs> OnPlayerSpawned;
    public EventHandler OnPlayerDead;
    public EventHandler OnPlayerOOL;
    public EventHandler<BulletDamageEventArgs> OnPlayerHealthChange;
    public EventHandler<AmmoEventArgs> OnPlayerRevolverShot;

    void Awake() {
        if (!instance) instance = this;
        this._currentPlayerLives = this._maxPlayerLives;
    }

    void Start() {
        CameraManager.instance.SwitchToSpawnPosCam(this._playerSpawnPoint);
        SpawnPlayer(); // Spawns start Player
    }

    void OnEnable() {
        OnPlayerDead += RespawnPlayer;
    }

    void OnDisable() {
        OnPlayerDead -= RespawnPlayer;
    }

    private void SpawnPlayer() {
        if (this.Player) return; // If Player isnt null
        this.Player = Instantiate(this._playerPrefab,
            this._playerSpawnPoint.position, Quaternion.identity, this.transform);
        this.Player.Init(this._currentPlayerLives, this._maxPlayerLives);
        CameraManager.instance.SwitchToFollowCam(this.Player.transform);
    }
    // Respawns Player after time
    private void RespawnPlayer(object sender, EventArgs e) {
        StartCoroutine(HandleRespawnPlayer());
    }
    private IEnumerator HandleRespawnPlayer() {
        if (this._currentPlayerLives == 0) {
            OnPlayerOOL?.Invoke(this, EventArgs.Empty);
            yield break;
        }
        yield return new WaitForSeconds(this._timeBeforePlayerRespawn);
        --this._currentPlayerLives; // Decrease lives when Player Respawns
        SpawnPlayer(); // Spawns a new Player at its spawnPoint
        PRevolverManager newRevolver = this._revolvers[Random.Range(0, this._revolvers.Count)];
        this.Player.EquipGun(newRevolver);
    }
}
