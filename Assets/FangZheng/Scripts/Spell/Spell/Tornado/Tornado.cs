using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Tornado : Projectile
{


    [SerializeField] private LayerMask PlayerLayer;
    public float PullForce;
    public float PullRadius;
    public float OrbitForce;
    public List<GameObject> SuckObj;

    public void GetOBJInRadius()
    {
        SuckObj.Clear();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, PullRadius , ~PlayerLayer);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.GetComponent<Rigidbody>() != null)
            {
                SuckObj.Add(hitCollider.gameObject);
            }
        }
    }

    public void Pull()
    {
        foreach (GameObject StuffToPull in SuckObj)
        {
            Vector3 DirectionOfPull = this.gameObject.transform.position - StuffToPull.transform.position;
            DirectionOfPull.y = 0;
            if (StuffToPull.GetComponent<Rigidbody>() != null)
            {
                //Vector3 DirectionOfPull = this.gameObject.transform.position - StuffToPull.transform.position;
                //DirectionOfPull.y = StuffToPull.transform.position.y;

                Rigidbody rb = StuffToPull.GetComponent<Rigidbody>();
                float PercentageOfForce = Mathf.Min(PullRadius / (DirectionOfPull.magnitude + PullRadius), 1);
                rb.AddForce(DirectionOfPull * PullForce * PercentageOfForce * Time.deltaTime, ForceMode.VelocityChange);

                Vector3 RotateDirection = Vector3.Cross(DirectionOfPull, Vector3.up).normalized;
                rb.AddForce(RotateDirection * OrbitForce * Time.deltaTime , ForceMode.VelocityChange);
            }
            else
            {
                Vector3 RotateDirection = Vector3.Cross(DirectionOfPull, Vector3.up).normalized;
                StuffToPull.transform.position += RotateDirection;
            }
        }
    }

    private void Update()
    {
        GetOBJInRadius();
        Pull();
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, PullRadius);
    }
}
