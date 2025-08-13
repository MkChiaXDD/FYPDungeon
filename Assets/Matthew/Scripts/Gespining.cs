using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gespining : MonoBehaviour
{
    private Vector3 rotation;
    public float rotationspeed;


    // Update is called once per frame
    void Update()
    {     
       rotation.y += rotationspeed;
       transform.rotation = Quaternion.Euler(rotation); 
    }
}
