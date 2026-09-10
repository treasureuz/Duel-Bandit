using System;
using RevolverEventArgs;
using UnityEngine;

public class PlayerRevolverAnim : MonoBehaviour {
    [SerializeField] private Animator _animator;

    [SerializeField] private AnimationClip _baseIdleClip;
    [SerializeField] private AnimationClip _baseShootClip;

    [SerializeField] private AnimationClip _ralphIdleClip;
    [SerializeField] private AnimationClip _ralphShootClip;

    [SerializeField] private AnimationClip _omenIdleClip;
    [SerializeField] private AnimationClip _omenShootClip;

    [SerializeField] private AnimationClip _soraIdleClip;
    [SerializeField] private AnimationClip _soraShootClip;

    // *Used to control/override animation clips at runtime (Industry pattern)*
    // Can't be done with the serialized "_animator.runtimeAnimatorController" (because it's readonly)
    // This overrideController setup removes the multiple Animator states that'd be needed for each revolver type
    // Only two animator states: Idle and Shoot, in which their animation clips are handled by overrideController
    private AnimatorOverrideController _overrideController;

    private static readonly int IdleHash = Animator.StringToHash("Revolver Idle");
    private static readonly int ShootHash = Animator.StringToHash("Shoot");

    void Awake() {
        // Sets overrideController to the original animator controller so it keeps the
        // animator structure (states, transitions, parameters, etc) intact
        this._overrideController = new (this._animator.runtimeAnimatorController);
        this._animator.runtimeAnimatorController = this._overrideController;
    }

    void Start() {
        PlayerManager.instance.OnPlayerRevolverShot += HandleRevolverShootAnim;
        PlayerManager.instance.OnPlayerRevolverEquipped += HandleRevolverAnimationClips;
    }

    void OnDisable() {
        PlayerManager.instance.OnPlayerRevolverShot -= HandleRevolverShootAnim;
        PlayerManager.instance.OnPlayerRevolverEquipped -= HandleRevolverAnimationClips;
    }

    private void HandleRevolverAnimationClips(object sender, RevolverEquippedEventArgs e) {
        RevolverDisplayInfoEventArgs revDisplayInfo = e.revolverDisplayInfoEArgs;
        switch (revDisplayInfo.RevolverName) {
            case "Base": SetAnimationClips(this._baseIdleClip, this._baseShootClip); break;
            case "Ralph": SetAnimationClips(this._ralphIdleClip, this._ralphShootClip); break;
            case "Omen": SetAnimationClips(this._omenIdleClip, this._omenShootClip); break;
            case "Sora": SetAnimationClips(this._soraIdleClip, this._soraShootClip); break;
        }
        // Instantly changes state to "Idle" once revolver is equipped and its respective animation clips are assigned
        // Starts at 0f seconds (frame 0), "0" is the Animator layer index in which "Idle" is on (Base Layer: 0)
        this._animator.Play(IdleHash, 0, 0f);
    }

    private void HandleRevolverShootAnim(object sender, EventArgs e) {
        // Sets trigger to "_Shoot" and checks if the current animation state has the "_Shoot"
        // trigger attached. If so, the animator switches its state to the trigger's linked state
        this._animator.SetTrigger(ShootHash);
    }

    private void SetAnimationClips (AnimationClip idleClip, AnimationClip shootClip) {
        // Sets the shared "RevolverIdle" and "RevolverShoot" variables to idleClip and shootClip, respectively,
        // meaning if the state changes to "Shoot," the AnimationClip variable currently references, plays.
        // These "variables" are just the names of the animation clips (below is same syntax as like getting an input action)
        // "controller[*animClipName*] = *ActualAnimationClip reference*"
        this._overrideController["MainRevolverIdle"] = idleClip;
        this._overrideController["MainRevolverShoot"] = shootClip;
    }
}
