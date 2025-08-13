using System.Collections;

using UnityEngine;


public class MimicClone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _lifetime = 3f;
    [SerializeField] private float _damageMultiplier = 0.3f;
    [SerializeField] private GameObject _spawnVFX;
    [SerializeField] private GameObject _despawnVFX;

    private Weapon _weaponCopy;
    private Transform _playerTransform;
    private bool _hasAttacked;

    public void Initialize(Weapon OriginalWeapon , Transform Pos , float lifetime , float dmg)
    {
        if (OriginalWeapon.gameObject.GetComponent<MeshCollider>() != null)
        {
            OriginalWeapon.gameObject.GetComponent<MeshCollider>().enabled = false;
        }

        if (OriginalWeapon.gameObject.GetComponent<BoxCollider>() != null)
        {
            OriginalWeapon.gameObject.GetComponent<BoxCollider>().enabled = false;
        }

        if (OriginalWeapon.gameObject.GetComponent<Pickupable>() != null)
        {
            OriginalWeapon.gameObject.GetComponent<Pickupable>().enabled = false;
        }

        _lifetime = lifetime;
        _damageMultiplier = dmg;
        _playerTransform = Pos;
        if (OriginalWeapon != null)
        {
            _weaponCopy = Instantiate(OriginalWeapon, transform);
            _weaponCopy.transform.localPosition = Vector3.zero;
            _weaponCopy.transform.SetParent(this.transform);
        }

        //ParticalManager.Instance.PlayVFX(_spawnVFX , this.transform);
        Destroy(gameObject, _lifetime);
        StartCoroutine(PerformAttack());
    }

    private IEnumerator PerformAttack()
    {
        transform.rotation = _playerTransform.rotation;

        yield return new WaitForSeconds(0.2f);

        if (!_hasAttacked && _weaponCopy != null)
        {
            _hasAttacked = true;
            _weaponCopy.MimicCast();
        }
        
        yield return null;

    }
    private void OnDestroy()
    {
        //ParticalManager.Instance.PlayVFX(_despawnVFX, this.transform);
    }
}
