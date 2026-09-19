using UnityEngine;

public abstract class Collectible : MonoBehaviour {
    protected abstract void Collect();

    private void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.CompareTag("Player")) Collect();
    }
}
