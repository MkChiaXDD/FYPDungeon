using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillCount : MonoBehaviour
{
    private void Update()
    {
        Vector3 targetPosition = Camera.main.transform.position;
        targetPosition.y = transform.position.y;
        transform.LookAt(-targetPosition);
    }
}
