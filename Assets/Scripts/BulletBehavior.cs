using UnityEngine;

public class BulletBehavior : MonoBehaviour {
    [SerializeField] private float _speed;

    private Rigidbody2D _rb2d;

    void Awake() {
        this._rb2d = this.GetComponent<Rigidbody2D>();
    }

    void Start() => Launch();
    private void Launch() {
        // V = direction * speed
        this._rb2d.linearVelocity = this.transform.up * this._speed;
        Destroy(this.gameObject, 3f);
    }

    public void OnTriggerEnter2D(Collider2D col) {
        Destroy(this.gameObject);
    }
}
