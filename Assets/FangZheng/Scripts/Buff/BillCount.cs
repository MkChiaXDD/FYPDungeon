using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillCount : MonoBehaviour
{
    [SerializeField] private Transform lookAtObj;

    private void Start()
    {
        lookAtObj = GameObject.Find("CameraPivot").transform;
        Vector3 targetPosition = Camera.main.transform.position;
        targetPosition.y = transform.position.y;
        transform.LookAt(-targetPosition);
    }
    //private void Update()
    //{
    //    Vector3 targetPosition = Camera.main.transform.position;
    //    targetPosition.y = transform.position.y;
    //    transform.LookAt(-targetPosition);
    //}
}
