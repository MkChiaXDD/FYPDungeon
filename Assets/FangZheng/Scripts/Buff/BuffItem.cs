using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BuffItem : MonoBehaviour
{
    [SerializeField] private BuffSelectionUI Buff;
    [SerializeField] private GameObject CanCollectUI;
    private bool PlayerIsInBound;
    private UnityEvent Collect;
    private TutorialProggresion _Tutorial;

    private void Start()
    {
        if(Buff == null)
        {
            Buff = FindFirstObjectByType<BuffSelectionUI>();
        }

        _Tutorial = FindFirstObjectByType<TutorialProggresion>();
    }
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player"))
        {
            PlayerIsInBound = true;
            CanCollectUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerIsInBound = false;
            CanCollectUI.SetActive(false);
        }
    }

    private void CollectBuff()
    {
        if(GamStates.instance.State == GamStates.GameState.Paused) return;

        
        if (Input.GetKeyDown(KeyCode.E) && PlayerIsInBound == true)
        {
            //Collect?.Invoke();
            SoundManager.Instance.PlayVariationSFX("CollectBuffSFX");
            if (_Tutorial != null)
            {
                _Tutorial.IfPlayerPerformAction("PickUpBuff");
            }

            GamStates.instance.AddPauseStuff();
            Buff.Select();
            Buff.CreateBuffCardUI();
            Destroy(this.gameObject);
            
        }
    }

    private void Update()
    {
        CollectBuff();
    }
}
