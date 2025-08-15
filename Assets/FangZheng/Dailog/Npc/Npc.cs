using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class Options
{
    public string _Option_Text;
    public int _Option_Value;
}

[System.Serializable]
public class DialogLine
{
    public enum Speaker
    {
        Player,
        Npc,
        Narrator,
    }

    public string _Text;
    public bool _Special_Text;
    public Speaker _SpeakerType;    
    public bool _MultiChoose;
    public List<Options> _Choices;
}

[CreateAssetMenu(menuName = "NPC")]
public class Npc : ScriptableObject
{
    public string _Name;

    public List<DialogLine> _Lines;

    public Sprite _Photo;


}
