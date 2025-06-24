using UnityEngine;

public class BossCheckDeath : MonoBehaviour
{
    public void DieProceed()
    {
        FindFirstObjectByType<FarthestRoom>().NextLevel();
    }
}
