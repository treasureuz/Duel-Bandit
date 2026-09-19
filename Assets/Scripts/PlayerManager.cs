using System;
using System.Collections;
using UnityEngine;
using RevolverEventArgs;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Player _playerPrefab;

    [Header("Player Settings")]
    [SerializeField] private PRevolverConfig _startingRevolver;
    [SerializeField] private int _maxPlayerLives = 3;
    [SerializeField] private float _timeBeforePlayerRespawn = 2f;

    public static PlayerManager instance;

    public Player Player { get; private set; }
    private List<PRevolverData> _revolvers = new();

    private int _currentPlayerLives;

    public EventHandler OnPlayerSpawned;
    public EventHandler OnPlayerDead;
    public EventHandler OnPlayerOOL;
    public EventHandler<EventArgs> OnPlayerHealthChange;
    public EventHandler<RevolverEquippedEventArgs> OnPlayerRevolverEquipped;
    public EventHandler<PlayerAmmoEventArgs> OnPlayerRevolverShot;
    public EventHandler<PlayerAmmoEventArgs> OnPlayerRevolverAmmoChanged;
    public EventHandler<RevolverReloadingEventArgs> OnPlayerRevolverReloading;
    public EventHandler OnPlayerRevolverUnableToReload;

    void Awake() {
        if (!instance) instance = this;
        PRevolverData newRevolver = new (this._startingRevolver,
            this._startingRevolver.startingTotalAmmo);
        this._revolvers.Add(newRevolver);
        this._currentPlayerLives = this._maxPlayerLives;
    }

    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnPlayerDead += RespawnPlayer;
    }

    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        OnPlayerDead -= RespawnPlayer;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == "GameScene") SpawnPlayer();
    }

    private void SpawnPlayer() {
        if (this.Player) return; // If Player isnt null
        this.Player = Instantiate(this._playerPrefab,
            this.transform.position, Quaternion.identity, this.transform);
        this.Player.Init(this._revolvers, this._currentPlayerLives, this._maxPlayerLives);
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
    }
}
