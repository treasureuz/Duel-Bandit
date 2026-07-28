using System;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _jumpForce = 5f;

    private Rigidbody2D _rb2d;

    private bool _isMovingForward;
    private bool _isMovingBackward;
    private bool _isOnGround;

    void Awake() {
        this._rb2d = this.GetComponent<Rigidbody2D>();
    }

    void OnEnable() {
        InputManager.instance.OnForwardStarted += HandleForwardStarted;
        InputManager.instance.OnForwardStopped += HandleForwardStopped;
        InputManager.instance.OnBackwardStarted += HandleBackwardStarted;
        InputManager.instance.OnBackwardStopped += HandleBackwardStopped;
        InputManager.instance.OnJumped += HandleJump;
    }

    void OnDisable() {
        InputManager.instance.OnForwardStarted -= HandleForwardStarted;
        InputManager.instance.OnForwardStopped -= HandleForwardStopped;
        InputManager.instance.OnBackwardStarted -= HandleBackwardStarted;
        InputManager.instance.OnBackwardStopped -= HandleBackwardStopped;
        InputManager.instance.OnJumped -= HandleJump;
    }

    void FixedUpdate() {
        if (this._isMovingForward) {
            this._rb2d.linearVelocity = new Vector2(this._moveSpeed,
                this._rb2d.linearVelocity.y);
        }
        if (this._isMovingBackward) {
            this._rb2d.linearVelocity = new Vector2(-this._moveSpeed,
                this._rb2d.linearVelocity.y);
        }
    }

    private void HandleForwardStarted(object sender, EventArgs e) {
        this._isMovingForward = true;
    }

    private void HandleForwardStopped(object sender, EventArgs e) {
        this._isMovingForward = false;
    }

    private void HandleBackwardStarted(object sender, EventArgs e) {
        this._isMovingBackward = true;
    }

    private void HandleBackwardStopped(object sender, EventArgs e) {
        this._isMovingBackward = false;
    }

    private void HandleJump(object sender, EventArgs e) {
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
