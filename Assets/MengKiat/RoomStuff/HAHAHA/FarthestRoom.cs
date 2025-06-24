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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player reach next level");
            difMgr.IncreaseRound();
            //mapGen.Generate();
            //collision.gameObject.transform.position = new Vector3(0, 4, 0);
        }
    }

    public void SummonBoss(GameObject boss)
    {
        Debug.Log("FARTHESTROOM: Boss Name: " + boss.name);
        Vector3 spawnPoint = transform.Find("EnemySpawnPoint").localPosition;
        GameObject newBoss = Instantiate(boss, spawnPoint, Quaternion.identity, transform);
        newBoss.transform.position = transform.position + new Vector3(0, spawnPoint.y, 0);
        Debug.Log("MiniBoss Spawned");
    }
}
