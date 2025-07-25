using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class WallTrans : MonoBehaviour
{
    public Transform player;
    public LayerMask Wall;
    public LayerMask Playerlayer;
    public float transparencyAmount = 0.5f;
    public float extraDetectionMargin = 0.1f;

    private BoxCollider playerCollider;

    public List<Renderer> AllWalls;

    public List<Renderer> currentlyTransparentWalls = new List<Renderer>();
    public Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    
    public Dictionary<Renderer, Material[]> transparentMaterials = new Dictionary<Renderer, Material[]>();

    public Material DitteringMat;

    public List<GameObject> HiddenObject;
    private void Start()
    {
        playerCollider = player.GetComponent<BoxCollider>();

        foreach (Renderer wall in AllWalls)
        {
            Material[] Materials = new Material[wall.materials.Length];
            for (int i = 0; i < Materials.Length; i++)
            {
                Materials[i] = wall.materials[i];
                Color color = Materials[i].color;
                color.a = transparencyAmount;
                Materials[i].color = color;

                Materials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                Materials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                Materials[i].EnableKeyword("_ALPHABLEND_ON");

                Materials[i].renderQueue = 3000;
            }
            transparentMaterials.Add(wall , Materials);
        }
    }

    private void Update()
    {
        Vector3 Direction =  transform.position - player.position;

        //float distance = Direction.magnitude;
        // Calculate collider bounds with extra margin
        Bounds colliderBounds = playerCollider.bounds;
        colliderBounds.Expand(extraDetectionMargin * 2); // Expand in all directions


        RaycastHit[] hits;
        hits = Physics.BoxCastAll(
            colliderBounds.center,
            colliderBounds.extents,
            Direction.normalized,
            player.rotation,
            1000,
            Wall

        );

    
        ResetWall();
        foreach (RaycastHit hit in hits)
        {
            Renderer wallRend = hit.transform.gameObject.GetComponent<Renderer>();
            if (wallRend != null)
            {
                TransWall(wallRend);
                //wallRend.enabled = false;
                HiddenObject.Add(hit.transform.gameObject);
            }
        }
    }

    public void ResetWall()
    {
        foreach (Renderer wallRenderer in currentlyTransparentWalls)
        {
            if (wallRenderer != null && originalMaterials.ContainsKey(wallRenderer))
            {
                wallRenderer.materials = originalMaterials[wallRenderer];
            }
        }
        currentlyTransparentWalls.Clear();

        foreach (GameObject hidenwalls in HiddenObject)
        {
            if (hidenwalls)
            hidenwalls.GetComponent<Renderer>().enabled = true;
        }
        HiddenObject.Clear();
    }

    public void TransWall(Renderer WallRenderer)
    {
        if (!originalMaterials.ContainsKey(WallRenderer))
        {
            originalMaterials[WallRenderer] = WallRenderer.materials;
        }

        Material[] Materials = new Material[WallRenderer.materials.Length];
        for (int i = 0; i < Materials.Length; i++)
        {
            //This is for cloning a new material then do funny modifcation to it so it will not do modificcation to the orignial materail
            Materials[i] = new Material(originalMaterials[WallRenderer][i]);
            Color color = Materials[i].color;
            color.a = transparencyAmount;
            Materials[i].color = color;

            //Enable for trans by configing the materail property
            Materials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            Materials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            Materials[i].EnableKeyword("_ALPHABLEND_ON");

            Materials[i].renderQueue = 3000;
            Materials[i] = DitteringMat;

        }
        //List<Material> Materials2 = new List<Material>();
        //for (int i = 0; i < Materials.Length; i++)
        //{
        //    Materials2.Add(Materials[i]);
        //}
        //Materials2.Add(DitteringMat);
        WallRenderer.materials = Materials;
        
        currentlyTransparentWalls.Add(WallRenderer);
    }

    //private void OnDrawGizmos()
    //{

    //        Gizmos.color = Color.red;
    //        Gizmos.DrawLine(transform.position, player.position );
        
    //}
}
