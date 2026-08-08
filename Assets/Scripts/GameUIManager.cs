using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour {
    [Header("Player Texts")]
    [SerializeField] private GameObject _respawnScreen;
    [SerializeField] private TextMeshProUGUI _playerHealthText;
    [SerializeField] private TextMeshProUGUI _playerCurrentLives;
    [SerializeField] private TextMeshProUGUI _magCountText;

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
        PlayerManager.instance.OnPlayerOOL += EnableRespawnScreen;
        PlayerManager.instance.OnPlayerGunShot += UpdatePlayerMagCountText;
    }

    void OnDisable() {
        PlayerManager.instance.OnPlayerSpawned -= UpdatePlayerHealthText;
        PlayerManager.instance.OnPlayerSpawned -= UpdatePlayerCurrLivesText;
        PlayerManager.instance.OnPlayerSpawned -= UpdatePlayerMagCountText;
        PlayerManager.instance.OnPlayerDamaged -= UpdatePlayerHealthText;
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

    private void UpdatePlayerCurrLivesText(object sender, EventArgs e) {
        var currLives = PlayerManager.instance.CurrentLives;
        var maxLives = PlayerManager.instance.GetMaxLives();
        this._playerCurrentLives.text = $"Lives: {currLives}/{maxLives}";
    }

    private void UpdatePlayerMagCountText(object sender, EventArgs e) {
        Player player = PlayerManager.instance.Player;
        this._magCountText.text = $"Bullets: {player.GunManager.CurrentMagCount}" +
                                  $"/{player.GunManager.GetMaxMagCount()}";
    }

    private void EnableRespawnScreen(object sender, EventArgs e) {
        this._respawnScreen.SetActive(true);
    }
}
