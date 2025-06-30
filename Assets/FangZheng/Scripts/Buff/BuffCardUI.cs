using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffCardUI : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text Name;
    [SerializeField] private TMP_Text Describe;

    [SerializeField] private BuffData buffdata;
    [SerializeField] private Transform Transform;
    [SerializeField] private Button Button;
    [SerializeField] private PlayerData PlayerData;
    public void Init(Transform Container , BuffData Data)
    {
        buffdata = Data;
        image.sprite = buffdata.Icon;
        Name.text = buffdata.Name;
        Describe.text = buffdata.Description;
        this.transform.SetParent(Container);
        Button.onClick.AddListener(() =>
        {
            //PlayerController.Instance.AddBuff(buffdata);
            PlayerData.Instance.AddBuff(buffdata);
            Container.GetComponentInParent<BuffSelectionUI>().ClearCard();                    
        });
    }


}
