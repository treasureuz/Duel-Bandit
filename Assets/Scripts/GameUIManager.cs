using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour {
    [SerializeField] private Canvas _canvas;

    [Header("Player UI")]
    [SerializeField] private GameObject _respawnScreen;
    [SerializeField] private Image _playerHealthBar;
    [SerializeField] private TextMeshProUGUI _playerHealthText;
    [SerializeField] private List<Image> _heartIcons;
    [SerializeField] private TextMeshProUGUI _ammoText;

    [Header("Henchman UI")]
    [SerializeField] private TextMeshProUGUI _damageTextPrefab;
    [SerializeField] private Vector2 _damageTextOffset = new (0.5f, 0.5f);

    public static GameUIManager instance;

    void Awake() {
        if (!instance) instance = this;
        this._respawnScreen.SetActive(false);
    }

    void Start() {
        PlayerManager.instance.OnPlayerSpawned += UpdateOnPlayerSpawnedUI;
        PlayerManager.instance.OnPlayerDamaged += UpdatePlayerHealthUI;
        HenchmanManager.instance.OnHenchmanDamaged += SpawnOnHenchmanDamagedText;
        PlayerManager.instance.OnPlayerOOL += EnableRespawnScreen;
        PlayerManager.instance.OnPlayerGunShot += UpdatePlayerAmmoText;
    }

    void OnDisable() {
        PlayerManager.instance.OnPlayerSpawned -= UpdateOnPlayerSpawnedUI;
        PlayerManager.instance.OnPlayerDamaged -= UpdatePlayerHealthUI;
        HenchmanManager.instance.OnHenchmanDamaged -= SpawnOnHenchmanDamagedText;
        PlayerManager.instance.OnPlayerOOL -= EnableRespawnScreen;
        PlayerManager.instance.OnPlayerGunShot -= UpdatePlayerAmmoText;
    }

    public void OnRespawnClicked() {
        EventSystem.current.SetSelectedGameObject(null);
        SceneManager.LoadScene("GameScene"); // Player is OOL, reload scene
    }

    private void UpdateOnPlayerSpawnedUI(object sender, AmmoEventArgs e) {
        UpdatePlayerHealthUI(sender, e);
        UpdatePlayerCurrLivesUI(sender, e);
        UpdatePlayerAmmoText(sender, e);
    }

    private void UpdatePlayerHealthUI(object sender, EventArgs e) {
        Player player = (Player) sender;
        this._playerHealthBar.fillAmount = player.CurrentHealth/player.GetMaxHealth();
        this._playerHealthText.text = $"HP: {player.CurrentHealth:F1}";
    }

    private void UpdatePlayerCurrLivesUI(object sender, EventArgs e) {
        Player player = (Player) sender;
        var heartIconsCount = this._heartIcons.Count;
        // How many to set false
        var iconsToLivesDiff = heartIconsCount - player.CurrentLives;
        for (var i = 1; i <= iconsToLivesDiff; ++i) {
            this._heartIcons[heartIconsCount - i].gameObject.SetActive(false);
        }
    }

    private void UpdatePlayerAmmoText(object sender, AmmoEventArgs e) {
        this._ammoText.text = $"Bullets: {e.CurrentAmmo}/{e.MaxAmmo}";
    }

    private void SpawnOnHenchmanDamagedText(object sender, BulletDamageEventArgs e) {
        Henchman henchman = (Henchman) sender;
        Vector2 henchmanPos = henchman.transform.position;
        var spawnPosX = henchmanPos.x + (henchman.transform.localScale.x < 0
            ? -this._damageTextOffset.x : this._damageTextOffset.x);
        var spawnPosY = henchmanPos.y + this._damageTextOffset.y;
        Vector3 spawnPos = new(spawnPosX, spawnPosY);
        TextMeshProUGUI damageText = Instantiate(this._damageTextPrefab, spawnPos,
            Quaternion.identity, this._canvas.transform);
        var dmgAmount = Mathf.RoundToInt(e.BulletDamage);
        damageText.text = $"{dmgAmount}";
        Destroy(damageText.gameObject, 0.25f);
    }

    private void EnableRespawnScreen(object sender, EventArgs e) {
        this._respawnScreen.SetActive(true);
    }
}
