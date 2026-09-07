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
    [SerializeField] private Image _playerHealthBar;
    [SerializeField] private GameObject _reloadUI;
    [SerializeField] private Image _reloadDurationBar;
    [SerializeField] private List<Image> _heartIcons;
    [SerializeField] private TextMeshProUGUI _playerHealthText;
    [SerializeField] private TextMeshProUGUI _ammoText;
    [SerializeField] private TextMeshProUGUI _reloadDurationText;
    [SerializeField] private TextMeshProUGUI _reloadText;
    [SerializeField] private TextMeshProUGUI _revolverNameText;

    [Header("Henchman UI")]
    [SerializeField] private TextMeshProUGUI _damageTextPrefab;
    [SerializeField] private Vector2 _damageTextOffset = new (0.5f, 0.5f);

    public static GameUIManager instance;

    void Awake() {
        if (!instance) instance = this;
        this._respawnScreen.SetActive(false);
        //DisableReloadUI();
    }

    void Start() {
        PlayerManager.instance.OnPlayerSpawned += UpdateOnPlayerSpawnedUI;
        PlayerManager.instance.OnPlayerHealthChange += UpdatePlayerHealthUI;
        HenchmanHelper.instance.OnHenchmanDamaged += DisplayPlayerDamageNumbers;
        PlayerManager.instance.OnPlayerOOL += EnableRespawnScreen;
        PlayerManager.instance.OnPlayerRevolverEquipped += UpdatePlayerRevolverDisplayInfo;
        PlayerManager.instance.OnPlayerRevolverAmmoChanged += UpdatePlayerAmmoText;
        // PlayerManager.instance.OnPlayerCantReload += DisplayPlayerCantReloadText;
        // PlayerManager.instance.OnPlayerReloading += DisplayPlayerReloadingUI;
    }

    void OnDisable() {
        PlayerManager.instance.OnPlayerSpawned -= UpdateOnPlayerSpawnedUI;
        PlayerManager.instance.OnPlayerHealthChange -= UpdatePlayerHealthUI;
        HenchmanHelper.instance.OnHenchmanDamaged -= DisplayPlayerDamageNumbers;
        PlayerManager.instance.OnPlayerOOL -= EnableRespawnScreen;
        PlayerManager.instance.OnPlayerRevolverEquipped -= UpdatePlayerRevolverDisplayInfo;
        PlayerManager.instance.OnPlayerRevolverAmmoChanged -= UpdatePlayerAmmoText;
        // PlayerManager.instance.OnPlayerReloading -= DisplayPlayerReloadingUI;
        // PlayerManager.instance.OnPlayerCantReload -= DisplayPlayerCantReloadText;
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
        this._playerHealthBar.fillAmount = playerCurrentHealth/playerMaxHealth;
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

    private void UpdatePlayerRevolverDisplayInfo(object sender, RevolverDisplayInfoEventArgs e) {
        Debug.Log("Revolver equipped");
        this._revolverNameText.text = e.RevolverName;
        //this._revolverNameText.color = e.RevolverColor;
    }

    private void UpdatePlayerAmmoText(object sender, AmmoEventArgs e) {
        this._ammoText.text = e.TotalAmmo == int.MaxValue ? "INF/INF (INF)" :
            $"{e.CurrentMagCount}/{e.ReserveAmmo} ({e.TotalAmmo})";
    }

    // private void DisplayPlayerReloadingUI(object sender, EventArgs e) {
    //     Player player = (Player) sender;
    //     var revolverReloadDur = player.GetRevolverReloadDuration();

    //     this._reloadUI.SetActive(true);
    //     SetPlayerReloadDurationUI(true);
    //     this._reloadText.text = "*Reloading...*";

    //     StartCoroutine(HandlePlayerReloadDurCountdown(revolverReloadDur));
    // }
    // private IEnumerator HandlePlayerReloadDurCountdown(float duration) {
    //     var remainingDur = duration;
    //     while (remainingDur > 0) {
    //         this._reloadDurationBar.fillAmount = remainingDur/duration;
    //         this._reloadDurationText.text = $"{remainingDur:F2}s";
    //         remainingDur -= Time.deltaTime;
    //         yield return null;
    //     }
    //     DisableReloadUI();
    // }

    // private void DisplayPlayerCantReloadText(object sender, EventArgs e) {
    //     Player player = (Player) sender;
    //     var revolverReloadDur = player.GetRevolverReloadDuration();

    //     StartCoroutine(HandlePlayerCantReloadText(revolverReloadDur));
    // }
    // private IEnumerator HandlePlayerCantReloadText(float duration) {
    //     this._reloadUI.SetActive(true);
    //     this._reloadText.text = "*Can't reload.*";
    //     yield return new WaitForSeconds(duration);
    //     this._reloadUI.SetActive(false);
    // }

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

    // private void SetPlayerReloadDurationUI(bool b) {
    //     this._reloadDurationBar.transform.parent.gameObject.SetActive(b);
    // }

    // private void DisableReloadUI() {
    //     this._reloadUI.SetActive(false);
    //     SetPlayerReloadDurationUI(false);
    // }
}
