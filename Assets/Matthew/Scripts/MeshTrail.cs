using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    public float activeTime = 2f;

    [Header("Mesh Related")]
    public float meshRefreshRate = 0.01f;
    public float meshDestroyDelay = 0.1f;

    [Header("Shader Related")]
    public Material meshMaterial;
    private bool isTrailActive;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;
   

    public void HandleTrailActivation()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTrailActive)
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));
        }
    }

    public IEnumerator ActivateTrail(float timeActive)
    {
        if (skinnedMeshRenderers == null)
        {
            skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        while (timeActive > 0)
        {
            timeActive -= meshRefreshRate;

            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                GameObject clonedMeshGO = new GameObject();
                clonedMeshGO.transform.SetPositionAndRotation(transform.position, transform.rotation); //copy the moment the character position/rotation at

                MeshRenderer meshRenderer = clonedMeshGO.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = clonedMeshGO.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                skinnedMeshRenderers[i].BakeMesh(mesh);

                meshFilter.mesh = mesh;
                meshRenderer.material = meshMaterial;

                Destroy(clonedMeshGO, meshDestroyDelay);

            }

            yield return new WaitForSeconds(meshRefreshRate);
        }

        isTrailActive = false;
    }

    
}
