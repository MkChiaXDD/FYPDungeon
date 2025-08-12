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

    [SerializeField] List<BuffData> ListOfBuffsCommon;
    [SerializeField] List<BuffData> ListOfBuffsUnCommon;
    [SerializeField] List<BuffData> ListOfBuffsRare;
    [SerializeField] List<BuffData> ListOfBuffsEpic;
    [SerializeField] List<BuffData> ListOfBuffsLegendary;

    [SerializeField] int BuffAmount;
    [SerializeField] private List<BuffData> BuffThatWeSelected;

    public UnityEvent Spawn;


    public void Awake()
    {
        Spawn?.Invoke();
    }

    public void ClearAllLists()
    {
        SelectedBuffs.Clear();
        ObtainAbleBuffs.Clear();
        ListOfBuffsCommon.Clear();
        ListOfBuffsUnCommon.Clear();
        ListOfBuffsRare.Clear();
        ListOfBuffsEpic.Clear();
        ListOfBuffsLegendary.Clear();
        BuffThatWeSelected.Clear();
        //if (BuffThatWeSelected == null)
        //{
        //    BuffThatWeSelected = new List<BuffData>();
        //}
        //else
        //{
        //    BuffThatWeSelected.Clear();
        //}
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
        //SelectedBuffs.Clear();
        //ObtainAbleBuffs.Clear();

        ClearAllLists();

        foreach (BuffData item in ListOfBuffs)
        {
            if (CheckCanShow(item) == true)
            {
                ObtainAbleBuffs.Add(item);
                if (item.rarity == Rarity.Common)
                {
                    ListOfBuffsCommon.Add(item);
                }
                else if (item.rarity == Rarity.UnCommon)
                {
                    ListOfBuffsUnCommon.Add(item);
                }
                else if (item.rarity == Rarity.Rare)
                {
                    ListOfBuffsRare.Add(item);
                }
                else if (item.rarity == Rarity.Epic)
                {
                    ListOfBuffsEpic.Add(item);
                }
                else
                {
                    ListOfBuffsLegendary.Add(item);
                }
            }
        }

        for (int i = 0; i < BuffAmount; i++) {
            bool CanContinue = false;
            while (!CanContinue)
            {
                int randomInt = Random.Range(0, 101);
                BuffData Buff =  GetBuff(GetRarity(randomInt));
                if (Buff != null)
                {
                    CanContinue = true;
                    SelectedBuffs.Add(Buff);
                }
            }
        }

        //List<int> list = new List<int>();
        //for(int i = 0; i < ObtainAbleBuffs.Count; i++)
        //{
        //    list.Add(i);
        //}

        //for (int i = 0; i < list.Count; i++)
        //{
        //    int temp = list[i];
        //    int randomIndex = Random.Range(i, list.Count);
        //    list[i] = list[randomIndex];
        //    list[randomIndex] = temp;
        //}

        //List<int> result = list.GetRange(0, 3);

        //foreach(int i in result)
        //{
        //    SelectedBuffs.Add(ObtainAbleBuffs[i]);
        //}
    }

    public void CreateBuffCardUI()
    {
        //Debug.Log("StartCreatingUI");
        StartCoroutine(CreateCardsWithDelay());
    }

    public IEnumerator CreateCardsWithDelay()
    {
        List<Button> cardButtons = new List<Button>();

        foreach (BuffData data in SelectedBuffs)
        {
            Debug.Log("Card Created: " + data.name);
            GameObject card = GameObject.Instantiate(CardPrefab, CardStorage);
            if (card.GetComponent<BuffCardUI>() != null)
            {
                card.GetComponent<BuffCardUI>().Init(CardStorage, data);
            }

            if (card.GetComponent<Image>() != null)
            {
                Color color = Color.white;
                if (data.rarity == Rarity.Common)
                {
                    color = Color.white;
                }
                else if (data.rarity == Rarity.UnCommon)
                {
                    color = Color.green;
                }
                else if (data.rarity == Rarity.Rare)
                {
                    color = Color.blue;
                }
                else if (data.rarity == Rarity.Epic)
                {
                    color = Color.yellow;
                }
                else
                {
                    color = Color.red;
                }
                card.GetComponent<Image>().color = color;
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


        foreach (BuffData BuffReq in PlayerData.Instance._BuffObtain)
        {
            if (Buff.CorrespondingBuffs.Contains(BuffReq) )
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
        else if (Value < 15)
        {
            return Rarity.Epic;
        }
        else if (Value < 30)
        {
            return Rarity.Rare;
        }
        else if (Value < 50)
        {
            return Rarity.UnCommon;
        }
        else
        {
            return Rarity.Common;
        }
    }

    private BuffData GetBuff(Rarity Value) {
        BuffData Buff_Chosen = null;
        if (Value == Rarity.Common)
        {
            if (ListOfBuffsCommon.Count > 0)
            {
                int randomIndex = Random.Range(0, ListOfBuffsCommon.Count);
                Buff_Chosen = ListOfBuffsCommon[randomIndex];
                ListOfBuffsCommon.Remove(ListOfBuffsCommon[randomIndex]);
            }
            //return ListOfBuffsCommon[randomIndex];
        }
        else if (Value == Rarity.UnCommon)
        {
            if (ListOfBuffsUnCommon.Count > 0)
            {
                int randomIndex = Random.Range(0, ListOfBuffsUnCommon.Count);
                Buff_Chosen = ListOfBuffsUnCommon[randomIndex];
                ListOfBuffsUnCommon.Remove(ListOfBuffsUnCommon[randomIndex]);
            }
        }
        else if (Value == Rarity.Rare)
        {
            if (ListOfBuffsRare.Count > 0)
            {
                int randomIndex = Random.Range(0, ListOfBuffsRare.Count);
                Buff_Chosen = ListOfBuffsRare[randomIndex];
                ListOfBuffsRare.Remove(ListOfBuffsRare[randomIndex]);
            }
        }
        else if (Value == Rarity.Epic)
        {
            if (ListOfBuffsEpic.Count > 0)
            {
                int randomIndex = Random.Range(0, ListOfBuffsEpic.Count);
                Buff_Chosen = ListOfBuffsEpic[randomIndex];
                ListOfBuffsEpic.Remove(ListOfBuffsEpic[randomIndex]);
            }
        }
        else if (Value == Rarity.Legendary)
        {
            if (ListOfBuffsLegendary.Count > 0)
            {
                int randomIndex = Random.Range(0, ListOfBuffsLegendary.Count);
                Buff_Chosen = ListOfBuffsLegendary[randomIndex];
                ListOfBuffsLegendary.Remove(ListOfBuffsLegendary[randomIndex]);
            }
        }
        return Buff_Chosen;
    }
}
