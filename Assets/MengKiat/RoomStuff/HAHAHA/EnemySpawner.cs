using RMG;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGen;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private float offSet = 1.5f;
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private List<GameObject> bossPrefabs;
    [SerializeField] private float healerSpawnChance = 0.001f;
    [SerializeField] private GameObject nextLevelPortal;

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
            // Get all child transforms that contain "EnemySpawnPoint" in their name
            Transform[] spawnPoints = room.GetComponentsInChildren<Transform>();
            List<Transform> validSpawnPoints = new List<Transform>();

            foreach (Transform t in spawnPoints)
            {
                if (t.name.Contains("EnemySpawnPoint"))
                {
                    validSpawnPoints.Add(t);
                }
            }

            if (validSpawnPoints.Count > 0)
            {
                int round = diffMgr.GetRound();
                int minEnemies = diffMgr.GetMinEnemies();
                int maxEnemies = diffMgr.GetMaxEnemies();
                int numOfEnemies = Random.Range(minEnemies, maxEnemies + 1);

                for (int i = 0; i < numOfEnemies; i++)
                {
                    // Pick a random spawn point from the available ones
                    Transform spawnPoint = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
                    Vector3 spawnOffset = new Vector3(Random.Range(-offSet, offSet), 0, Random.Range(-offSet, offSet));
                    Vector3 spawnPos = spawnPoint.position + spawnOffset;

                    GameObject chosenEnemy = null;

                    // Try to spawn a healer with low chance (independent of scaling)
                    if (Random.value <= healerSpawnChance)
                    {
                        foreach (GameObject enemy in enemyPrefabs)
                        {
                            if (enemy.name.ToLower().Contains("healer"))
                            {
                                chosenEnemy = enemy;
                                break;
                            }
                        }
                    }

                    // If not healer, pick based on round-scaling
                    if (chosenEnemy == null)
                    {
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
                            else if (!name.Contains("fast") && !name.Contains("ranged") && !name.Contains("tank") && !name.Contains("bomber") && !name.Contains("healer"))
                                availableEnemies.Add(enemy); // Basic/default enemies only
                        }

                        if (availableEnemies.Count == 0)
                        {
                            availableEnemies.Add(enemyPrefabs[0]);
                            Debug.LogWarning("No enemies unlocked at round " + round + ". Using fallback.");
                        }

                        chosenEnemy = availableEnemies[Random.Range(0, availableEnemies.Count)];
                    }

                    GameObject enemyInstance = Instantiate(chosenEnemy, spawnPos, Quaternion.identity, enemyParent);
                }
            }
            else
            {
                //Debug.LogWarning("No 'EnemySpawnPoint' found in " + room.name);
            }
        }

    }

    public void ClearEnemies()
    {
        if (enemyParent == null)
        {
            Debug.LogWarning("Enemy parent not assigned.");
            return;
        }

        // Loop through all children of enemyParent and destroy them
        for (int i = enemyParent.childCount - 1; i >= 0; i--)
        {
            Transform child = enemyParent.GetChild(i);
            Destroy(child.gameObject);
        }

        Debug.Log("All enemies cleared.");
    }


    public void ChooseBoss()
    {
        GameObject spawnObj = GameObject.Find("BossSpawnPos");

        if (spawnObj == null)
        {
            Debug.LogError("❌ BossSpawnPos not found in the scene!");
            return;
        }

        Transform spawnPoint = spawnObj.transform;

        if (bossPrefabs == null || bossPrefabs.Count == 0)
        {
            Debug.LogError("❌ No boss prefabs assigned!");
            return;
        }

        GameObject bossPrefab = bossPrefabs[Random.Range(0, bossPrefabs.Count)];
        GameObject chosenBoss = Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity, enemyParent);

        GameObject nextLvlPortal = Instantiate(nextLevelPortal, spawnPoint.position, Quaternion.identity);
        nextLvlPortal.SetActive(false);

        chosenBoss.AddComponent<BossCheckDeath>();
    }

}
