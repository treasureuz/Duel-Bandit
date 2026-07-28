using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _jumpForce = 5f;

    public static Player instance;
    private Rigidbody2D _rb2d;

    private InputAction _forward;
    private InputAction _backward;
    private bool _isOnGround;

    void Awake() {
        if (!instance) instance = this;
        this._rb2d = this.GetComponent<Rigidbody2D>();
        PlayerInput playerInput = this.GetComponent<PlayerInput>();
        this._forward = playerInput.actions.FindAction("Forward"); // Or "["Forward"]"
        this._backward = playerInput.actions.FindAction("Backward");
    }

    void FixedUpdate() {
        var direction = 0f;
        if (this._forward.IsPressed()) direction = 1f;
        if (this._backward.IsPressed()) direction = -1f;
        this._rb2d.linearVelocity = new Vector2(direction * this._moveSpeed,
            this._rb2d.linearVelocity.y);
    }

    public void OnJump() {
        if (!this._isOnGround) return;
        this._rb2d.AddForce(Vector2.up * this._jumpForce, ForceMode2D.Impulse);
        this._isOnGround = false;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            this._isOnGround = true;
        }
    }
}
