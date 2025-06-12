using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public class PlayerController : MonoBehaviour
{

    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _speed = 4;
    [SerializeField] private float _turnspeed = 360;
    [SerializeField] private Transform _body;
    [SerializeField] private Camera _camera;
    
    [SerializeField] private GameObject ball;
    
    private Vector3 _MousePos;
    private Vector3 _Input;

    [SerializeField] private List<Weapon> weapons;
    private int currentIndex = 0;

    // Update is called once per frame
    void Update()
    {
        GatherInput();
        look();
        MousePosition();

        if (Input.GetMouseButtonDown(0) && weapons.Count > 0)
            weapons[currentIndex].Attack();

        // Switch weapons with number keys
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectWeapon(1);
    }

    void SelectWeapon(int idx)
    {
        if (idx >= 0 && idx < weapons.Count)
            currentIndex = idx;
    }

    private void FixedUpdate()
    {
        Move();
        
    }
    void GatherInput()
    {
        _Input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    void look()
    {
        if (_Input != Vector3.zero || _MousePos != Vector3.zero)
        {
            // Keep the mouse target's Y level the same as the player's
            Vector3 flatMousePos = new Vector3(_MousePos.x, _body.position.y, _MousePos.z);

            // Get direction to look at (on the horizontal plane only)
            Vector3 direction = (flatMousePos - _body.position).normalized;

            // If direction is valid, rotate
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
        foreach (RaycastHit hit in hits) {
            if (hit.transform.gameObject.tag == "Ground")
            {
                _MousePos = hit.point;
                ball.transform.position = _MousePos;
                return;
            }
        }
        
        //Debug.Log("Mouse pos : " + _MousePos);
         
/*         {

            Camera.main.ViewportPointToRay(mousePos);
            Vector3 Position = new Vector3(_camera.ScreenToWorldPoint(mousePos).x, ball.transform.position.y, _camera.ScreenToWorldPoint(mousePos).z);
                
             Debug.Log(_camera.ScreenToWorldPoint(mousePos));
                ball.transform.position = Position;
             //DrawRay(_camera.transform.position, Position, 100);
              
         }*/
        
    }

    public void Interact()
    {
        if (Input.GetKeyDown("E"))
        {
            Debug.Log("Interact");
        }
    }

    public Vector3 GetDirection()
    {
        return ( _MousePos - transform.position ).normalized;
    }

    private void Move()
    {
        //_rb.MovePosition(transform.position + (transform.forward * _Input.magnitude )* _speed * Time.deltaTime);

        _rb.MovePosition(_body.position + _Input.ToIso() * _Input.normalized.magnitude * _speed * Time.deltaTime);
    }
    public void DrawRay(Vector3 origin, Vector3 direction, float length)
    {
        Debug.DrawRay(origin, direction.normalized * length, Color.red);
    }

    public void OnDrawGizmos()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = _camera.ScreenPointToRay(mousePos);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(ray.origin, ray.direction * 100f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(ray.origin, 10000f);
    }

} 
public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}