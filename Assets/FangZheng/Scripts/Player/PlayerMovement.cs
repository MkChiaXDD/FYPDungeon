using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] public Rigidbody _rb;
    [SerializeField] public Transform _body;
    [SerializeField] public Camera _camera;
    [SerializeField] public MeshTrail _meshTrail;

    [Header("Movement")]
    [SerializeField] private float _normalspeed = 4;
    [SerializeField] private float _turnspeed = 360;
    [SerializeField] private float _dashSpeed = 30;

    [Space, Header("PlayerData")]
    [SerializeField] private PlayerData playerData;

    private Vector3 _input;
    private Vector3 _mousePos;
    [SerializeField] private float _currentSpeed;
    private bool _isMovementLocked = false;
    private bool _isDashing = false;

    public static PlayerMovement Instance { get; private set; }

    // Public property to check movement state
    public bool IsMoving => _input.magnitude > 0.1f && !_isMovementLocked;
    public bool IsMovementLocked => _isMovementLocked;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void Start()
    {
        Addmodifier();
    }

    public void OnEnable()
    {
        playerData.DataChange.AddListener(Addmodifier);
    }

    private void Update()
    {
        MousePosition();
        GatherInput();
        look();
        if (_isMovementLocked) return;
        Dash();
    }

    private void FixedUpdate()
    {
        if (_isMovementLocked) return;

        Move();
    }

    void GatherInput()
    {
        _input.x = UnityEngine.Input.GetAxisRaw("Horizontal");
        _input.z = UnityEngine.Input.GetAxisRaw("Vertical");
    }

    void Addmodifier()
    {
        _normalspeed = playerData.Speed;
        _dashSpeed = playerData.Dash;
        _currentSpeed = _normalspeed;
    }

    void look()
    {
        if (_input != Vector3.zero || _mousePos != Vector3.zero)
        {
            Vector3 flatMousePos = new Vector3(_mousePos.x, _body.position.y, _mousePos.z);

            Vector3 direction = (flatMousePos - _body.position).normalized;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                _body.rotation = Quaternion.RotateTowards(_body.rotation, targetRotation, _turnspeed * Time.deltaTime);
            }
        }
    }

    void MousePosition()
    {
        Vector3 mousePos = UnityEngine.Input.mousePosition;
        Ray ray = _camera.ScreenPointToRay(mousePos);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject.tag == "Ground")
            {
                _mousePos = hit.point;
                return;
            }
        }
    }

    public Vector3 GetDirection()
    {
        return (_mousePos - transform.position).normalized;
    }

    public Quaternion GetDirectionQuaternion()
    {
        Vector3 direction = GetDirection();
        return Quaternion.LookRotation(direction);
    }

    private void Dash()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _rb.velocity != Vector3.zero && !_isMovementLocked)
            StartCoroutine(Dashing());
    }

    private IEnumerator Dashing()
    {
        _isDashing = true;
        _meshTrail.HandleTrailActivation();
        _currentSpeed = playerData.Dash + playerData.Speed;
        yield return new WaitForSeconds(0.1f);
        _currentSpeed = playerData.Speed;
        _isDashing = false;
    }

    public void Stun()
    {
        _normalspeed = 0;
        _dashSpeed = 0;
    }

    public void Unstun()
    {
        _normalspeed = playerData.Speed;
        _dashSpeed = playerData.Dash;
        _currentSpeed = _normalspeed;
    }

    private void Move()
    {
        _rb.velocity = new Vector3(_rb.velocity.x, _rb.velocity.y, _rb.velocity.z);

        Vector3 force = _input.ToIso().normalized * _currentSpeed;

        _rb.AddForce(force, ForceMode.Impulse);

        if (_rb.velocity.magnitude > _currentSpeed)
        {
            _rb.velocity = _rb.velocity.normalized * _currentSpeed;
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    // New method to lock/unlock movement
    public void SetMovementLock(bool lockState)
    {
        _isMovementLocked = lockState;

        if (lockState)
        {
            // Immediately stop movement when locked
            _rb.velocity = Vector3.zero;
        }
        else
        {
            // Reset speed when unlocked
            _currentSpeed = _normalspeed;
        }
    }
}

public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}