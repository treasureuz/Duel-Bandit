using UnityEngine;

public class HealthBarFollow : MonoBehaviour {
    private Transform _followPos;

    public void Init(Transform follow) {
        this._followPos = follow;
    }

    void LateUpdate() {
        this.transform.position = this._followPos.position;
    }
}
