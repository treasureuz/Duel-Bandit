using UnityEngine;

public class HealthBarFollow : MonoBehaviour {
    private Transform _targetPos;
    private Vector3 _offset;

    public void Init(Transform target, Vector3 offset) {
        this._targetPos = target;
        this._offset = offset;
    }

    void LateUpdate() {
        this.transform.position = this._targetPos.position + this._offset;
    }
}
