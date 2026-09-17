using UnityEngine;

public abstract class Collectible : MonoBehaviour {
    protected abstract void OnCollected();

    private void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.CompareTag("Player")) OnCollected();
    }
}
