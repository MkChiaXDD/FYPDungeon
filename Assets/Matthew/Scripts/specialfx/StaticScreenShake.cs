using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class StaticScreenShake
{
    // Default configuration parameters
    public static ShakeParams DefaultParams = new ShakeParams()
    {
        ShakeType = ShakeType.Rotational,
        ShakeDuration = 0.2f,
        ShakeMagnitude = 10f,
        DampingSpeed = 15f,
        RotationalNoiseSpeed = 20f,
        TranslationalShakeMagnitude = new Vector3(2.0f, 2.0f, 0f),
        TranslationalNoiseSpeed = 1000f,
        UseSeparateNoiseForTranslation = true,
        EnableX = true,
        EnableY = true,
        EnableZ = true
    };

    public class ShakeState
    {
        public Camera Camera;
        public ShakeParams Params;
        public Quaternion OriginalRotation;
        public Vector3 OriginalPosition;
        public float RotationalNoiseOffset;
        public float TranslationalNoiseOffset;
        public Coroutine ActiveRoutine;
        public bool IsShaking;
    }

    private static Dictionary<Camera, ShakeState> activeStates = new Dictionary<Camera, ShakeState>();
    private static ScreenShakeRunner runner;

    public struct ShakeParams
    {
        public ShakeType ShakeType;
        public float ShakeDuration;
        public float ShakeMagnitude;
        public float DampingSpeed;
        public float RotationalNoiseSpeed;
        public Vector3 TranslationalShakeMagnitude;
        public float TranslationalNoiseSpeed;
        public bool UseSeparateNoiseForTranslation;
        public bool EnableX;
        public bool EnableY;
        public bool EnableZ;
    }

    // Initialize the static runner
    private static void Initialize()
    {
        if (runner != null) return;

        GameObject obj = new GameObject("ScreenShakeRunner");
        runner = obj.AddComponent<ScreenShakeRunner>();
        Object.DontDestroyOnLoad(obj);
    }

    /// <summary>
    /// Trigger a screen shake effect
    /// </summary>
    /// <param name="camera">Target camera (uses main camera if null)</param>
    /// <param name="parameters">Shake configuration</param>
    public static void Shake(Camera camera = null, ShakeParams? parameters = null)
    {
        Initialize();
        camera = camera ?? Camera.main;

        ShakeParams finalParams = parameters ?? DefaultParams;

        if (activeStates.TryGetValue(camera, out ShakeState state))
        {
            // Cancel active shake if running
            if (state.ActiveRoutine != null)
            {
                runner.StopCoroutine(state.ActiveRoutine);
            }
        }
        else
        {
            state = new ShakeState();
            activeStates[camera] = state;
        }

        // Initialize state
        state.Camera = camera;
        state.Params = finalParams;
        state.OriginalRotation = camera.transform.localRotation;
        state.OriginalPosition = camera.transform.localPosition;
        state.RotationalNoiseOffset = Random.Range(0f, 100f);
        state.TranslationalNoiseOffset = finalParams.UseSeparateNoiseForTranslation
            ? Random.Range(0f, 100f)
            : state.RotationalNoiseOffset;

        // Start shake routine
        state.ActiveRoutine = runner.StartCoroutine(PerformShake(state));
    }

    private static IEnumerator PerformShake(ShakeState state)
    {
        state.IsShaking = true;
        Camera camera = state.Camera;
        ShakeParams p = state.Params;
        float elapsed = 0f;
        float originalZ = state.OriginalRotation.eulerAngles.z;

        // Active shaking phase
        while (elapsed < p.ShakeDuration)
        {
            float intensity = 1f - (elapsed / p.ShakeDuration);

            // Apply rotational shake
            if (p.ShakeType == ShakeType.Rotational || p.ShakeType == ShakeType.Both)
            {
                float noise = Mathf.PerlinNoise(
                    state.RotationalNoiseOffset,
                    elapsed * p.RotationalNoiseSpeed
                ) * 2f - 1f;

                float zRot = originalZ + noise * p.ShakeMagnitude * intensity;
                camera.transform.localRotation = Quaternion.Euler(
                    state.OriginalRotation.eulerAngles.x,
                    state.OriginalRotation.eulerAngles.y,
                    zRot
                );
            }

            // Apply translational shake
            if (p.ShakeType == ShakeType.Translational || p.ShakeType == ShakeType.Both)
            {
                Vector3 offset = new Vector3(
                    p.EnableX ? (Mathf.PerlinNoise(state.TranslationalNoiseOffset + 100, elapsed * p.TranslationalNoiseSpeed)) * 2 - 1 : 0,
                    p.EnableY ? (Mathf.PerlinNoise(state.TranslationalNoiseOffset + 200, elapsed * p.TranslationalNoiseSpeed)) * 2 - 1 : 0,
                    p.EnableZ ? (Mathf.PerlinNoise(state.TranslationalNoiseOffset + 300, elapsed * p.TranslationalNoiseSpeed)) * 2 - 1 : 0
                );

                offset.Scale(p.TranslationalShakeMagnitude * intensity);
                camera.transform.localPosition = state.OriginalPosition + offset;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Damping phase
        state.IsShaking = false;
        while (NeedsReset(camera.transform, state))
        {
            // Smoothly reset rotation
            if (p.ShakeType != ShakeType.Translational)
            {
                camera.transform.localRotation = Quaternion.Slerp(
                    camera.transform.localRotation,
                    state.OriginalRotation,
                    Time.deltaTime * p.DampingSpeed
                );
            }

            // Smoothly reset position
            if (p.ShakeType != ShakeType.Rotational)
            {
                camera.transform.localPosition = Vector3.Lerp(
                    camera.transform.localPosition,
                    state.OriginalPosition,
                    Time.deltaTime * p.DampingSpeed
                );
            }
            yield return null;
        }

        // Final reset and cleanup
        camera.transform.localRotation = state.OriginalRotation;
        camera.transform.localPosition = state.OriginalPosition;
        activeStates.Remove(camera);
    }

    private static bool NeedsReset(Transform t, ShakeState state)
    {
        if (state.Params.ShakeType == ShakeType.Translational)
            return Vector3.Distance(t.localPosition, state.OriginalPosition) > 0.01f;

        if (state.Params.ShakeType == ShakeType.Rotational)
            return Quaternion.Angle(t.localRotation, state.OriginalRotation) > 0.01f;

        return Vector3.Distance(t.localPosition, state.OriginalPosition) > 0.01f ||
               Quaternion.Angle(t.localRotation, state.OriginalRotation) > 0.01f;
    }

    // Hidden MonoBehaviour to run coroutines
    private class ScreenShakeRunner : MonoBehaviour { }
}