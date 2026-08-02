using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character {
    [SerializeField] private float _jumpForce = 5f;

    public static Player instance;

    private Rigidbody2D _rb2d;
    private PWeaponManager _weaponManager;
    private Camera _cam;

    private InputAction _forward;
    private InputAction _backward;
    private bool _isOnGround;

    void Awake() {
        if (!instance) instance = this;
        this._cam = Camera.main;
        this._rb2d = this.GetComponent<Rigidbody2D>();
        this._weaponManager = this.GetComponentInChildren<PWeaponManager>();
        PlayerInput playerInput = this.GetComponent<PlayerInput>();
        this._forward = playerInput.actions.FindAction("Forward"); // Or "["Forward"]"
        this._backward = playerInput.actions.FindAction("Backward");
    }

    void FixedUpdate() {
        var direction = 0f;
        HandleLocalScale(); // Flips localScale if mouse is within FOV
        if (this._forward.IsPressed()) direction = 1f;
        if (this._backward.IsPressed()) direction = -1f;
        this._rb2d.linearVelocity = new Vector2(direction * this.moveSpeed,
            this._rb2d.linearVelocity.y);
    }

    private void HandleLocalScale() {
        Vector2 mousePos = this._cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dirToMouse = (mousePos - this._rb2d.position).normalized;
        // Absolute/Target angle
        var angleToMouse = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg;

        // If direction based on mousePos rotates the gun backwards
        Vector3 localScale = this.transform.localScale;
        var isMouseWithinFOV = Mathf.Abs(angleToMouse) <= (this.FOV / 2f);
        localScale.x = isMouseWithinFOV ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        this.transform.localScale = localScale;
    }

    public void OnJump() {
        if (!this._isOnGround) return;
        this._rb2d.AddForce(Vector2.up * this._jumpForce, ForceMode2D.Impulse);
        this._isOnGround = false;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Platform")) {
            this._isOnGround = true;
        }
    }
}
