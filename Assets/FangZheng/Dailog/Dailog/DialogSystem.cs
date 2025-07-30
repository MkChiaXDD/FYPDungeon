
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogSystem : MonoBehaviour
{


    [SerializeField] public Text _Current_Dialog;

    [SerializeField] public List<char> _Text_Array;

    [SerializeField] public List<char> _Current_Char;

    [SerializeField] public int _Current_Line;

    [SerializeField] public int _Dialog_Length;

    [SerializeField] public Text _Name;

    [SerializeField] public Image _NPC_Photo;

    [SerializeField] public Npc _npc;

    //Speed Of Generated Text
    [SerializeField] public int _Speed = 1;

    [SerializeField] public Transform buttonContainer;

    [SerializeField] public Button _ButtonPrefab;

    [SerializeField] public float Cooldown_Duration = 0.2f;

    [SerializeField] public float Cooldown = 0;

    [SerializeField] public GameObject ChatlogsContainer;

    public bool DailogEnd = true;

    public UnityEvent DailogFinish;


    void Start()
    {
        _Text_Array = new List<char>();
        _Current_Char = new List<char>();
        //Activate(_npc);
        Cooldown = Cooldown_Duration;
    }

    private void Update()
    {

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) ) && Cooldown > Cooldown_Duration)
        {
            Cooldown = 0;
            if (_Current_Dialog.text != _npc._Lines[_Current_Line]._Text) {
                _Text_Array.Clear();
                _Current_Char.Clear();
                _Current_Dialog.text = _npc._Lines[_Current_Line]._Text;
                Debug.Log("Spacebar was pressed!");
                
            }
            else
            {

                if (_npc._Lines[_Current_Line]._MultiChoose == false) {
                    NextLine();
                }
            }
        }

        Cooldown += Time.deltaTime;
    }
    public void Activate(Npc npc)
    {
        Debug.Log("Start Chat");
        DailogEnd = false;
        _Current_Line = 0;
        _Dialog_Length = npc._Lines.Count;
        _Name.text = npc._Name;
        _NPC_Photo.sprite = npc._Photo;
        _Text_Array = new List<char>();
        _Current_Char = new List<char>();
        _Current_Dialog.text = "";
        _npc = npc;
        ChatlogsContainer.SetActive(true);
        foreach (char Ch in npc._Lines[_Current_Line]._Text)
        {
            _Text_Array.Add(Ch);
        }

        
        StartCoroutine(GenerateLine());
    }

    public void NextLine()
    {
        if (_npc._Lines[_Current_Line]._MultiChoose == true)
        {
            CreateOption();
        }
        else
        {

            _Current_Line += 1;
            if (_Current_Line < _Dialog_Length)
            {

                if (_npc._Lines[_Current_Line]._Special_Text == false)
                {


                    _Current_Char.Clear();
                    _Text_Array.Clear();
                    foreach (char Ch in _npc._Lines[_Current_Line]._Text)
                    {
                        _Text_Array.Add(Ch);
                    }
                    StartCoroutine(GenerateLine());

                }
                else
                {
                    int i = _Current_Line;
                    for (i = _Current_Line; i < _Dialog_Length; i++)
                    {
                        if (_npc._Lines[i]._Special_Text == false)
                        {
                            _Current_Line = i;
                            _Current_Char.Clear();
                            _Text_Array.Clear();
                            foreach (char Ch in _npc._Lines[_Current_Line]._Text)
                            {
                                _Text_Array.Add(Ch);
                            }
                            StartCoroutine(GenerateLine());
                        }
                    }

                }
            }
            else
            {
                DailogEnd = true;
                Deactivate();
                DailogFinish?.Invoke();
            }
        }
        //Deactivate();
    }

    public void Deactivate()
    {
        ChatlogsContainer.SetActive(false);
    }

    public IEnumerator GenerateLine()
    {
        Debug.Log("Activate Chat");
        DisableButtons();
        for (int i = 0; i < _Text_Array.Count; i++)
        {

            _Current_Char.Add(_Text_Array[i]);
            _Current_Dialog.text = new string(_Current_Char.ToArray());
            yield return new WaitForSeconds(0.12f / _Speed);
        }


        if (_npc._Lines[_Current_Line]._MultiChoose == true)
        {
            CreateOption();
        }
        else
        {
            CreateButtons("Next Line", () => NextLine());

        }
        //NextLine();
    }

    public void CreateButtons(string buttonText, UnityEngine.Events.UnityAction onClickAction)
    {
        Button newButton = Instantiate(_ButtonPrefab, buttonContainer);
        TextMeshProUGUI tmpText = newButton.GetComponentInChildren<TextMeshProUGUI>();

        if (tmpText != null)
        {
            tmpText.text = buttonText;
        }

        newButton.onClick.AddListener(onClickAction);
    } 

    public void CreateOption()
    {
        foreach (var item in _npc._Lines[_Current_Line]._Choices) {
            CreateButtons(item._Option_Text , () => SkipToLine(item._Option_Value));
        }
    }

    public void SkipToLine(int index)
    {
        DisableButtons();
        _Current_Line = index;
        _Current_Char.Clear();
        _Text_Array.Clear();
        foreach (char Ch in _npc._Lines[index]._Text)
        {
            _Text_Array.Add(Ch);
        }
        StartCoroutine(GenerateLine());

    }

    public void DisableButtons()
    {
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
