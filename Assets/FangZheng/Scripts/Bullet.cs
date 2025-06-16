using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector3 direction;
    private float speed;

    [SerializeField] private float lifeTime = 10f;
    
    public void Initialize(Vector3 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != this.gameObject)
        {
            if (other.CompareTag("Player"))
            {
                if (other.GetComponent<PlayerController>() != null)
                {
                    /*                    if (other.GetComponent<PlayerController>().GetParry() == false) {
                                            Destroy(gameObject);
                                        }
                                        else
                                        {
                                            direction = new Vector3(other.GetComponent<PlayerController>().GetDirection().x , direction.y, other.GetComponent<PlayerController>().GetDirection().z);
                                            //direction = other.GetComponent<PlayerController>().GetDirection();
                                            Debug.Log("Parry");
                                            return;
                                        }*/

                }

                Destroy(gameObject);
            }
            else if (other.CompareTag("Parry"))
            {
                direction = new Vector3(-direction.x, direction.y, -direction.z);
                Debug.Log("Parry");
                return;
            }
            Destroy(gameObject);
        }
    }
}
