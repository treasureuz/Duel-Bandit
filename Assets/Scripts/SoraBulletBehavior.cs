using UnityEngine;

public class SoraBulletBehavior : BulletBehavior {
	// slows enemy movement/fire speed by this every hit
	[SerializeField] private float _slowMultiplier; 

    private void HandleHenchmanSlowEffect(Henchman henchman) {
		float hMoveSpeed = henchman.CurrentMoveSpeed;
		float hFireRate = henchman.GetCurrRevolverFireRate();

		float newHMoveSpeed = hMoveSpeed / this._slowMultiplier;
		float newHFireRate = hFireRate / this._slowMultiplier;

		henchman.SetCurrentMoveSpeed(newHMoveSpeed);
		henchman.SetCurrRevolverFireRate(newHFireRate);
	}

	public override void OnCollisionEnter2D(Collision2D collision) {
		GameObject colObj = collision.gameObject;
		if (colObj.CompareTag("Henchman")){
			Henchman henchman = colObj.GetComponent<Henchman>();
			HandleHenchmanSlowEffect(henchman);
		}
		base.OnCollisionEnter2D(collision); // Destroys this gameObj
	}
}
