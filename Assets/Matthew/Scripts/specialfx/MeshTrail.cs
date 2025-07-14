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
    private Queue<GameObject> trailPool;
    private Transform poolParent;

    void Start()
    {
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        InitializePool();
    }

    void InitializePool()
    {
        // Create pool parent for organization
        poolParent = new GameObject("Trail Pool").transform;

        // Calculate optimal pool size (trails per second * delay * number of meshes)
        int poolSize = Mathf.CeilToInt((1f / meshRefreshRate) * meshDestroyDelay * skinnedMeshRenderers.Length) + 10;
        trailPool = new Queue<GameObject>(poolSize);

        for (int i = 0; i < poolSize; i++)
        {
            GameObject trailObj = new GameObject($"Trail_{i}");
            trailObj.SetActive(false);

            // Add necessary components
            MeshFilter filter = trailObj.AddComponent<MeshFilter>();
            MeshRenderer renderer = trailObj.AddComponent<MeshRenderer>();
            renderer.material = meshMaterial;

            // Add pooling controller
            TrailObjectController controller = trailObj.AddComponent<TrailObjectController>();
            controller.Initialize(this);

            trailObj.transform.SetParent(poolParent);
            trailPool.Enqueue(trailObj);
        }
    }

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
        while (timeActive > 0)
        {
            timeActive -= meshRefreshRate;

            foreach (SkinnedMeshRenderer skinnedRenderer in skinnedMeshRenderers)
            {
                if (trailPool.Count > 0)
                {
                    GameObject trailObj = trailPool.Dequeue();
                    ConfigureTrailObject(trailObj, skinnedRenderer);
                }
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }

        isTrailActive = false;
    }

    void ConfigureTrailObject(GameObject trailObj, SkinnedMeshRenderer skinnedRenderer)
    {
        trailObj.SetActive(true);
        trailObj.transform.SetPositionAndRotation(transform.position, transform.rotation);

        // Get or create mesh
        MeshFilter filter = trailObj.GetComponent<MeshFilter>();
        if (filter.sharedMesh == null)
            filter.sharedMesh = new Mesh();

        // Bake mesh from skinned renderer
        skinnedRenderer.BakeMesh(filter.sharedMesh);

        // Start return timer
        trailObj.GetComponent<TrailObjectController>().Activate(meshDestroyDelay);
    }

    public void ReturnToPool(GameObject trailObj)
    {
        trailObj.SetActive(false);
        trailPool.Enqueue(trailObj);
    }
}

// Helper class for pool objects
public class TrailObjectController : MonoBehaviour
{
    private MeshTrail meshTrail;

    public void Initialize(MeshTrail trail)
    {
        meshTrail = trail;
    }

    public void Activate(float delay)
    {
        StartCoroutine(DeactivateAfterDelay(delay));
    }

    IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        meshTrail.ReturnToPool(gameObject);
    }
}