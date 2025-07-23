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
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float dragSpeed = 0.5f;
    [SerializeField] private Vector3 OrignialPosition;

    private Vector3 dragOrigin;
    private bool isDragging = false;
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
                OrignialPosition = camera.transform.position;
            }
            else
            {
                Map.SetActive(false);
                camera.fieldOfView = MiniCameraSize;
                camera.transform.position = OrignialPosition;
            }
        }
    }

    public void MapInteraction()
    {
        if (!Map.activeSelf) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Debug.Log("Scroll : " + scroll);
        if (scroll != 0)
        {
            camera.fieldOfView = Mathf.Clamp(camera.fieldOfView - (scroll * zoomSpeed), MiniCameraSize, NormalCameraSize * 2);

        }

        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = Input.mousePosition;
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }

        if(isDragging)
        {
            Vector3 diffrence =  (dragOrigin - Input.mousePosition )* dragSpeed;
            Vector3 move = new Vector3(diffrence.x * dragSpeed, 0, diffrence.y * dragSpeed);
            camera.transform.position += move;
            //Debug.Log("Mouse Diffrence : "+diffrence);
            dragOrigin = Input.mousePosition;
        }
    }

    public void Update()
    {
        ToggelMinimap();
        Toggelmap();
        MapInteraction();
    }
}
