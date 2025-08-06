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
        Cooldown = Cooldown_Duration;
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && Cooldown > Cooldown_Duration)
        {
            Cooldown = 0;

            if (_npc != null && _npc._Lines != null && _Current_Line < _npc._Lines.Count &&
                _Current_Dialog != null && _Current_Dialog.text != _npc._Lines[_Current_Line]._Text)
            {
                _Text_Array.Clear();
                _Current_Char.Clear();
                _Current_Dialog.text = _npc._Lines[_Current_Line]._Text;
                Debug.Log("Spacebar was pressed!");
            }
            else if (_npc != null && _npc._Lines != null && _Current_Line < _npc._Lines.Count &&
                     _npc._Lines[_Current_Line]._MultiChoose == false)
            {
                NextLine();
            }
        }

        Cooldown += Time.deltaTime;
    }

    public void Activate(Npc npc)
    {
        if (npc == null || npc._Lines == null || npc._Lines.Count == 0) return;

        Debug.Log("Start Chat");
        GamStates.instance.AddPauseStuff();

        DailogEnd = false;
        _Current_Line = 0;
        _Dialog_Length = npc._Lines.Count;

        if (_Name != null) _Name.text = npc._Name;
        if (_NPC_Photo != null) _NPC_Photo.sprite = npc._Photo;
        if (_Current_Dialog != null) _Current_Dialog.text = "";
        if (ChatlogsContainer != null) ChatlogsContainer.SetActive(true);

        _Text_Array = new List<char>();
        _Current_Char = new List<char>();
        _npc = npc;

        foreach (char Ch in npc._Lines[_Current_Line]._Text)
        {
            _Text_Array.Add(Ch);
        }

        StartCoroutine(GenerateLine());
    }

    public void NextLine()
    {
        if (_npc == null || _npc._Lines == null || _Current_Line >= _npc._Lines.Count) return;

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
                    for (int i = _Current_Line; i < _Dialog_Length; i++)
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
                            break;
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
    }

    public void Deactivate()
    {
        if (ChatlogsContainer != null)
        {
            GamStates.instance.RemovePauseStuff();
            ChatlogsContainer.SetActive(false);
        }
    }

    public IEnumerator GenerateLine()
    {
        Debug.Log("Activate Chat");
        DisableButtons();

        for (int i = 0; i < _Text_Array.Count; i++)
        {
            _Current_Char.Add(_Text_Array[i]);
            if (_Current_Dialog != null)
                _Current_Dialog.text = new string(_Current_Char.ToArray());

            yield return new WaitForSeconds(0.12f / Mathf.Max(1, _Speed));
        }

        if (_npc != null && _npc._Lines != null && _Current_Line < _npc._Lines.Count)
        {
            if (_npc._Lines[_Current_Line]._MultiChoose == true)
            {
                CreateOption();
            }
            else
            {
                CreateButtons("Next Line", () => NextLine());
            }
        }
    }

    public void CreateButtons(string buttonText, UnityAction onClickAction)
    {
        if (_ButtonPrefab == null || buttonContainer == null) return;

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
        if (_npc == null || _npc._Lines == null || _Current_Line >= _npc._Lines.Count) return;

        foreach (var item in _npc._Lines[_Current_Line]._Choices)
        {
            CreateButtons(item._Option_Text, () => SkipToLine(item._Option_Value));
        }
    }

    public void SkipToLine(int index)
    {
        if (_npc == null || _npc._Lines == null || index < 0 || index >= _npc._Lines.Count) return;

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
        if (buttonContainer == null) return;

        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
