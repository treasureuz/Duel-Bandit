using UnityEngine;

public class AmmoCollectible : Collectible {
    [SerializeField] private int _minAmmoAmount;
    [SerializeField] private int _maxAmmoAmount;

    protected override void Collect() {
        Player player = PlayerManager.instance.Player;
        if (player.AddRevolverAmmo(AmmoAmount)) Destroy(this.gameObject);
    }

    private int AmmoAmount =>
        Random.Range(this._minAmmoAmount, this._maxAmmoAmount + 1);
}
