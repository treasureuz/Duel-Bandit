using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using RevolverEventArgs;
using System.Collections;

public class GameUIManager : MonoBehaviour {
    [SerializeField] private Canvas _canvas;

    [Header("Player UI")]
    [SerializeField] private GameObject _respawnScreen;
    [SerializeField] private GameObject _reloadUIObj;
    [SerializeField] private List<Image> _heartIcons;
    [SerializeField] private Image _playerHealthBar;
    [SerializeField] private Image _reloadDurationBar;
    [SerializeField] private TextMeshProUGUI _playerHealthText;
    [SerializeField] private TextMeshProUGUI _ammoText;
    [SerializeField] private TextMeshProUGUI _reloadDurationText;
    [SerializeField] private TextMeshProUGUI _reloadStatusText;
    [SerializeField] private TextMeshProUGUI _revolverNameText;

    [Header("Henchman UI")]
    [SerializeField] private TextMeshProUGUI _damageTextPrefab;
    [SerializeField] private Vector2 _damageTextOffset = new (0.5f, 0.5f);

    public static GameUIManager instance;

    private bool _isInUnableToReload;

    void Awake() {
        if (!instance) instance = this;
        this._respawnScreen.SetActive(false);
        ResetPlayerReloadUI();
    }

    void Start() {
        PlayerManager.instance.OnPlayerSpawned += UpdateOnPlayerSpawnedUI;
        PlayerManager.instance.OnPlayerHealthChange += UpdatePlayerHealthUI;
        HenchmanHelper.instance.OnHenchmanDamaged += DisplayPlayerDamageNumbers;
        PlayerManager.instance.OnPlayerOOL += EnableRespawnScreen;
        PlayerManager.instance.OnPlayerRevolverEquipped += UpdatePlayerRevolverEquippedUI;
        PlayerManager.instance.OnPlayerRevolverShot += UpdatePlayerAmmoText;
        PlayerManager.instance.OnPlayerReloading += DisplayPlayerReloadingUI;
        PlayerManager.instance.OnPlayerUnableToReload += DisplayPlayerUnableToReloadText;
    }

    void OnDisable() {
        PlayerManager.instance.OnPlayerSpawned -= UpdateOnPlayerSpawnedUI;
        PlayerManager.instance.OnPlayerHealthChange -= UpdatePlayerHealthUI;
        HenchmanHelper.instance.OnHenchmanDamaged -= DisplayPlayerDamageNumbers;
        PlayerManager.instance.OnPlayerOOL -= EnableRespawnScreen;
        PlayerManager.instance.OnPlayerRevolverEquipped -= UpdatePlayerRevolverEquippedUI;
        PlayerManager.instance.OnPlayerRevolverShot -= UpdatePlayerAmmoText;
        PlayerManager.instance.OnPlayerReloading -= DisplayPlayerReloadingUI;
        PlayerManager.instance.OnPlayerUnableToReload -= DisplayPlayerUnableToReloadText;
    }

    public void OnRespawnClicked() {
        EventSystem.current.SetSelectedGameObject(null);
        SceneManager.LoadScene("GameScene"); // Player is OOL, reload scene
    }

    private void UpdateOnPlayerSpawnedUI(object sender, EventArgs e) {
        UpdatePlayerHealthUI(sender, e);
        UpdatePlayerCurrLivesUI(sender, e);
    }
    private void UpdatePlayerHealthUI(object sender, EventArgs e) {
        Player player = (Player) sender;
        var playerCurrentHealth = Mathf.RoundToInt(player.CurrentHealth);
        var playerMaxHealth = Mathf.RoundToInt(player.GetMaxHealth());
        this._playerHealthBar.fillAmount = player.CurrentHealth/player.GetMaxHealth();
        this._playerHealthText.text = $"HP: {playerCurrentHealth}/{playerMaxHealth}";
    }
    private void UpdatePlayerCurrLivesUI(object sender, EventArgs e) {
        Player player = (Player) sender;
        var heartIconsCount = this._heartIcons.Count;
        // How many heartIcons to disable
        var iconsToLivesDiff = heartIconsCount - player.CurrentLives;
        for (var i = 1; i <= iconsToLivesDiff; ++i) {
            this._heartIcons[heartIconsCount - i].color = Color.black;
        }
    }

    private void UpdatePlayerRevolverEquippedUI(object sender, RevolverEquippedEventArgs e) {
        UpdatePlayerRevolverDisplayInfo(sender, e.revolverDisplayInfoEArgs);
        UpdatePlayerAmmoText(sender, e.ammoEventArgs);
    }
    private void UpdatePlayerRevolverDisplayInfo(object sender, RevolverDisplayInfoEventArgs e) {
        this._revolverNameText.text = $"{e.RevolverName}";
        this._revolverNameText.color = e.RevolverColor;
    }
    private void UpdatePlayerAmmoText(object sender, AmmoEventArgs e) {
        this._ammoText.text = e.TotalAmmo == int.MaxValue ? "INF/INF (INF)" :
            $"{e.CurrentMagCount}/{e.ReserveAmmo} ({e.TotalAmmo})";
    }

    private void DisplayPlayerReloadingUI(object sender, RevolverReloadingEventArgs e) {
        var revolverReloadDur = e.ReloadDuration;

        this._reloadUIObj.SetActive(true);
        SetPlayerReloadDurUI(true);
        this._reloadStatusText.text = "*Reloading...*";

        StartCoroutine(HandlePlayerReloadDurCountdown(revolverReloadDur));
    }
    private IEnumerator HandlePlayerReloadDurCountdown(float duration) {
        var remainingDur = duration;
        while (remainingDur > 0) {
            this._reloadDurationBar.fillAmount = remainingDur/duration;
            this._reloadDurationText.text = $"{remainingDur:F2}s";
            remainingDur -= Time.deltaTime;
            yield return null;
        }
        ResetPlayerReloadUI();
    }

    private void DisplayPlayerUnableToReloadText(object sender, EventArgs e) {
        StartCoroutine(HandlePlayerUnableToReloadText(2f));
    }
    private IEnumerator HandlePlayerUnableToReloadText(float duration) {
        if (this._isInUnableToReload) yield break;

        this._isInUnableToReload = true;
        this._reloadUIObj.SetActive(true);
        this._reloadStatusText.text = "*Unable to reload.*";

        yield return new WaitForSeconds(duration);

        this._reloadUIObj.SetActive(false);
        this._isInUnableToReload = false;
    }

    // Called on Henchman damaged
    private void DisplayPlayerDamageNumbers(object sender, DamageTakenEventArgs e) {
        Henchman henchman = (Henchman) sender;
        Vector2 henchmanPos = henchman.transform.position;
        var spawnPosX = henchmanPos.x + (henchman.transform.localScale.x < 0
            ? -this._damageTextOffset.x : this._damageTextOffset.x);
        var spawnPosY = henchmanPos.y + this._damageTextOffset.y;
        Vector3 spawnPos = new(spawnPosX, spawnPosY);
        TextMeshProUGUI damageText = Instantiate(this._damageTextPrefab, spawnPos,
            Quaternion.identity, this._canvas.transform);
        var dmgAmount = Mathf.RoundToInt(e.Damage);
        damageText.text = $"{dmgAmount}";
        Destroy(damageText.gameObject, 0.25f);
    }

    private void EnableRespawnScreen(object sender, EventArgs e) {
        this._respawnScreen.SetActive(true);
    }

    private void SetPlayerReloadDurUI(bool b) {
        this._reloadDurationBar.transform.parent.gameObject.SetActive(b);
    }

    private void ResetPlayerReloadUI() {
        this._reloadUIObj.SetActive(false);
        SetPlayerReloadDurUI(false);
    }
}
