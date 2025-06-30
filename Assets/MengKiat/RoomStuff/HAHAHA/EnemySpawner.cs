using RMG;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGen;
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private float offSet = 1.5f;
    [SerializeField] private List<GameObject> minibossPrefabs;
    [SerializeField] private List<GameObject> bigBossPrefabs;

    [Header("Scaling")]
    [SerializeField] private DifficultyManager diffMgr;
    [SerializeField] private int RoundToSpawnFast = 2;
    [SerializeField] private int RoundToSpawnRanged = 3;
    [SerializeField] private int RoundToSpawnTank = 4;
    [SerializeField] private int RoundToSpawnBomber = 5;

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
                int round = diffMgr.GetRound();
                int minEnemies = diffMgr.GetMinEnemies();
                int maxEnemies = diffMgr.GetMaxEnemies();
                int numOfEnemies = Random.Range(minEnemies, maxEnemies + 1);

                for (int i = 0; i < numOfEnemies; i++)
                {
                    Vector3 spawnOffset = new Vector3(Random.Range(-offSet, offSet), 0, Random.Range(-offSet, offSet));
                    Vector3 spawnPos = spawnPoint.position + spawnOffset;

                    List<GameObject> availableEnemies = new List<GameObject>();

                    foreach (GameObject enemy in enemyPrefabs)
                    {
                        string name = enemy.name.ToLower();

                        if (name.Contains("fast") && round >= RoundToSpawnFast)
                            availableEnemies.Add(enemy);
                        else if (name.Contains("ranged") && round >= RoundToSpawnRanged)
                            availableEnemies.Add(enemy);
                        else if (name.Contains("tank") && round >= RoundToSpawnTank)
                            availableEnemies.Add(enemy);
                        else if (name.Contains("bomber") && round >= RoundToSpawnBomber)
                            availableEnemies.Add(enemy);
                        else if (!name.Contains("fast") && !name.Contains("ranged") && !name.Contains("tank") && !name.Contains("bomber"))
                            availableEnemies.Add(enemy); // Basic/default enemy
                    }

                    if (availableEnemies.Count == 0)
                    {
                        availableEnemies.Add(enemyPrefabs[0]);
                        Debug.LogWarning("No enemies unlocked at round " + round + ". Using fallback.");
                    }

                    GameObject chosenEnemy = availableEnemies[Random.Range(0, availableEnemies.Count)];
                    GameObject enemyInstance = Instantiate(chosenEnemy, spawnPos, Quaternion.identity, enemyParent);
                    enemyInstance.transform.parent = room.transform;
                }
            }
            else
            {
                Debug.LogWarning("No 'EnemySpawnPoint' found in " + room.name);
            }
        }
    }

    public void ChooseMiniBoss()
    {
        GameObject boss = minibossPrefabs[Random.Range(0, minibossPrefabs.Count)];
        Debug.Log("Mini-Boss Selected: " + boss.name);
        FindFirstObjectByType<FarthestRoom>()?.SummonBoss(boss);
    }

    public void ChooseBigBoss()
    {
        GameObject boss = bigBossPrefabs[Random.Range(0, bigBossPrefabs.Count)];
        Debug.Log("Big Boss Selected: " + boss.name);
        FindFirstObjectByType<FarthestRoom>()?.SummonBoss(boss);
    }
}
