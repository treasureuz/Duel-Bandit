using UnityEngine;

public class Collectible : MonoBehaviour {
    protected virtual void OnPlayerCollected() {
        Destroy(this.gameObject);
    }

    public void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.CompareTag("Player")) OnPlayerCollected();
    }
}
