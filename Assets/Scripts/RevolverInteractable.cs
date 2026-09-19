using UnityEngine;

public class RevolverInteractable : MonoBehaviour {
    [SerializeField] private Transform _interactPromptPos;
    [SerializeField] private PRevolverConfig revolver;

    public void Equip() {
        Player player = PlayerManager.instance.Player;
        player.EquipRevolver(revolver);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (!col.CompareTag("Player")) return;
        InteractableManager.instance.ShowInteractable(this);
    }

    private void OnTriggerExit2D(Collider2D col) {
        InteractableManager.instance.HideInteractable();
    }

    public Vector2 GetInteractPromptPos() => this._interactPromptPos.position;
}
