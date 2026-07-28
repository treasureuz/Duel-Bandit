using UnityEngine;

public class Henchman : MonoBehaviour {
    private Rigidbody2D _rb2d;

    void Awake() {
        this._rb2d = this.GetComponent<Rigidbody2D>();
    }
}
