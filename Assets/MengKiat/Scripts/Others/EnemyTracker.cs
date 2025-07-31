using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyKillScale
{
    public int round;
    public int killCount;
}

public class EnemyTracker : MonoBehaviour
{
    [SerializeField] private List<EnemyKillScale> enemyKillsEachRound = new List<EnemyKillScale>();
    [SerializeField] private DifficultyManager difMgr;

    private int currentRound;

    void Start()
    {
        if (difMgr != null)
        {
            currentRound = difMgr.GetRound();
        }
        else
        {
            Debug.LogWarning("DifficultyManager is not assigned.");
        }
    }

    void Update()
    {
        // Optional: you can update currentRound here if it changes in real-time
        // currentRound = difMgr.GetRound();
    }

    public void IncreaseKills()
    {
        if (difMgr == null) return;

        currentRound = difMgr.GetRound();

        // Find existing record for current round
        EnemyKillScale currentEntry = enemyKillsEachRound.Find(e => e.round == currentRound);

        if (currentEntry != null)
        {
            currentEntry.killCount++;
        }
        else
        {
            enemyKillsEachRound.Add(new EnemyKillScale
            {
                round = currentRound,
                killCount = 1
            });
        }
    }

    // Optional: Get total kills for a specific round
    public int GetKillCountForRound(int round)
    {
        var entry = enemyKillsEachRound.Find(e => e.round == round);
        return entry != null ? entry.killCount : 0;
    }

    // Optional: Debug log all rounds and kills
    public void PrintKillData()
    {
        foreach (var entry in enemyKillsEachRound)
        {
            Debug.Log($"Round {entry.round}: {entry.killCount} kills");
        }
    }
}
