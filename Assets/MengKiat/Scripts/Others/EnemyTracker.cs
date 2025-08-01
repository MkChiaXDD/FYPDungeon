using System.Collections.Generic;
using UnityEngine;
using TMPro; // Add this namespace for TextMeshPro support

[System.Serializable]
public class EnemyKillScale
{
    public int round;
    public int killCount;
    public int killGoal;
}

public class EnemyTracker : MonoBehaviour
{
    [SerializeField] private List<EnemyKillScale> enemyKillsEachRound = new List<EnemyKillScale>();
    [SerializeField] private DifficultyManager difMgr;
    [SerializeField] private TMP_Text killCountText; // Reference to your TMP text component

    private int currentRound;
    private GameObject bossPortal;

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

        UpdateKillCountText(); // Update text on start
        PrintKillData();
    }

    public void SetBossPortal(GameObject newPortal)
    {
        if (newPortal == null)
        {
            Debug.LogWarning("SetBossPortal was called with null.");
            return;
        }

        bossPortal = newPortal;
        bossPortal.SetActive(false);
        Debug.Log("New BossPortal assigned and set inactive.");
    }

    public void IncreaseKills()
    {
        if (difMgr == null) return;

        currentRound = difMgr.GetRound();

        EnemyKillScale currentEntry = enemyKillsEachRound.Find(e => e.round == currentRound);

        if (currentEntry != null)
        {
            currentEntry.killCount++;
            Debug.Log($"Round {currentRound}: {currentEntry.killCount} / {currentEntry.killGoal} kills");

            if (currentEntry.killCount >= currentEntry.killGoal)
            {
                Debug.Log($"✅ Quest complete for Round {currentRound}! Kill Goal: {currentEntry.killGoal}");

                if (bossPortal != null && !bossPortal.activeSelf)
                {
                    bossPortal.SetActive(true);
                    Debug.Log("BossPortal activated!");
                }
            }
        }
        else
        {
            Debug.LogWarning($"No kill goal defined for round {currentRound}. Adding default goal (10).");

            var newEntry = new EnemyKillScale
            {
                round = currentRound,
                killCount = 1,
                killGoal = 10
            };

            enemyKillsEachRound.Add(newEntry);
            Debug.Log($"Round {currentRound}: 1 / {newEntry.killGoal} kills");
        }

        UpdateKillCountText(); // Update the UI text after increasing kills
    }

    public void UpdateKillCountText()
    {
        if (killCountText == null) return;

        var currentEntry = enemyKillsEachRound.Find(e => e.round == currentRound);
        if (currentEntry != null)
        {
            if (currentEntry.killCount < currentEntry.killGoal)
            {
                killCountText.text = $"Kills: {currentEntry.killCount}/{currentEntry.killGoal}";
            }
            else
            {
                killCountText.text = "Proceed to the portal";
            }
        }
        else
        {
            killCountText.text = $"Kills: 0/10"; // Default display if no entry exists
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