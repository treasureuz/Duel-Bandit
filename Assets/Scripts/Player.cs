using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character<PWeaponManager> {
    [SerializeField] private float _jumpForce = 5f;

    public static Player instance;

    private Camera _cam;
    private InputAction _forward;
    private InputAction _backward;
    private bool _isOnGround;

    protected override void Awake() {
        if (!instance) instance = this;
        base.Awake();
        this._cam = Camera.main;
        PlayerInput playerInput = this.GetComponent<PlayerInput>();
        this._forward = playerInput.actions.FindAction("Forward"); // Or "["Forward"]"
        this._backward = playerInput.actions.FindAction("Backward");
    }

    void FixedUpdate() {
        var direction = 0f;
        this.weaponManager.MeasureAngleToTargetPos();
        HandleLocalScale(); // Flips localScale if mouse is within FOV
        this.weaponManager.ApplyRotation();
        if (this._forward.IsPressed()) direction = 1f;
        if (this._backward.IsPressed()) direction = -1f;
        this.rb2d.linearVelocity = new Vector2(direction * this.moveSpeed,
            this.rb2d.linearVelocity.y);
    }

    protected override void HandleLocalScale() {
        Vector2 mousePos = this._cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dirToMouse = (mousePos - this.rb2d.position).normalized;
        FlipScaleWithDir(dirToMouse); // Uses dirToMouse to flip localScale
    }

    public void OnJump() {
        if (!this._isOnGround) return;
        this.rb2d.AddForce(Vector2.up * this._jumpForce, ForceMode2D.Impulse);
        this._isOnGround = false;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Platform")) {
            this._isOnGround = true;
        }
    }
}
