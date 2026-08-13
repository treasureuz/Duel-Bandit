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
    [SerializeField] private TextMeshProUGUI _ammoText;

    [Header("Henchman UI")]
    [SerializeField] private TextMeshProUGUI _damageTextPrefab;
    [SerializeField] private Vector2 _damageTextOffset = new (0.5f, 0.5f);
    //[SerializeField] private float _henchmanHealthTextOffsetY = 1f;
    private TextMeshProUGUI _henchmanHealthText; // Switch text -> bar

    public static GameUIManager instance;

    void Awake() {
        if (!instance) instance = this;
        this._respawnScreen.SetActive(false);
    }

    void Start() {
        PlayerManager.instance.OnPlayerSpawned += UpdateUIOnPlayerSpawned;
        PlayerManager.instance.OnPlayerDamaged += UpdatePlayerHealthText;
        HenchmanManager.instance.OnHenchmanDamaged += SpawnOnHenchmanDamagedText;
        //HenchmanManager.instance.OnHenchmanDamaged += UpdateHenchmanHealthUI;
        PlayerManager.instance.OnPlayerOOL += EnableRespawnScreen;
        PlayerManager.instance.OnPlayerGunShot += UpdatePlayerAmmoText;
    }

    void OnDisable() {
        PlayerManager.instance.OnPlayerSpawned -= UpdateUIOnPlayerSpawned;
        PlayerManager.instance.OnPlayerDamaged -= UpdatePlayerHealthText;
        HenchmanManager.instance.OnHenchmanDamaged -= SpawnOnHenchmanDamagedText;
        //HenchmanManager.instance.OnHenchmanDamaged -= UpdateHenchmanHealthUI;
        PlayerManager.instance.OnPlayerOOL -= EnableRespawnScreen;
        PlayerManager.instance.OnPlayerGunShot -= UpdatePlayerAmmoText;
    }

    public void OnRespawnClicked() {
        EventSystem.current.SetSelectedGameObject(null);
        SceneManager.LoadScene("GameScene"); // Player is OOL, reload scene
    }

    private void UpdateUIOnPlayerSpawned(object sender, AmmoEventArgs e) {
        UpdatePlayerHealthText(sender, e);
        UpdatePlayerCurrLivesText(sender, e);
        UpdatePlayerAmmoText(sender, e);
    }

    private void UpdatePlayerHealthText(object sender, EventArgs e) {
        Player player = (Player) sender;
        this._playerHealthText.text = $"HP: {player.CurrentHealth:F1}" +
                                      $"/{player.GetMaxHealth()}";
    }

    private void UpdateHenchmanHealthUI(object sender, EventArgs e) {
        Henchman henchman = (Henchman) sender;
        Vector2 spawnPos = henchman.GetHealthBarPos().position;
        if (!this._henchmanHealthText) {
            this._henchmanHealthText = Instantiate(this._damageTextPrefab, spawnPos,
                Quaternion.identity, this._canvas.transform);
        }
        this._henchmanHealthText.rectTransform.position = spawnPos;
        this._henchmanHealthText.text = $"HP: {henchman.CurrentHealth:F1}" +
                                        $"/{henchman.GetMaxHealth()}";
    }

    private void SpawnOnHenchmanDamagedText(object sender, BulletDamageEventArgs e) {
        Henchman henchman = (Henchman) sender;
        Vector2 spawnPos = (Vector2)henchman.transform.position + this._damageTextOffset;
        TextMeshProUGUI damageText = Instantiate(this._damageTextPrefab, spawnPos,
            Quaternion.identity, this._canvas.transform);
        var dmgAmount = Mathf.RoundToInt(e.BulletDamage);
        damageText.text = $"{dmgAmount}";
        Destroy(damageText, 0.45f);
    }

    private void UpdatePlayerCurrLivesText(object sender, EventArgs e) {
        Player player = (Player) sender;
        this._playerCurrentLives.text = $"Lives: {player.CurrentLives}/{player.MaxLives}";
    }

    private void UpdatePlayerAmmoText(object sender, AmmoEventArgs e) {
        this._ammoText.text = $"Bullets: {e.CurrentAmmo}/{e.MaxAmmo}";
    }

    private void EnableRespawnScreen(object sender, EventArgs e) {
        this._respawnScreen.SetActive(true);
    }
}
