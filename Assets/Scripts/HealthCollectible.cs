using UnityEngine;

public class HeartCollectible : Collectible {
    [SerializeField] private float _healAmount;

    protected override void Collect() {
        Player player = PlayerManager.instance.Player;
        if (player.AddHealth(this._healAmount)) Destroy(this.gameObject);
    }
}
