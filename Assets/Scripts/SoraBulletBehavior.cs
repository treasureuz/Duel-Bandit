using UnityEngine;

public class SoraBulletBehavior : BulletBehavior {
	// slows enemy movement/fire speed by this every hit
	[SerializeField] private float _slowMultiplier;

    private void HandleHenchmanSlowEffect(Henchman henchman) {
		var hMoveSpeed = henchman.CurrentMoveSpeed;
		var hTimeBetweenShots = henchman.GetRevolverCurrentTBS();

		var newHMoveSpeed = hMoveSpeed / this._slowMultiplier;
		var newHTimeBetweenShots = hTimeBetweenShots * this._slowMultiplier;

		henchman.SetCurrentMoveSpeed(newHMoveSpeed);
		henchman.SetRevolverCurrentTBS(newHTimeBetweenShots);
        henchman.EnableSoraSlowEffect();
	}

	public override void OnCollisionEnter2D(Collision2D collision) {
		GameObject colObj = collision.gameObject;
		if (colObj.CompareTag("Henchman")) {
			Henchman henchman = colObj.GetComponent<Henchman>();
			HandleHenchmanSlowEffect(henchman);
		}
		base.OnCollisionEnter2D(collision); // Destroys this gameObj
	}
}
