using UnityEngine;

public class BulletBehavior : MonoBehaviour {
    private Rigidbody2D _rb2d;

    public float Damage { get; private set; }

    private const float _speed = 35f;
    private const float _timeBeforeDestroyed = 2f;

    void Awake() {
        this._rb2d = this.GetComponent<Rigidbody2D>();
    }

    void Start() => Launch();
    public void Init(float damage) {
        this.Damage = damage;
    }
    private void Launch() {
        // V = direction * speed
        this._rb2d.linearVelocity = this.transform.right * _speed;
        Destroy(this.gameObject, _timeBeforeDestroyed);
    }

    public virtual void OnCollisionEnter2D(Collision2D col) {
        Debug.Log(col.gameObject.name + " " + col.gameObject.layer);
        Destroy(this.gameObject);
    }
}
