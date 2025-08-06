
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
    [SerializeField] private bool _IsStun = false;

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

    private float _speedMultiplier = 1f;
    private float _baseNormalSpeed;
    private float _baseDashSpeed;
    private float baseMovementSpeedModifier = 1f;
    private Coroutine _speedModifierCoroutine;

    [Header("Movement Modifiers")]
    [SerializeField] private float _movementModifierDuration = 0f;
    [SerializeField] private float _currentMovementModifier = 1f;
    [SerializeField] private float _targetMovementModifier = 1f;
    [SerializeField] private float _modifierChangeSpeed = 5f;

    [SerializeField] private CooldownSystem dashCooldown;


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
        ResetSpeedModifiers();
    }

    public void OnEnable()
    {
        playerData.DataChange.AddListener(ResetSpeedModifiers);
    }

    private void Update()
    {
        if (GamStates.instance.State == GamStates.GameState.Paused) 
        {
            ResetInput();
            return; 
        }

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
        _input.x = Input.GetAxisRaw("Horizontal");
        _input.z = Input.GetAxisRaw("Vertical");
    }

    void ResetInput()
    {
        _input.x = 0;
        _input.z = 0;
    }

    void ResetSpeedModifiers()
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
        Vector3 mousePos = Input.mousePosition;
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
        {
            StartCoroutine(Dashing());
            dashCooldown.StartCooldown(0.1f);
        }
    }

    private IEnumerator Dashing()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("PlayerDash", this.gameObject);
        }
        _isDashing = true;
        _meshTrail.HandleTrailActivation();
        _currentSpeed = playerData.Dash + playerData.Speed;
        yield return new WaitForSeconds(0.1f);
        _currentSpeed = playerData.Speed;
        _isDashing = false;
    }

    public void StunPlayer(float duration)
    {
        StartCoroutine(Stunned(duration));
    }

    private IEnumerator Stunned(float Duration)
    {
        _IsStun = true;
        _meshTrail.HandleTrailActivation2();

        yield return new WaitForSeconds(Duration);

        _IsStun = false;
    }

    public void Stun()
    {
        _IsStun = true;
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

        Vector3 force = _input.ToIso().normalized * (_currentSpeed * _currentMovementModifier);

        if (_IsStun)
        {
            _rb.AddForce(force, ForceMode.Force);
        }
        else
        {
            _rb.AddForce(force, ForceMode.VelocityChange);
        }

        if (_rb.velocity.magnitude > (_currentSpeed * _currentMovementModifier))
        {
            _rb.velocity = _rb.velocity.normalized * (_currentSpeed * _currentMovementModifier);
        }
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

    public void ChangePlayerMovementModifier(float modifier, float duration = 0f, bool immediate = false)
    {
        // Stop any existing modifier coroutine
        if (_speedModifierCoroutine != null)
        {
            StopCoroutine(_speedModifierCoroutine);
        }

        _targetMovementModifier = Mathf.Clamp(modifier, 0f, 2f); // Clamp between 0% and 200% (dont handle the negative)
        _movementModifierDuration = duration;

        if (immediate || duration <= 0)
        {
            _currentMovementModifier = _targetMovementModifier;
        }
        else
        {
            _speedModifierCoroutine = StartCoroutine(UpdateMovementModifier());
        }
    }


    public void ResetPlayerMovementModifier()
    {
        // Stop any existing modifier coroutine
        if (_speedModifierCoroutine != null)
        {
            StopCoroutine(_speedModifierCoroutine);
        }

        _targetMovementModifier = 1; // Clamp between 0% and 200% (dont handle the negative)
        _speedModifierCoroutine = StartCoroutine(UpdateMovementModifier());

    }


    private IEnumerator UpdateMovementModifier()
    {
        float elapsedTime = 0f;

        while (elapsedTime < _movementModifierDuration ||
               Mathf.Abs(_currentMovementModifier - _targetMovementModifier) > 0.01f)
        {
            // Smoothly interpolate to target modifier
            _currentMovementModifier = Mathf.Lerp(
                _currentMovementModifier,
                _targetMovementModifier,
                _modifierChangeSpeed * Time.deltaTime
            );

            // Apply the modifier to the current speed
            baseMovementSpeedModifier = _currentMovementModifier;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure we exactly reach the target
        _currentMovementModifier = _targetMovementModifier;
        baseMovementSpeedModifier = _currentMovementModifier;
        _speedModifierCoroutine = null;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}

public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}