using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : Character<PGunManager> {
    [SerializeField] private int _maxLives = 3;
    [SerializeField] private float _jumpForce = 5f;

    public static Player instance;

    private InputAction _forward;
    private InputAction _backward;

    private int _currentLives;
    private bool _isOnGround;

    public EventHandler OnDamaged;

    protected override void Awake() {
        if (!instance) instance = this;
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
        Vector2 mousePos = this.gunManager.GetTargetPosition();
        Vector2 dirToMouse = (mousePos - this.rb2d.position).normalized;
        var angle = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg;
        // Flip localScale if angle created by direction to mousePos "<" or ">" this FOV
        Vector3 localScale = this.transform.localScale;
        var isWithinFOV = Mathf.Abs(angle) <= (this.FOV / 2f);
        localScale.x = isWithinFOV ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        this.transform.localScale = localScale;
    }

    protected override void TakeDamage(float amount) {
        base.TakeDamage(amount);
        OnDamaged?.Invoke(this, EventArgs.Empty);
        //if (this.currentHealth == 0) SceneManager.LoadScene("GameScene");
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
