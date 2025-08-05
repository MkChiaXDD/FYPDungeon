using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Insert Door(s)")]
    [SerializeField] private List<Door> doors;

    [Header("Plate Visual")]
    [SerializeField] private Transform plateMesh; // The part you step on
    [SerializeField] private float pressDepth = 0.1f;
    [SerializeField] private float pressSpeed = 4f;

    private Vector3 plateStartPos;
    private Coroutine plateCoroutine;
    private bool isOn = false;

    private void Start()
    {
        if (plateMesh != null)
            plateStartPos = plateMesh.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOn = true;
            ToggleDoor(true);
            AnimatePlate(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOn = false;
            ToggleDoor(false);
            AnimatePlate(false);
        }
    }

    private void ToggleDoor(bool open)
    {
        foreach (Door door in doors)
        {
            door.ToggleDoor(open);
        }
    }

    private void AnimatePlate(bool pressed)
    {
        if (plateCoroutine != null)
            StopCoroutine(plateCoroutine);

        Vector3 targetPos = plateStartPos + (pressed ? Vector3.down * pressDepth : Vector3.zero);
        plateCoroutine = StartCoroutine(MovePlate(targetPos));
    }

    private IEnumerator MovePlate(Vector3 targetPos)
    {
        while (Vector3.Distance(plateMesh.localPosition, targetPos) > 0.001f)
        {
            plateMesh.localPosition = Vector3.Lerp(plateMesh.localPosition, targetPos, Time.deltaTime * pressSpeed);
            yield return null;
        }
        plateMesh.localPosition = targetPos;
    }
}
