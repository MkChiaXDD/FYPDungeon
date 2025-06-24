using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [SerializeField] private int round = 1;

    public void IncreaseRound()
    {
        round++;
    }

    public int GetRound()
    {
        return round;
    }
}
