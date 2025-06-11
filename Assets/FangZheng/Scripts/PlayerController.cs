using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _speed = 4;
    [SerializeField] private float _turnspeed = 360;
    private Vector3 _Input;


    // Update is called once per frame
    void Update()
    {
        GatherInput();
        look();
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
        if (_Input != Vector3.zero)
        {
            var rot = Quaternion.LookRotation(_Input.ToIso(), Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, _turnspeed * Time.deltaTime);
        }

    }

    private void Move()
    {
        _rb.MovePosition(transform.position + (transform.forward * _Input.magnitude)* _speed * Time.deltaTime);
    }

}
public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}