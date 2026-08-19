using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character<PGunManager> {
    [Header("Settings")]
    [SerializeField] private int _maxNumOfJumpsInAir = 2;
    [SerializeField] private float _jumpForce = 5f;

    private InputAction _forward;
    private InputAction _backward;

    private Camera _cam;

    public int CurrentLives { get; private set; }
    public int MaxLives { get; private set; }

    private int _currNumOfJumpsInAir;
    //private bool _isOnGround;

    protected override void Awake() {
        base.Awake();
        // Getting components
        this._cam = Camera.main;
        PlayerInput playerInput = this.GetComponent<PlayerInput>();
        this._forward = playerInput.actions.FindAction("Forward"); // Or "["Forward"]"
        this._backward = playerInput.actions.FindAction("Backward");

        // Player spawns in the air, so double jump should be disabled on start
        this._currNumOfJumpsInAir = this._maxNumOfJumpsInAir;
    }

    // Gets called the same frame Awake is (before Start)
    public void Init(int currlives, int maxLives) {
        this.CurrentLives = currlives;
        this.MaxLives = maxLives;
    }

    protected void Start() {
        AmmoEventArgs ammoEventArgs = new (this.GunManager.CurrentAmmo, this.GunManager.GetMaxAmmo());
        PlayerManager.instance.OnPlayerSpawned?.Invoke(this, ammoEventArgs);
    }

    void FixedUpdate() {
        var direction = 0f;
        HandleLocalScale(); // Flips localScale if mouse is left or right of Player
        if (this._forward.IsPressed()) direction = 1f;
        if (this._backward.IsPressed()) direction = -1f;
        this.rb2d.linearVelocity = new Vector2(
            direction * this.moveSpeed, this.rb2d.linearVelocity.y);
    }

    public void OnJump() {
        if (this._currNumOfJumpsInAir == this._maxNumOfJumpsInAir) return;
        this.rb2d.AddForce(Vector2.up * this._jumpForce, ForceMode2D.Impulse);
        ++this._currNumOfJumpsInAir;
        //this._isOnGround = false;
    }

    protected override void HandleLocalScale() {
        Vector2 mousePos = this._cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        var dirXToMouse = mousePos.x - this.rb2d.position.x;
        Vector2 localScale = this.transform.localScale;
        localScale.x = dirXToMouse > 0f ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        this.transform.localScale = localScale;
    }

    protected override void OnDamaged(float amount) {
        base.OnDamaged(amount);
        PlayerManager.instance.OnPlayerDamaged?.Invoke(this, new BulletDamageEventArgs(amount));
        if (this.CurrentHealth != 0f) return;
        PlayerManager.instance.OnPlayerDead?.Invoke(this, EventArgs.Empty);
        Destroy(this.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        GameObject colObj = collision.gameObject;
        if (colObj.CompareTag("Ground")) {
            //this._isOnGround = true;
            this._currNumOfJumpsInAir = 0;
        } else if (colObj.CompareTag("HenchmanBullet")) {
            BulletBehavior bullet = colObj.GetComponent<BulletBehavior>();
            OnDamaged(bullet.Damage);
        }
    }
}
