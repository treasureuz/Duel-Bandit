using System;
using RevolverEventArgs;
using UnityEngine;
using UnityEngine.UIElements;

public class ChamberAnimManager : MonoBehaviour {
    [Header("Chamber Idle/Shoot Clips")]
    [SerializeField] private AnimationClip _chamberFullIdleClip;

    [SerializeField] private AnimationClip _chamberSixthToLastIdleClip;
    [SerializeField] private AnimationClip _chamberSixthToLastShotClip;

    [SerializeField] private AnimationClip _chamberFifthToLastIdleClip;
    [SerializeField] private AnimationClip _chamberFifthToLastShotClip;

    [SerializeField] private AnimationClip _chamberFourthToLastIdleClip;
    [SerializeField] private AnimationClip _chamberFourthToLastShotClip;

    [SerializeField] private AnimationClip _chamberThirdToLastIdleClip;
    [SerializeField] private AnimationClip _chamberThirdToLastShotClip;

    [SerializeField] private AnimationClip _chamberSecondToLastIdleClip;
    [SerializeField] private AnimationClip _chamberSecondToLastShotClip;

    [SerializeField] private AnimationClip _chamberLastIdleClip;
    [SerializeField] private AnimationClip _chamberLastShotClip;

    [Header("References")]
    [SerializeField] private Animator _animator;
    // *Used to control/override animation clips at runtime (Industry pattern)*
    // Can't be done with the serialized "_animator.runtimeAnimatorController" (because it's readonly)
    // This overrideController setup removes the multiple Animator states that'd be needed for each revolver type
    // Only two animator states: Idle and Shoot, in which their animation clips are handled by overrideController
    private AnimatorOverrideController _overrideController;

    private static readonly int ChamberFullIdleStateHash = Animator.StringToHash("ChamberFullIdle");
    private static readonly int ChamberIdleStateHash = Animator.StringToHash("ChamberIdle");
    private static readonly int ChamberReloadStateHash = Animator.StringToHash("ChamberReload");
    private static readonly int RevolverShotTriggerHash = Animator.StringToHash("RevolverShot");

    void Awake() {
        // Sets overrideController to the original animator controller so it keeps the
        // animator structure (states, transitions, parameters, etc) intact
        this._overrideController = new (this._animator.runtimeAnimatorController);
        this._animator.runtimeAnimatorController = this._overrideController;
    }

    void Start() {
        PlayerManager.instance.OnPlayerRevolverEquipped += HandleChamberOnRevolverEquippedAnim;
        PlayerManager.instance.OnPlayerRevolverShot += HandleChamberOnRevolverShotAnim;
        PlayerManager.instance.OnPlayerRevolverReloading += HandleChamberOnRevolverReloadAnim;
    }

    void OnDisable() {
        PlayerManager.instance.OnPlayerRevolverEquipped -= HandleChamberOnRevolverEquippedAnim;
        PlayerManager.instance.OnPlayerRevolverShot -= HandleChamberOnRevolverShotAnim;
        PlayerManager.instance.OnPlayerRevolverReloading -= HandleChamberOnRevolverReloadAnim;

    }

    private void HandleChamberOnRevolverEquippedAnim(object sender, RevolverEquippedEventArgs e) {
        var currentMag = e.ammoEventArgs.CurrentMagCount;
        HandleChamberAnimationClips(currentMag);
    }

    private void HandleChamberOnRevolverShotAnim(object sender, AmmoEventArgs e) {
        var bulletShot = e.CurrentMagCount + 1;
        if (bulletShot > 6) return;
        HandleChamberAnimationClips(bulletShot);
        // Sets trigger to "ChamberShot" and checks if the current animation state has the "ChamberShot"
        // trigger attached. If so, the animator switches its state to the trigger's linked state
        this._animator.SetTrigger(RevolverShotTriggerHash);
    }

    private void HandleChamberOnRevolverReloadAnim(object sender, EventArgs e) {
        // Instantly plays the anim clip attached to "ChamberReload" state.
        // Starts at 0f seconds (frame 0), 2nd param is the Animator layer index
        // that "ChamberReload" is on (Base Layer: 0)
        this._animator.Play(ChamberReloadStateHash, 0, 0f);
    }

    private void HandleChamberAnimationClips(float bulletsCount) {
        switch (bulletsCount) {
            case 6:
                SetChamberAnimClips(this._chamberSixthToLastIdleClip, this._chamberSixthToLastShotClip);
                break;
            case 5:
                SetChamberAnimClips(this._chamberFifthToLastIdleClip, this._chamberFifthToLastShotClip);
                break;
            case 4:
                SetChamberAnimClips(this._chamberFourthToLastIdleClip, this._chamberFourthToLastShotClip);
                break;
            case 3:
                SetChamberAnimClips(this._chamberThirdToLastIdleClip, this._chamberThirdToLastShotClip);
                break;
            case 2:
                SetChamberAnimClips(this._chamberSecondToLastIdleClip, this._chamberSecondToLastShotClip);
                break;
            case 1:
                SetChamberAnimClips(this._chamberLastIdleClip, this._chamberLastShotClip);
                break;
            default: this._animator.Play(ChamberFullIdleStateHash, 0, 0f); break;
        }
    }

    private void SetChamberAnimClips(AnimationClip idleClip, AnimationClip shotClip) {
         // Sets the shared "ChamberIdle" and "ChamberShot" variables to idleClip and shotClip, respectively,
        // meaning if the state changes to "Shoot," the AnimationClip variable currently references, plays.
        // These "variables" are just the names of the anim clips (below is same syntax as getting an input action)
        // "controller[*animClipName*] = *ActualAnimationClip reference*"
        this._overrideController["MainChamberIdle"] = idleClip;
        this._overrideController["MainChamberShot"] = shotClip;
    }
}
