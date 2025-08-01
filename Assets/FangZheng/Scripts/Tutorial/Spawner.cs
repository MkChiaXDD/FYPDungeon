using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject SpawnPrefab;
    private GameObject SpawnPrefabTarget;

    void Start()
    {
        SpawnItem();
    }

    void Update()
    {
        if (SpawnPrefabTarget == null) 
        {
            SpawnItem();
        }
    }

    void SpawnItem()
    {
        if (SpawnPrefab != null)
        {
            GameObject item = Instantiate(SpawnPrefab, transform.position + Vector3.up * 3, Quaternion.identity);
            SpawnPrefabTarget = item;
        }
        else
        {
            Debug.Log("You forgor about spawprefab");
        }
    }
}
