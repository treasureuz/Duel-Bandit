using UnityEngine;

public class HWeaponManager : WeaponManager {
    private Henchman _henchman;

    protected override void Awake() {
        base.Awake();
        this._henchman = this.GetComponentInParent<Henchman>();
    }

    protected override void Update() {
        if (this._henchman.GetCurrentState() != HenchmanState.Attack) return;
        HandleShooting();
    }


    protected override void HandleShooting() {
        base.HandleShooting();
        this.nextShootTime += Time.deltaTime;
        if (this.nextShootTime < this.timeBetweenShots) return;
        Shoot(); this.nextShootTime = 0f;
    }

    protected override Vector3 GetTargetPosition() {
        return Player.instance.transform.position;
    }
}
