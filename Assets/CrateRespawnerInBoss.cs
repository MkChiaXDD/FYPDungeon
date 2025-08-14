using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateRespawnerInBoss : MonoBehaviour
{
    [SerializeField] private GameObject crates;
  
    public void SpawnCrates()
    {
        Instantiate(crates);
    }
}
