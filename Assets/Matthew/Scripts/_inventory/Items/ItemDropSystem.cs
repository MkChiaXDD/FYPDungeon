
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
    [SerializeField] private float _throwForce = 5f;

    private TutorialProggresion _progression;
    private void Start()
    {
        _progression = FindFirstObjectByType<TutorialProggresion>();
    }
    public void SpawnDropItem()
    {
        if (_progression != null)
        {
            _progression.IfPlayerPerformAction("BreakCrate");
        }
        foreach (var _drop in _dropItem)
        {
            if (Random.Range(0.0f, 100.0f) <= _drop._dropRate)
            {
                // Spawn position above the object
                Vector3 spawnPosition = transform.position + Vector3.up * _dropHeight;

                // Spawn the item
                GameObject droppedItem = Instantiate(_drop._dropItemPrefab, spawnPosition, Quaternion.identity);

                // Random throw direction on horizontally (xz)
                Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0.5f, Random.Range(-1f, 1f)).normalized;

                // Apply force if it got rb
                Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(randomDir * _throwForce, ForceMode.Impulse);
                }
            }
        }
    }
}
