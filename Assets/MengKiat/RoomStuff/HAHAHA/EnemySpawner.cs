using RMG;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGen;
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private int minAmount = 1;
    [SerializeField] private int maxAmount = 3;
    [SerializeField] private float offSet = 1.5f;
    [SerializeField] private List<GameObject> minibossPrefabs;

    public void GetAllRoomSpawnPoint()
    {
        if (mapGen == null || enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("Spawner is not set up correctly.");
            return;
        }

        List<Room> allRooms = mapGen.spawnedRooms;

        foreach (Room room in allRooms)
        {
            Transform spawnPoint = room.transform.Find("EnemySpawnPoint");

            if (spawnPoint != null)
            {
                int numOfEnemies = Random.Range(minAmount, maxAmount + 1);

                for (int i = 0; i < numOfEnemies; i++)
                {
                    Vector3 spawnOffset = new Vector3(Random.Range(-offSet, offSet), 0, Random.Range(-offSet, offSet));

                    Vector3 spawnPos = spawnPoint.position + spawnOffset;

                    GameObject enemy = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Count)], spawnPos, Quaternion.identity, enemyParent);

                    enemy.transform.parent = room.transform;
                }
            }
            else
            {
                Debug.LogWarning("No 'EnemySpawnPoint' found in " + room.name);
            }
        }
    }

    public void ChooseBoss()
    {
        GameObject boss = minibossPrefabs[Random.Range(0, minibossPrefabs.Count)];
        Debug.Log("Boss Selected: " + boss.name);
        FindFirstObjectByType<FarthestRoom>()?.SummonBoss(boss);
    }
}
