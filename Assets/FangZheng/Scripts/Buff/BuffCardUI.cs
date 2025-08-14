
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

    private TutorialProggresion _Tutorial;
    public void Init(Transform Container , BuffData Data)
    {
        _Tutorial = FindFirstObjectByType<TutorialProggresion>();
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

    public void UnStopTime()
    {
        GamStates.instance.RemovePauseStuff();
        SoundManager.Instance.PlayVariationSFX("SelecetedBuffSFX");
    }

    public void SlecetCard()
    {
        if (_Tutorial != null)
        {
            _Tutorial.IfPlayerPerformAction("SelectCard");
        }
    }
}
