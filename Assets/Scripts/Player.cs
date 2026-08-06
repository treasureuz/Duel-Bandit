using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : Character<PGunManager> {
    [Header("Settings")]
    [SerializeField] private int _maxLives = 3;
    [SerializeField] private float _jumpForce = 5f;

    private InputAction _forward;
    private InputAction _backward;

    private int _currentLives;
    private bool _isOnGround;

    protected override void Awake() {
        base.Awake();
        PlayerInput playerInput = this.GetComponent<PlayerInput>();
        this._forward = playerInput.actions.FindAction("Forward"); // Or "["Forward"]"
        this._backward = playerInput.actions.FindAction("Backward");
    }

    protected override void Start() {
        base.Start();
        this._currentLives = this._maxLives;
    }

    void FixedUpdate() {
        var direction = 0f;
        this.gunManager.MeasureAngleToTargetPos();
        HandleLocalScale(); // Flips localScale if mouse is within FOV
        this.gunManager.ApplyRotation();
        if (this._forward.IsPressed()) direction = 1f;
        if (this._backward.IsPressed()) direction = -1f;
        this.rb2d.linearVelocity = new Vector2(direction * this.moveSpeed,
            this.rb2d.linearVelocity.y);
    }

    public void OnJump() {
        if (!this._isOnGround) return;
        this.rb2d.AddForce(Vector2.up * this._jumpForce, ForceMode2D.Impulse);
        this._isOnGround = false;
    }

    protected override void HandleLocalScale() {
        Vector2 mousePos = this.gunManager.GetTargetPos(); // No need for recalculation
        var dirXToMouse = mousePos.x - this.rb2d.position.x;
        Vector2 localScale = this.transform.localScale;
        localScale.x = dirXToMouse > 0f ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        this.transform.localScale = localScale;
    }

    protected override void TakeDamage(float amount) {
        base.TakeDamage(amount);
        PlayerManager.instance.OnPlayerDamaged?.Invoke(this, EventArgs.Empty);
        if (this.currentHealth != 0f) return;
        PlayerManager.instance.OnPlayerDead?.Invoke(this, EventArgs.Empty);
        Destroy(this.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        GameObject colObj = collision.gameObject;
        if (colObj.CompareTag("Platform")) {
            this._isOnGround = true;
        } else if (colObj.CompareTag("HenchmanBullet")) {
            BulletBehavior bullet = colObj.GetComponent<BulletBehavior>();
            TakeDamage(bullet.GetDamage());
        }
    }
}
