using UnityEngine;

public class Lever : MonoBehaviour
{
    [SerializeField] private GameObject lever;
    [SerializeField] private GameObject canvas;

    [Header("Insert Door")]
    [SerializeField] private Door door;

    public bool isOn = false;
    private bool isColliding = false;

    private void Start()
    {
        if (!isOn)
        {
            canvas.SetActive(isOn);
        }
    }

    private void Update()
    {
        if (isColliding)
        {
            Debug.Log("Can be pressed");
            if (Input.GetKeyDown(KeyCode.E))
            {
                ToggleLever();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PlayerBody"))
        {
            isColliding = true;
            canvas.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerBody"))
        {
            isColliding = false;
            canvas.SetActive(false);
        }
    }

    public void ToggleLever()
    {
        isOn = !isOn;

        if (!isOn)
        {
            lever.transform.localRotation = Quaternion.Euler(-45f, 0f, 0f);
        }
        else
        {
            lever.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
        }

        door.ToggleDoor(isOn);
    }
}
