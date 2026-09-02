using UnityEngine;

public class HeartCollectible : Collectible {
    [SerializeField] private float _minHealAmount;
    [SerializeField] private float _maxHealAmount;

    protected override void OnCollected() {
        base.OnCollected();
        Player player = PlayerManager.instance.Player;
        player.AddHealth(HealAmount);
    }

    private float HealAmount => Random.Range(this._minHealAmount, this._maxHealAmount);
}
