using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private const float GROUND_CHECK_RADII = 0.08f;
    private const float GROUND_CHECK_ALLOWANCE = 0.325f;

    private const float Y_FALL_LIMIT = 1.85f;
    private const float MAX_FALL_SPEED = 30;

    [Header("Movement Variables")]
    [SerializeField] private int _playerSpeed = 10;
    [SerializeField] private float _jumpForce = 6;
    [SerializeField] private float _fallMultiplier = 2.5f;
    [SerializeField] private float _lowJumpMultiplier = 4f;


    [Header("Sensitivity")]
    [SerializeField] private float _rotateSpeed_X = 0.4f;
    [SerializeField] private float _rotateSpeed_Y = 0.5f;
    [SerializeField] Transform _cameraPoint;
    private float _yaw;
    private float _pitch;
    private float minPitch = -80f;
    private float maxPitch = 80f;


    [Header("Miscellaneous")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private GameObject _groundCheck;

    private bool _onGround()
    {
        if (Physics.SphereCast(_groundCheck.transform.position, GROUND_CHECK_RADII, Vector3.down,
            out RaycastHit _hit, GROUND_CHECK_ALLOWANCE, _groundLayer)) return Vector3.Angle(_hit.normal, Vector3.up) < 20f;

        return false;
    }

    private Rigidbody _rb;
    private PlayerInput _playerInput;


    void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        _rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void Update()
    {
        if (_playerInput.GetJumpInput.WasPressedThisFrame() && _onGround())
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        ApplyMovement();

        AdjustedGravity();
    }

    void Jump()
    {
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }

    void AdjustedGravity()
    {
        if (_rb.linearVelocity.y <= Y_FALL_LIMIT)
        {
            _rb.linearVelocity += Vector3.up * Physics.gravity.y * (_fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (_rb.linearVelocity.y > 0 && !_playerInput.GetJumpInput.IsPressed())
        {
            _rb.linearVelocity += Vector3.up * Physics.gravity.y * (_lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }

        if (_rb.linearVelocity.y < -MAX_FALL_SPEED)
        {
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, -MAX_FALL_SPEED, _rb.linearVelocity.z);
        }
    }

    void ApplyMovement()
    {
        float y = _rb.linearVelocity.y;
        Vector3 player_movement = (transform.forward * _playerInput.MoveDir.y + transform.right * _playerInput.MoveDir.x);

        if(_onGround()) player_movement *= _playerSpeed;
        else if(!_onGround()) player_movement *= (_playerSpeed / 1.1f);

        Vector3 move = player_movement * Time.fixedDeltaTime;

        _rb.linearVelocity = new Vector3(player_movement.x, y, player_movement.z);
    }

    private void LateUpdate()
    {
        UpdateYawPitch();

        transform.localRotation = Quaternion.Euler(0f, _yaw, 0f);
        _cameraPoint.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    void UpdateYawPitch()
    {
        _yaw += _playerInput.LookDir.x * _rotateSpeed_X;
        _pitch -= _playerInput.LookDir.y * _rotateSpeed_Y;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
    }
}
