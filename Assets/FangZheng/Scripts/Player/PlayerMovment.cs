using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

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
    [SerializeField] private float _gravityScale = 1.0f;

    [Space, Header("PlayerData")]
    [SerializeField] private PlayerData playerData;

    private Vector3 _input;
    private Vector3 _mousePos;
    [SerializeField]  private float _currentSpeed;
    private const float _defaultGravity = -9.81f;

    public static PlayerMovement Instance { get; private set; }

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
        DontDestroyOnLoad(this.gameObject);

    }

    public void Start()
    {
        _currentSpeed = playerData.Speed;
    }
    public void OnEnable()
    {
        playerData.DataChange.AddListener(Addmodifier);
    }

    private void Update()
    {
        GatherInput();
        look();
        MousePosition();
        Dash();
    }

    private void FixedUpdate()
    {
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
                //ball.transform.position = _MousePos;
                return;
            }
        }
    }

    public Vector3 GetDirection()
    {
        return (_mousePos - transform.position).normalized;
    }

    private void Dash()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Space) && _rb.velocity != Vector3.zero)
            StartCoroutine(Dashing());
    }

    private IEnumerator Dashing()
    {
        _meshTrail.HandleTrailActivation();
        _currentSpeed = playerData.Dash;
        yield return new WaitForSeconds(0.1f);
        _currentSpeed = playerData.Speed;
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

    public Transform GetThisTransform()
    {
        return transform;
    }

}

public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}
