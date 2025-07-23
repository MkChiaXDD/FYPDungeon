using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class EnemyHitSquash : MonoBehaviour
{
    [Header("Squash Settings")]
    [SerializeField, Tooltip("Duration of the squash effect in seconds")]
    private float squashDuration = 0.3f;

    [SerializeField, Tooltip("How much the enemy squashes vertically when hit (0-1)")]
    private float squashAmount = 0.5f;

    [SerializeField, Tooltip("How much the enemy stretches horizontally when hit")]
    private float stretchMultiplier = 1.2f;

    [SerializeField, Tooltip("Curve to control the squash animation")]
    private AnimationCurve squashCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.3f, 1f),
        new Keyframe(1f, 0f)
    );

    [Header("Recovery Settings")]
    [SerializeField, Tooltip("Duration of the recovery after squash in seconds")]
    private float recoveryDuration = 0.2f;

    [SerializeField, Tooltip("Curve to control the recovery animation")]
    private AnimationCurve recoveryCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Mesh originalMesh;
    private Mesh modifiedMesh;
    private Vector3[] originalVertices;
    private Vector3[] modifiedVertices;

    private float currentSquashTime = -1f;
    private float currentRecoveryTime = -1f;
    private bool isSquashing = false;
    private bool isRecovering = false;

    private void Awake()
    {
        // Cache the original mesh and create a copy to modify
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        originalMesh = meshFilter.mesh;
        modifiedMesh = Instantiate(originalMesh);
        meshFilter.mesh = modifiedMesh;

        originalVertices = originalMesh.vertices;
        modifiedVertices = new Vector3[originalVertices.Length];
        System.Array.Copy(originalVertices, modifiedVertices, originalVertices.Length);
    }

    private void Update()
    {
        if (isSquashing)
        {
            currentSquashTime += Time.deltaTime;
            float progress = Mathf.Clamp01(currentSquashTime / squashDuration);
            float curveValue = squashCurve.Evaluate(progress);

            ApplySquashEffect(curveValue);

            if (currentSquashTime >= squashDuration)
            {
                isSquashing = false;
                StartRecovery();
            }
        }
        else if (isRecovering)
        {
            currentRecoveryTime += Time.deltaTime;
            float progress = Mathf.Clamp01(currentRecoveryTime / recoveryDuration);
            float curveValue = recoveryCurve.Evaluate(progress);

            ApplyRecoveryEffect(curveValue);

            if (currentRecoveryTime >= recoveryDuration)
            {
                isRecovering = false;
                ResetMesh();
            }
        }
    }

    /// <summary>
    /// Call this method when the enemy gets hit to trigger the squash effect
    /// </summary>
    public void PlaySquashEffect()
    {
        if (isSquashing || isRecovering) return;

        currentSquashTime = 0f;
        isSquashing = true;
    }

    private void ApplySquashEffect(float amount)
    {
        float verticalScale = 1f - (amount * squashAmount);
        float horizontalScale = 1f + (amount * (stretchMultiplier - 1f));

        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];

            // Apply squash/stretch based on vertex position
            float verticalFactor = vertex.y * verticalScale;
            float horizontalFactorX = vertex.x * horizontalScale;
            float horizontalFactorZ = vertex.z * horizontalScale;

            modifiedVertices[i] = new Vector3(horizontalFactorX, verticalFactor, horizontalFactorZ);
        }

        modifiedMesh.vertices = modifiedVertices;
        modifiedMesh.RecalculateNormals();
        modifiedMesh.RecalculateBounds();
    }

    private void StartRecovery()
    {
        currentRecoveryTime = 0f;
        isRecovering = true;
    }

    private void ApplyRecoveryEffect(float amount)
    {
        // Interpolate between current deformed state and original state
        for (int i = 0; i < originalVertices.Length; i++)
        {
            modifiedVertices[i] = Vector3.Lerp(modifiedVertices[i], originalVertices[i], amount);
        }

        modifiedMesh.vertices = modifiedVertices;
        modifiedMesh.RecalculateNormals();
        modifiedMesh.RecalculateBounds();
    }

    private void ResetMesh()
    {
        // Reset to original vertices
        System.Array.Copy(originalVertices, modifiedVertices, originalVertices.Length);
        modifiedMesh.vertices = modifiedVertices;
        modifiedMesh.RecalculateNormals();
        modifiedMesh.RecalculateBounds();
    }

    private void OnDestroy()
    {
        // Clean up the modified mesh to prevent memory leaks
        if (modifiedMesh != null)
        {
            if (Application.isPlaying)
            {
                Destroy(modifiedMesh);
            }
            else
            {
                DestroyImmediate(modifiedMesh);
            }
        }
    }
}