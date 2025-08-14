using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingScript : MonoBehaviour
{

    [SerializeField] private GameObject EndingPanel;

    public void ProceedToWinscreen()
    {
        EndingPanel.SetActive(true);
        GamStates.instance.AddPauseStuff();
    }
}
