//using Cdm.Figma; //I comment this out as it is showing error.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitEffect : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    [SerializeField] private List<GameObject> PlayerModels = new List<GameObject>();
    [SerializeField] private List<GameObject> ModelWIthMaterial = new List<GameObject>();
    //[SerializeField] private Material[] Orignial;
    private Dictionary<SkinnedMeshRenderer, Material> originalMaterials = new Dictionary<SkinnedMeshRenderer, Material>();
    [SerializeField] private Material hitMaterials;
    [SerializeField] private float hitduration;
    [SerializeField] private Color HitColor = Color.red;
    [SerializeField] private float ColorSpeed = 2.0f;
    [SerializeField] private bool fixthenactive = false;
    private bool hitActive = false;

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
        if (fixthenactive == true)
        {
            StartCoroutine(HitEffectDuration());
        }
    }

    public IEnumerator HitEffectDuration()
    {
        float TimeWait = 0;
        hitActive = true;
        //int ii = 0;

        Dictionary<SkinnedMeshRenderer, Material> MaterialStorage = new Dictionary<SkinnedMeshRenderer, Material>();
        foreach (GameObject child in ModelWIthMaterial)
        {
            MaterialStorage.Add(child.GetComponent<SkinnedMeshRenderer>(), new Material(child.GetComponent<SkinnedMeshRenderer>().material));
        }

        while (TimeWait < hitduration / 2)
        {
            foreach (GameObject child in ModelWIthMaterial)
            {
                Material material = new Material(child.GetComponent<SkinnedMeshRenderer>().material);
                material.color = Color.Lerp(material.color, HitColor, TimeWait * ColorSpeed);
                child.GetComponent<SkinnedMeshRenderer>().material = material;
            }

            TimeWait += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
            

        }

        TimeWait = 0.0f ;

        while (TimeWait < hitduration)
        {
            foreach (GameObject child in ModelWIthMaterial)
            {
                SkinnedMeshRenderer Render = child.GetComponent<SkinnedMeshRenderer>();
                for (int i = 0; i < originalMaterials.Count; i++)
                {
                    if (originalMaterials.TryGetValue(Render, out Material OriginMat))
                    {

                        //Render.material.color = OriginMat.color;
                        Render.material.color = Color.Lerp(Render.material.color , OriginMat.color , TimeWait / ColorSpeed);

                    }
                }
            }
            
            TimeWait += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);

        }
            //yield return new WaitForSeconds(hitduration);

        //    foreach (GameObject child in ModelWIthMaterial)
        //{
        //    SkinnedMeshRenderer Render = child.GetComponent<SkinnedMeshRenderer>();
        //    for (int i = 0; i < originalMaterials.Count; i++)
        //    {
        //        if (originalMaterials.TryGetValue(Render, out Material OriginMat))
        //        {

        //            //Render.material.color = OriginMat.color;
        //            Render.material = OriginMat;

        //        }
        //    }
        //}
    }
}
