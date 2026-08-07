using System;
using TMPro;
using UnityEngine;

public class GameUIManager : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _playerHealthText;
    [SerializeField] private TextMeshProUGUI _magCountText;

    public static GameUIManager instance;

    void Awake() {
        if (!instance) instance = this;
    }

    void Start() {
        PlayerManager.instance.OnPlayerSpawned += UpdatePlayerHealthText;
        PlayerManager.instance.OnPlayerSpawned += UpdateMagazineCount;
        PlayerManager.instance.OnPlayerDamaged += UpdatePlayerHealthText;
        PlayerManager.instance.OnPlayerGunShot += UpdateMagazineCount;
    }

    void OnDisable() {
        PlayerManager.instance.OnPlayerSpawned -= UpdatePlayerHealthText;
        PlayerManager.instance.OnPlayerSpawned -= UpdateMagazineCount;
        PlayerManager.instance.OnPlayerDamaged -= UpdatePlayerHealthText;
        PlayerManager.instance.OnPlayerGunShot -= UpdateMagazineCount;
    }

    private void UpdatePlayerHealthText(object sender, EventArgs e) {
        Player player = PlayerManager.instance.Player;
        this._playerHealthText.text = $"HP: {player.CurrentHealth:F1}" +
                                      $"/{player.GetMaxHealth()}";
    }

    private void UpdateMagazineCount(object sender, EventArgs e) {
        Player player = PlayerManager.instance.Player;
        this._magCountText.text = $"Bullets: {player.GunManager.CurrentMagCount}" +
                                  $"/{player.GunManager.GetMaxMagCount()}";
    }
}
