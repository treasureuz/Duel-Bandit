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
    [Header("Player UI")]
    [SerializeField] private GameObject _respawnScreen;
    [SerializeField] private List<Image> _heartIcons;
    [SerializeField] private Image _playerHealthBar;
    [SerializeField] private Image _reloadDurationBar;
    [SerializeField] private TextMeshProUGUI _playerHealthText;
    [SerializeField] private TextMeshProUGUI _ammoText;
    [SerializeField] private TextMeshProUGUI _revolverStatusText;
    [SerializeField] private TextMeshProUGUI _revolverNameText;

    [Header("Henchman UI")]
    [SerializeField] private Canvas _damageTextCanvasPrefab;
    [SerializeField] private Vector2 _damageTextOffset = new (0.5f, 0.5f);

    [Header("Other References")]
    [SerializeField] private TextMeshProUGUI _timerText;

    public static GameUIManager instance;

    private bool _isInUnableToReload;

    void Awake() {
        if (!instance) instance = this;
        this._respawnScreen.SetActive(false);
        SetPlayerRevolverReloadUI(false);
    }

    void Start() {
        PlayerManager.instance.OnPlayerSpawned += UpdateOnPlayerSpawnedUI;
        PlayerManager.instance.OnPlayerHealthChange += UpdatePlayerHealthUI;
        HenchmanHelper.instance.OnHenchmanDamaged += DisplayPlayerDamageNumbers;
        PlayerManager.instance.OnPlayerOOL += EnableRespawnScreen;
        PlayerManager.instance.OnPlayerRevolverEquipped += UpdatePlayerRevolverEquippedUI;
        PlayerManager.instance.OnPlayerRevolverShot += UpdatePlayerAmmoText;
        PlayerManager.instance.OnPlayerRevolverAmmoChanged += UpdatePlayerAmmoText;
        PlayerManager.instance.OnPlayerRevolverReloading += DisplayPlayerReloadingUI;
        PlayerManager.instance.OnPlayerRevolverOutOfAmmo += DisplayPlayerRevolverOutOfAmmoText;
        PlayerManager.instance.OnPlayerRevolverUnableToReload += DisplayPlayerUnableToReloadText;
    }

    void OnDisable() {
        PlayerManager.instance.OnPlayerSpawned -= UpdateOnPlayerSpawnedUI;
        PlayerManager.instance.OnPlayerHealthChange -= UpdatePlayerHealthUI;
        HenchmanHelper.instance.OnHenchmanDamaged -= DisplayPlayerDamageNumbers;
        PlayerManager.instance.OnPlayerOOL -= EnableRespawnScreen;
        PlayerManager.instance.OnPlayerRevolverEquipped -= UpdatePlayerRevolverEquippedUI;
        PlayerManager.instance.OnPlayerRevolverShot -= UpdatePlayerAmmoText;
        PlayerManager.instance.OnPlayerRevolverAmmoChanged -= UpdatePlayerAmmoText;
        PlayerManager.instance.OnPlayerRevolverReloading -= DisplayPlayerReloadingUI;
        PlayerManager.instance.OnPlayerRevolverUnableToReload -= DisplayPlayerUnableToReloadText;
        PlayerManager.instance.OnPlayerRevolverOutOfAmmo += DisplayPlayerRevolverOutOfAmmoText;
    }

    public void UpdateTimerText(float timer) {
        var min = Mathf.FloorToInt(timer / 60);
        var sec = Mathf.RoundToInt(timer % 60);
        // "00" - makes min and sec two digits for any int
        // e.g. min = 3 -> 03, sec = 12 -> 12
        this._timerText.text = $"Timer - {min:00}:{sec:00}";
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
        UpdatePlayerRevolverDisplayInfo(e.revolverDisplayInfoEArgs);
        UpdatePlayerAmmoText(sender, e.ammoEventArgs);
    }
    private void UpdatePlayerRevolverDisplayInfo(RevolverDisplayInfoEventArgs e) {
        this._revolverNameText.text = $"{e.RevolverName}";
        this._revolverNameText.color = e.RevolverColor;
    }

    private void UpdatePlayerAmmoText(object sender, PlayerAmmoEventArgs args) {
        PRevolverData e = args.RevolverData;
        this._ammoText.text = e.CurrentTotalAmmo == int.MaxValue ? "INF/INF (INF)" :
            $"{e.CurrentMagCount}/{e.CurrentReserveAmmo} ({e.CurrentTotalAmmo})";
    }

    private void DisplayPlayerReloadingUI(object sender, RevolverReloadingEventArgs e) {
        this._revolverStatusText.text = "*Reloading...*";
        SetPlayerRevolverReloadUI(true);
        StartCoroutine(CountdownPlayerReloadDur(e.ReloadDuration));
    }
    private IEnumerator CountdownPlayerReloadDur(float duration) {
        var remainingDur = duration;
        while (remainingDur > 0) {
            this._reloadDurationBar.fillAmount = remainingDur/duration;
            remainingDur -= Time.deltaTime;
            yield return null;
        }
        SetPlayerRevolverReloadUI(false);
    }

    private void DisplayPlayerRevolverOutOfAmmoText(object sender, EventArgs e) {
        this._revolverStatusText.text = "*Out of ammo*";
    }

    private void DisplayPlayerUnableToReloadText(object sender, EventArgs e) {
        StartCoroutine(HandlePlayerUnableToReloadText());
    }
    private IEnumerator HandlePlayerUnableToReloadText() {
        if (this._isInUnableToReload) yield break;

        this._isInUnableToReload = true;
        this._revolverStatusText.text = "*Unable to reload.*";
        this._revolverStatusText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        this._revolverStatusText.gameObject.SetActive(false);
        this._isInUnableToReload = false;
    }

    // Called on Henchman damaged
    private void DisplayPlayerDamageNumbers(object sender, DamageTakenEventArgs e) {
        Henchman henchman = (Henchman) sender;
        Vector2 henchmanPos = henchman.transform.position;

        // Calculates spawnPos depending on the dir the Henchman was shot
        var spawnPosX = henchmanPos.x + (henchman.transform.localScale.x < 0
            ? -this._damageTextOffset.x : this._damageTextOffset.x);
        var spawnPosY = henchmanPos.y + this._damageTextOffset.y;
        Vector3 spawnPos = new(spawnPosX, spawnPosY);

        // Spawns and sets the damageText
        Canvas damageTextCanvas = Instantiate
            (this._damageTextCanvasPrefab, spawnPos, Quaternion.identity);
        TextMeshProUGUI damageText = damageTextCanvas.GetComponentInChildren<TextMeshProUGUI>();
        damageText.text = $"{Mathf.RoundToInt(e.Damage)}";
        Destroy(damageTextCanvas.gameObject, 0.25f);
    }

    public void OnRespawnClicked() {
        EventSystem.current.SetSelectedGameObject(null);
        SceneManager.LoadScene("GameScene"); // Player is OOL, reload scene
    }
    private void EnableRespawnScreen(object sender, EventArgs e) {
        this._respawnScreen.SetActive(true);
    }

    private void SetPlayerRevolverReloadUI(bool b) {
        this._reloadDurationBar.transform.parent.gameObject.SetActive(b);
        this._revolverStatusText.gameObject.SetActive(b);
    }
}
