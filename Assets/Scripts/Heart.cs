using UnityEngine;

public class Heart : Collectible {
    [SerializeField] private float _minHealAmount;
    [SerializeField] private float _maxHealAmount;

    protected override void OnPlayerCollected() {
        base.OnPlayerCollected();
        Player player = PlayerManager.instance.Player;
        player.GiveHeal(HealAmount);
    }

    private float HealAmount => Random.Range(this._minHealAmount, this._maxHealAmount);
}
