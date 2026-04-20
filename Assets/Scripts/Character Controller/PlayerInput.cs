using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private InputActionAsset m_PlayerInput;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;

    public InputAction GetMoveInput { get { return _moveAction; } }
    public InputAction GetLookInput { get { return _lookAction; } }
    public InputAction GetJumpInput { get { return _jumpAction; } }

    
    public Vector2 MoveDir = Vector2.zero;
    public Vector2 LookDir = Vector2.zero;

    private void OnEnable()
    {
        m_PlayerInput.Enable();
    }

    private void OnDisable()
    {
        m_PlayerInput.Disable();
    }

    private void Awake()
    {
        _moveAction = m_PlayerInput.FindAction("Move");
        _lookAction = m_PlayerInput.FindAction("Look");
        _jumpAction = m_PlayerInput.FindAction("Jump");
    }

    // Update is called once per frame
    void Update()
    {   
        MoveDir = _moveAction.ReadValue<Vector2>();
        LookDir = _lookAction.ReadValue<Vector2>();
    }
}
