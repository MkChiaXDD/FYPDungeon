using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyKillScale
{
    public int round;
    public int killCount;
    public int killGoal; // 👈 Goal to complete the quest
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

        PrintKillData();
    }

    public void IncreaseKills()
    {
        if (difMgr == null) return;

        currentRound = difMgr.GetRound();

        EnemyKillScale currentEntry = enemyKillsEachRound.Find(e => e.round == currentRound);

        if (currentEntry != null)
        {
            currentEntry.killCount++;

            // 👇 Show status after each kill
            Debug.Log($"Round {currentRound}: {currentEntry.killCount} / {currentEntry.killGoal} kills");

            // ✅ Check if quest complete
            if (currentEntry.killCount >= currentEntry.killGoal)
            {
                Debug.Log($"✅ Quest complete for Round {currentRound}! Kill Goal: {currentEntry.killGoal}");
            }
        }
        else
        {
            Debug.LogWarning($"No kill goal defined for round {currentRound}. Adding default goal (10).");

            // Add a new entry with default goal and log it
            var newEntry = new EnemyKillScale
            {
                round = currentRound,
                killCount = 1,
                killGoal = 10
            };

            enemyKillsEachRound.Add(newEntry);
            Debug.Log($"Round {currentRound}: 1 / {newEntry.killGoal} kills");
        }
    }


    public int GetKillCountForRound(int round)
    {
        var entry = enemyKillsEachRound.Find(e => e.round == round);
        return entry != null ? entry.killCount : 0;
    }

    public bool IsKillGoalReached(int round)
    {
        var entry = enemyKillsEachRound.Find(e => e.round == round);
        return entry != null && entry.killCount >= entry.killGoal;
    }

    public void PrintKillData()
    {
        if (enemyKillsEachRound == null || enemyKillsEachRound.Count == 0)
        {
            Debug.Log("No enemy kill data recorded yet.");
            return;
        }

        foreach (var entry in enemyKillsEachRound)
        {
            Debug.Log($"Round {entry.round}: {entry.killCount}/{entry.killGoal} kills");
        }
    }
}
