using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Difficulty Curve")]
    [SerializeField] private AnimationCurve difficultyCurve;
    [SerializeField] private List<Vector2> curvePoints = new();

    [Header("Enemy Spawn Scaling")]
    [SerializeField] private int baseMinEnemies = 1;
    [SerializeField] private int baseMaxEnemies = 3;
    [SerializeField] private float minMultiplier = 1f;
    [SerializeField] private float maxMultiplier = 2f;

    [SerializeField] private int currentRound = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        GenerateDifficultyCurve();
    }

    private void GenerateDifficultyCurve()
    {
        difficultyCurve = new AnimationCurve();

        foreach (Vector2 point in curvePoints)
            difficultyCurve.AddKey(new Keyframe(point.x, point.y));

        difficultyCurve.postWrapMode = WrapMode.ClampForever;
    }

    public float GetDifficultyMultiplier()
    {
        return difficultyCurve.Evaluate(currentRound);
    }

    public int GetMinEnemies()
    {
        float scale = GetDifficultyMultiplier();
        return Mathf.FloorToInt(baseMinEnemies + scale * minMultiplier);
    }

    public int GetMaxEnemies()
    {
        float scale = GetDifficultyMultiplier();
        return Mathf.CeilToInt(baseMaxEnemies + scale * maxMultiplier);
    }

    public void IncreaseRound()
    {
        currentRound++;
    }

    public int GetRound()
    {
        return currentRound;
    }
}
