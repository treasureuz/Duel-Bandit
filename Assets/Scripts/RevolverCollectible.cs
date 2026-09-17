using UnityEngine;

public class RevolverCollectible : Collectible {
    [SerializeField] private PRevolverConfig revolver;

    protected override void OnCollected() {
        Player player = (Player) PlayerManager.instance.Player;
        player.PickupRevolver(revolver);
        Destroy(this.gameObject);
    }
}
