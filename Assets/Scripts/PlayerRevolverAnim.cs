using System;
using UnityEngine;

public class PlayerRevolverAnim : MonoBehaviour {
    [SerializeField] private Animator _animator;

    void Start() {
        PlayerManager.instance.OnPlayerRevolverShot += HandleRevolverAnimation;
    }

    void OnDisable() {
        PlayerManager.instance.OnPlayerRevolverShot -= HandleRevolverAnimation;
    }

    private void HandleRevolverAnimation(object sender, EventArgs e) {
        this._animator.SetTrigger("Shoot");
    }
}
