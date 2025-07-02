using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static BuffData;

public class BuffSelectionUI : MonoBehaviour
{
    [SerializeField] List<BuffData> ListOfBuffs;
    [SerializeField] GameObject CardPrefab;
    [SerializeField] List<BuffData> SelectedBuffs;

    [SerializeField] List<BuffData> ObtainAbleBuffs;

    [SerializeField] Transform CardStorage;
    [SerializeField] float DelayTime = 1;
    public UnityEvent Spawn;
    public void Awake()
    {
        Spawn?.Invoke();
    }


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            Select();
            CreateBuffCardUI();
        }
    }
    public void Select()
    {
        SelectedBuffs.Clear();
        ObtainAbleBuffs.Clear();

        foreach (BuffData item in ListOfBuffs)
        {
            if (CheckCanShow(item) == true)
            {
                ObtainAbleBuffs.Add(item);
            }
        }

        List<int> list = new List<int>();
        for(int i = 0; i < ObtainAbleBuffs.Count; i++)
        {
            list.Add(i);
        }

        for (int i = 0; i < list.Count; i++)
        {
            int temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }

        List<int> result = list.GetRange(0, 3);

        foreach(int i in result)
        {
            SelectedBuffs.Add(ObtainAbleBuffs[i]);
        }
    }

    public void CreateBuffCardUI()
    {
        StartCoroutine(CreateCardsWithDelay());
    }

    public IEnumerator CreateCardsWithDelay()
    {
        List<Button> cardButtons = new List<Button>();

        foreach (BuffData data in SelectedBuffs)
        {
            GameObject card = GameObject.Instantiate(CardPrefab, CardStorage);
            if (card.GetComponent<BuffCardUI>() != null)
            {
                card.GetComponent<BuffCardUI>().Init(CardStorage, data);
            }

            Button button = card.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
                cardButtons.Add(button);
            }
        }

        yield return new WaitForSeconds(DelayTime);

        foreach (Button button in cardButtons)
        {
            button.interactable = true;
        }
    }


    public void ClearCard()
    {
        foreach (Transform child in CardStorage)
        {
            Destroy(child.gameObject);
        }
    }

    public bool CheckCanShow(BuffData Buff)
    {
        if (Buff.OneTimeUnlock == true)
        {
            if (PlayerData.Instance._BuffObtain.Contains(Buff))
            {
                return false;
            }
        }

        foreach (BuffData BuffReq in Buff.RequiredBuffs)
        {
            if (!PlayerData.Instance._BuffObtain.Contains(BuffReq))
            {
                return false;
            }
        }
        return true;
    }

    private Rarity GetRarity(int Value){
        if (Value < 5)
        {
            return Rarity.Legendary;
        }
        else if (Value > 15)
        {
            return Rarity.Epic;
        }
        else if (Value > 30)
        {
            return Rarity.Rare;
        }
        else if (Value > 50)
        {
            return Rarity.UnCommon;
        }
        else
        {
            return Rarity.Common;
        }
    }
}
