using UnityEngine;

public class HeartCollectible : Collectible {
    [SerializeField] private float _minHealAmount;
    [SerializeField] private float _maxHealAmount;

    protected override void OnCollected() {
        Player player = PlayerManager.instance.Player;
        if (player.AddHealth(HealAmount))
            Destroy(this.gameObject);
    }

    private float HealAmount => Random.Range(this._minHealAmount, this._maxHealAmount);
}
