using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffSelectionUI : MonoBehaviour
{
    [SerializeField] List<BuffData> ListOfBuffs;
    [SerializeField] GameObject CardPrefab;
    [SerializeField] List<BuffData> SelectedBuffs;
    [SerializeField] Transform CardStorage;

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
        List<int> list = new List<int>();
        for(int i = 0; i < ListOfBuffs.Count; i++)
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
            SelectedBuffs.Add(ListOfBuffs[i]);
        }
    }
    
    public void CreateBuffCardUI()
    {
        foreach (BuffData data in SelectedBuffs) {
            GameObject card = GameObject.Instantiate(CardPrefab, CardStorage);
            if (card.GetComponent<BuffCardUI>() != null)
            {
                card.GetComponent<BuffCardUI>().Init(CardStorage, data);
            }
        }
    }

    public void ClearCard()
    {
        foreach (Transform child in CardStorage)
        {
            Destroy(child.gameObject);
        }
    }
}
