using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using System.Collections.Generic;

public class Player : Character<PRevolverManager> {
    [Header("References")]
    [SerializeField] private LayerMask _platformsLayerMask;

    [Header("Settings")]
    [FormerlySerializedAs("standardMoveSpeed")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private int _maxNumOfJumpsInAir = 2;

    private InputAction _forward;
    private InputAction _backward;

    private Camera _cam;

    public int CurrentLives { get; private set; }
    public int MaxLives { get; private set; }

    private int _currNumOfJumpsInAir;

    protected override void Awake() {
        base.Awake();
        this._cam = Camera.main;
        PlayerInput playerInput = this.GetComponent<PlayerInput>();
        this._forward = playerInput.actions.FindAction("Forward"); // Or "["Forward"]"
        this._backward = playerInput.actions.FindAction("Backward");
    }

    // Gets called the same frame Awake is (before Start)
    public void Init(List<PRevolverData> revolvers, int currlives, int maxLives) {
        this.RevolverManager.SetRevolversList(revolvers);
        this.CurrentLives = currlives;
        this.MaxLives = maxLives;
    }

    protected void Start() {
        PlayerManager.instance.OnPlayerSpawned?.Invoke(this, EventArgs.Empty);
    }

    void FixedUpdate() {
        var direction = 0f;
        Vector2 mousePos = this._cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dirToMousePos = mousePos - this.rb2d.position;
        HandleLocalScale(dirToMousePos); // Flips localScale if mouse is left or right of Player
        if (this._forward.IsPressed()) direction = 1f;
        if (this._backward.IsPressed()) direction = -1f;
        this.rb2d.linearVelocity = new Vector2(
            direction * this._moveSpeed, this.rb2d.linearVelocity.y);
    }

    public void OnJump() {
        if (this._currNumOfJumpsInAir == this._maxNumOfJumpsInAir) return;
        this.rb2d.AddForce(Vector2.up * this._jumpForce, ForceMode2D.Impulse);
        ++this._currNumOfJumpsInAir;
    }

    protected override void TakeDamage(float amount) {
        base.TakeDamage(amount);
        PlayerManager.instance.OnPlayerHealthChange?.Invoke(this, EventArgs.Empty);
        if (this.CurrentHealth != 0f) return;
        PlayerManager.instance.OnPlayerDead?.Invoke(this, EventArgs.Empty);
        Destroy(this.gameObject);
    }

    public bool AddHealth(float amount) {
        if (this.CurrentHealth == this.maxHealth) return false;
        SetCurrentHealth(this.CurrentHealth + amount);
        PlayerManager.instance.OnPlayerHealthChange?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public void OnInteract() {
        if (InteractableManager.instance.TryGetCurrentInteractable
            (out var interactable)) {
            // Interacts with Interactable thats within range
            interactable.Equip(); // (in this case its a RevolverInteractable)
        }
    }

    public void OnReload() => this.RevolverManager.Reload();
    public void EquipRevolver(PRevolverConfig revolverConfig) {
        this.RevolverManager.SetCurrentRevolverConfig(revolverConfig);
    }
    public void OnSwapToNextRevolver() => this.RevolverManager.SwitchToNextRevolver();
    public void OnSwapToPreviousRevolver() => this.RevolverManager.SwitchToPreviousRevolver();

    public bool AddRevolverAmmo(int amount) {
        return this.RevolverManager.AddToReserveAmmo(amount);
    }

    private void OnCollisionStay2D(Collision2D collision) {
        // Bitwise operations. Shifting to the left (<<) multiplies the number by 2 (2^n)
        // Therefore, "1 << 6" shifts to the left 6 times == 2^6 = 64. 
        // "&" checks to see if the collisionLayer and any layer in the LayerMask match.
        // If they do, the result becomes the bitwise shift operation (64), which != 0.
        if ((1 << collision.gameObject.layer & this._platformsLayerMask) == 0) return;
        // "contact.normal" gets the direction the surface is facing at the contact point
        // Checks if the normal vector is pointing in the same dir as Vector.up (0, 1)
        foreach (ContactPoint2D contact in collision.contacts) {
            // Within 0 to 60 degrees counts as straight up: cos(60) = 0.5)
            if (Vector2.Dot(contact.normal, Vector2.up) > 0.5f) {
                // Reset if was in air and now transitioning to on ground
                if (!this.isOnGround) this._currNumOfJumpsInAir = 0;
                this.isOnGround = true;
                return;
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        GameObject colObj = collision.gameObject;
        if (colObj.CompareTag("HenchmanBullet")) {
            BulletBehavior bullet = colObj.GetComponent<BulletBehavior>();
            TakeDamage(bullet.Damage);
        }
    }
    private void OnCollisionExit2D(Collision2D collision) {
        if ((1 << collision.gameObject.layer & this._platformsLayerMask) == 0) return;
        this.isOnGround = false;
    }
}
