using UnityEngine;

public class HWeaponManager : WeaponManager {
    private Player _player;

    protected override void Awake() {
        base.Awake();
        this._player = FindAnyObjectByType<Player>();
    }

    protected override void HandleShoot() {
        this.nextShootTime += Time.deltaTime;
        if (this.nextShootTime < this.timeBetweenShots) return;
        Shoot(); this.nextShootTime = 0f;
    }

    protected override Vector3 GetTargetPosition() {
        return this._player.transform.position;
    }
}
