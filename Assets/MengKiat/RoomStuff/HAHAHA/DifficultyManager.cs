using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [SerializeField] private int round;

    public void IncreaseRound()
    {
        round++;
    }
}
