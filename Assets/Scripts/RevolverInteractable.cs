using UnityEngine;

public class RevolverInteractable : MonoBehaviour {
    [SerializeField] private PRevolverConfig revolver;
    public Transform InteractPromptPos {get; private set;}

    void Awake() {
        this.InteractPromptPos = this.transform.GetChild(0);
    }

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
}
