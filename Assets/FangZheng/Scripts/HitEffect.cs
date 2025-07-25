//using Cdm.Figma; //I comment this out as it is showing error. - Kapaw
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    [SerializeField] private List<GameObject> PlayerModels = new List<GameObject>();
    [SerializeField] private List<GameObject> ModelWIthMaterial = new List<GameObject>();
    //[SerializeField] private Material[] Orignial;
    private Dictionary<SkinnedMeshRenderer, Material> originalMaterials = new Dictionary<SkinnedMeshRenderer, Material>();
    [SerializeField] private Material hitMaterials;
    [SerializeField] private float hitduration;

    [SerializeField] private List<GameObject> Enemies = new List<GameObject>();
    [SerializeField] private Dictionary<MeshRenderer, Material> OriginEnemyRenderer = new Dictionary<MeshRenderer, Material>();

    public void Start()
    {
        PlayerModels.Clear();

        GetAllChildrenRecursive(Player, ref PlayerModels);

        foreach (GameObject child in PlayerModels)
        {
            SkinnedMeshRenderer Render = child.GetComponent<SkinnedMeshRenderer>();
            if (Render != null)
            {
                ModelWIthMaterial.Add(child);
                originalMaterials.Add(Render , Render.material);
            }
        }

        
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            TriggerHit();
        }
    }
    private void GetAllChildrenRecursive(GameObject parent, ref List<GameObject> childrenList)
    {

        foreach (Transform child in parent.transform)
        {

            childrenList.Add(child.gameObject);

            GetAllChildrenRecursive(child.gameObject, ref childrenList);
        }
    }

    public void TriggerHit()
    {
        StartCoroutine(HitEffectDuration());
    }

    public IEnumerator HitEffectDuration()
    {
        foreach (GameObject child in ModelWIthMaterial)
        {
            child.GetComponent<SkinnedMeshRenderer>().material = hitMaterials;
        }

        yield return new WaitForSeconds(hitduration);

        foreach (GameObject child in ModelWIthMaterial)
        {
            SkinnedMeshRenderer Render = child.GetComponent<SkinnedMeshRenderer>();
            for (int i = 0; i < originalMaterials.Count; i++) {
                if (originalMaterials.TryGetValue(Render , out Material OriginMat) ) {
                    Render.material = OriginMat;

                }
            }
        }
    }

    public void TriggerEnemyEffect()
    {

    }
}
