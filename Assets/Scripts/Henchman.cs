using UnityEngine;

public class Henchman : MonoBehaviour {
    //[SerializeField] private float _moveSpeed = 2.67f;
    [SerializeField] private float _raycastDist = 2.5f;
    [SerializeField] private float _raycastHeight = 3f;
    //[SerializeField] private float _stoppingDist = 2f;
    [SerializeField] private LayerMask _playerLayerMask;

    private Rigidbody2D _rb2d;

    public enum HenchmanState {
        Idle,
        Attack
    }
    public HenchmanState state { get; private set; } = HenchmanState.Idle;

    void Awake() {
        this._rb2d = this.GetComponent<Rigidbody2D>();
    }

    void FixedUpdate() {
        RaycastHit2D hit = Physics2D.BoxCast(this.transform.position,
            new Vector2(0.1f, this._raycastHeight), 0f, Vector2.left,
                this._raycastDist, this._playerLayerMask);
        this.state = hit ? HenchmanState.Attack : HenchmanState.Idle;
        //HandleStateSwitch();
    }

    private void HandleStateSwitch() {
        switch (this.state) {
            case HenchmanState.Idle: break;
            case HenchmanState.Attack: {
                break;
            }
        }
    }
}
