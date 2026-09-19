using System;
using UnityEngine;

public class InteractableManager : MonoBehaviour {
    [SerializeField] private GameObject _interactableCanvas;

    public static InteractableManager instance;

    private RevolverInteractable _currentInteractable;

    void Awake() {
        if (!instance) instance = this;
        this._interactableCanvas.SetActive(false);
    }

    public void ShowInteractable(RevolverInteractable interactable) {
        this._currentInteractable = interactable;
        Vector2 targetPos = interactable.GetInteractPromptPos();
        this._interactableCanvas.transform.position = targetPos;
        this._interactableCanvas.SetActive(true);
    }

    public void HideInteractable() {
        this._currentInteractable = null;
        if (!this._interactableCanvas) return;
        this._interactableCanvas.SetActive(false);
    }

    public bool TryGetCurrentInteractable(out RevolverInteractable interactable) {
        interactable = this._currentInteractable;
        return interactable != null;
    }
}
