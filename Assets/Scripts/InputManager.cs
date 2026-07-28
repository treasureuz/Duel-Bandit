using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour {
    public static InputManager instance;

    private PlayerInput _playerInput;
    private InputAction _forward;
    private InputAction _backward;

    public EventHandler OnForwardStarted;
    public EventHandler OnForwardStopped;
    public EventHandler OnBackwardStarted;
    public EventHandler OnBackwardStopped;
    public EventHandler OnJumped;

    void Awake() {
        if (!instance) instance = this;
        this._playerInput = this.GetComponent<PlayerInput>();
        this._forward = this._playerInput.actions.FindAction("Forward"); // Or "["Forward"]"
        this._backward = this._playerInput.actions.FindAction("Backward");
    }

    void OnEnable() {
        this._forward.performed += HandleForwardPerformed;
        this._forward.canceled += HandleForwardCanceled;
        this._backward.performed += HandleBackwardPerformed;
        this._backward.canceled += HandleBackwardCanceled;
    }

    void OnDisable() {
        this._forward.performed -= HandleForwardPerformed;
        this._forward.canceled -= HandleForwardCanceled;
        this._backward.performed -= HandleBackwardPerformed;
        this._backward.canceled -= HandleBackwardCanceled;
    }

    void Start() {
        // Disables all action maps and enables the one to use
        foreach (InputActionMap map in this._playerInput.actions.actionMaps)
            map.Disable();
        this._playerInput.actions.FindActionMap("Player").Enable();
    }

    public void HandleForwardPerformed(InputAction.CallbackContext ctx) {
        OnForwardStarted?.Invoke(this, EventArgs.Empty);
    }
    public void HandleForwardCanceled(InputAction.CallbackContext ctx) {
        OnForwardStopped?.Invoke(this, EventArgs.Empty);
    }
    public void HandleBackwardPerformed(InputAction.CallbackContext ctx) {
        OnBackwardStarted?.Invoke(this, EventArgs.Empty);
    }
    public void HandleBackwardCanceled(InputAction.CallbackContext ctx) {
        OnBackwardStopped?.Invoke(this, EventArgs.Empty);
    }

    public void OnJump() {
        OnJumped?.Invoke(this, EventArgs.Empty);
    }
}
