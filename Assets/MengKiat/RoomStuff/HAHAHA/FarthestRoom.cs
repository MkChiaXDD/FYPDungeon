using RMG;
using UnityEngine;

public class FarthestRoom : MonoBehaviour
{
    private DifficultyManager difMgr;
    private MapGenerator mapGen;

    // Start is called before the first frame update
    void Awake()
    {
        difMgr = FindFirstObjectByType<DifficultyManager>();
        mapGen = FindFirstObjectByType<MapGenerator>();
        Debug.Log($"Script Connected to {gameObject.name}");
    }

    public void SummonBoss(GameObject boss)
    {
        Debug.Log("FARTHESTROOM: Boss Name: " + boss.name);
        Vector3 spawnPoint = transform.Find("EnemySpawnPoint").localPosition;
        GameObject newBoss = Instantiate(boss, spawnPoint, Quaternion.identity);
        newBoss.transform.position = transform.position * 4;
        newBoss.AddComponent<BossCheckDeath>();
        Debug.Log("Boss Spawned");
    }

    public void NextLevel()
    {
        Debug.Log("Player reach next level");
        difMgr.IncreaseRound();
        mapGen.Generate();
        GameObject player = GameObject.FindWithTag("Player");
        player.transform.position = new Vector3(0, 4, 0);
    }
}
