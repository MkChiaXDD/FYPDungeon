using System.Collections.Generic;
using UnityEngine;

public class WormController : MonoBehaviour
{
    [Header("Worm Settings")]
    public int segmentCount = 5;
    public float segmentSpacing = 0.5f;
    public GameObject segmentPrefab;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float turnSpeed = 180f;

    private List<Transform> segments = new List<Transform>();
    private List<Vector3> positionHistory = new List<Vector3>();

    void Start()
    {
        // Create segments
        for (int i = 0; i < segmentCount; i++)
        {
            GameObject seg = Instantiate(segmentPrefab);
            seg.transform.position = transform.position - transform.forward * segmentSpacing * (i + 1);
            segments.Add(seg.transform);
        }
    }

    void Update()
    {
        // Move head
        float h = Input.GetAxisRaw("Horizontal");
        transform.Rotate(Vector3.up, h * turnSpeed * Time.deltaTime);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        // Store current head position
        positionHistory.Insert(0, transform.position);
        int maxHistory = segmentCount * 10;
        if (positionHistory.Count > maxHistory)
            positionHistory.RemoveAt(positionHistory.Count - 1);

        // Move segments
        for (int i = 0; i < segments.Count; i++)
        {
            int historyIndex = (i + 1) * 10;
            if (positionHistory.Count > historyIndex)
            {
                Vector3 targetPos = positionHistory[historyIndex];
                Transform segment = segments[i];

                segment.position = Vector3.Lerp(segment.position, targetPos, Time.deltaTime * 10f);

                Vector3 dir = targetPos - segment.position;
                if (dir != Vector3.zero)
                    segment.rotation = Quaternion.LookRotation(dir);
            }
        }
    }
}
