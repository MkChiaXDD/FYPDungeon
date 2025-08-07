using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    [SerializeField] private GameObject MinimapObj;
    [SerializeField] private GameObject Map;
    [SerializeField] private Camera _camera;

    [SerializeField] private float MiniCameraSize;
    [SerializeField] private float NormalCameraSize;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float dragSpeed = 0.5f;
    [SerializeField] private Vector3 OrignialPosition;

    [SerializeField] public LayerMask MiniCam;
    [SerializeField] public LayerMask OrignalCam;

    private Vector3 dragOrigin;
    private bool isDragging = false;
    public void Start()
    {
        MiniCameraSize = _camera.orthographicSize;
    }
    public void ToggelMinimap()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            MinimapObj.SetActive(!MinimapObj.activeSelf);
        }
    }

    public void Toggelmap()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Map.activeSelf == false)
            {
                Map.SetActive(true);
                _camera.orthographicSize = NormalCameraSize;
                OrignialPosition = _camera.transform.position;
                _camera.cullingMask = OrignalCam;
                GamStates.instance.AddPauseStuff();
            }
            else
            {
                Map.SetActive(false);
                _camera.orthographicSize = MiniCameraSize;
                _camera.transform.position = OrignialPosition;
                _camera.cullingMask = MiniCam;
                GamStates.instance.RemovePauseStuff();
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
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - (scroll * zoomSpeed), MiniCameraSize, NormalCameraSize * 2);

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
            _camera.transform.position += move;
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
