using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBreak : MonoBehaviour
{
    [SerializeField] Material Dissolve_Shader;
    [SerializeField] float dissolveSpeed = 1f;
    [SerializeField] string dissolveAmountProperty = "_DissolveAmount";
    
    private Material[] Original_M;
    private Renderer weaponRenderer;
    private bool WepoanBreak;
    private float currentDissolve = 0;

    private void OnEnable()
    {
        
    }
    private void Start()
    {
        weaponRenderer = GetComponent<Renderer>();

        Original_M = weaponRenderer.materials;
        currentDissolve = 0;

        StartDisolve();
    }

    private void StartDisolve()
    {
        Material[] dissolveMats = new Material[Original_M.Length];
        for (int i = 0; i < dissolveMats.Length; i++)
        {
            dissolveMats[i] = new Material(Dissolve_Shader);
            if (Original_M[i].HasProperty("_BaseMap"))
            {
                dissolveMats[i].SetTexture("_MainTexture" , Original_M[i].GetTexture("_BaseMap"));
                
            }

            if (Original_M[i].HasProperty("_MetallicGlossMap"))
            {
                dissolveMats[i].SetTexture("_MetalicTexture", Original_M[i].GetTexture("_MetallicGlossMap"));
            }

            if (Original_M[i].HasProperty("_OcclusionMap"))
            {
                dissolveMats[i].SetTexture("_AmbientOcclusionTexture", Original_M[i].GetTexture("_OcclusionMap"));
            }

            if (Original_M[i].HasProperty("_ParallaxMap"))
            {
                dissolveMats[i].SetTexture("_HightTexture", Original_M[i].GetTexture("_ParallaxMap"));
            }

            if (Original_M[i].HasProperty("_BumpMap"))
            {
                dissolveMats[i].SetTexture("_NormalTexture", Original_M[i].GetTexture("_BumpMap"));
            }
        }

        weaponRenderer.materials = dissolveMats;

        StartCoroutine(Dissolving());
    }

    private IEnumerator Dissolving()
    {
        while (currentDissolve < 1)
        {
            currentDissolve += Time.deltaTime * dissolveSpeed;

            foreach (var mat in GetComponent<Renderer>().materials)
            {
                mat.SetFloat("_DisolveValue", currentDissolve);
            }

            yield return null;
        }

        yield return null;
    }
}
