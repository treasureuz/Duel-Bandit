using UnityEngine;

public class HealthBarFollow : MonoBehaviour {
    [SerializeField] private Transform _healthBarPos;

    void LateUpdate() {
        this.transform.position = this._healthBarPos.position;
    }
}
