using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("Door References (N,E,S,W)")]
    [SerializeField] private GameObject[] _doorways;
    [SerializeField] private GameObject[] _doorBlocks;

    public void InitializeDoors(bool[] activeDoors)
    {
        for (int i = 0; i < 4; i++)
        {
            // Ensure we don't exceed array bounds
            bool validDoorway = i < _doorways.Length && _doorways[i] != null;
            bool validDoorBlock = i < _doorBlocks.Length && _doorBlocks[i] != null;

            // Activate doorways only when connected
            if (validDoorway)
            {
                _doorways[i].SetActive(activeDoors[i]);
            }

            // Activate doorblocks only when NOT connected
            if (validDoorBlock)
            {
                _doorBlocks[i].SetActive(!activeDoors[i]);
            }

            // Final safety: Never allow both to be active
            if (validDoorway && validDoorBlock)
            {
                if (_doorways[i].activeSelf && _doorBlocks[i].activeSelf)
                {
                    Debug.LogError($"Conflict in {name}'s direction {i}! Fixing...");
                    _doorBlocks[i].SetActive(false);
                }
            }
        }
    }
}