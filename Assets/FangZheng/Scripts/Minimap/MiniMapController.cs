using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    [SerializeField] private GameObject MinimapObj;
    [SerializeField] private GameObject Map;
    [SerializeField] private Camera camera;

    [SerializeField] private float MiniCameraSize;
    [SerializeField] private float NormalCameraSize;
    public void Start()
    {
        MiniCameraSize = camera.fieldOfView;
    }
    public void ToggelMinimap()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            MinimapObj.SetActive(!MinimapObj.gameObject.active);
        }
    }

    public void Toggelmap()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Map.active == false)
            {
                Map.SetActive(true);
                camera.fieldOfView = NormalCameraSize;
            }
            else
            {
                Map.SetActive(false);
                camera.fieldOfView = MiniCameraSize;
            }
        }
    }

    public void Update()
    {
        ToggelMinimap();
        Toggelmap();
    }
}
