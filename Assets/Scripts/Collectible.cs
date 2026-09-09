using UnityEngine;

public class Collectible : MonoBehaviour {
    protected virtual void OnCollected() {
        Destroy(this.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.CompareTag("Player")) OnCollected();
    }
}
