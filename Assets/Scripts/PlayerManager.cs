using System;
using System.Collections;
using UnityEngine;
using RevolverEventArgs;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Player _playerPrefab;

    [Header("Player Settings")]
    [SerializeField] private List<PRevolverData> _revolvers;
    [SerializeField] private int _maxPlayerLives = 3;
    [SerializeField] private float _timeBeforePlayerRespawn = 2f;

    public static PlayerManager instance;

    public Player Player { get; private set; }

    private int _currentPlayerLives;

    public EventHandler OnPlayerSpawned;
    public EventHandler OnPlayerDead;
    public EventHandler OnPlayerOOL;
    public EventHandler<EventArgs> OnPlayerHealthChange;
    public EventHandler<RevolverEquippedEventArgs> OnPlayerRevolverEquipped;
    public EventHandler<PlayerAmmoEventArgs> OnPlayerRevolverShot;
    public EventHandler<RevolverReloadingEventArgs> OnPlayerRevolverReloading;
    public EventHandler<PlayerAmmoEventArgs> OnPlayerRevolverReloaded;
    public EventHandler OnPlayerRevolverUnableToReload;

    void Awake() {
        if (!instance) instance = this;
        this._currentPlayerLives = this._maxPlayerLives;
    }

    void OnEnable() {
        GameManager.instance.OnGameStart += SpawnPlayer;
        OnPlayerDead += RespawnPlayer;
    }

    void OnDisable() {
        GameManager.instance.OnGameStart -= SpawnPlayer;
        OnPlayerDead -= RespawnPlayer;
    }

    private void SpawnPlayer(object sender, EventArgs e) {
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
        SpawnPlayer(this, EventArgs.Empty); // Spawns a new Player at its spawnPoint
    }
}
