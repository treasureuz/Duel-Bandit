using UnityEngine;

public class RevolverCollectible : Collectible {
    [SerializeField] private PRevolverData revolver;

    protected override void OnCollected() {
        Player player = (Player) PlayerManager.instance.Player;
        player.EquipRevolver(revolver);
        base.OnCollected();
    }
}
