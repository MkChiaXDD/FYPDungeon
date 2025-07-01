using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MimicSpawner : MonoBehaviour
{
    [SerializeField] public GameObject _mimicClonePrefab;
    [SerializeField] private float _lifetime = 3f;
    [SerializeField] private float _damageMultiplier = 0.2f;
    [SerializeField] private int SpawnAmount = 1;
    [SerializeField, Range(0f, 1f)] private float _spawnChance = 0.2f;

    //private PlayerCombat _playerCombat;

    //private void Awake()
    //{
    //    _playerCombat = GetComponent<PlayerCombat>();
    //}
    public void TrySpawnMimic()
    {
        if (_mimicClonePrefab == null || Random.value > _spawnChance)
        {
            return;
        }
        Vector3 SpawnPosition = CalculateSpawnPosition();
        GameObject Mimic = Instantiate(_mimicClonePrefab , SpawnPosition , Quaternion.identity);
        MimicClone mimicClone = Mimic.GetComponent<MimicClone>();
        if (mimicClone != null)
        {
            mimicClone.Initialize(PlayerCombat.Instance.WeaponChoosen , PlayerMovement.Instance._body , _lifetime , 1);
        }
        else
        {
            mimicClone.AddComponent<MimicClone>();
            mimicClone.Initialize(PlayerCombat.Instance.WeaponChoosen, PlayerMovement.Instance._body, _lifetime, 1);
        }
    }

    private Vector3 CalculateSpawnPosition()
    {
        return transform.position +
               (transform.right * 1.5f) +
               (Vector3.up * 0.5f);
    }
}
