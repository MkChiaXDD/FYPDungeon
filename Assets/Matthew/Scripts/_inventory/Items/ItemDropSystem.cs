using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DropItem
{
    public GameObject _dropItemPrefab;

    [Range(0.0f, 100.0f)] public float _dropRate;
}

public class ItemDropSystem : MonoBehaviour
{
    public DropItem[] _dropItem;

    [SerializeField] private float _dropHeight = 3.5f;

    public void SpawnDropItem()
    {
        foreach (var _drop in _dropItem)
        {
            if (Random.Range(0.0f, 100.0f) <= _drop._dropRate)
            {
                Vector3 spawnPosition = transform.position + Vector3.up * _dropHeight;
                Instantiate(_drop._dropItemPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }
}
