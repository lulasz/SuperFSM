using SuperFSM;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SuperFSMPlayerMovement : MonoBehaviour
{
    [SerializeField] private KeyCode Up = KeyCode.W;
    [SerializeField] private KeyCode Down = KeyCode.S;
    [SerializeField] private KeyCode Left = KeyCode.A;
    [SerializeField] private KeyCode Right = KeyCode.D;
    [SerializeField] private KeyCode Jump = KeyCode.Space;

    [SerializeField, Range(1f, 5f)] private float SpeedForce = 4f;
    [SerializeField, Range(1f, 10f)] private float JumpForce = 2f;
    [SerializeField, Range(-20f, 0f)] private float GravityForce = -9.81f;

    private CharacterController _controller;
    private Vector3 _direction;
    private Vector3 _playerVelocity;
    private bool _groundedPlayer;

    private float _up;
    private float _down;
    private float _left;
    private float _right;
    private bool _jump;

    private void Start()
    {
        _controller = gameObject.GetComponent<CharacterController>();

        FSM _fsm = new FSM(this);

        _fsm.AddState("Idle",
            OnEntry: () =>
            {
                // Debug
                gameObject.GetComponentInChildren<TextMesh>().text = _fsm.CurrentState.Name;
            },
            OnUpdate: () =>
            {
                CheckInput();
            })
        .AddTransition("IsMoving", "Moving", () => (_up + _down + _left + _right) != 0f)
        .AddTransition("IsJumping", "Jumping", () => _jump);

        _fsm.AddState("Moving",
            OnEntry: () =>
            {
                // Debug
                gameObject.GetComponentInChildren<TextMesh>().text = _fsm.CurrentState.Name;
            },
            OnUpdate: () =>
            {
                CheckInput();

                if (_groundedPlayer && _playerVelocity.y < 0)
                    _playerVelocity.y = 0f;
            })
        .AddTransition("IsJumping", "Jumping", () => _jump)
        .AddTransition("IsNotMoving", "Idle", () => (_up + _down + _left + _right) == 0f);

        _fsm.AddState("Jumping",
            OnEntry: () =>
            {
                // Debug
                gameObject.GetComponentInChildren<TextMesh>().text = _fsm.CurrentState.Name;
            },
            OnUpdate: () =>
            {
                if (_groundedPlayer)
                    _playerVelocity.y += Mathf.Sqrt(JumpForce * -3.0f * GravityForce);
            })
        .AddTransition("IsNotJumping", "Idle", () => _groundedPlayer);

        _fsm.SetState("Idle");
        _fsm.Start();
    }

    private void CheckInput()
    {
        _direction.x = _left + _right;
        _direction.z = _up + _down;

        _up = Input.GetKey(Up) ? 1f : 0f;
        _down = Input.GetKey(Down) ? -1f : 0f;
        _left = Input.GetKey(Left) ? -1f : 0f;
        _right = Input.GetKey(Right) ? 1f : 0f;
        _jump = Input.GetKey(Jump);
    }

    private void Update()
    {
        _groundedPlayer = _controller.isGrounded;
        _controller.Move(_direction * Time.deltaTime * SpeedForce);

        _playerVelocity.y += GravityForce * Time.deltaTime;
        _controller.Move(_playerVelocity * Time.deltaTime);
    }
}
