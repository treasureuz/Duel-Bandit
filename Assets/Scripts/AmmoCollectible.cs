using UnityEngine;

public class AmmoCollectible : Collectible {
    [SerializeField] private int _minAmmoAmount;
    [SerializeField] private int _maxAmmoAmount;

    protected override void OnCollected() {
        base.OnCollected();
        Player player = PlayerManager.instance.Player;
        player.AddAmmo(AmmoAmount);
    }

    private int AmmoAmount => Random.Range(this._minAmmoAmount, this._maxAmmoAmount + 1);
}
