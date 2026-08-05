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
        Player.instance.OnDamaged += UpdatePlayerHealthText;
        Player.instance.GunManager.OnShot += UpdateMagazineCount;
    }

    void OnDisable() {
        Player.instance.OnDamaged -= UpdatePlayerHealthText;
        Player.instance.GunManager.OnShot -= UpdateMagazineCount;
    }

    private void UpdatePlayerHealthText(object sender, EventArgs e) {
        this._playerHealthText.text = $"HP: {Player.instance.GetCurrentHealth():F1}" +
                                      $"/{Player.instance.GetMaxHealth()}";
    }

    private void UpdateMagazineCount(object sender, EventArgs e) {
        PGunManager gunManager = Player.instance.GunManager;
        this._magCountText.text = $"Bullets: {gunManager.GetCurrentMagCount()}" +
                                  $"/{gunManager.GetMaxMagCount()}";
    }
}
