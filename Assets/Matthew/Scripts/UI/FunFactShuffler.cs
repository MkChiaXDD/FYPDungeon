using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;



public class FunFactShuffler : MonoBehaviour
{

    [Header("Fun Facts Settings")]   
    [SerializeField] private List<FunFact> funFacts = new List<FunFact>();
    [SerializeField] private float displayDuration = 5f; 

    [Header("UI References")]
    [SerializeField] private TMP_Text factTextComponent;
    [SerializeField] private Image factImageComponent; 

    private List<FunFact> unusedFacts = new List<FunFact>();
    private float timer;
    private int currentIndex = -1;

    [System.Serializable]
    public class FunFact
    {
        [TextArea(2, 5)] 
        public string factText;
        public Sprite optionalImage; // Optional
    }

    private void Awake()
    {
        // Initialize the unused facts list
        ResetUnusedFacts();
    }

    private void OnEnable()
    {
        // Show a new fact when the object becomes active
        ShowNextFact();
    }

    private void Update()
    {
        // Cycle through facts after displayDuration
        timer += Time.deltaTime;
        if (timer >= displayDuration)
        {
            timer = 0f;
            ShowNextFact();
        }
    }

    public void ShowNextFact()
    {
        // rset after used up
        if (unusedFacts.Count == 0)
        {
            ResetUnusedFacts();
        }

        // Get a random index
        int randomIndex = Random.Range(0, unusedFacts.Count);
        FunFact selectedFact = unusedFacts[randomIndex];

        //prevent repeats until after finishing
        unusedFacts.RemoveAt(randomIndex);

        // Display  fact
        DisplayFact(selectedFact);
    }

    private void DisplayFact(FunFact fact)
    {
        if (factTextComponent != null)
        {
            factTextComponent.text = fact.factText;
        }

        
        if (factImageComponent != null)
        {
            factImageComponent.sprite = fact.optionalImage;
            factImageComponent.gameObject.SetActive(fact.optionalImage != null);
        }
    }

    private void ResetUnusedFacts()
    {
        unusedFacts.Clear();
        unusedFacts.AddRange(funFacts);
    }

   
    [ContextMenu("Add New Fact")]
    private void AddNewFact()
    {
        funFacts.Add(new FunFact());
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}