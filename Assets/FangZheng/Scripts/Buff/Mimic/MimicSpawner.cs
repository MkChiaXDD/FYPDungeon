using Unity.VisualScripting;
using UnityEngine;

public class MimicSpawner : MonoBehaviour
{
    [SerializeField] public GameObject _mimicClonePrefab;
    [SerializeField] private float _lifetime = 3f;
    //[SerializeField] private float _damageMultiplier = 0.2f;
    [SerializeField] private int SpawnAmount = 1;
    [SerializeField, Range(0f, 1f)] private float _spawnChance = 0.1f;

    private void Start()
    {
        _spawnChance = PlayerData.Instance.MimicSpawnChance;
        SpawnAmount = PlayerData.Instance.MimicCount;
    }

    public void OnEnable()
    {

        PlayerData.Instance.DataChange.AddListener(Addmodifier);
    }

    public void Addmodifier()
    {
        _spawnChance = PlayerData.Instance.MimicSpawnChance;
        SpawnAmount = PlayerData.Instance.MimicCount;
    }
    public void TrySpawnMimic()
    {
        for (int i = 0; i < SpawnAmount; i++)
        {
            if (_mimicClonePrefab == null || Random.value > _spawnChance)
            {
                continue;
            }
            Vector3 SpawnPosition = CalculateSpawnPosition();
            GameObject Mimic = Instantiate(_mimicClonePrefab, SpawnPosition, Quaternion.identity);
            MimicClone mimicClone = Mimic.GetComponent<MimicClone>();
            if (mimicClone != null)
            {
                mimicClone.Initialize(PlayerCombat.Instance._currentWeapon, PlayerMovement.Instance._body, _lifetime, 1);
            }
            else
            {
                mimicClone.AddComponent<MimicClone>();
                mimicClone.Initialize(PlayerCombat.Instance._currentWeapon, PlayerMovement.Instance._body, _lifetime, 1);
            }
        }
    }

    private Vector3 CalculateSpawnPosition()
    {
        return transform.position +
               (transform.right * 1.5f) +
               (Vector3.up * 0.5f);
    }
}
