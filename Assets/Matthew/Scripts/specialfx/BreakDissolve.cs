using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakDissolve : MonoBehaviour
{
    public Material Dissolve_Shader;
    public float DissolveSpeed { get; set; }
    [SerializeField] string dissolveAmountProperty = "_DissolveAmount";

    private Material[] Original_M;
    private MeshRenderer meshRenderer;
    private float currentDissolve = 0;


    private void Start()
    {

        meshRenderer = GetComponent<MeshRenderer>();

        Original_M = meshRenderer.materials;
        currentDissolve = 0;

        StartDisolve();
    }



    private void StartDisolve()
    {
        gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up, ForceMode.Impulse);

        Material[] dissolveMats = new Material[Original_M.Length];
        for (int i = 0; i < dissolveMats.Length; i++)
        {
            dissolveMats[i] = new Material(Dissolve_Shader);

            if (Original_M[i].HasProperty("_BaseMap"))
            {
                dissolveMats[i].SetTexture("_MainTexture", Original_M[i].GetTexture("_BaseMap"));

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

        meshRenderer.materials = dissolveMats;

        StartCoroutine(HandleDissolve());
    }

    private IEnumerator HandleDissolve()
    {
        while (currentDissolve < 1)
        {
            currentDissolve += Time.deltaTime * DissolveSpeed;

            foreach (var mat in GetComponent<Renderer>().materials)
            {
                mat.SetFloat("_DisolveValue", currentDissolve);
            }

            yield return null;
        }

        yield return null;

        Destroy(this.gameObject);
    }
}
