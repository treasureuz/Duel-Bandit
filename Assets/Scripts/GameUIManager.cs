using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour {
    [SerializeField] private Canvas _canvas;

    [Header("Player UI")]
    [SerializeField] private GameObject _respawnScreen;
    [SerializeField] private TextMeshProUGUI _playerHealthText;
    [SerializeField] private TextMeshProUGUI _playerCurrentLives;
    [SerializeField] private TextMeshProUGUI _magCountText;

    [Header("Henchman UI")]
    //[SerializeField] private float _henchmanHealthTextOffset = 1f;
    [SerializeField] private TextMeshProUGUI _henchmanDamageText;

    public static GameUIManager instance;

    void Awake() {
        if (!instance) instance = this;
        this._respawnScreen.SetActive(false);
    }

    void Start() {
        PlayerManager.instance.OnPlayerSpawned += UpdatePlayerHealthText;
        PlayerManager.instance.OnPlayerSpawned += UpdatePlayerCurrLivesText;
        PlayerManager.instance.OnPlayerSpawned += UpdatePlayerMagCountText;
        PlayerManager.instance.OnPlayerDamaged += UpdatePlayerHealthText;
        HenchmanManager.instance.OnHenchmanDamaged += SpawnHenchmanDamageText;
        PlayerManager.instance.OnPlayerOOL += EnableRespawnScreen;
        PlayerManager.instance.OnPlayerGunShot += UpdatePlayerMagCountText;
    }

    void OnDisable() {
        PlayerManager.instance.OnPlayerSpawned -= UpdatePlayerHealthText;
        PlayerManager.instance.OnPlayerSpawned -= UpdatePlayerCurrLivesText;
        PlayerManager.instance.OnPlayerSpawned -= UpdatePlayerMagCountText;
        PlayerManager.instance.OnPlayerDamaged -= UpdatePlayerHealthText;
        HenchmanManager.instance.OnHenchmanDamaged -= SpawnHenchmanDamageText;
        PlayerManager.instance.OnPlayerOOL -= EnableRespawnScreen;
        PlayerManager.instance.OnPlayerGunShot -= UpdatePlayerMagCountText;
    }

    public void OnRespawnClicked() {
        EventSystem.current.SetSelectedGameObject(null);
        SceneManager.LoadScene("GameScene"); // Player is OOL, reload scene
    }

    private void UpdatePlayerHealthText(object sender, EventArgs e) {
        Player player = PlayerManager.instance.Player;
        this._playerHealthText.text = $"HP: {player.CurrentHealth:F1}" +
                                      $"/{player.GetMaxHealth()}";
    }

    private void SpawnHenchmanDamageText(object sender, BulletDamageEventArgs e) {
        Henchman henchman = (Henchman) sender;
        Vector3 spawnPos = henchman.transform.position;
        TextMeshProUGUI damageText = Instantiate(this._henchmanDamageText,
            spawnPos, Quaternion.identity, this._canvas.transform);
        var dmgAmount = Mathf.RoundToInt(e.BulletDamage);
        damageText.text = $"{dmgAmount}";
        Destroy(damageText, 1.45f);
    }

    private void UpdatePlayerCurrLivesText(object sender, EventArgs e) {
        var currLives = PlayerManager.instance.CurrentLives;
        var maxLives = PlayerManager.instance.GetMaxLives();
        this._playerCurrentLives.text = $"Lives: {currLives}/{maxLives}";
    }

    private void UpdatePlayerMagCountText(object sender, AmmoEventArgs e) {
        this._magCountText.text = $"Bullets: {e.CurrentAmmo}/{e.MaxAmmo}";
    }

    private void EnableRespawnScreen(object sender, EventArgs e) {
        this._respawnScreen.SetActive(true);
    }
}
