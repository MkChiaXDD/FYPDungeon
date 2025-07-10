using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashScript : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private float radius = 5f;
    [SerializeField] private float angle = 0f;
    [SerializeField] private Transform centerPoint;

    private float currentAngle;
    private Vector3 rightDirection;
    private bool isInitialized = false;

    public void Init(Transform playerTransform)
    {
        centerPoint = playerTransform;
        rightDirection = playerTransform.right;
        currentAngle = -Mathf.PI / 2f;
        UpdatePosition();
        transform.rotation = Quaternion.LookRotation(rightDirection);


    }
    void Update()
    {

            angle += speed * Time.deltaTime;
            angle = Mathf.Clamp(angle, 0f, Mathf.PI);
            UpdatePosition();

    }

    public void UpdatePosition()
    {
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        transform.position = centerPoint.position + new Vector3(x, 0f, z);
    }
}
